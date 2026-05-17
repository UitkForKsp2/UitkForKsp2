using System;
using System.Collections.Generic;
using System.Reflection;
using ReduxLib.GameInterfaces;
using UitkForKsp2.API.Manipulator;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Extension methods for UIDocument and VisualElements.
/// </summary>
[PublicAPI]
public static class Extensions
{
    internal static event Action<VisualElement>? ElementHidden;

    #region UIDocument extensions

    /// <summary>
    /// Automatically localize elements in a document. Only elements with a string property "text" whose value is a
    /// localization key starting with '#' will be localized.
    /// </summary>
    /// <param name="document">The document in which to localize all localizable elements.</param>
    /// <returns>The DocumentLocalization component which was added to the document.</returns>
    public static DocumentLocalization EnableLocalization(this UIDocument document)
    {
        return document.gameObject.AddComponent<DocumentLocalization>();
    }

    /// <summary>
    /// Enable delegated UI sounds for elements marked with sound USS classes.
    /// </summary>
    /// <param name="document">The document whose root element should listen for UI sound events.</param>
    /// <returns>The document with delegated UI sounds enabled.</returns>
    public static UIDocument EnableUiSounds(this UIDocument document)
    {
        document.rootVisualElement.EnableUiSounds();
        return document;
    }

    /// <summary>
    /// Show a UIDocument by setting its root VisualElement's display style to DisplayStyle.Flex.
    /// </summary>
    /// <param name="document">The document to show.</param>
    public static void Show(this UIDocument document)
    {
        document.rootVisualElement.Show();
    }

    /// <summary>
    /// Hide a UIDocument by setting its root VisualElement's display style to DisplayStyle.None.
    /// </summary>
    /// <param name="document">The document to hide.</param>
    public static void Hide(this UIDocument document)
    {
        document.rootVisualElement.Hide();
    }

    /// <summary>
    /// Toggle the display of a UIDocument between DisplayStyle.Flex and DisplayStyle.None.
    /// </summary>
    /// <param name="document">The document to toggle the display of.</param>
    public static void ToggleDisplay(this UIDocument document)
    {
        document.rootVisualElement.ToggleDisplay();
    }

    #endregion

    #region VisualElement extensions

    private static MethodInfo _setMethod = typeof(VisualElement).GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    /// <summary>
    /// Set a property on a VisualElement.
    /// </summary>
    /// <param name="element">The element on which to set the property.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element on which the property was set.</returns>
    public static T Set<T>(this T element, string name, object value) where T : VisualElement
    {
        _setMethod.Invoke(element, new[] { name, value });
        return element;
    }

    /// <summary>
    /// Set the text of a TextElement.
    /// </summary>
    /// <param name="element">The element on which to set the text property.</param>
    /// <param name="text">The text to set.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of TextElement.</typeparam>
    /// <returns>The element on which the text property was set.</returns>
    public static T Text<T>(this T element, string text) where T : TextElement
    {
        element.text = text;
        return element;
    }

    /// <summary>
    /// Add children to a VisualElement.
    /// </summary>
    /// <param name="element">The element to which to add the children.</param>
    /// <param name="children">The children VisualElements to add.</param>
    /// <typeparam name="T">The type of the parent element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The parent element to which the children were added.</returns>
    public static T AddChildren<T>(this T element, IEnumerable<VisualElement>? children) where T : VisualElement
    {
        if (children == null)
        {
            return element;
        }

        foreach (var child in children)
        {
            element.Add(child);
        }

        return element;
    }

    /// <summary>
    /// Show a VisualElement by setting its display style to DisplayStyle.Flex.
    /// </summary>
    /// <param name="element">The element to show.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was shown.</returns>
    public static T Show<T>(this T element) where T : VisualElement
    {
        element.style.display = DisplayStyle.Flex;
        return element;
    }

    /// <summary>
    /// Hide a VisualElement by setting its display style to DisplayStyle.None.
    /// </summary>
    /// <param name="element">The element to hide.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was hidden.</returns>
    public static T Hide<T>(this T element) where T : VisualElement
    {
        BlurFocusedElementWithin(element);
        ReleaseTextInputLocksWithin(element);
        NotifyElementHidden(element);
        element.style.display = DisplayStyle.None;
        NotifyElementHidden(element);
        return element;
    }

    internal static void NotifyElementHidden(VisualElement element)
    {
        ElementHidden?.Invoke(element);
    }

    /// <summary>
    /// Toggle the display of a VisualElement between DisplayStyle.Flex and DisplayStyle.None.
    /// </summary>
    /// <param name="element">The element to toggle the display of.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was toggled.</returns>
    public static T ToggleDisplay<T>(this T element) where T : VisualElement
    {
        return element.style.display == DisplayStyle.None
            ? element.Show()
            : element.Hide();
    }

