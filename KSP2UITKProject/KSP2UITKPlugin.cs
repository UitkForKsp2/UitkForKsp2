using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace KSP2UITK;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class KSP2UITKPlugin : BaseUnityPlugin
{
    public const string ModFolder = "ksp2_uitk";
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    internal static readonly Dictionary<string, Shader> Shaders = new();
    internal new static ManualLogSource Logger;

    private static readonly string BundlePath = Path.Combine(
        Paths.PluginPath, ModFolder, "assets/bundles/shaders"
    );

    protected KSP2UITKPlugin()
    {
        Logger = base.Logger;
    }

    private void Awake()
    {
        LoadShaders();
        
        Harmony.CreateAndPatchAll(typeof(UIElementsPatches));
        
        Logger.LogInfo($"Plugin {ModName} loaded");
    }

    private void LoadShaders()
    {
        var bundle = AssetBundle.LoadFromFile(BundlePath);
        if (!bundle)
        {
            Logger.LogError("Failed to load shaders bundle!");
            return;
        }

        Logger.LogInfo($"Loaded shaders bundle successfully from \"{BundlePath}\".");

        var shaders = bundle.LoadAllAssets<Shader>();
        foreach (var shader in shaders)
        {
            Shaders[shader.name] = shader;
            Logger.LogInfo($"Shader loaded: {shader.name}");
        }
    }
}