using BepInEx.Configuration;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2;

/// <summary>
/// Configuration class for the UITK for KSP 2 plugin.
/// </summary>
public static class Configuration
{
    private static ConfigFile _configFile;

    private const string SectionUiScaling = "UI Scaling";

    private const string KeyAutomaticScaling = "Automatic UI Scaling (restart game after changing)";
    private const string KeyManualUiScale = "Manual UI Scale";
    private const string KeyDisableBindingWarnings = "Disable input binding warnings";

    private const float ManualScalingDpi = 96f;

    private static ConfigEntry<bool> _automaticScaling;
    private static ConfigEntry<float> _manualUiScale;
    private static ConfigEntry<bool> _disableBindingWarnings;

    /// <summary>
    /// Whether automatic UI scaling is enabled.
    /// </summary>
    [PublicAPI]
    public static bool IsAutomaticScalingEnabled => _automaticScaling.Value;

    /// <summary>
    /// If automatic UI scaling is disabled, this UI scale is used.
    /// </summary>
    [PublicAPI]
    public static float ManualUiScale => _manualUiScale.Value;

    /// <summary>
    /// The current UITK screen width, taking into account whether automatic UI scaling is enabled.
    /// </summary>
    [PublicAPI]
    public static int CurrentScreenWidth => IsAutomaticScalingEnabled ? ReferenceResolution.Width : (int)Math.Round(Screen.width / ManualUiScale);

    /// <summary>
    /// The current UITK screen height, taking into account whether automatic UI scaling is enabled.
    /// </summary>
    [PublicAPI]
    public static int CurrentScreenHeight => IsAutomaticScalingEnabled ? ReferenceResolution.Height : (int)Math.Round(Screen.height / ManualUiScale);

    /// <summary>
    /// The current mouse position, adjusted to UITK coordinates, taking into account whether automatic UI scaling
    /// is enabled.
    /// </summary>
    /// <returns>The adjusted current mouse position.</returns>
    [PublicAPI]
    public static Vector2 GetAdjustedMousePosition()
    {
        if (IsAutomaticScalingEnabled)
        {
            return ReferenceResolution.GetReferenceMousePosition();
        }

        var mousePosition = Input.mousePosition;
        return new Vector2(
            mousePosition.x / ManualUiScale,
            CurrentScreenHeight - mousePosition.y / ManualUiScale
        );
    }

    /// <summary>
    /// Whether the game's input binding warnings are disabled.
    /// </summary>
    internal static bool DisableBindingWarnings => _disableBindingWarnings.Value;

    internal static void Initialize(ConfigFile configFile)
    {
        _configFile = configFile;

        _automaticScaling = _configFile.Bind(
            SectionUiScaling,
            KeyAutomaticScaling,
            true,
            new ConfigDescription("Automatically scale UI elements based on screen resolution?")
        );
        _automaticScaling.SettingChanged += OnAutomaticScalingChanged;

        _manualUiScale = _configFile.Bind(
            SectionUiScaling,
            KeyManualUiScale,
            1.0f,
            new ConfigDescription(
                "The UI scale when automatic scaling is disabled.",
                new AcceptableValueRange<float>(0.25f, 4f)
            )
        );
        _manualUiScale.SettingChanged += OnManualUiScaleChanged;

        _disableBindingWarnings = _configFile.Bind(
            SectionUiScaling,
            KeyDisableBindingWarnings,
            true,
            "Disable input binding warnings after interacting with UITK text fields?"
        );

        UpdateScaling();
    }

    private static void OnAutomaticScalingChanged(object sender, EventArgs e)
    {
        UpdateScaling();
    }

    private static void OnManualUiScaleChanged(object sender, EventArgs e)
    {
        if (_automaticScaling.Value)
        {
            return;
        }
        UitkForKsp2Plugin.Logger.LogDebug($"Manual UI scale changed to {_manualUiScale.Value}");
        UitkForKsp2Plugin.PanelSettings.scale = _manualUiScale.Value;
    }

    private static void UpdateScaling()
    {
        if (_automaticScaling.Value)
        {
            UitkForKsp2Plugin.Logger.LogDebug("Automatic UI scaling enabled");
            UitkForKsp2Plugin.PanelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            UitkForKsp2Plugin.PanelSettings.referenceResolution = new Vector2Int(
                ReferenceResolution.Width,
                ReferenceResolution.Height
            );
            UitkForKsp2Plugin.PanelSettings.scale = 1;
        }
        else
        {
            UitkForKsp2Plugin.Logger.LogDebug("Automatic UI scaling disabled");
            UitkForKsp2Plugin.PanelSettings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            UitkForKsp2Plugin.PanelSettings.referenceDpi = ManualScalingDpi;
            UitkForKsp2Plugin.PanelSettings.fallbackDpi = ManualScalingDpi;
            UitkForKsp2Plugin.PanelSettings.scale = _manualUiScale.Value;
        }

        UitkForKsp2Plugin.PanelSettings.ApplyPanelSettings();
    }
}