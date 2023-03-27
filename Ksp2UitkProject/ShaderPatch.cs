using HarmonyLib;
using UnityEngine;

namespace Ksp2Uitk;

public static class ShaderPatch
{
    [HarmonyPatch(typeof(Shader),nameof(Shader.Find))]
    [HarmonyPrefix]
    public static bool ShaderFind(string name, ref Shader __result) {
        if (!Ksp2UitkPlugin.Shaders.TryGetValue(name, out var injected))
        {
            return true;
        }
        __result = injected;
        return false;
    }
}