    /// <summary>
    /// Make a VisualElement draggable by adding a DragManipulator.
    /// </summary>
    /// <param name="element">The element to make draggable.</param>
    /// <param name="checkScreenBounds">Should the element be draggable only within the screen bounds?</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was made draggable.</returns>
    public static T MakeDraggable<T>(this T element, bool checkScreenBounds) where T : VisualElement
    {
        element.AddManipulator(new DragManipulator(!checkScreenBounds));
        return element;
    }

    /// <summary>
    /// Make a VisualElement act as a drag handle for another VisualElement.
    /// </summary>
    /// <param name="handle">The element that receives pointer events.</param>
    /// <param name="dragTarget">The element to move.</param>
    /// <param name="checkScreenBounds">Should the element be draggable only within the screen bounds?</param>
    /// <typeparam name="T">The handle element type.</typeparam>
    /// <returns>The handle element.</returns>
    public static T MakeDraggable<T>(this T handle, VisualElement dragTarget, bool checkScreenBounds) where T : VisualElement
    {
        handle.AddManipulator(new DragManipulator(dragTarget, !checkScreenBounds));
        return handle;
    }

    /// <summary>
    /// Make a VisualElement resizable by adding a ResizeManipulator.
    /// </summary>
    /// <param name="element">The element to make resizable.</param>
    /// <param name="options">Resize behavior options.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was made resizable.</returns>
    public static T MakeResizable<T>(this T element, ResizeOptions options) where T : VisualElement
    {
        element.AddManipulator(new ResizeManipulator(options));
        return element;
    }

    /// <summary>
    /// Make a VisualElement draggable by adding a DragManipulator.
    /// </summary>
    /// <param name="element">The element to make draggable.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was made draggable.</returns>
    [Obsolete("This method will be removed in 3.0.0. Use MakeDraggable(T, bool) instead.")]
    public static T MakeDraggable<T>(this T element) where T : VisualElement
    {
        return element.MakeDraggable(true);
    }

    /// <summary>
    /// Enables the F2 hiding functionality for a visual element.
    /// </summary>
    /// <param name="element">The visual element to enable hiding for.</param>
    /// <typeparam name="T">The type of the visual element.</typeparam>
    /// <returns>The visual element with the hiding functionality enabled.</returns>
    public static T EnableHiding<T>(this T element) where T : VisualElement
    {
        element.AddManipulator(new HideManipulator());
        return element;
    }

    /// <summary>
    /// Enable delegated UI sounds for child elements marked with sound USS classes.
    /// </summary>
    /// <param name="element">The root element that should listen for child sound events.</param>
    /// <typeparam name="T">The type of the root visual element.</typeparam>
    /// <returns>The root element with delegated UI sounds enabled.</returns>
    public static T EnableUiSounds<T>(this T element) where T : VisualElement
    {
        element.AddManipulator(new DocumentSoundManipulator());
        return element;
    }

    /// <summary>
    /// Block gameplay input while the pointer is over or interacting with a VisualElement.
    /// </summary>
    /// <param name="element">The root element that should block gameplay input.</param>
    /// <typeparam name="T">The type of the root visual element.</typeparam>
    /// <returns>The root element with gameplay input blocking enabled.</returns>
    public static T BlockGameInput<T>(this T element) where T : VisualElement
    {
        element.AddManipulator(new GameInputBlockManipulator());
        return element;
    }

    /// <summary>
    /// Set the default position of an element using a callback to calculate the position.
    /// </summary>
    /// <param name="element">The element to position.</param>
    /// <param name="calculatePosition">
    /// The callback which will be called when the element is resized. The callback will be passed the size of the
    /// element and should return the position to set the element to in the reference resolution.
    /// </param>
    public static void SetDefaultPosition(this VisualElement? element, Func<Vector2, Vector2> calculatePosition)
    {
        EventCallback<GeometryChangedEvent> geometryChanged = null;
        geometryChanged = evt => { GeometryChangedHandler(evt, element, calculatePosition, geometryChanged); };

        element.RegisterCallback(geometryChanged);
    }

    /// <summary>
    /// Set the default position of an element to the center of the screen.
    /// </summary>
    /// <param name="element">The element to center.</param>
    public static void CenterByDefault(this VisualElement element)
    {
        element.SetDefaultPosition(windowSize =>
        {
            Rect rect = element.panel?.visualTree?.contentRect ??
                        new Rect(0, 0, ReferenceResolution.Width, ReferenceResolution.Height);

            return new Vector2(
                (rect.width - windowSize.x) / 2,
                (rect.height - windowSize.y) / 2
            );
        });
    }

