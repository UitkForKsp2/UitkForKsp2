using HarmonyLib;
using UnityEngine;

namespace KSP2UITK;

public static class ShaderPatch
{
    [HarmonyPatch(typeof(Shader),nameof(Shader.Find))]
    [HarmonyPrefix]
    public static bool ShaderFind(string name, ref Shader __result) {
        if (!KSP2UITKPlugin.Shaders.TryGetValue(name, out var injected))
        {
            return true;
        }
        __result = injected;
        return false;
    }
}