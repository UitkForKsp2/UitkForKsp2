using UitkForKsp2.API;

namespace UitkForKsp2;

/// <summary>
/// Configuration class for the UITK for KSP 2 plugin.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// The current UITK screen width, taking into account whether automatic UI scaling is enabled.
    /// </summary>
    [PublicAPI]
    public static int CurrentScreenWidth => ReferenceResolution.Width;

    /// <summary>
    /// The current UITK screen height, taking into account whether automatic UI scaling is enabled.
    /// </summary>
    [PublicAPI]
    public static int CurrentScreenHeight => ReferenceResolution.Height;

    [PublicAPI] public static float CurrentScale => UitkForKsp2Plugin.PanelSettings.scale;

    [PublicAPI]
    public static float ScaledScreenWidth => CurrentScale * ReferenceResolution.Width;

    [PublicAPI]
    public static float ScaledScreenHeight => CurrentScale * ReferenceResolution.Height;
}