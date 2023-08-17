using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// A simple drag manipulator for UIElements.
/// </summary>
[PublicAPI]
public class DragManipulator : IManipulator
{
    private VisualElement _target;
    private Vector3 _offset;
    private PickingMode _mode;

    /// <summary>
    /// Is the target currently being dragged?
    /// </summary>
    public bool Dragging { get; set; }

    /// <summary>
    /// Is dragging currently enabled?
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Allow dragging off screen?
    /// </summary>
    public bool AllowDraggingOffScreen { get; set; }

    /// <summary>
    /// The target element to drag.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            _target = value;

            _target.RegisterCallback<PointerDownEvent>(DragBegin);
            _target.RegisterCallback<PointerUpEvent>(DragEnd);
            _target.RegisterCallback<PointerMoveEvent>(PointerMove);
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DragManipulator"/> class.
    /// </summary>
    /// <param name="allowDraggingOffScreen">Allow dragging off screen?</param>
    public DragManipulator(bool allowDraggingOffScreen = true)
    {
        AllowDraggingOffScreen = allowDraggingOffScreen;
    }

    private void DragBegin(PointerDownEvent evt)
    {
        if (evt.target is TextField.TextInput)
        {
            evt.StopPropagation();
            return;
        }

        _mode = target.pickingMode;
        target.pickingMode = PickingMode.Ignore;
        _offset = evt.localPosition;
        Dragging = true;
        target.CapturePointer(evt.pointerId);
    }

    private void DragEnd(PointerUpEvent evt)
    {
        target.ReleasePointer(evt.pointerId);
        Dragging = false;
        target.pickingMode = _mode;
    }

    private void PointerMove(PointerMoveEvent evt)
    {
        if (!IsEnabled || !Dragging)
        {
            return;
        }

        var delta = evt.localPosition - _offset;
        var newPosition = target.transform.position + delta;

        if (!AllowDraggingOffScreen)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width - target.resolvedStyle.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height - target.resolvedStyle.height);
        }

        target.transform.position = newPosition;
    }
}