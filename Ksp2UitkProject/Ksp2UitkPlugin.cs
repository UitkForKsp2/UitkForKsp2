﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Ksp2Uitk.Patch;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ksp2Uitk;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Ksp2UitkPlugin : BaseUnityPlugin
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public const string ModFolder = "ksp2_uitk";
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;

    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global

    internal new static ManualLogSource Logger;

    public static PanelSettings PanelSettings;
    internal static readonly Dictionary<string, Shader> Shaders = new();

    private static readonly string PanelSettingsPath = Path.Combine(
        Paths.PluginPath, ModFolder, "assets/bundles/kerbalui"
    );

    public Ksp2UitkPlugin()
    {
        Logger = base.Logger;
    }

    private void Awake()
    {
        LoadPanelSettings();

        Harmony.CreateAndPatchAll(typeof(ShaderPatch));

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