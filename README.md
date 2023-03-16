# KSP 2 UITK

*(experimental)* This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## Requirements
- Unity 2020.3.33
- Visual Studio 2022
- .NET SDK which conforms to the .NET Standard 2.0 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0))

## Building
1. Open the included Unity project in `Unity/UITKProject`.
2. Copy the following shaders from `Unity/UITKProject/Library/PackageCache/com.unity.ui@1.0.0-preview.18/Shaders/`
   to `Unity/UITKProject/Assets/Shaders`:
   - `UIE-AtlasBlit.shader`
   - `UIE-ColorConversionBlitShader.shader`
   - `UIE-Runtime.shader`
   - `UIE-RuntimeWorld.shader`
3. Inside Unity's Project view, click on the `Shaders` folder and on the bottom of the inspector window
   click on picker next to the "AssetBundle" label and select "New", then type in `shaders`.
4. In the main toolbar at the top of the window, select Assets -> Build AssetBundles. This should create a folder
   called `AssetBundles` in the `Unity/UITKProject/Assets` folder with the file `shaders` inside it.
5. Copy the `shaders` file from the previous step into `ksp2_utik/bundles`.
6. Build the Unity project (File -> Build Settings -> Build) into a folder of your choice.
7. Copy all the `.dll` files from `<KSP 2 Folder>/KSP2_x64_Data/Managed` into `external_dlls`.
8. Copy the following files from `<Unity build folder>/UITKProject_Data/Managed` into both `ksp2_uitk/lib` AND
   `external_dlls` (replace any files already there):
   - `UnityEngine.TextCoreTextEngineModule.dll`
   - `UnityEngine.UI.dll`
   - `UnityEngine.UIElementsInputSystemModule.dll`
   - `UnityEngine.UIElementsModule.dll`
   - `UnityEngine.UIElementsNativeModule.dll`
   - `UnityEngine.UIModule.dll`
9. Open the `KSP2UITK.sln` solution and build it.
10. Depending on the chosen build configuration, the full plugin can be found in one of the `Debug` or `Release` folders.