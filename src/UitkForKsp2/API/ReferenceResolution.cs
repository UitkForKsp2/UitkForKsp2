using UnityEngine;

namespace UitkForKsp2.API;

/// <summary>
/// Reference resolution for UI elements. All UI elements should be designed for this resolution, they will
/// automatically scale to the user's real resolution.
/// </summary>
[PublicAPI]
public class ReferenceResolution
{
    /// <summary>
    /// The width of the reference resolution.
    /// </summary>
    public static readonly int Width = 1920;

    /// <summary>
    /// The height of the reference resolution.
    /// </summary>
    public static readonly int Height = 1080;

    /// <summary>
    /// Converts percentage of screen width and height to reference resolution pixels.
    /// </summary>
    /// <param name="screenPercent">Vector with width and height in percentage of screen size.</param>
    /// <returns>Vector with corresponding width and height in the reference resolution.</returns>
    public static Vector2 ConvertFromScreenPercentage(Vector2 screenPercent)
    {
        return new Vector2(
            screenPercent.x * Width,
            screenPercent.y * Height
        );
    }

    /// <summary>
    /// Converts absolute pixels to reference resolution pixels.
    /// </summary>
    /// <param name="absolutePixels">Vector with screen size based width and height in pixels.</param>
    /// <returns>Vector with corresponding width and height in the reference resolution.</returns>
    public static Vector2 ConvertFromScreenPixels(Vector2 absolutePixels)
    {
        return new Vector2(
            absolutePixels.x * Width / Screen.width,
            absolutePixels.y * Height / Screen.height
        );
    }

    /// <summary>
    /// Gets mouse position converted to reference resolution pixels.
    /// </summary>
    /// <returns>Mouse position with corresponding x and flipped y in the reference resolution.</returns>
    public static Vector2 GetReferenceMousePosition()
    {
        var mousePosition = Input.mousePosition;
        return new Vector2(
            mousePosition.x * Width / Screen.width,
            Height - mousePosition.y * Height / Screen.height
        );
    }
}