using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UitkForKsp2.Patch;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace UitkForKsp2;

/// <summary>
/// UITK for KSP 2 main plugin class.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class UitkForKsp2Plugin : BaseUnityPlugin
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    /// <summary>
    /// Plugin GUID.
    /// </summary>
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    /// <summary>
    /// Plugin name.
    /// </summary>
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    /// <summary>
    /// Plugin version.
    /// </summary>
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global

    internal new static ManualLogSource Logger;

    /// <summary>
    /// The default UITK for KSP 2 panel settings with the KerbalUI theme.
    /// </summary>
    public static PanelSettings PanelSettings;
    internal static readonly Dictionary<string, Shader> Shaders = new();

    private static readonly string PanelSettingsPath = Path.Combine(
        Paths.PluginPath, ModGuid, "assets/bundles/kerbalui"
    );

    /// <summary>
    /// Creates a new instance of the <see cref="UitkForKsp2Plugin"/> class.
    /// </summary>
    public UitkForKsp2Plugin()
    {
        Logger = base.Logger;
    }

    private void Awake()
    {
        LoadPanelSettings();

        Harmony.CreateAndPatchAll(typeof(UitkForKsp2Plugin).Assembly);

        Logger.LogInfo($"Plugin {ModName} loaded");
    }

    private static void LoadPanelSettings()
    {
        var bundle = AssetBundle.LoadFromFile(PanelSettingsPath);
        if (!bundle)
        {
            Logger.LogError("Failed to load PanelSettings bundle!");
            return;
        }

        Logger.LogInfo($"Loaded PanelSettings bundle successfully from \"{PanelSettingsPath}\".");

        PanelSettings = bundle.LoadAllAssets<PanelSettings>()[0];
        Logger.LogInfo($"PanelSettings loaded: {PanelSettings}");

        Shaders["Hidden/UIE-Runtime"] = PanelSettings.m_RuntimeShader;
        Logger.LogInfo($"Shader loaded: {PanelSettings.m_RuntimeShader}");
        Shaders["Hidden/UIE-RuntimeWorld"] = PanelSettings.m_RuntimeWorldShader;
        Logger.LogInfo($"Shader loaded: {PanelSettings.m_RuntimeWorldShader}");
        Shaders["Hidden/UIE-AtlasBlit"] = PanelSettings.m_AtlasBlitShader;
        Logger.LogInfo($"Shader loaded: {PanelSettings.m_AtlasBlitShader}");
    }
}