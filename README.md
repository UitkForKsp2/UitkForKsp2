# KSP 2 UITK

*(experimental)* This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## Requirements
- Unity 2020.3.33
- Visual Studio 2022
- .NET SDK which conforms to the .NET Standard 2.0 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0))

## Building
1. Open the included Unity project in `Unity/UITKProject` and build it.
2. Copy the following files from `<your build folder>/UITKProject_Data/Managed` to `ksp2_uitk/lib`:
    - `UnityEngine.TextCoreTextEngineModule.dll`
    - `UnityEngine.UI.dll`
    - `UnityEngine.UIElementsInputSystemModule.dll`
    - `UnityEngine.UIElementsModule.dll`
    - `UnityEngine.UIElementsNativeModule.dll`
    - `UnityEngine.UIModule.dll`
3. Open the `KSP2UITK.sln` solution and build it.
4. Depending on the chosen build configuration, the full plugin can be found in one of the `Debug` or `Release` folders.