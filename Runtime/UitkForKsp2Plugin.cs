global using JetBrains.Annotations;
global using UnityObject = UnityEngine.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using UitkForKsp2.API;
using UitkForKsp2.MVVM.Converters;
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

    private static readonly List<RuntimePanelSettingsRegistration> RuntimePanelSettings = new();
    private static float UiScale = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void AttachToReduxLib()
    {
        // With Domain Reload disabled this runs on every Play Mode enter, so reset mutable static
        // state and re-subscribe idempotently to the static event (UDR0005).
        PanelSettings = null;
        FixedPanelSettings = null;
        Logger = null;
        UiScale = 1f;
        RuntimePanelSettings.Clear();

        ReduxLib.ReduxLib.OnReduxLibInitialized -= PreInitializeUitkForKsp2;
        ReduxLib.ReduxLib.OnReduxLibInitialized += PreInitializeUitkForKsp2;
    }

    private static void PreInitializeUitkForKsp2()
    {
        Logger = ReduxLib.ReduxLib.GetLogger("UITK For KSP2");
        Logger.LogInfo("Pre-initialized");
    }

    public static void InitializeUitkForKsp2()
    {
        ConverterGroupRegistry.Register(
            new LocalizationGroup(),
            new UppercaseLocalizationGroup()
        );
        LoadPanelSettings();

        ConfigurePanelSettings(PanelSettings, true);
        ConfigurePanelSettings(FixedPanelSettings, false);

        Logger.LogInfo("Initialized!");
    }

    public static void RescalePercent(float percent)
    {
        UiScale = Mathf.Max(0.01f, percent / 100f);
        ConfigureAllPanelSettings();
    }

    public static void RegisterRuntimePanelSettings(PanelSettings panelSettings, bool useUiScale = false)
    {
        RuntimePanelSettings.Add(new RuntimePanelSettingsRegistration(panelSettings, useUiScale));
    }

    public static void ConfigurePanelSettings(PanelSettings ps)
    {
        ConfigurePanelSettings(ps, ps == PanelSettings);
    }

    public static void ConfigurePanelSettings(PanelSettings ps, bool useUiScale)
    {
        try
        {
            if (ps == null)
            {
                return;
            }

            Vector2Int backingResolution = GetBackingResolution();
            Vector2Int gameResolution = GetGameResolution(backingResolution);

            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.scale = useUiScale ? UiScale : 1f;
            ps.referenceResolution = GetPanelReferenceResolution(backingResolution, gameResolution);
            ps.match = GetMatchAxis(gameResolution);

            ApplyPanelSettings.Invoke(ps, Array.Empty<object>());
        }
        catch (Exception e)
        {
            Logger?.LogError($"Failed to configure panel settings: {e}");
        }
    }

    private static void ConfigureAllPanelSettings()
    {
        ConfigurePanelSettings(PanelSettings, true);
        ConfigurePanelSettings(FixedPanelSettings, false);

        for (int i = RuntimePanelSettings.Count - 1; i >= 0; i--)
        {
            RuntimePanelSettingsRegistration registration = RuntimePanelSettings[i];
            if (registration.TryGetTarget(out PanelSettings panelSettings) && panelSettings != null)
            {
                ConfigurePanelSettings(panelSettings, registration.UseUiScale);
            }
            else
            {
                RuntimePanelSettings.RemoveAt(i);
            }
        }
    }

    private static Vector2Int GetBackingResolution()
    {
        return new Vector2Int(
            Mathf.Max(1, Screen.width),
            Mathf.Max(1, Screen.height)
        );
    }

    private static Vector2Int GetGameResolution(Vector2Int backingResolution)
    {
        Display display = Display.main;
        if (display != null &&
            display.renderingWidth > 0 &&
            display.renderingHeight > 0 &&
            display.renderingWidth <= backingResolution.x &&
            display.renderingHeight <= backingResolution.y)
        {
            return new Vector2Int(display.renderingWidth, display.renderingHeight);
        }

        return backingResolution;
    }

    private static Vector2Int GetPanelReferenceResolution(Vector2Int backingResolution, Vector2Int gameResolution)
    {
        float gameScaleX = gameResolution.x / (float)ReferenceResolution.Width;
        float gameScaleY = gameResolution.y / (float)ReferenceResolution.Height;

        return new Vector2Int(
            Mathf.Max(1, Mathf.RoundToInt(backingResolution.x / gameScaleX)),
            Mathf.Max(1, Mathf.RoundToInt(backingResolution.y / gameScaleY))
        );
    }

    private static float GetMatchAxis(Vector2Int gameResolution)
    {
        float gameAspectRatio = gameResolution.x / (float)gameResolution.y;
        float referenceAspectRatio = ReferenceResolution.Width / (float)ReferenceResolution.Height;

        return gameAspectRatio >= referenceAspectRatio ? 1f : 0f;
    }

    private readonly struct RuntimePanelSettingsRegistration
    {
        private readonly WeakReference<PanelSettings> _panelSettings;

        public RuntimePanelSettingsRegistration(PanelSettings panelSettings, bool useUiScale)
        {
            _panelSettings = new WeakReference<PanelSettings>(panelSettings);
            UseUiScale = useUiScale;
        }

        public bool UseUiScale { get; }

        public bool TryGetTarget(out PanelSettings panelSettings)
        {
            return _panelSettings.TryGetTarget(out panelSettings);
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
        private Vector2Int _lastGameResolution;
        private FullScreenMode _lastFullScreenMode;

        private void Awake()
        {
            _last = GetBackingResolution();
            _lastGameResolution = GetGameResolution(_last);
            _lastFullScreenMode = Screen.fullScreenMode;
        }

        private void Update()
        {
            Vector2Int now = GetBackingResolution();
            Vector2Int gameResolution = GetGameResolution(now);
            if (now == _last && gameResolution == _lastGameResolution && Screen.fullScreenMode == _lastFullScreenMode)
            {
                return;
            }

            _last = now;
            _lastGameResolution = gameResolution;
            _lastFullScreenMode = Screen.fullScreenMode;
            ConfigureAllPanelSettings();
        }
    }
}
