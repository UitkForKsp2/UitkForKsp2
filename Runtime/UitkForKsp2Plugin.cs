global using JetBrains.Annotations;
global using UnityObject = UnityEngine.Object;
using System;
using System.Reflection;
// using BepInEx;
// using BepInEx.Logging;
// using HarmonyLib;

using UitkForKsp2.API;
using UitkForKsp2.Controls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using ILogger = ReduxLib.Logging.ILogger;

namespace UitkForKsp2;

/// <summary>
/// UITK for KSP 2 main plugin class.
/// </summary>
public static class UitkForKsp2Plugin /* : BaseUnityPlugin */
{

    /// <summary>
    /// The default UITK for KSP 2 panel settings with the KerbalUI theme. Do not modify this, as all mods using UITK
    /// will be affected. It is also strongly discouraged to create your own instance of the PanelSettings class,
    /// as having multiple panels in use will negatively impact performance.
    /// </summary>
    public static PanelSettings PanelSettings { get; private set; }

    internal static ILogger Logger;

    private const string PanelSettingsLabel = "kerbalui";
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void AttachToReduxLib()
    {
        ReduxLib.ReduxLib.OnReduxLibInitialized += PreInitializeUitkForKsp2;
    }

    private static void PreInitializeUitkForKsp2()
    {
        Logger = ReduxLib.ReduxLib.GetLogger("UITK For KSP2");
        Logger.LogInfo("Pre-initialized");
    }
    
    public static void InitializeUitkForKsp2()
    {
        LoadPanelSettings();
        PanelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        PanelSettings.referenceResolution = new Vector2Int(
            ReferenceResolution.Width,
            ReferenceResolution.Height
        );
        PanelSettings.scale = 1;
        _applyPanelSettings.Invoke(PanelSettings, new object[] { });
        /*
            Redo configuration once the game exists
            Configuration.Initialize();
        */
        // Harmony.CreateAndPatchAll(typeof(UitkForKsp2Plugin).Assembly);

        // Register custom controls
        // var controlsAssembly = typeof(BaseControl).Assembly;
        // CustomControls.RegisterFromAssembly(controlsAssembly);

        Logger.LogInfo("Initialized!");
    }

    private static MethodInfo _applyPanelSettings = typeof(PanelSettings).GetMethod("ApplyPanelSettings", BindingFlags.Instance | BindingFlags.NonPublic)!;
    public static void RescalePercent(float percent)
    {
        PanelSettings.scale = percent / 100f;
        _applyPanelSettings.Invoke(PanelSettings, new object[] { });
    }
    private static void LoadPanelSettings()
    {
        try
        {

            var panelSettingsHandle = Addressables.LoadAssetAsync<PanelSettings>(PanelSettingsLabel);
            panelSettingsHandle.WaitForCompletion();
            if (panelSettingsHandle.Status == AsyncOperationStatus.Failed)
            {
                Logger.LogError($"Failed to load PanelSettings asset from label '{PanelSettingsLabel}'");
                return;
            }

            PanelSettings = panelSettingsHandle.Result;
            Logger.LogInfo($"PanelSettings loaded: {PanelSettings}");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to load addressables: {e}");
        }
    }
}