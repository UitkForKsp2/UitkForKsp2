using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// A manipulator to fix broken TextElement events and mouse capturing.
/// </summary>
public class FixTextElementManipulator : IManipulator
{
    private VisualElement _target;

    /// <summary>
    /// The target root element of the window to apply this fix to.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            if (_target != null)
            {
                _target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                _target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                _target.UnregisterCallback<FocusInEvent>(OnFocusIn);
                _target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            }

            _target = value;
            _target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _target.RegisterCallback<FocusInEvent>(OnFocusIn);
            _target.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }
    }

    private TextElement _currentTarget;
    private IVisualElementScheduledItem _scheduledItem;
    private void OnPointerDown(PointerDownEvent evt)
    {
        Debug.Log("\tOnPointerDown");
        Debug.Log($"\t\ttarget: {evt.target.GetType().Name}");
        Debug.Log($"\t\t\tHasMouseCapture: {evt.target.HasMouseCapture()}");
        Debug.Log($"\t\t\tHasBubbleUpHandlers: {evt.target.HasBubbleUpHandlers()}");
        Debug.Log($"\t\t\tHasTrickleDownHandlers: {evt.target.HasTrickleDownHandlers()}");

        if (evt.target is TextField.TextInput)
        {
            evt.PreventDefault();
            evt.StopPropagation();
            return;
        }

        if (evt.target is TextElement { parent: TextField.TextInput } element && element.HasMouseCapture() )
        {
            Debug.Log("...Releasing mouse capture");
            _currentTarget = element;
            _currentTarget.ReleaseMouse();
            _scheduledItem = _target.schedule.Execute(CheckMouseButtonState).Every(100);
        }
    }

    private void CheckMouseButtonState()
    {
        Debug.Log("...Checking mouse button state");

        if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            Debug.Log("\tMouse button clicked");

            // Check if current mouse position is outside of the position of _currentTarget
            // If so, then blur the text field
            var mousePosition = Configuration.GetAdjustedMousePosition();
            var targetBound = _currentTarget.worldBound;

            if (!targetBound.Contains(mousePosition))
            {
                _currentTarget.Blur();

                _scheduledItem.Pause();
                _scheduledItem = null;
            }
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        Debug.Log("\tOnPointerDown");
        Debug.Log($"\t\ttarget: {evt.target.GetType().Name}");
        Debug.Log($"\t\t\tHasMouseCapture: {evt.target.HasMouseCapture()}");
        Debug.Log($"\t\t\tHasBubbleUpHandlers: {evt.target.HasBubbleUpHandlers()}");
        Debug.Log($"\t\t\tHasTrickleDownHandlers: {evt.target.HasTrickleDownHandlers()}");
    }

    private void OnFocusIn(FocusInEvent evt)
    {
        Debug.Log("\tOnFocusIn");
        Debug.Log($"\t\ttarget: {evt.target.GetType().Name}");
        Debug.Log($"\t\t\tHasMouseCapture: {evt.target.HasMouseCapture()}");
        Debug.Log($"\t\t\tHasBubbleUpHandlers: {evt.target.HasBubbleUpHandlers()}");
        Debug.Log($"\t\t\tHasTrickleDownHandlers: {evt.target.HasTrickleDownHandlers()}");
    }

    private void OnFocusOut(FocusOutEvent evt)
    {
        Debug.Log("\tOnFocusOut");
        Debug.Log($"\t\ttarget: {evt.target.GetType().Name}");
        Debug.Log($"\t\t\tHasMouseCapture: {evt.target.HasMouseCapture()}");
        Debug.Log($"\t\t\tHasBubbleUpHandlers: {evt.target.HasBubbleUpHandlers()}");
        Debug.Log($"\t\t\tHasTrickleDownHandlers: {evt.target.HasTrickleDownHandlers()}");
    }
}