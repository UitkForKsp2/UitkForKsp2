# UITK for KSP 2

This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## Build requirements
- .NET SDK which conforms to the .NET Standard 2.1 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1#select-net-standard-version))
- Unity 2022.5.3 - **if you want to build the theme yourself**

## Building
1. Get a copy of the `kerbalui` bundle. You have two options:
   - Download it from the [latest release of KerbalUI](https://github.com/jan-bures/KerbalUI/releases/latest)
   - Build it yourself from the [KerbalUI project](https://github.com/jan-bures/KerbalUI). All you need to do is open it in Unity, and in the
     toolbar choose `Assets` -> `Build AssetBundles`. You will find the built bundle in
     `KerbalUI/Assets/AssetBundles/kerbalui`.
2. Copy the `kerbalui` file into `plugin_template/assets/bundles`.
3. Open the `UitkForKsp2.sln` solution and build it, using either Visual Studio 2022, JetBrains Rider, or the `dotnet` CLI tool.
4. Depending on the chosen build configuration, the full plugin can be found in one of the folders inside `dist`.
