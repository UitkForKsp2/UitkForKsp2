# UITK for KSP 2

This mod brings Unity UI Toolkit support to Kerbal Space Program 2.

## Installation

### Recommended

- Download the latest [ckan.exe](https://github.com/ksp-ckan/ckan/releases/latest) and place it into your KSP2 folder
- Open CKAN, click the checkbox next to `UITK for KSP2` in the list of mods and click the "Apply changes" button in the
  toolbar at the top.

### Manual

- Download the [latest release](https://github.com/jan-bures/UitkForKsp2/releases/latest).
- Extract the zip file's contents into your KSP2 folder.

## Building

### Build requirements

- .NET SDK which conforms to the .NET Standard
  2.1 ([list here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1#select-net-standard-version))
- Unity 2022.5.3 - **if you want to build the theme yourself**

### Building process

1. Get a copy of the `kerbalui` addressables. You have two options:
    - Download the latest `kerbalui.zip` from the
      [latest release of UitkForKsp2.Unity](https://github.com/UitkForKsp2/UitkForKsp2.Unity/releases/latest).
    - Build it yourself from the included Unity project:
        1. Run the command `git submodule update --remote` to fetch the latest version of the submodule.
        2. Add the project folder `src/UitkForKsp2.Unity/UitkForKsp2.Unity` to Unity Hub and open it.
        3. Open `Windows -> Asset Management -> Addressables -> Groups`.
        4. In this window click on `Build -> New Build -> Default Build Script`.
        5. You will find the build addressables in
           `src/UitkForKsp2.Unity/UitkForKsp2.Unity/Library/com.unity.addressables/aa/Windows/`.
2. Copy the contents of either the downloaded `kerbalui.zip` file or the built
   `src/UitkForKsp2.Uitk/UitkForKsp2.Unity/Library/com.unity.addressables/aa/Windows/` folder into
   `plugin_template/addressables`, so that the following hierarchy is created:
   ```
    plugin_template/addressables
    ├── AddressablesLink/...
    ├── StandaloneWindows64/...
    ├── catalog.json
    └── settings.json
    ```
3. Open the `UitkForKsp2.sln` solution and build it, using either Visual Studio 2022, JetBrains Rider, or the `dotnet`
   CLI tool.
4. Depending on the chosen build configuration, the full plugin can be found in one of the folders inside `dist`.
