using UnityEngine.UIElements;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace UitkForKsp2.API;

/// <summary>
/// Contains methods for creating UI elements.
/// </summary>
public static class Element
{
    /// <summary>
    /// Creates a new UI element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New UI element.</returns>
    public static T Create<T>(string name = null, string classes = null, IEnumerable<VisualElement> children = null)
        where T : VisualElement, new()
    {
        var element = new T();

        if (name != null)
        {
            element.name = name;
        }

        if (classes != null)
        {
            foreach (var className in classes.Split(' '))
            {
                element.AddToClassList(className);
            }
        }

        element.AddChildren(children);

        return element;
    }

    /// <summary>
    /// Creates a new TextElement-based UI element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="text">Text inside the element.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New TextElement-based UI element.</returns>
    public static T CreateText<T>(
        string name = null,
        string text = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    ) where T : TextElement, new()
    {
        var element = Create<T>(name, classes, children);
        element.text = text;

        return element;
    }

    /// <summary>
    /// Creates a new VisualElement.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New VisualElement.</returns>
    public static VisualElement VisualElement(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        return Create<VisualElement>(name, classes, children);
    }

    /// <summary>
    /// Creates a new ScrollView element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New ScrollView element.</returns>
    public static ScrollView ScrollView(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<ScrollView>(name, classes);
        element.contentContainer.AddChildren(children);

        return element;
    }

    /// <summary>
    /// Creates a new ListView element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New ListView element.</returns>
    public static ListView ListView(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<ListView>(name, classes);
        element.contentContainer.AddChildren(children);

        return element;
    }

    /// <summary>
    /// Creates a new IMGUIContainer element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New IMGUIContainer element.</returns>
    public static IMGUIContainer IMGUIContainer(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        return Create<IMGUIContainer>(name, classes, children);
    }

    /// <summary>
    /// Creates a new Label element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="text">Label text.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Label element.</returns>
    public static Label Label(
        string name = null,
        string text = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        return CreateText<Label>(name, text, classes, children);
    }

    /// <summary>
    /// Creates a new Button element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="text">Button label text.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Button element.</returns>
    public static Button Button(
        string name = null,
        string text = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        return CreateText<Button>(name, text, classes, children);
    }

    /// <summary>
    /// Creates a new Toggle element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="label">Toggle label text.</param>
    /// <param name="isActive">Toggle status.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Toggle element.</returns>
    public static Toggle Toggle(
        string name = null,
        string label = null,
        bool isActive = false,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<Toggle>(name, classes, children);
        element.AddToClassList("toggle");
        element.value = isActive;
        if (label != null)
        {
            element.text = label;
        }

        return element;
    }

    /// <summary>
    /// Creates a new Scroller element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Scroller element.</returns>
    public static Scroller Scroller(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        return Create<Scroller>(name, classes, children);
    }

    /// <summary>
    /// Creates a new TextField element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="value">TextField value.</param>
    /// <param name="multiline">Is TextField multiline?</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New TextField element.</returns>
    public static TextField TextField(
        string name = null,
        string value = null,
        bool multiline = false,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<TextField>(name, classes, children);
        element.multiline = multiline;
        if (value != null)
        {
            element.value = value;
        }

        return element;
    }

    /// <summary>
    /// Creates a new Foldout element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="label">Foldout label text.</param>
    /// <param name="isOpen">Foldout status.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Foldout element.</returns>
    public static Foldout Foldout(
        string name = null,
        string label = null,
        bool isOpen = false,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<Foldout>(name, classes);
        element.contentContainer.AddChildren(children);
        element.value = isOpen;
        if (label != null)
        {
            element.text = label;
        }

        return element;
    }

    /// <summary>
    /// Creates a new Slider element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="min">Slider minimum value with 0 being default.</param>
    /// <param name="max">Slider maximum value with 10 being default.</param>
    /// <param name="value">Slider default value.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New Slider element.</returns>
    public static Slider Slider(
        string name = null,
        float? min = null,
        float? max = null,
        float? value = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<Slider>(name, classes, children);
        if (min != null)
        {
            element.lowValue = min.Value;
        }

        if (max != null)
        {
            element.highValue = max.Value;
        }

        if (value != null)
        {
            element.value = value.Value;
        }

        return element;
    }

    /// <summary>
    /// Creates a new SliderInt element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="min">SliderInt minimum value with 0 being default.</param>
    /// <param name="max">SliderInt maximum value with 10 being default.</param>
    /// <param name="value">SliderInt default value.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New SliderInt element.</returns>
    public static SliderInt SliderInt(
        string name = null,
        int? min = null,
        int? max = null,
        int? value = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<SliderInt>(name, classes, children);
        if (min != null)
        {
            element.lowValue = min.Value;
        }

        if (max != null)
        {
            element.highValue = max.Value;
        }

        if (value != null)
        {
            element.value = value.Value;
        }

        return element;
    }

    /// <summary>
    /// Creates a new MinMaxSlider element.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="minLimit">MinMaxSlider minimum limit.</param>
    /// <param name="maxLimit">MinMaxSlider maximum limit.</param>
    /// <param name="minValue">MinMaxSlider default low value.</param>
    /// <param name="maxValue">MinMaxSlider default high value.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New MinMaxSlider element.</returns>
    public static MinMaxSlider MinMaxSlider(
        string name = null,
        int? minLimit = null,
        int? maxLimit = null,
        int? minValue = null,
        int? maxValue = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = Create<MinMaxSlider>(name, classes, children);
        if (minLimit != null)
        {
            element.lowLimit = minLimit.Value;
        }

        if (maxLimit != null)
        {
            element.highLimit = maxLimit.Value;
        }

        if (minValue != null)
        {
            element.minValue = minValue.Value;
        }

        if (maxValue != null)
        {
            element.maxValue = maxValue.Value;
        }

        return element;
    }

    /// <summary>
    /// Creates a new root VisualElement with default styling.
    /// </summary>
    /// <param name="name">Element name.</param>
    /// <param name="classes">Element classes separated by spaces.</param>
    /// <param name="children">Children elements.</param>
    /// <returns>New root VisualElement with default styling.</returns>
    public static VisualElement Root(
        string name = null,
        string classes = null,
        IEnumerable<VisualElement> children = null
    )
    {
        var element = VisualElement(name, classes, children);
        element.AddToClassList("root");
        return element;
    }
}