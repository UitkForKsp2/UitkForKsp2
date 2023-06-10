using JetBrains.Annotations;
using Mono.Cecil;

namespace UitkForKsp2.Patcher;

[UsedImplicitly]
public static class Patcher
{
    private static readonly string BasePath = Path.Combine("BepInEx", "plugins", UitkForKsp2Plugin.ModGuid, "lib");

    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs { get; } = new[]
    {
        "UnityEngine.TextCoreTextEngineModule.dll",
        "UnityEngine.UI.dll",
        "UnityEngine.UIElementsInputSystemModule.dll",
        "UnityEngine.UIElementsModule.dll",
        "UnityEngine.UIElementsNativeModule.dll",
        "UnityEngine.UIModule.dll"
    };

    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        assembly = AssemblyDefinition.ReadAssembly(Path.Combine(BasePath, $"{assembly.Name.Name}.dll"));
    }
}