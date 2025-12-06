using System;
using System.Reflection;
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
        SetupRootElement(root, document, options);

        _rootVisualElement.SetValue(document, root);
        _addRootVisualElementToTree.Invoke(document, Array.Empty<object>());

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

        VisualElement? rootElement = document.rootVisualElement.hierarchy.ElementAt(0);
        SetupRootElement(rootElement, document, options);

        return document;
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
            root.MakeDraggable(options.MoveOptions.CheckScreenBounds);

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

        if (options.DisableGameInputForTextFields)
        {
            root.Query<TextField>().ForEach(textField => textField.DisableGameInputOnFocus());
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
}