# Overview

## What is UITK for KSP2?

UITK for KSP2 is a library for creating user interfaces with Unity UI Toolkit in Kerbal Space Program 2 mods.

The library provides:
- APIs to help create new windows and UI elements from C# code,
- APIs for easier manipulation of UI elements, such as moving windows by dragging, 

It contains APIs to create windows and elements, various helper methods to make UI development easier, and default
themes in the style of KSP 2.

## Unity UI Toolkit

**[UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)**, also known as UITK, is the newest UI library for
Unity. It uses the UXML and USS languages (inspired by HTML and CSS) to create UIs similarly to how web pages are
created.

It is a modern alternative to Unity's two older UI libraries:

- the **[Unity UI](https://docs.unity3d.com/2022.3/Documentation/Manual/com.unity.ugui.html)** library used by KSP
  and KSP 2, also commonly known as uGUI,
- and the **[Immediate Mode GUI](https://docs.unity3d.com/2022.3/Documentation/Manual/GUIScriptingGuide.html)** library
  commonly used by KSP mods, also known as IMGUI.

For Kerbal Space Program 2 versions before 0.1.5, the mod used
to also provide the UI Toolkit assemblies and inject them into the game, however, those are now included with the base
game in versions 0.1.5+.

## Comparisons

### UI Toolkit vs. Unity UI {collapsible="true"}

**Unity UI** is the UI library used by the base game in both KSP and KSP 2.

UI is created in the Unity Editor by creating GameObjects with various components, such as `Canvas`, `Image`, `Text`,
etc., and then referencing those components from code to add functionality.

**UI Toolkit** is a more modern UI library, and as such, it has many advantages over Unity UI:

- **Flexibility**: UI Toolkit is much more flexible than Unity UI, and allows more complex UIs to be composed more
  easily.
- **Extensibility**: It is much easier to extend UI Toolkit than Unity UI, and it is possible to create custom elements.
- **Styling**: UI Toolkit uses a styling system similar to CSS, which makes it much easier to style UIs and create
  general themes.

It also has some disadvantages compared to Unity UI:

- **Documentation**: UI Toolkit is a new library, and as such, there is less documentation and fewer tutorials
  available for it.
- **Learning curve**: For non-web developers, the HTML and CSS-like syntax of UI Toolkit can be harder to learn than
  Unity UI.
- **Features**: Some of the features of Unity UI are not yet available in UI Toolkit, such as VFX with custom shaders
  and materials.

### UI Toolkit vs. IMGUI {collapsible="true"}

**IMGUI** is an older UI system that was commonly used by KSP mods. It is often used for debug UIs and prototyping,
as its main purpose is to create simple UIs quickly.

UI is created directly in C# by calling various methods in
the [`GUI`](https://docs.unity3d.com/ScriptReference/GUI.html)
and [`GUILayout`](https://docs.unity3d.com/ScriptReference/GUILayout.html) classes within
a [`OnGUI`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnGUI.html) method, which is called multiple times
per frame, and the UI is recreated every time.

**UI Toolkit**'s advantages over IMGUI include all the advantages it has over Unity UI, as well as:

- **Performance**: UI Toolkit offers much better performance than IMGUI, especially when it comes to larger UIs.
- **Debugging**: UI Toolkit has a built-in UI debugger, which makes it much easier to debug UIs.
- **Layout**: UI Toolkit makes it much easier to create complex and responsive layouts.

It also has some disadvantages compared to IMGUI:

- **Learning curve**: For non-web developers, the HTML and CSS-like syntax of UI Toolkit can be harder to learn than
  IMGUI.
- **Complexity**: UI Toolkit is a more complex library than IMGUI, and it may take longer to create very simple UIs 
  quickly.
- **Unity Editor**: While UI Toolkit UIs can be created dynamically in code, it is much easier to create them in the
  Unity Editor. This means that UI Toolkit is not as well suited for debug UIs and prototyping as IMGUI, which can be
  used to create UIs directly in code quickly.

### Official comparison

For an in-depth comparison of the various Unity UI systems, please
visit [Comparison of UI systems in Unity](https://docs.unity3d.com/2023.3/Documentation/Manual/UI-system-compare.html).