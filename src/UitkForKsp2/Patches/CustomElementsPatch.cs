using BepInEx.Bootstrap;
using HarmonyLib;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace UitkForKsp2.Patches;

[HarmonyPatch]
internal static class CustomElementsPatch
{
#pragma warning disable CS0618
    [HarmonyPatch(typeof(VisualElementFactoryRegistry), nameof(VisualElementFactoryRegistry.RegisterUserFactories))]
    [HarmonyPostfix]
    private static void VisualElementFactoryRegistry_RegisterUserFactories()
    {
        foreach (var plugin in Chainloader.Plugins)
        {
            CustomControls.RegisterFromAssembly(plugin.GetType().Assembly);
        }
    }
#pragma warning restore CS0618
}