    /// <summary>
    /// Disable dragging on a specific element.
    /// </summary>
    /// <param name="element">The element to disable dragging on.</param>
    public static void DisableDragging(this VisualElement element)
    {
        element.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
        element.RegisterCallback<PointerUpEvent>(evt => evt.StopPropagation());
        element.RegisterCallback<PointerMoveEvent>(evt => evt.StopPropagation());
    }

    /// <summary>
    /// Disable game input when an element is focused.
    /// </summary>
    /// <param name="element">The element to disable game input on focus for.</param>
    public static void DisableGameInputOnFocus(this VisualElement element)
    {
        bool hasFocusLock = false;
        object focusLockOwner = new();
        IVisualElementScheduledItem? focusLockStateCheck = null;
        bool isListeningForHiddenElements = false;

        void AcquireFocusLock()
        {
            if (hasFocusLock)
            {
                return;
            }

            hasFocusLock = SetTextInputDisabled(focusLockOwner, true, element);
        }

        void ReleaseFocusLock()
        {
            if (!hasFocusLock)
            {
                return;
            }

            hasFocusLock = false;
            SetTextInputDisabled(focusLockOwner, false);
        }

        void OnElementHidden(VisualElement hiddenElement)
        {
            if (hasFocusLock && IsSameElementOrAncestor(hiddenElement, element))
            {
                ReleaseFocusLock();
            }
        }

        void SubscribeToHiddenElements()
        {
            if (isListeningForHiddenElements)
            {
                return;
            }

            ElementHidden += OnElementHidden;
            isListeningForHiddenElements = true;
        }

        void UnsubscribeFromHiddenElements()
        {
            if (!isListeningForHiddenElements)
            {
                return;
            }

            ElementHidden -= OnElementHidden;
            isListeningForHiddenElements = false;
        }

        void StartFocusLockStateCheck()
        {
            focusLockStateCheck?.Pause();
            VisualElement scheduleTarget = element.panel?.visualTree ?? element;
            focusLockStateCheck = scheduleTarget.schedule.Execute(CheckFocusLockState).Every(100);
        }

        void CheckFocusLockState()
        {
            if (hasFocusLock &&
                (!IsElementAvailableForFocusLock(element) || !IsElementOrDescendantFocused(element)))
            {
                ReleaseFocusLock();
            }
        }

        element.RegisterCallback<FocusInEvent>(_ =>
        {
            Log($"FocusInEvent: {element.GetType().Name} {element.name}");
            Log("\tDisabling game input");
            element.ReleaseMouse();
            AcquireFocusLock();
        });

        element.RegisterCallback<FocusOutEvent>(_ =>
        {
            Log($"FocusOutEvent: {element.GetType().Name} {element.name}");
            Log("\tEnabling game input");
            ReleaseFocusLock();
        });

        element.RegisterCallback<GeometryChangedEvent>(_ => CheckFocusLockState());
        element.RegisterCallback<DetachFromPanelEvent>(_ =>
        {
            ReleaseFocusLock();
            focusLockStateCheck?.Pause();
            UnsubscribeFromHiddenElements();
        });
        element.RegisterCallback<AttachToPanelEvent>(_ =>
        {
            SubscribeToHiddenElements();
            StartFocusLockStateCheck();
        });

        SubscribeToHiddenElements();
        StartFocusLockStateCheck();
    }

    #endregion

    /// <summary>
    /// Log a message to the console only in debug builds.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private static void Log(string message)
    {
#if !RELEASE
        UitkForKsp2Plugin.Logger.LogInfo(message);
#endif
    }

    /// <summary>
    /// The statuses of all input definitions before they were disabled.
    /// </summary>
    private static readonly object LegacyGameInputLockOwner = new();

    private static readonly object LegacyTextInputLockOwner = new();

    private static readonly HashSet<object> GameInputLockOwners = new();

    private static readonly HashSet<object> TextInputLockOwners = new();

    private static readonly Dictionary<object, VisualElement> TextInputLockOwnerElements = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetGameInputLockCount()
    {
        GameInputLockOwners.Clear();
        TextInputLockOwners.Clear();
        TextInputLockOwnerElements.Clear();
    }

    /// <summary>
    /// Disable or enable the game input.
    /// </summary>
    /// <param name="isDisabled">True to disable the game input, false to enable it.</param>
    internal static bool SetGameInputDisabled(bool isDisabled)
    {
        return SetGameInputDisabled(LegacyGameInputLockOwner, isDisabled);
    }

