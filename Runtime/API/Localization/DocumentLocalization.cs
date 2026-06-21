using System.Collections.Generic;
using System.Reflection;
using ReduxLib.GameInterfaces;
using UitkForKsp2;
using UitkForKsp2.API;
using UitkForKsp2.MVVM.Converters;
using UitkForKsp2.Panel;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// <para>A component which automatically localizes all elements in a window. Only elements with a string property
/// "text" whose value is a localization key starting with '#' will be localized.</para>
/// <para>The <c>LocalizationManager.OnLocalizeEvent</c> is handled to automatically update the localization of all
/// registered elements when the game language is changed.</para>
/// </summary>
[PublicAPI]
[DisallowMultipleComponent]
public class DocumentLocalization : MonoBehaviour
{
    public const string UppercaseClass = "uppercase";

    private PanelRenderer? _renderer;

    private readonly Dictionary<VisualElement, string> _elementDictionary = new();
    private static readonly MethodInfo CreateBindingRequestsMethod = typeof(VisualElement).GetMethod(
        "CreateBindingRequests",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );
    private static readonly MethodInfo ProcessBindingRequestsMethod = typeof(VisualElement).GetMethod(
        "ProcessBindingRequests",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );
    private static readonly PropertyInfo SourceToUiConvertersStringProperty = typeof(DataBinding).GetProperty(
        "sourceToUiConvertersString",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );

    private void Awake()
    {
        if (_renderer == null && gameObject.GetComponentInParent<PanelRenderer>(includeInactive: true) is { } renderer)
        {
            RegisterRenderer(renderer);
        }
    }

    private void OnEnable() => ILocalizer.Instance.OnLocalize += Localize;

    private void OnDisable() => ILocalizer.Instance.OnLocalize -= Localize;

    private void OnDestroy()
    {
        if (_renderer != null && _renderer.GetComponent<WindowComponent>() is { } component)
        {
            component.RootResolved -= OnRootResolved;
        }
    }

    /// <summary>
    /// Register or update an element to be localized. The element must have a property named "text" of type string,
    /// and the value of the property must be a localization key starting with '#'.
    /// </summary>
    /// <param name="element">The element to register.</param>
    public void RegisterElement(VisualElement element)
    {
        PropertyInfo? textProperty = element?.GetType().GetProperty("text");
        if (textProperty?.GetValue(element) is not string key || !IsLocalizationKey(key))
        {
            return;
        }

        string? trimmedKey = key.TrimStart('#');
        _elementDictionary[element] = trimmedKey;
        UpdateElementLocalization(element, trimmedKey);
    }

