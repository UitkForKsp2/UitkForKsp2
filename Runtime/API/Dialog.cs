using System;
using System.Collections.Generic;
using UitkForKsp2.API.Order;
using UitkForKsp2.Controls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Button definition for a generic UITK dialog.
/// </summary>
[PublicAPI]
public readonly struct DialogAction
{
    public DialogAction(string text, Action? callback = null, bool closeDialog = true, string? className = null)
    {
        Text = text;
        Callback = callback;
        CloseDialog = closeDialog;
        ClassName = className;
    }

    /// <summary>
    /// Button label. Localization keys starting with '#' are localized when dialog localization is enabled.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Callback invoked when the button is clicked.
    /// </summary>
    public Action? Callback { get; }

    /// <summary>
    /// Whether the dialog should close after this button is clicked.
    /// </summary>
    public bool CloseDialog { get; }

    /// <summary>
    /// Optional extra USS class for caller-specific button styling.
    /// </summary>
    public string? ClassName { get; }
}

/// <summary>
/// Options for opening a generic UITK dialog.
/// </summary>
[PublicAPI]
public struct DialogOptions
{
    /// <summary>
    /// Dialog title. Localization keys starting with '#' are localized when localization is enabled.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Dialog message. Localization keys starting with '#' are localized when localization is enabled.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Optional addressable Texture2D or Sprite used as the title-bar icon.
    /// </summary>
    public string? IconAddress { get; set; }

    /// <summary>
    /// Optional direct Texture2D used as the title-bar icon.
    /// </summary>
    public Texture2D? IconTexture { get; set; }

    /// <summary>
    /// Optional direct Sprite used as the title-bar icon.
    /// </summary>
    public Sprite? IconSprite { get; set; }

    /// <summary>
    /// Dialog buttons. Button text accepts localization keys starting with '#' when localization is enabled.
    /// </summary>
    public IReadOnlyList<DialogAction>? Actions { get; set; }

    public WindowOptions WindowOptions { get; set; }

    /// <summary>
    /// Enables the same '#' localization-key handling used by UITK windows.
    /// </summary>
    public bool EnableLocalization { get; set; }

    public bool EnableUiSounds { get; set; }
    public bool ShowCloseButton { get; set; }

    /// <summary>
    /// Adds a full-screen dimming curtain behind the dialog and blocks clicks to other UI.
    /// </summary>
    public bool UseCurtain { get; set; }

    public Action? OnClose { get; set; }

    public static DialogOptions Default => new()
    {
        Title = string.Empty,
        Message = string.Empty,
        IconAddress = null,
        IconTexture = null,
        IconSprite = null,
        Actions = null,
        WindowOptions = WindowOptions.Default with
        {
            WindowId = null,
            MoveOptions = MoveOptions.Default with { HandleElementName = "header" },
            ResizeOptions = ResizeOptions.Default,
        },
        EnableLocalization = true,
        EnableUiSounds = true,
        ShowCloseButton = false,
        UseCurtain = false,
        OnClose = null
    };
}

/// <summary>
/// Handle returned from <see cref="Dialog.Open(DialogOptions)"/>.
/// </summary>
[PublicAPI]
public sealed class DialogHandle
{
    internal DialogHandle(PanelRenderer renderer, VisualElement root, VisualElement dialogElement, Action? onClose)
    {
        Renderer = renderer;
        Root = root;
        DialogElement = dialogElement;
        _onClose = onClose;
    }

    private readonly Action? _onClose;
    private bool _isClosed;

    public PanelRenderer Renderer { get; }
    public VisualElement Root { get; }
    public VisualElement DialogElement { get; }
    public bool IsClosed => _isClosed;

    public void Close()
    {
        if (_isClosed)
        {
            return;
        }

        _isClosed = true;
        try
        {
            _onClose?.Invoke();
        }
        finally
        {
            Root.RemoveFromHierarchy();

            if (Renderer != null)
            {
                OrderManager.Unregister(Renderer);
                UnityObject.Destroy(Renderer.gameObject);
            }
        }
    }
}

