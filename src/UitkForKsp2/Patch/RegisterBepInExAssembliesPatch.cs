using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.Patch;

#pragma warning disable CS1591
public static class RegisterBepInExAssembliesPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScriptingRuntime), nameof(ScriptingRuntime.GetAllUserAssemblies))]
    public static void RegisterBepInExAssemblies(ref string[] __result)
    {
        __result = __result.AddRangeToArray(Chainloader.Plugins.SelectMany(plugin => plugin.GetType().Assembly.GetFiles())
            .Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray());
    }
}