    /// <summary>
    /// Localization keys start with '#' followed by a key path (e.g. <c>#Category/Key</c>). Strings like CSS hex
    /// colors (<c>#000000</c>, <c>#RGB</c>, <c>#RRGGBBAA</c>) also start with '#' but must not be treated as keys.
    /// </summary>
    private static bool IsLocalizationKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key[0] != '#')
        {
            return false;
        }

        string body = key[1..];
        return body.Length is not (3 or 4 or 6 or 8) || !IsAllHex(body);
    }

    private static bool IsAllHex(string value)
    {
        foreach (char c in value)
        {
            if (c is (< '0' or > '9') and (< 'a' or > 'f') and (< 'A' or > 'F'))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Register the window whose elements should be localized, and re-localize automatically whenever its UI is
    /// rebuilt (live reload). Only elements with a string property "text" whose value is a localization key starting
    /// with '#' are registered.
    /// </summary>
    /// <param name="renderer">The window renderer whose elements to localize.</param>
    public void RegisterRenderer(PanelRenderer renderer)
    {
        if (renderer == null || _renderer == renderer)
        {
            // Already registered to this renderer. EnableLocalization both adds the component (whose Awake
            // registers) and calls this explicitly, so guard against re-running the registration twice.
            return;
        }

        if (_renderer != null && _renderer.GetComponent<WindowComponent>() is { } previous)
        {
            previous.RootResolved -= OnRootResolved;
        }

        _renderer = renderer;

        if (renderer.GetComponent<WindowComponent>() is { } component)
        {
            component.RootResolved -= OnRootResolved;
            component.RootResolved += OnRootResolved;
        }

        if (renderer.GetPanelRoot() is { } panelRoot)
        {
            RegisterTree(panelRoot);
        }
    }

    private void OnRootResolved(VisualElement windowRoot)
    {
        if (_renderer.GetPanelRoot() is { } panelRoot)
        {
            RegisterTree(panelRoot);
        }
    }

    private void RegisterTree(VisualElement panelRoot)
    {
        WalkAndLocalize(panelRoot);

        // Re-localize when the tree attaches to a panel. Some sub-elements are created lazily on attach (e.g.
        // PlaceholderTextField builds its placeholder label only once it is attached), so a walk over a detached
        // or hidden window misses them; this catches them when the window is shown.
        panelRoot.UnregisterCallback<AttachToPanelEvent>(OnPanelRootAttached);
        panelRoot.RegisterCallback<AttachToPanelEvent>(OnPanelRootAttached);
    }

    private void OnPanelRootAttached(AttachToPanelEvent evt)
    {
        // Defer a tick so the lazily-created children have been built by their own AttachToPanelEvent handlers.
        // Only register/localize elements that are new since the last walk (e.g. a freshly-built placeholder label),
        // rather than clearing and re-localizing the whole tree on every show.
        if (evt.currentTarget is VisualElement panelRoot)
        {
            panelRoot.schedule.Execute(() => RegisterNewElementsInternal(panelRoot));
        }
    }

    private void RegisterNewElementsInternal(VisualElement element)
    {
        if (element == null)
        {
            return;
        }

        // RegisterElement both adds the element and localizes it once; skip elements we already know so existing
        // ones are not re-localized and the converter bindings are not refreshed for the whole tree on every show.
        if (!_elementDictionary.ContainsKey(element))
        {
            RegisterElement(element);
        }

        VisualElement.Hierarchy hierarchy = element.hierarchy;
        for (int i = 0; i < hierarchy.childCount; i++)
        {
            RegisterNewElementsInternal(hierarchy.ElementAt(i));
        }
    }

    private void WalkAndLocalize(VisualElement panelRoot)
    {
        _elementDictionary.Clear();
        RegisterElementsInternal(panelRoot);
        Localize();
    }

    /// <summary>
    /// Update the localization of all registered elements.
    /// </summary>
    public void Localize()
    {
        foreach ((VisualElement key, string value) in _elementDictionary)
        {
            UpdateElementLocalization(key, value);
        }

        RefreshConverterBindings();
    }

    private void RegisterElementsInternal(VisualElement element)
    {
        if (element == null)
        {
            return;
        }

        RegisterElement(element);

        VisualElement.Hierarchy hierarchy = element.hierarchy;
        for (int i = 0; i < hierarchy.childCount; i++)
        {
            RegisterElementsInternal(hierarchy.ElementAt(i));
        }
    }

    private void RefreshConverterBindings()
    {
        // The converter binding refresh below uses CreateBindingRequests, which asserts unless the element is
        // attached to a panel. When the window is hidden its PanelRenderer is disabled and detached, so skip it —
        // direct text localization still applies, and Unity re-evaluates bindings when the panel re-attaches on show.
        if (_renderer.GetPanelRoot() is not { panel: not null } panelRoot)
        {
            return;
        }

        RefreshConverterBindingsInternal(panelRoot);
    }

    private static void RefreshConverterBindingsInternal(VisualElement element)
    {
        bool needsProcessing = false;

        foreach (BindingInfo bindingInfo in element.GetBindingInfos())
        {
            if (bindingInfo.binding is not DataBinding dataBinding || !RefreshesOnLocalizationChange(dataBinding))
            {
                continue;
            }

            dataBinding.MarkDirty();
            needsProcessing = true;
        }

        if (needsProcessing)
        {
            CreateBindingRequestsMethod?.Invoke(element, null);
            ProcessBindingRequestsMethod?.Invoke(element, null);
        }

        VisualElement.Hierarchy hierarchy = element.hierarchy;
        for (int i = 0; i < hierarchy.childCount; i++)
        {
            RefreshConverterBindingsInternal(hierarchy.ElementAt(i));
        }
    }

    private static bool RefreshesOnLocalizationChange(DataBinding dataBinding)
    {
        string converters = SourceToUiConvertersStringProperty?.GetValue(dataBinding) as string;
        if (string.IsNullOrWhiteSpace(converters))
        {
            return false;
        }

        string[] converterNames = converters.Split(',');
        foreach (string converterName in converterNames)
        {
            if (ConverterGroupRegistry.RefreshesOnLocalizationChange(converterName.Trim()))
            {
                return true;
            }
        }

        return false;
    }

    private static void UpdateElementLocalization(VisualElement element, string localizationKey)
    {
        string? localization = ILocalizer.Instance.GetTranslation(localizationKey);
        if (localization == null)
        {
            UitkForKsp2Plugin.Logger.LogError($"Localization key '{localizationKey}' not found");
        }
        else
        {
            if (element.ClassListContains(UppercaseClass))
            {
                localization = localization.ToUpperInvariant();
            }

            element.GetType().GetProperty("text")?.SetValue(element, localization);
        }
    }
}
