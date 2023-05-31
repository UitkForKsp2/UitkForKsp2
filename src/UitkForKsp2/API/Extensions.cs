using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Contains extension methods for UI Toolkit documents and elements.
/// </summary>
[PublicAPI]
public static class Extensions
{
    /// <summary>
    /// Set a property on a VisualElement.
    /// </summary>
    /// <param name="element">The element on which to set the property.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element on which the property was set.</returns>
    public static T Set<T>(this T element, string name, object value) where T : VisualElement
    {
        element.SetProperty(name, value);
        return element;
    }

    /// <summary>
    /// Set the text of a TextElement.
    /// </summary>
    /// <param name="element">The element on which to set the text property.</param>
    /// <param name="text">The text to set.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of TextElement.</typeparam>
    /// <returns>The element on which the text property was set.</returns>
    public static T Text<T>(this T element, string text) where T : TextElement
    {
        element.text = text;
        return element;
    }

    /// <summary>
    /// Add children to a VisualElement.
    /// </summary>
    /// <param name="element">The element to which to add the children.</param>
    /// <param name="children">The children VisualElements to add.</param>
    /// <typeparam name="T">The type of the parent element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The parent element to which the children were added.</returns>
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

    /// <summary>
    /// Make a VisualElement draggable by adding a DragManipulator.
    /// </summary>
    /// <param name="element">The element to make draggable.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was made draggable.</returns>
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