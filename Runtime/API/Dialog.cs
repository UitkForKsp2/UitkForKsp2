using System;
using System.Collections.Generic;
using UitkForKsp2.API.Order;
using UnityEngine;
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
        Actions = null,
        WindowOptions = WindowOptions.Default with
        {
            WindowId = null,
            MoveOptions = MoveOptions.Default with { HandleElementName = "dialog-header" },
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
    internal DialogHandle(UIDocument document, VisualElement root, VisualElement dialogElement, Action? onClose)
    {
        Document = document;
        Root = root;
        DialogElement = dialogElement;
        _onClose = onClose;
    }

    private readonly Action? _onClose;
    private bool _isClosed;

    public UIDocument Document { get; }
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

            if (Document.panelSettings != null)
            {
                OrderManager.Unregister(Document.panelSettings);
            }

            UnityObject.Destroy(Document.gameObject);
        }
    }
}

/// <summary>
/// Simple API for opening generic KSP-styled dialogs.
/// </summary>
[PublicAPI]
public static class Dialog
{
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
        UIDocument document = Window.Create(windowOptions, dialogElement);
        VisualElement documentRoot = document.rootVisualElement;
        ConfigureDocumentRoot(documentRoot, options.UseCurtain);
        OrderManager.Register(document.panelSettings);
        OrderManager.BringToFront(document.panelSettings);

        handle = new DialogHandle(document, documentRoot, dialogElement, options.OnClose);

        CenterDialogByDefault(dialogElement);

        if (options.UseCurtain)
        {
            dialogElement.MakeDraggable(true);
        }

        if (options.EnableLocalization)
        {
            EnableLocalization(document);
        }

        if (options.EnableUiSounds)
        {
            document.EnableUiSounds();
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

    private static void EnableLocalization(UIDocument document)
    {
        DocumentLocalization localization = document.TryGetComponent(out DocumentLocalization existing)
            ? existing
            : document.gameObject.AddComponent<DocumentLocalization>();

        localization.RegisterDocument(document);
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
        VisualElement dialogElement = Element.Root(classes: "uitk-dialog")!;

        dialogElement.Add(CreateHeader(options.Title, options.ShowCloseButton, getHandle));
        dialogElement.Add(Element.Label("dialog-message", options.Message, "uitk-dialog__message"));
        dialogElement.Add(CreateButtonRow(actions, getHandle));

        return dialogElement;
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

    private static VisualElement CreateHeader(string title, bool showCloseButton, Func<DialogHandle?> getHandle)
    {
        VisualElement header = Element.VisualElement("dialog-header", "uitk-dialog__header")!;
        header.Add(Element.Label("dialog-title", title, "uitk-dialog__title"));
        header.Add(CreateHeaderDashes());

        if (showCloseButton)
        {
            Button closeButton = Element.Button("dialog-close-button", "x", "uitk-dialog__close-button ui-sound-close");
            closeButton.clicked += () => getHandle()?.Close();
            header.Add(closeButton);
        }

        return header;
    }

    private static VisualElement CreateHeaderDashes()
    {
        VisualElement dashes = Element.VisualElement("dialog-header-dashes", "uitk-dialog__header-dashes")!;
        dashes.Add(Element.Label("dialog-header-dashes-run", new string('-', 250), "uitk-dialog__header-dashes-run"));
        dashes.Add(Element.Label("dialog-header-dashes-slash", "/", "uitk-dialog__header-dashes-slash"));

        return dashes;
    }

    private static VisualElement CreateButtonRow(IReadOnlyList<DialogAction> actions, Func<DialogHandle?> getHandle)
    {
        VisualElement row = Element.VisualElement("dialog-actions", "uitk-dialog__actions")!;

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

        return row;
    }
}
