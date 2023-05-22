using HarmonyLib;
using UnityEngine;

namespace UitkForKsp2.Patch;

#pragma warning disable CS1591

[HarmonyPatch]
public static class ShaderPatch
{
    [HarmonyPatch(typeof(Shader), nameof(Shader.Find))]
    [HarmonyPrefix]
    public static bool ShaderFind(string name, ref Shader __result)
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