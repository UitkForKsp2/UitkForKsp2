using KSP2UITK;
using Mono.Cecil;

namespace UITKPatcher;

public static class Patcher
{
    private static readonly string BasePath = Path.Combine("BepInEx", "plugins", KSP2UITKPlugin.ModFolder, "lib");
    
    public static IEnumerable<string> TargetDLLs { get; } = new[]
    {
        "UnityEngine.UI.dll",
        "UnityEngine.UIElementsModule.dll",
        "UnityEngine.UIElementsNativeModule.dll",
        "UnityEngine.UIModule.dll",
        "UnityEngine.TextCoreTextEngineModule.dll",
    };

    public static void Patch(ref AssemblyDefinition assembly)
    {
        assembly = AssemblyDefinition.ReadAssembly(Path.Combine(BasePath, $"{assembly.Name.Name}.dll"));
    }
}