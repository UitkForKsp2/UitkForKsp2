global using JetBrains.Annotations;
global using UnityObject = UnityEngine.Object;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UitkForKsp2.API;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace UitkForKsp2;

/// <summary>
/// UITK for KSP 2 main plugin class.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class UitkForKsp2Plugin : BaseUnityPlugin
{
    /// <summary>
    /// Plugin GUID.
    /// </summary>
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;

    /// <summary>
    /// Plugin name.
    /// </summary>
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;

    /// <summary>
    /// Plugin version.
    /// </summary>
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// <summary>
    /// The default UITK for KSP 2 panel settings with the KerbalUI theme. Do not modify this, as all mods using UITK
    /// will be affected. It is also strongly discouraged to create your own instance of the PanelSettings class,
    /// as having multiple panels in use will negatively impact performance.
    /// </summary>
    public static PanelSettings PanelSettings { get; private set; }

    internal new static ManualLogSource Logger;

    private static readonly string CatalogPath = Path.Combine(
        Paths.PluginPath, ModGuid, "addressables", "catalog.json"
    );
    private const string PanelSettingsLabel = "kerbalui";

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
        Configuration.Initialize(Config);

        Harmony.CreateAndPatchAll(typeof(UitkForKsp2Plugin).Assembly);

        // Register custom controls
        var controlsAssembly = Assembly.LoadFile(
            Path.Combine(Paths.PluginPath, ModGuid, "UitkForKsp2.Unity.dll")
        );
        CustomControls.RegisterFromAssembly(controlsAssembly);

        Logger.LogInfo($"Plugin {ModName} loaded");
    }

    private static void LoadPanelSettings()
    {
        try
        {
            var catalogHandle = Addressables.LoadContentCatalogAsync(CatalogPath);
            catalogHandle.WaitForCompletion();
            if (catalogHandle.Status == AsyncOperationStatus.Failed)
            {
                Logger.LogError($"Failed to load addressables catalog {CatalogPath}");
                return;
            }

            Logger.LogInfo($"Loaded addressables catalog {CatalogPath}");
            Logger.LogInfo($"{CatalogPath} ----- {catalogHandle.Result.LocatorId}");
            Addressables.AddResourceLocator(catalogHandle.Result);

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