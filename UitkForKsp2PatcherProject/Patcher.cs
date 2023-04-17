using UitkForKsp2;
using Mono.Cecil;

namespace UitkForKsp2Patcher;

public static class Patcher
{
    private static readonly string BasePath = Path.Combine("BepInEx", "plugins", UitkForKsp2Plugin.ModFolder, "lib");

    public static IEnumerable<string> TargetDLLs { get; } = new[]
    {
        "UnityEngine.TextCoreTextEngineModule.dll",
        "UnityEngine.UI.dll",
        "UnityEngine.UIElementsInputSystemModule.dll",
        "UnityEngine.UIElementsModule.dll",
        "UnityEngine.UIElementsNativeModule.dll",
        "UnityEngine.UIModule.dll"
    };

    public static void Patch(ref AssemblyDefinition assembly)
    {
        assembly = AssemblyDefinition.ReadAssembly(Path.Combine(BasePath, $"{assembly.Name.Name}.dll"));
    }
}