using HarmonyLib;
using KSP.Logging;

namespace UitkForKsp2.Patches;

[HarmonyPatch]
internal static class GlobalLogPatch
{
    [HarmonyPatch(typeof(GlobalLog), nameof(GlobalLog.Warn), typeof(object))]
    [HarmonyPrefix]
    private static bool GlobalLog_Warn(object message)
    {
        return !Configuration.DisableBindingWarnings ||
               message is not string strMessage ||
               !strMessage.Contains("has not defined a binding for id");
    }
}