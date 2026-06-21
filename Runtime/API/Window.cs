using System;
using UitkForKsp2.API.Manipulator;
using UitkForKsp2.Panel;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Contains methods for creating UI Toolkit windows backed by <see cref="PanelRenderer"/>.
/// </summary>
[PublicAPI]
public static class Window
{
    private const string UIMainCanvasPath = "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas";

    /// <summary>
    /// Creates an empty window.
    /// </summary>
    /// <param name="options">Options for creating the window.</param>
    /// <param name="root">Root element of the window. If null, a new empty VisualElement is created.</param>
    /// <returns>The <see cref="PanelRenderer"/> driving the new window.</returns>
    public static PanelRenderer Create(WindowOptions options, VisualElement? root = null)
    {
        (GameObject gameObject, PanelRenderer renderer) = CreateInternal(options);

        root ??= Element.Root();

        var component = gameObject.AddComponent<WindowComponent>()!;
        component.Initialize(renderer, options, uxml: null, programmaticRoot: root);

        gameObject.SetActive(true);
        component.ResolveNow();

        return renderer;
    }

    /// <summary>
    /// Creates a new window from a UXML asset.
    /// </summary>
    /// <param name="options">Options for creating the window.</param>
    /// <param name="uxml">UXML asset containing the UI.</param>
    /// <returns>The <see cref="PanelRenderer"/> driving the new window.</returns>
    public static PanelRenderer Create(WindowOptions options, VisualTreeAsset uxml)
    {
        (GameObject gameObject, PanelRenderer renderer) = CreateInternal(options);

        // Assigning visualTreeAsset makes PanelRenderer clone the UXML into its root automatically on enable.
        renderer.visualTreeAsset = uxml;

        var component = gameObject.AddComponent<WindowComponent>()!;
        component.Initialize(renderer, options, uxml: uxml, programmaticRoot: null);

        gameObject.SetActive(true);
        component.ResolveNow();

        return renderer;
    }

    internal static void ConfigureDocumentRoot(VisualElement documentRoot)
    {
        documentRoot.style.alignItems = Align.FlexStart;
    }

    internal static VisualElement? ResolveWindowRoot(VisualElement documentRoot)
    {
        if (documentRoot.hierarchy.childCount <= 0)
        {
            return null;
        }

        VisualElement windowRoot = documentRoot.hierarchy.ElementAt(0);
        while (windowRoot is TemplateContainer && windowRoot.hierarchy.childCount == 1)
        {
            ConfigureTemplateContainer(windowRoot);
            windowRoot = windowRoot.hierarchy.ElementAt(0);
        }

        return windowRoot;
    }

    private static void ConfigureTemplateContainer(VisualElement container)
    {
        container.style.alignSelf = Align.FlexStart;
        container.style.alignItems = Align.FlexStart;
        container.style.flexGrow = 0f;
        container.style.width = new StyleLength(StyleKeyword.Auto);
        container.style.height = new StyleLength(StyleKeyword.Auto);
    }

    private static (GameObject, PanelRenderer) CreateInternal(WindowOptions options)
    {
        var gameObject = new GameObject(options.WindowId ?? $"ui-{Guid.NewGuid()}");
        UnityObject.DontDestroyOnLoad(gameObject);
        gameObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        // Keep the GameObject inactive until the renderer, panel settings and companion component are all configured,
        // so that exactly one fully-populated UI-reload callback fires when it is activated.
        gameObject.SetActive(false);

        var renderer = gameObject.AddComponent<PanelRenderer>()!;

        // Each window owns a cloned PanelSettings so it can control its own z-order (its sortingOrder governs
        // stacking among UITK panels and relative to uGUI canvases). PanelSettingsOwner destroys it on teardown.
        PanelSettings panelSettings = PanelFactory.CreateForWindow(options);
        renderer.panelSettings = panelSettings;

        var owner = gameObject.AddComponent<PanelSettingsOwner>()!;
        owner.Owned = panelSettings;

        Transform? parent = options.Parent;
        if (parent == null && GameObject.Find(UIMainCanvasPath) is var uiMainCanvas)
        {
            if (uiMainCanvas != null)
            {
                parent = uiMainCanvas.transform;
            }
        }

        gameObject.transform.parent = parent;

        return (gameObject, renderer);
    }

    internal static void SetupRootElement(VisualElement root, PanelRenderer renderer, WindowOptions options)
    {
        if (root == null)
        {
            return;
        }

        if (options.MoveOptions.IsMovingEnabled)
        {
            VisualElement moveHandle = options.MoveOptions.ResolveHandle(root);
            moveHandle.MakeDraggable(root, options.MoveOptions.CheckScreenBounds);

            if (options.MoveOptions.CheckScreenBounds)
            {
                // Display window within screen bounds by default
                root.SetDefaultPosition(windowSize =>
                {
                    Rect panelRect = root.panel?.visualTree?.contentRect ??
                                     new Rect(0, 0, ReferenceResolution.Width, ReferenceResolution.Height);

                    float clampedX = Mathf.Clamp(
                        root.transform!.position.x,
                        0,
                        Mathf.Max(0, panelRect.width  - windowSize.x)
                    );

                    float clampedY = Mathf.Clamp(
                        root.transform!.position.y,
                        0,
                        Mathf.Max(0, panelRect.height - windowSize.y)
                    );

                    return new Vector2(clampedX, clampedY);
                });

            }
        }

        if (options.IsHidingEnabled)
        {
            root.EnableHiding();
        }

        if (options.ResizeOptions.IsResizingEnabled)
        {
            root.MakeResizable(options.ResizeOptions);
        }

        if (options.DisableGameInputForTextFields)
        {
            root.Query<TextField>().ForEach(textField => textField.DisableGameInputOnFocus());
            VisualElement panelRoot = renderer.GetPanelRoot() ?? root;
            RegisterTextFieldBlurOnOutsidePointerDown(root, panelRoot);
        }

        if (options.BlockGameInput)
        {
            root.BlockGameInput();
        }

        if (options.BringToFrontOnPointerDown)
        {
            root.AddManipulator(new OrderManipulator(renderer));
        }

        root.schedule!.Execute(() =>
        {
            PanelFactory.Apply(renderer.panelSettings!);
        });
    }

    private static void RegisterTextFieldBlurOnOutsidePointerDown(VisualElement root, VisualElement documentRoot)
    {
        documentRoot.RegisterCallback<PointerDownEvent>(
            evt => BlurTextFieldOnOutsidePointerDown(root, documentRoot, evt),
            TrickleDown.TrickleDown
        );
    }

    private static void BlurTextFieldOnOutsidePointerDown(
        VisualElement root,
        VisualElement documentRoot,
        PointerDownEvent evt
    )
    {
        if (evt.target is VisualElement target && Extensions.IsSameElementOrAncestor(root, target))
        {
            return;
        }

        if (documentRoot.panel?.focusController?.focusedElement is not VisualElement focusedElement ||
            !Extensions.IsSameElementOrAncestor(root, focusedElement))
        {
            return;
        }

        focusedElement.Blur();
    }
}
