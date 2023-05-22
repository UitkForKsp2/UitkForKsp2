using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine.UIElements;

namespace UitkForKsp2.Patch;

[HarmonyPatch]
public static class CustomElementsPatch
{
#pragma warning disable CS0618
    [HarmonyPatch(typeof(VisualElementFactoryRegistry), nameof(VisualElementFactoryRegistry.RegisterUserFactories))]
    [HarmonyPostfix]
    public static void VisualElementFactoryRegistry_RegisterUserFactories()
    {
        Chainloader.Plugins
            .SelectMany(plugin => plugin.GetType().Assembly.GetTypes())
            .Where(type => typeof(IUxmlFactory).IsAssignableFrom(type)
                           && !type.IsInterface
                           && !type.IsAbstract
                           && !type.IsGenericType)
            .ToList()
            .ForEach(type =>
            {
                var factory = (IUxmlFactory)Activator.CreateInstance(type);
                VisualElementFactoryRegistry.RegisterFactory(factory);
            });
    }
#pragma warning restore CS0618
}