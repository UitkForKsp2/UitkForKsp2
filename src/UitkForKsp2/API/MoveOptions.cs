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
    /// Default options for dragging an element.
    /// </summary>
    public static MoveOptions Default => new()
    {
        IsMovingEnabled = true,
        CheckScreenBounds = true
    };
}