    internal static bool SetGameInputDisabled(object owner, bool isDisabled)
    {
        if (isDisabled)
        {
            if (!IInputManager.Instance.Ready)
            {
                // UitkForKsp2Plugin.Logger.LogError(
                //     "Attempted to disable game input, but the game instance has not yet been initialized"
                // );
                return false;
            }

            if (!GameInputLockOwners.Add(owner))
            {
                return true;
            }

            if (GameInputLockOwners.Count > 1)
            {
                return true;
            }

            IInputManager.Instance.SetUitkInputLocks();
            return true;
        }

        if (!GameInputLockOwners.Remove(owner))
        {
            return false;
        }

        if (GameInputLockOwners.Count > 0)
        {
            return true;
        }

        if (IInputManager.Instance.Ready)
        {
            IInputManager.Instance.RestoreUitkInputLocks();
        }

        return true;
    }

    internal static bool SetTextInputDisabled(bool isDisabled)
    {
        return SetTextInputDisabled(LegacyTextInputLockOwner, isDisabled);
    }

    internal static bool SetTextInputDisabled(object owner, bool isDisabled)
    {
        return SetTextInputDisabled(owner, isDisabled, null);
    }

    private static bool SetTextInputDisabled(object owner, bool isDisabled, VisualElement? element)
    {
        if (isDisabled)
        {
            if (!IInputManager.Instance.Ready)
            {
                return false;
            }

            if (element != null)
            {
                TextInputLockOwnerElements[owner] = element;
            }

            if (!TextInputLockOwners.Add(owner))
            {
                return true;
            }

            if (TextInputLockOwners.Count > 1)
            {
                return true;
            }

            IInputManager.Instance.SetUitkTextInputLocks();
            return true;
        }

        if (!TextInputLockOwners.Remove(owner))
        {
            TextInputLockOwnerElements.Remove(owner);
            return false;
        }

        TextInputLockOwnerElements.Remove(owner);

        if (TextInputLockOwners.Count > 0)
        {
            return true;
        }

        if (IInputManager.Instance.Ready)
        {
            IInputManager.Instance.RestoreUitkTextInputLocks();
        }

        return true;
    }

    private static void ReleaseTextInputLocksWithin(VisualElement hiddenElement)
    {
        List<object>? ownersToRelease = null;
        foreach (KeyValuePair<object, VisualElement> ownerElement in TextInputLockOwnerElements)
        {
            if (!IsSameElementOrAncestor(hiddenElement, ownerElement.Value))
            {
                continue;
            }

            ownersToRelease ??= new List<object>();
            ownersToRelease.Add(ownerElement.Key);
        }

        if (ownersToRelease == null)
        {
            return;
        }

        foreach (object owner in ownersToRelease)
        {
            SetTextInputDisabled(owner, false);
        }
    }

    private static bool IsElementAvailableForFocusLock(VisualElement element)
    {
        if (element.panel == null || element.worldBound.width <= 0 || element.worldBound.height <= 0)
        {
            return false;
        }

        VisualElement? current = element;
        while (current != null)
        {
            if (current.resolvedStyle.display == DisplayStyle.None ||
                current.resolvedStyle.visibility == Visibility.Hidden)
            {
                return false;
            }

            current = current.parent;
        }

        return true;
    }

    private static bool IsElementOrDescendantFocused(VisualElement element)
    {
        if (element.panel?.focusController?.focusedElement is not VisualElement focusedElement)
        {
            return false;
        }

        return IsSameElementOrAncestor(element, focusedElement);
    }

    private static void BlurFocusedElementWithin(VisualElement element)
    {
        if (element.panel?.focusController?.focusedElement is not VisualElement focusedElement ||
            !IsSameElementOrAncestor(element, focusedElement))
        {
            return;
        }

        focusedElement.Blur();
    }

    internal static bool IsSameElementOrAncestor(VisualElement possibleAncestor, VisualElement element)
    {
        VisualElement? current = element;
        while (current != null)
        {
            if (current == possibleAncestor)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Callback to recalculate element position when the geometry of an element changes.
    /// </summary>
    /// <param name="evt">The geometry changed event.</param>
    /// <param name="element">The element to position.</param>
    /// <param name="calculatePosition">The callback to calculate the position.</param>
    /// <param name="geometryChanged">The handler itself.</param>
    private static void GeometryChangedHandler(
        GeometryChangedEvent evt,
        VisualElement? element,
        Func<Vector2, Vector2> calculatePosition,
        EventCallback<GeometryChangedEvent> geometryChanged
    )
    {
        if (evt.newRect.width == 0 || evt.newRect.height == 0)
        {
            return;
        }

        Vector2 position = calculatePosition(new Vector2(evt.newRect.width, evt.newRect.height));

        element.style.position = Position.Absolute;
        element.style.left = position.x;
        element.style.top = position.y;

        element.UnregisterCallback(geometryChanged);
    }
}
