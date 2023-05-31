using HarmonyLib;
using UnityEngine;

namespace UitkForKsp2.Patches;

[HarmonyPatch]
internal static class ShaderPatch
{
    [HarmonyPatch(typeof(Shader), nameof(Shader.Find))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool ShaderFind(string name, ref Shader __result)
    {
        if (!UitkForKsp2Plugin.Shaders.TryGetValue(name, out var injected))
        {
            return true;
        }

        UitkForKsp2Plugin.Logger.LogInfo($"Injected shader {name}");
        __result = injected;
        return false;
    }
}