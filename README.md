# KSP 2 UITK

*(experimental)* This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## How to use
1. Create a new BepInEx or SpaceWarp plugin project.

## Build requirements
- Visual Studio 2022
- .NET SDK which conforms to the .NET Standard 2.0 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0))

## Building
1. Copy all the `.dll` files from `<KSP 2 Folder>/KSP2_x64_Data/Managed` into `external_dlls`.
2. Open the `KSP2UITK.sln` solution and build it.
3. Depending on the chosen build configuration, the full plugin can be found in one of the `Debug` or `Release` folders.