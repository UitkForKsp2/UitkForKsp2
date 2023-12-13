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

    private static MouseCheckCoroutine _coroutine;

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

            if (_coroutine is not { IsRunning: true })
            {
                Log("\tStarting mouse button state check coroutine");
                _coroutine = new MouseCheckCoroutine(element).Start();
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

            if (_coroutine is { IsRunning: true })
            {
                Log("\tStopping mouse button state check coroutine");
                _coroutine.Stop();
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

                if (_coroutine is { IsRunning: true })
                {
                    Log("\tStopping mouse button state check coroutine");
                    _coroutine.Stop();
                }
                else
                {
                    Log("\tCoroutine not running, not stopping it");
                }
            }, TrickleDown.TrickleDown);
        }
    }

    private static void ReleaseTextFieldMouse(TextField textField)
    {
        textField.Blur();
        textField.ReleaseMouse();
        textField.textInput.Blur();
        textField.textInput.ReleaseMouse();
        textField.textInput.textElement.Blur();
        textField.textInput.textElement.ReleaseMouse();
    }

    private class MouseCheckCoroutine
    {
        private VisualElement _target;
        public MouseCheckCoroutine(VisualElement target = null)
        {
            _target = target;
        }

        public bool IsRunning { get; private set; }

        private Coroutine _coroutineImpl;

        public MouseCheckCoroutine Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Coroutine is already running");
            }

            _coroutineImpl = CoroutineUtil.Instance.StartCoroutine(CheckMouseButtonState());

            return this;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Coroutine is not running");
            }

            CoroutineUtil.Instance.StopCoroutine(_coroutineImpl);
            IsRunning = false;
        }

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
                var targetBound = _target.worldBound;

                if (targetBound.Contains(mousePosition))
                {
                    continue;
                }

                Log("\t\t\tMouse position outside of element");

                Log("\t\t\tBlurring element");
                _target.Blur();
                _target.ReleaseMouse();

                if (_target is TextField { multiline: false } textField)
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

    private static void Log(string message)
    {
        #if !RELEASE
        Debug.Log(message);
        #endif
    }
}