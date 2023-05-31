using I2.Loc;
using UitkForKsp2;
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
    private readonly Dictionary<VisualElement, string> _elementDictionary = new();

    private void Awake()
    {
        var document = gameObject.GetComponentInParent<UIDocument>(includeInactive: true);
        RegisterDocument(document);
    }

    private void OnEnable() => LocalizationManager.OnLocalizeEvent += Localize;

    private void OnDisable() => LocalizationManager.OnLocalizeEvent -= Localize;

    /// <summary>
    /// Register or update an element to be localized. The element must have a property named "text" of type string,
    /// and the value of the property must be a localization key starting with '#'.
    /// </summary>
    /// <param name="element">The element to register.</param>
    public void RegisterElement(VisualElement element)
    {
        var textProperty = element?.GetType().GetProperty("text");
        if (textProperty?.GetValue(element) is not string key || string.IsNullOrEmpty(key) || key[0] != '#')
        {
            return;
        }

        var trimmedKey = key.TrimStart('#');
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
        foreach (var item in _elementDictionary)
        {
            UpdateElementLocalization(item.Key, item.Value);
        }
    }

    private void RegisterElementsInternal(VisualElement element)
    {
        if (element == null)
        {
            return;
        }

        RegisterElement(element);

        var hierarchy = element.hierarchy;
        for (var i = 0; i < hierarchy.childCount; i++)
        {
            RegisterElementsInternal(hierarchy.ElementAt(i));
        }
    }

    private static void UpdateElementLocalization(VisualElement element, string localizationKey)
    {
        var localization = LocalizationManager.GetTranslation(localizationKey);
        if (localization == null)
        {
            UitkForKsp2Plugin.Logger.LogError($"Localization key '{localizationKey}' not found");
        }
        else
        {
            element.GetType().GetProperty("text")?.SetValue(element, localization);
        }
    }
}