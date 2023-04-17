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
}