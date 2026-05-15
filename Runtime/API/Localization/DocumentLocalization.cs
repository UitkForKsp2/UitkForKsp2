using System.Collections.Generic;
using System.Reflection;
using ReduxLib.GameInterfaces;
using UitkForKsp2;
using UitkForKsp2.MVVM.Converters;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// <para>A component which automatically localizes all elements in a document. Only elements with a string property
/// "text" whose value is a localization key starting with '#' will be localized.</para>
/// <para>The <c>LocalizationManager.OnLocalizeEvent</c> is handled to automatically update the localization of all
/// registered elements when the game language is changed.</para>
/// </summary>
[PublicAPI]
[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class DocumentLocalization : MonoBehaviour
{
    public const string UppercaseClass = "uppercase";

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
        var document = gameObject.GetComponentInParent<UIDocument>(includeInactive: true);
        RegisterDocument(document);
    }

    private void OnEnable() => ILocalizer.Instance.OnLocalize += Localize;

    private void OnDisable() => ILocalizer.Instance.OnLocalize -= Localize;

    /// <summary>
    /// Register or update an element to be localized. The element must have a property named "text" of type string,
    /// and the value of the property must be a localization key starting with '#'.
    /// </summary>
    /// <param name="element">The element to register.</param>
    public void RegisterElement(VisualElement element)
    {
        PropertyInfo? textProperty = element?.GetType().GetProperty("text");
        if (textProperty?.GetValue(element) is not string key || string.IsNullOrEmpty(key) || key[0] != '#')
        {
            return;
        }

        string? trimmedKey = key.TrimStart('#');
        _elementDictionary[element] = trimmedKey;
        UpdateElementLocalization(element, trimmedKey);
    }

    /// <summary>
    /// Register all elements in the document to be localized. Only elements with a string property "text" whose value
    /// is a localization key starting with '#' will be registered.
    /// </summary>
    /// <param name="document">The document in which to register all localizable elements.</param>
    public void RegisterDocument(UIDocument document)
    {
        if (document == null)
        {
            return;
        }

        _elementDictionary.Clear();
        RegisterElementsInternal(document.rootVisualElement);
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
        var document = gameObject.GetComponentInParent<UIDocument>(includeInactive: true);
        if (document?.rootVisualElement == null)
        {
            return;
        }

        RefreshConverterBindingsInternal(document.rootVisualElement);
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
