using System;
using System.Reflection;
using ReduxLib.Engine;
using UitkForKsp2.API.Manipulator;
using UitkForKsp2.Panel;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Contains methods for creating UIDocument windows.
/// </summary>
[PublicAPI]
public static class Window
{
    private const string UIMainCanvasPath = "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas";

    private static FieldInfo _rootVisualElement =
        typeof(UIDocument).GetField("m_RootVisualElement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static MethodInfo _addRootVisualElementToTree =
        typeof(UIDocument).GetMethod("AddRootVisualElementToTree", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static FieldInfo _sourceAsset =
        typeof(UIDocument).GetField("sourceAsset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    private static MethodInfo _recreateUi =
        typeof(UIDocument).GetMethod("RecreateUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    /// <summary>
    /// Creates an empty UIDocument.
    /// </summary>
    /// <param name="options">Options for creating the window.</param>
    /// <param name="root">Root element of the UIDocument. If null, a new empty VisualElement is created.</param>
    /// <returns>New empty UIDocument.</returns>
    public static UIDocument Create(WindowOptions options, VisualElement? root = null)
    {
        UIDocument document = CreateInternal(options);

        root ??= Element.Root();
        VisualElement documentRoot = CreateDocumentRoot(document, root);
        _rootVisualElement.SetValue(document, documentRoot);
        _addRootVisualElementToTree.Invoke(document, Array.Empty<object>());
        SetupRootElement(root, document, options);

        return document;
    }

    /// <summary>
    /// Creates a new UIDocument from a UXML asset.
    /// </summary>
    /// <param name="options">Options for creating the window.</param>
    /// <param name="uxml">UXML asset containing the UI.</param>
    /// <returns>UIDocument with the UI defined in UXML.</returns>
    public static UIDocument Create(WindowOptions options, VisualTreeAsset uxml)
    {
        UIDocument document = CreateInternal(options);

        _sourceAsset.SetValue(document, uxml);
        _recreateUi.Invoke(document, Array.Empty<object>());

        if (document.rootVisualElement.hierarchy.childCount <= 0)
        {
            return document;
        }

        ConfigureDocumentRoot(document.rootVisualElement);
        VisualElement? rootElement = ResolveWindowRoot(document.rootVisualElement);
        SetupRootElement(rootElement, document, options);

        return document;
    }

    private static VisualElement CreateDocumentRoot(UIDocument document, VisualElement root)
    {
        Type documentRootType = _rootVisualElement.FieldType;
        if (documentRootType.IsInstanceOfType(root))
        {
            return root;
        }

        ConstructorInfo? documentConstructor = documentRootType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(UIDocument), typeof(VisualTreeAsset) },
            null
        );

        if (documentConstructor == null)
        {
            throw new MissingMethodException(
                documentRootType.FullName,
                ".ctor(UnityEngine.UIElements.UIDocument, UnityEngine.UIElements.VisualTreeAsset)"
            );
        }

        var documentRoot = (VisualElement)documentConstructor.Invoke(new object?[] { document, null })!;
        ConfigureDocumentRoot(documentRoot);
        documentRoot.Add(root);
        return documentRoot;
    }

    private static void ConfigureDocumentRoot(VisualElement documentRoot)
    {
        documentRoot.style.alignItems = Align.FlexStart;
    }

    private static VisualElement? ResolveWindowRoot(VisualElement documentRoot)
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

    private static UIDocument CreateInternal(WindowOptions options)
    {
        var gameObject = new GameObject(options.WindowId ?? $"ui-{Guid.NewGuid()}");
        UnityObject.DontDestroyOnLoad(gameObject);
        gameObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        var document = gameObject.AddComponent<UIDocument>()!;

        PanelSettings panelSettings = PanelFactory.CreateForWindow(options);
        document.panelSettings = panelSettings;

        var owner = gameObject.AddComponent<PanelSettingsOwner>()!;
        owner.Owned = panelSettings;

        document.enabled = true;

        Transform? parent = options.Parent;
        if (parent == null && GameObject.Find(UIMainCanvasPath) is var uiMainCanvas)
        {
            if (uiMainCanvas != null)
            {
                parent = uiMainCanvas.transform;
            }
            else
            {
                // UitkForKsp2Plugin.Logger.LogWarning(
                //     $"Could not assign default parent to new window with ID {options.WindowId}"
                // );
            }
        }

        gameObject.transform.parent = parent;
        gameObject.SetActive(true);

        return document;
    }

    public static void SetupRootElement(VisualElement? root, UIDocument document, WindowOptions options)
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
            RegisterTextFieldBlurOnOutsidePointerDown(root, document.rootVisualElement);
        }

        if (options.BlockGameInput)
        {
            root.BlockGameInput();
        }

        if (options.BringToFrontOnPointerDown)
        {
            root.AddManipulator(new OrderManipulator(document.panelSettings!));
        }

        root.schedule!.Execute(() =>
        {
            PanelFactory.Apply(document.panelSettings!);
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
