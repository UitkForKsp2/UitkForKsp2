using UnityEngine.UIElements;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace UitkForKsp2.API;

public static class Extensions
{
    public static T Set<T>(this T element, string name, object value) where T : VisualElement
    {
        element.SetProperty(name, value);
        return element;
    }

    public static T Text<T>(this T element, string text) where T : TextElement
    {
        element.text = text;
        return element;
    }

    public static T AddChildren<T>(this T element, IEnumerable<VisualElement> children) where T : VisualElement
    {
        if (children == null)
        {
            return element;
        }

        foreach (var child in children)
        {
            element.Add(child);
        }

        return element;
    }

    public static T MakeDraggable<T>(this T element) where T : VisualElement
    {
        element.AddManipulator(new DragManipulator());
        return element;
    }

    /// <summary>
    /// Automatically localize elements in a document. Only elements with a string property "text" whose value is a
    /// localization key starting with '#' will be localized.
    /// </summary>
    /// <param name="document">The document in which to localize all localizable elements.</param>
    /// <returns>The DocumentLocalization component which was added to the document.</returns>
    public static DocumentLocalization EnableLocalization(this UIDocument document)
    {
        return document.gameObject.AddComponent<DocumentLocalization>();
    }
}