/// <summary>
/// Simple API for opening generic KSP-styled dialogs.
/// </summary>
[PublicAPI]
public static class Dialog
{
    private const string DialogTemplateAddress = "Packages/uitkforksp2.controls/Assets/Templates/Dialog/DialogTemplate.uxml";
    private const string DialogStyleAddress = "Packages/uitkforksp2.controls/Assets/Templates/Dialog/DialogTemplate.uss";
    private const string RootName = "root";
    private const string DialogIconName = "dialog-header-icon";
    private const string DialogTitleName = "dialog-title";
    private const string DialogMessageName = "dialog-message";
    private const string DialogActionsName = "dialog-actions";
    private const string DialogCloseContainerName = "dialog-close-container";
    private const string DialogCloseButtonName = "dialog-close-button";

    private static VisualTreeAsset? _dialogTemplate;
    private static StyleSheet? _dialogStyleSheet;
    private static bool _templateLoadAttempted;
    private static bool _styleLoadAttempted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _dialogTemplate = null;
        _dialogStyleSheet = null;
        _templateLoadAttempted = false;
        _styleLoadAttempted = false;
    }

    public static DialogHandle Open(string title, string message, params DialogAction[] actions)
    {
        return Open(DialogOptions.Default with
        {
            Title = title,
            Message = message,
            Actions = actions
        });
    }

    public static DialogHandle Open(DialogOptions options)
    {
        IReadOnlyList<DialogAction> actions = options.Actions is { Count: > 0 }
            ? options.Actions
            : new[] { new DialogAction("OK") };

        DialogHandle? handle = null;
        WindowOptions windowOptions = options.WindowOptions;
        if (options.UseCurtain)
        {
            windowOptions.MoveOptions = MoveOptions.Default with { IsMovingEnabled = false };
            windowOptions.ResizeOptions = ResizeOptions.Default;
            windowOptions.BlockGameInput = true;
        }

        VisualElement dialogElement = CreateDialogElement(options, actions, () => handle);
        PanelRenderer renderer = Window.Create(windowOptions, dialogElement);
        VisualElement documentRoot = renderer.GetPanelRoot()!;
        ConfigureDocumentRoot(documentRoot, options.UseCurtain);
        OrderManager.Register(renderer);
        OrderManager.BringToFront(renderer);

        handle = new DialogHandle(renderer, documentRoot, dialogElement, options.OnClose);

        CenterDialogByDefault(dialogElement);

        if (options.UseCurtain)
        {
            dialogElement.MakeDraggable(true);
        }

        if (options.EnableLocalization)
        {
            renderer.EnableLocalization();
        }

        if (options.EnableUiSounds)
        {
            renderer.EnableUiSounds();
        }

        return handle;
    }

    private static void CenterDialogByDefault(VisualElement dialogElement)
    {
        dialogElement.CenterByDefault();
        dialogElement.schedule.Execute(() => CenterDialog(dialogElement));
    }

    private static void CenterDialog(VisualElement dialogElement)
    {
        Rect windowRect = dialogElement.contentRect;
        if (windowRect.width == 0 || windowRect.height == 0)
        {
            return;
        }

        Rect panelRect = dialogElement.panel?.visualTree?.contentRect ??
                         new Rect(0, 0, ReferenceResolution.Width, ReferenceResolution.Height);

        dialogElement.style.position = Position.Absolute;
        dialogElement.style.left = (panelRect.width - windowRect.width) / 2f;
        dialogElement.style.top = (panelRect.height - windowRect.height) / 2f;
    }

    public static DialogHandle Alert(
        string title,
        string message,
        string buttonText = "#Application/OK",
        Action? onClose = null,
        bool useCurtain = false
    )
    {
        return Open(DialogOptions.Default with
        {
            Title = title,
            Message = message,
            Actions = new[] { new DialogAction(buttonText) },
            UseCurtain = useCurtain,
            OnClose = onClose
        });
    }

    public static DialogHandle Confirm(
        string title,
        string message,
        Action? onConfirm,
        Action? onCancel = null,
        string confirmText = "#Application/OK",
        string cancelText = "#Application/Cancel",
        bool useCurtain = false
    )
    {
        return Open(DialogOptions.Default with
        {
            Title = title,
            Message = message,
            Actions = new[]
            {
                new DialogAction(cancelText, onCancel),
                new DialogAction(confirmText, onConfirm)
            },
            UseCurtain = useCurtain
        });
    }

    private static VisualElement CreateDialogElement(
        DialogOptions options,
        IReadOnlyList<DialogAction> actions,
        Func<DialogHandle?> getHandle
    )
    {
        VisualElement dialogElement = CreateTemplateElement();
        ConfigureIcon(dialogElement, options);
        ConfigureTitle(dialogElement, options.Title);
        ConfigureCloseButton(dialogElement, options.ShowCloseButton, getHandle);
        ConfigureMessage(dialogElement, options.Message);
        ConfigureActions(dialogElement, actions, getHandle);

        return dialogElement;
    }

    private static VisualElement CreateTemplateElement()
    {
        LoadTemplate();
        if (_dialogTemplate == null)
        {
            return CreateFallbackTemplateElement();
        }

        TemplateContainer container = _dialogTemplate.CloneTree();
        var root = container.Q<VisualElement>(RootName);
        if (root == null)
        {
            AddDialogStyleSheet(container);
            return container;
        }

        root.RemoveFromHierarchy();
        AddDialogStyleSheet(root);
        return root;
    }

    private static VisualElement CreateFallbackTemplateElement()
    {
        var root = new AppShell
        {
            name = RootName,
            TitleName = DialogTitleName,
            IconName = DialogIconName,
            DashesName = "dialog-header-dashes",
            CloseContainerName = DialogCloseContainerName,
            CloseButtonName = DialogCloseButtonName,
            UppercaseTitle = true
        };
        root.AddToClassList("uitk-dialog");

        VisualElement body = Element.VisualElement("dialog-body", "uitk-dialog__body oab-window-body")!;
        body.Add(Element.Label(DialogMessageName, classes: "uitk-dialog__message"));

        root.Add(body);
        root.Add(Element.VisualElement(DialogActionsName, "uitk-dialog__actions oab-window-actions"));
        AddDialogStyleSheet(root);
        return root;
    }

    private static void AddDialogStyleSheet(VisualElement root)
    {
        LoadStyleSheet();
        if (_dialogStyleSheet != null)
        {
            root.styleSheets.Add(_dialogStyleSheet);
        }
    }

    private static void LoadTemplate()
    {
        if (_templateLoadAttempted)
        {
            return;
        }

        _templateLoadAttempted = true;
        AsyncOperationHandle<VisualTreeAsset> handle =
            Addressables.LoadAssetAsync<VisualTreeAsset>(DialogTemplateAddress);
        _dialogTemplate = handle.WaitForCompletion();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError(
                $"Failed to load dialog UXML from address '{DialogTemplateAddress}': {handle.OperationException}"
            );
            _dialogTemplate = null;
        }
    }

    private static void LoadStyleSheet()
    {
        if (_styleLoadAttempted)
        {
            return;
        }

        _styleLoadAttempted = true;
        AsyncOperationHandle<StyleSheet> handle =
            Addressables.LoadAssetAsync<StyleSheet>(DialogStyleAddress);
        _dialogStyleSheet = handle.WaitForCompletion();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError(
                $"Failed to load dialog USS from address '{DialogStyleAddress}': {handle.OperationException}"
            );
            _dialogStyleSheet = null;
        }
    }

    private static void ConfigureTitle(VisualElement dialogElement, string title)
    {
        var titleLabel = dialogElement.Q<Label>(DialogTitleName);
        if (titleLabel != null)
        {
            titleLabel.text = string.IsNullOrEmpty(title) || title[0] == '#'
                ? title
                : title.ToUpperInvariant();
        }
    }

    private static void ConfigureIcon(VisualElement dialogElement, DialogOptions options)
    {
        VisualElement? icon = dialogElement.Q(DialogIconName);
        if (icon == null)
        {
            return;
        }

        icon.style.display = DisplayStyle.None;
        icon.style.backgroundImage = StyleKeyword.Null;

        if (options.IconTexture != null)
        {
            icon.style.backgroundImage = new StyleBackground(options.IconTexture);
            icon.style.display = DisplayStyle.Flex;
            return;
        }

        if (options.IconSprite != null)
        {
            icon.style.backgroundImage = new StyleBackground(options.IconSprite);
            icon.style.display = DisplayStyle.Flex;
            return;
        }

        if (string.IsNullOrWhiteSpace(options.IconAddress))
        {
            return;
        }

        AsyncOperationHandle<UnityEngine.Object> handle =
            Addressables.LoadAssetAsync<UnityEngine.Object>(options.IconAddress);
        UnityEngine.Object iconAsset = handle.WaitForCompletion();
        if (handle.Status != AsyncOperationStatus.Succeeded || iconAsset == null)
        {
            Debug.LogError(
                $"Failed to load dialog icon from address '{options.IconAddress}': {handle.OperationException}"
            );
            return;
        }

        switch (iconAsset)
        {
            case Texture2D texture:
                icon.style.backgroundImage = new StyleBackground(texture);
                icon.style.display = DisplayStyle.Flex;
                break;
            case Sprite sprite:
                icon.style.backgroundImage = new StyleBackground(sprite);
                icon.style.display = DisplayStyle.Flex;
                break;
            default:
                Debug.LogError(
                    $"Dialog icon address '{options.IconAddress}' resolved to unsupported asset type '{iconAsset.GetType().Name}'"
                );
                break;
        }
    }

    private static void ConfigureMessage(VisualElement dialogElement, string message)
    {
        var messageLabel = dialogElement.Q<Label>(DialogMessageName);
        if (messageLabel != null)
        {
            messageLabel.text = message;
        }
    }

    private static void ConfigureCloseButton(
        VisualElement dialogElement,
        bool showCloseButton,
        Func<DialogHandle?> getHandle
    )
    {
        VisualElement? closeContainer = dialogElement.Q(DialogCloseContainerName);
        if (closeContainer != null)
        {
            closeContainer.style.display = showCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
        }

        var closeButton = dialogElement.Q<Button>(DialogCloseButtonName);
        if (closeButton != null)
        {
            closeButton.clicked += () => getHandle()?.Close();
        }
    }

    private static void ConfigureActions(
        VisualElement dialogElement,
        IReadOnlyList<DialogAction> actions,
        Func<DialogHandle?> getHandle
    )
    {
        VisualElement? row = dialogElement.Q(DialogActionsName);
        if (row == null)
        {
            row = Element.VisualElement(DialogActionsName, "uitk-dialog__actions oab-window-actions")!;
            dialogElement.Add(row);
        }

        row.Clear();

        for (int i = 0; i < actions.Count; i++)
        {
            DialogAction action = actions[i];
            Button button = Element.Button(null, action.Text, "uitk-dialog__button");
            button.name = $"dialog-action-{i}";

            if (!string.IsNullOrWhiteSpace(action.ClassName))
            {
                button.AddToClassList(action.ClassName);
            }

            button.clicked += () =>
            {
                try
                {
                    action.Callback?.Invoke();
                }
                finally
                {
                    if (action.CloseDialog)
                    {
                        getHandle()?.Close();
                    }
                }
            };

            row.Add(button);
        }
    }

    private static void ConfigureDocumentRoot(VisualElement root, bool useCurtain)
    {
        root.style.flexGrow = 1;
        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);

        if (!useCurtain)
        {
            root.style.backgroundColor = Color.clear;
            root.pickingMode = PickingMode.Ignore;
            return;
        }

        root.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);
        root.pickingMode = PickingMode.Position;
        root.BlockGameInput();
        RegisterCurtainBlockers(root);
    }

    private static void RegisterCurtainBlockers(VisualElement curtain)
    {
        curtain.RegisterCallback<PointerDownEvent>(evt => BlockCurtainEvent(evt));
        curtain.RegisterCallback<PointerUpEvent>(evt => BlockCurtainEvent(evt));
        curtain.RegisterCallback<PointerMoveEvent>(evt => BlockCurtainEvent(evt));
        curtain.RegisterCallback<PointerCancelEvent>(evt => BlockCurtainEvent(evt));
        curtain.RegisterCallback<ClickEvent>(evt => BlockCurtainEvent(evt));
        curtain.RegisterCallback<WheelEvent>(evt => BlockCurtainEvent(evt));
    }

    private static void BlockCurtainEvent(EventBase evt)
    {
        if (evt.target != evt.currentTarget)
        {
            return;
        }

        evt.StopPropagation();
        evt.PreventDefault();
    }
}
