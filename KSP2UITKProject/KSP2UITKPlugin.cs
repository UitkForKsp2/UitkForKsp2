using BepInEx;

namespace KSP2UITK;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class KSP2UITKPlugin : BaseUnityPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    public const string ModFolder = "ksp2_uitk";
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    private void Awake()
    {
        Logger.LogInfo($"Plugin ${ModName} loaded");
    }
}
