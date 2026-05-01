namespace UitkForKsp2.API;

/// <summary>
/// Options for allowing an element to be resized.
/// </summary>
public struct ResizeOptions
{
    /// <summary>
    /// Should the element be resized by dragging its resize handle?
    /// </summary>
    public bool IsResizingEnabled { get; set; }

    /// <summary>
    /// Should the element stay within the screen bounds while resizing?
    /// </summary>
    public bool CheckScreenBounds { get; set; }

    /// <summary>
    /// Minimum width in panel pixels. Uses the element style when zero or lower.
    /// </summary>
    public float MinWidth { get; set; }

    /// <summary>
    /// Minimum height in panel pixels. Uses the element style when zero or lower.
    /// </summary>
    public float MinHeight { get; set; }

    /// <summary>
    /// Default options for resizing an element.
    /// </summary>
    public static ResizeOptions Default => new()
    {
        IsResizingEnabled = false,
        CheckScreenBounds = true,
        MinWidth = 0,
        MinHeight = 0
    };
}
