global using JetBrains.Annotations;
global using UnityObject = UnityEngine.Object;
using System;
using System.Reflection;
using UitkForKsp2.API;
using UitkForKsp2.API.Localization;
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

    public static PanelSettings FixedPanelSettings { get; private set; }

    internal static ILogger Logger;

    private const string PanelSettingsLabel = "kerbalui";

    private const string FixedPanelSettingsLabel = "Packages/uitkforksp2.controls/Assets/UI Toolkit/FixedKerbalPanelSettings.asset";

    private static readonly MethodInfo ApplyPanelSettings = typeof(PanelSettings).GetMethod(
        "ApplyPanelSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
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
        LocalizationConverter.Register();
        LoadPanelSettings();

        PanelSettings.referenceResolution = new Vector2Int(
            ReferenceResolution.Width,
            ReferenceResolution.Height
        );
        ApplyPanelSettings.Invoke(PanelSettings, new object[] { });
        AutoAdjustMatch(PanelSettings);

        FixedPanelSettings.referenceResolution = new Vector2Int(
            ReferenceResolution.Width,
            ReferenceResolution.Height
        );
        ApplyPanelSettings.Invoke(FixedPanelSettings, new object[] { });
        AutoAdjustMatch(FixedPanelSettings);

        Logger.LogInfo("Initialized!");
    }

    public static void RescalePercent(float percent)
    {
        PanelSettings.scale = percent / 100f;
        ApplyPanelSettings.Invoke(PanelSettings, new object[] { });
    }

    private static void AutoAdjustMatch(PanelSettings ps)
    {
        try
        {
            if (ps == null || ps.referenceResolution.x <= 0 || ps.referenceResolution.y <= 0)
            {
                return;
            }

            float screenW = Screen.width;
            float screenH = Screen.height;
            float refW = ps.referenceResolution.x;
            float refH = ps.referenceResolution.y;

            // Current aspect vs. reference
            float screenAR = screenW / screenH;
            float refAR = refW / refH;

            // Pick whichever axis is more constraining
            ps.match = screenAR >= refAR ? 1f : 0f;

            MethodInfo? apply = typeof(PanelSettings).GetMethod(
                "ApplyPanelSettings",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            apply?.Invoke(ps, Array.Empty<object>());
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to auto-adjust match: {e}");
        }
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

            var fixedPanelSettingsLabel = Addressables.LoadAssetAsync<PanelSettings>(FixedPanelSettingsLabel);
            fixedPanelSettingsLabel.WaitForCompletion();
            if (fixedPanelSettingsLabel.Status == AsyncOperationStatus.Failed)
            {
                Logger.LogError($"Failed to load FixedPanelSettings asset from label '{FixedPanelSettingsLabel}'");
                return;
            }

            FixedPanelSettings = fixedPanelSettingsLabel.Result;

            Logger.LogInfo($"PanelSettings loaded: {PanelSettings}");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to load addressables: {e}");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void WatchForResolutionChange()
    {
        var go = new GameObject("UITK_AspectRatio_Watcher");
        UnityObject.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<ResolutionWatcher>();
    }

    private class ResolutionWatcher : MonoBehaviour
    {
        private Vector2Int _last;
        private void Awake() => _last = new Vector2Int(Screen.width, Screen.height);
        private void Update()
        {
            var now = new Vector2Int(Screen.width, Screen.height);
            if (now != _last)
            {
                _last = now;
                AutoAdjustMatch(PanelSettings);
                AutoAdjustMatch(FixedPanelSettings);
            }
        }
    }
}
