global using JetBrains.Annotations;
global using UnityObject = UnityEngine.Object;
using System;
using System.Reflection;
using UitkForKsp2.API;
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

    private static readonly MethodInfo ApplyPanelSettings = typeof(PanelSettings).GetMethod(
        "ApplyPanelSettings",
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;


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
        PanelSettings.sortingOrder = 0;
        PanelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        PanelSettings.referenceResolution = new Vector2Int(
            ReferenceResolution.Width,
            ReferenceResolution.Height
        );
        PanelSettings.scale = 1;
        ApplyPanelSettings.Invoke(PanelSettings, new object[] { });

        Logger.LogInfo("Initialized!");
    }

    public static void RescalePercent(float percent)
    {
        PanelSettings.scale = percent / 100f;
        ApplyPanelSettings.Invoke(PanelSettings, new object[] { });
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