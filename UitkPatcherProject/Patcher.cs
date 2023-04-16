using Ksp2Uitk;
using Mono.Cecil;

namespace UITKPatcher;

public static class Patcher
{
    private static readonly string BasePath = Path.Combine("BepInEx", "plugins", Ksp2UitkPlugin.ModFolder, "lib");

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