using System.Collections;
using KSP.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Extension methods for UIDocument and VisualElements.
/// </summary>
[PublicAPI]
public static class Extensions
{
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
        element.SetProperty(name, value);
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
    public static T AddChildren<T>(this T element, IEnumerable<VisualElement> children) where T : VisualElement
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
        element.style.display = DisplayStyle.None;
        return element;
    }

    /// <summary>
    /// Toggle the display of a VisualElement between DisplayStyle.Flex and DisplayStyle.None.
    /// </summary>
    /// <param name="element">The element to toggle the display of.</param>
    /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
    /// <returns>The element which was toggled.</returns>
    public static T ToggleDisplay<T>(this T element) where T : VisualElement
    {
        element.style.display = element.style.display == DisplayStyle.None
            ? DisplayStyle.Flex
            : DisplayStyle.None;
        return element;
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
    /// Set the default position of an element using a callback to calculate the position.
    /// </summary>
    /// <param name="element">The element to position.</param>
    /// <param name="calculatePosition">
    /// The callback which will be called when the element is resized. The callback will be passed the size of the
    /// element and should return the position to set the element to in the reference resolution.
    /// </param>
    public static void SetDefaultPosition(this VisualElement element, Func<Vector2, Vector2> calculatePosition)
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
        element.SetDefaultPosition(windowSize => new Vector2(
            (Configuration.CurrentScreenWidth - windowSize.x) / 2,
            (Configuration.CurrentScreenHeight - windowSize.y) / 2
        ));
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
    /// <param name="element"></param>
    public static void DisableGameInputOnFocus(this VisualElement element)
    {
        element.RegisterCallback<FocusInEvent>(_ =>
        {
            Log($"FocusInEvent: {element.GetType().Name} {element.name}");
            Log("\tDisabling game input");
            element.ReleaseMouse();
            GameManager.Instance?.Game?.Input.Disable();

            if (_mouseCheckCoroutine is not { IsRunning: true })
            {
                Log("\tStarting mouse button state check coroutine");
                _mouseCheckCoroutine = new MouseCheckCoroutine(element).Start();
            }
            else
            {
                Log("\tCoroutine already running, not starting another one");
            }
        });
        element.RegisterCallback<FocusOutEvent>(_ =>
        {
            Log($"FocusOutEvent: {element.GetType().Name} {element.name}");
            Log("\tEnabling game input");
            GameManager.Instance?.Game?.Input.Enable();

            if (_mouseCheckCoroutine is { IsRunning: true })
            {
                Log("\tStopping mouse button state check coroutine");
                _mouseCheckCoroutine.Stop();
            }
            else
            {
                Log("\tCoroutine not running, not stopping it");
            }
        });

        if (element is TextField { multiline: false } textField)
        {
            textField.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter))
                {
                    return;
                }

                Log($"KeyUpEvent Enter: {textField.GetType().Name} {textField.name}");
                Log("\tReleasing mouse");

                ReleaseTextFieldMouse(textField);

                if (_mouseCheckCoroutine is { IsRunning: true })
                {
                    Log("\tStopping mouse button state check coroutine");
                    _mouseCheckCoroutine.Stop();
                }
                else
                {
                    Log("\tCoroutine not running, not stopping it");
                }
            }, TrickleDown.TrickleDown);
        }
    }

    #endregion

    /// <summary>
    /// The coroutine used by DisableGameInputOnFocus.
    /// </summary>
    private static MouseCheckCoroutine _mouseCheckCoroutine;

    /// <summary>
    /// Callback to recalculate element position when the geometry of an element changes.
    /// </summary>
    /// <param name="evt">The geometry changed event.</param>
    /// <param name="element">The element to position.</param>
    /// <param name="calculatePosition">The callback to calculate the position.</param>
    /// <param name="geometryChanged">The handler itself.</param>
    private static void GeometryChangedHandler(
        GeometryChangedEvent evt,
        VisualElement element,
        Func<Vector2, Vector2> calculatePosition,
        EventCallback<GeometryChangedEvent> geometryChanged
    )
    {
        if (evt.newRect.width == 0 || evt.newRect.height == 0)
        {
            return;
        }

        element.transform.position = calculatePosition(new Vector2(evt.newRect.width, evt.newRect.height));
        element.UnregisterCallback(geometryChanged);
    }

    /// <summary>
    /// Release the focus and the mouse pointer capture from a text field.
    /// </summary>
    /// <param name="textField">The text field to release from.</param>
    private static void ReleaseTextFieldMouse(TextField textField)
    {
        textField.Blur();
        textField.ReleaseMouse();
        textField.textInput.Blur();
        textField.textInput.ReleaseMouse();
        textField.textInput.textElement.Blur();
        textField.textInput.textElement.ReleaseMouse();
    }

    /// <summary>
    /// Coroutine which checks the state of the mouse button and releases the pointer and blurs the target element
    /// if the left mouse button is clicked outside of the target element.
    /// </summary>
    /// <param name="target">The target element to check.</param>
    private class MouseCheckCoroutine(VisualElement target = null)
    {
        public bool IsRunning { get; private set; }

        private Coroutine _coroutineImpl;

        /// <summary>
        /// Start the coroutine.
        /// </summary>
        /// <returns>The started coroutine.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the coroutine is already running.</exception>
        public MouseCheckCoroutine Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Coroutine is already running");
            }

            _coroutineImpl = CoroutineUtil.Instance.StartCoroutine(CheckMouseButtonState());

            return this;
        }

        /// <summary>
        /// Stop the coroutine.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the coroutine is not running.</exception>
        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Coroutine is not running");
            }

            CoroutineUtil.Instance.StopCoroutine(_coroutineImpl);
            IsRunning = false;
        }

        /// <summary>
        /// The actual coroutine implementation.
        /// </summary>
        /// <returns>IEnumerator for the coroutine.</returns>
        private IEnumerator CheckMouseButtonState()
        {
            Log("Coroutine starting...");
            if (IsRunning)
            {
                Log("Coroutine already running, exiting");
                yield break;
            }

            IsRunning = true;
            Log("Coroutine started");

            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                Log("\tChecking mouse button state");

                if (!Input.GetMouseButton((int)MouseButton.LeftMouse))
                {
                    continue;
                }

                Log("\t\tMouse button clicked");

                // Check if current mouse position is outside of the position of element
                // If so, then blur the text field
                var mousePosition = Configuration.GetAdjustedMousePosition();
                var targetBound = target.worldBound;

                if (targetBound.Contains(mousePosition))
                {
                    continue;
                }

                Log("\t\t\tMouse position outside of element");

                Log("\t\t\tBlurring element");
                target.Blur();
                target.ReleaseMouse();

                if (target is TextField { multiline: false } textField)
                {
                    Log("\t\t\tTarget is text field, calling ReleaseTextFieldMouse");
                    ReleaseTextFieldMouse(textField);
                }

                break;
            }

            Log("\tCoroutine ending...");
            IsRunning = false;
        }
    }

    /// <summary>
    /// Log a message to the console only in debug builds.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private static void Log(string message)
    {
#if !RELEASE
        Debug.Log(message);
#endif
    }
}