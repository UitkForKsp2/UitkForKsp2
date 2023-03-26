# UITK for KSP 2

*(experimental)* This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## Build requirements
- Visual Studio 2022
- .NET SDK which conforms to the .NET Standard 2.0 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0))
- Unity 2020.3.33 - **if you want to build the theme yourself**

## Building
1. Copy all the `.dll` files from `<KSP 2 Folder>/KSP2_x64_Data/Managed` into `external_dlls`.
2. Get a copy of the `panelsettings` bundle. You have two options:
   - Download it from the [latest release of Kerbal UI](https://github.com/jan-bures/KerbalUI/releases/latest)
   - Build it yourself from the included `KerbalUI` project. All you need to do is open it in Unity, and in the toolbar
     choose `Assets` -> `Build AssetBundles`. You will find the built bundle in `KerbalUI/Assets/AssetBundles/panelsettings`.
3. Copy the `panelassets` file into `ksp2_uitk/assets/bundles`.
4. Open the `Ksp2Uitk.sln` solution and build it.
5. Depending on the chosen build configuration, the full plugin can be found in one of the `Debug` or `Release` folders.