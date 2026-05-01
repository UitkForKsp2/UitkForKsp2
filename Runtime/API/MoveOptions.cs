using System;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Options for allowing an element to be dragged.
/// </summary>
public struct MoveOptions
{
    /// <summary>
    /// Should the element be moved by dragging?
    /// </summary>
    public bool IsMovingEnabled { get; set; }

    /// <summary>
    /// Should the element be moved only within the screen bounds?
    /// </summary>
    public bool CheckScreenBounds { get; set; }

    /// <summary>
    /// Optional child element name to use as the drag handle. The window root is used when null.
    /// </summary>
    public string? HandleElementName { get; set; }

    /// <summary>
    /// Optional callback that resolves the drag handle from the window root. Takes precedence over <see cref="HandleElementName"/>.
    /// </summary>
    public Func<VisualElement, VisualElement?>? HandleSelector { get; set; }

    /// <summary>
    /// Default options for dragging an element.
    /// </summary>
    public static MoveOptions Default => new()
    {
        IsMovingEnabled = true,
        CheckScreenBounds = true,
        HandleElementName = null,
        HandleSelector = null
    };

    internal readonly VisualElement ResolveHandle(VisualElement root)
    {
        return HandleSelector?.Invoke(root)
            ?? (!string.IsNullOrWhiteSpace(HandleElementName) ? root.Q(HandleElementName) : null)
            ?? root;
    }
}
