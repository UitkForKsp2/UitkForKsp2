using KSP.Game;
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

    private static EventCallback<GeometryChangedEvent> _geometryChanged;

    /// <summary>
    /// Set the default position of an element using a callback to calculate the position.
    /// </summary>
    /// <param name="element">The element to position.</param>
    /// <param name="calculatePosition">
    ///     The callback which will be called when the element is resized. The callback will be passed the size of the
    ///     element and should return the position to set the element to in the reference resolution.
    /// </param>
    public static void SetDefaultPosition(this VisualElement element, Func<Vector2, Vector2> calculatePosition)
    {
        _geometryChanged ??= evt =>
        {
            if (evt.newRect.width == 0 || evt.newRect.height == 0)
            {
                return;
            }

            element.transform.position = calculatePosition(new Vector2(evt.newRect.width, evt.newRect.height));
            element.UnregisterCallback(_geometryChanged);
        };

        element.RegisterCallback(_geometryChanged);
    }

    /// <summary>
    /// Set the default position of an element to the center of the screen.
    /// </summary>
    /// <param name="element">The element to center.</param>
    public static void CenterByDefault(this VisualElement element)
    {
        element.SetDefaultPosition(windowSize => new Vector2(
            (ReferenceResolution.Width - windowSize.x) / 2,
            (ReferenceResolution.Height - windowSize.y) / 2
        ));
    }

    /// <summary>
    /// Disable dragging on a specific element.
    /// </summary>
    /// <param name="element">The element to disable dragging on.</param>
    public static void DisableDragging(this VisualElement element)
    {
        element.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
        element.RegisterCallback<PointerUpEvent>(evt => evt.StopPropagation());
        element.RegisterCallback<PointerMoveEvent>(evt => evt.StopPropagation());
    }

    /// <summary>
    /// Disable game input when an element is focused.
    /// </summary>
    /// <param name="element"></param>
    public static void DisableGameInputOnFocus(this VisualElement element)
    {
        element.RegisterCallback<FocusInEvent>(_ => GameManager.Instance?.Game?.Input.Disable());
        element.RegisterCallback<FocusOutEvent>(_ => GameManager.Instance?.Game?.Input.Enable());
    }
}