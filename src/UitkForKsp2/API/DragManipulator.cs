using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A manipulator to make UI Toolkit elements draggable within the screen bounds.
/// </summary>
public class DragManipulator : IManipulator
{
    private VisualElement _target;
    private Vector2 _offset;
    private PickingMode _mode;
    private IVisualElementScheduledItem _scheduledItem;

    /// <summary>
    /// Indicates whether the element is currently being dragged.
    /// </summary>
    public bool IsDragging { get; private set; }

    /// <summary>
    /// Enables or disables the dragging functionality.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Indicates whether the element can be dragged off screen.
    /// </summary>
    public bool AllowDraggingOffScreen { get; set; }

    /// <summary>
    /// The target element that will be made draggable.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            _target = value;
            _target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DragManipulator"/> class.
    /// </summary>
    /// <param name="allowDraggingOffScreen">Allow dragging off screen?</param>
    public DragManipulator(bool allowDraggingOffScreen = false)
    {
        AllowDraggingOffScreen = allowDraggingOffScreen;
    }

    /// <summary>
    /// Handles the initiation of the dragging process.
    /// </summary>
    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!IsEnabled || evt.target is TextField.TextInput)
        {
            evt.StopPropagation();
            return;
        }

        _mode = target.pickingMode;
        target.pickingMode = PickingMode.Ignore;
        IsDragging = true;
        _offset = evt.localPosition;
        _target.CapturePointer(evt.pointerId);

        // Schedule a recurring check for the mouse button state
        _scheduledItem = _target.schedule.Execute(CheckMouseButtonState).Every(100);
    }

    private void CheckMouseButtonState()
    {
        if (!Input.GetMouseButton(0))
        {
            OnPointerUp(null);
        }
    }

    /// <summary>
    /// Handles the movement of the draggable element.
    /// </summary>
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!IsDragging || !IsEnabled)
        {
            return;
        }

        var newPosition = new Vector2(evt.position.x, evt.position.y) - _offset;

        if (!AllowDraggingOffScreen)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width - _target.resolvedStyle.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height - _target.resolvedStyle.height);
        }

        _target.transform.position = new Vector3(newPosition.x, newPosition.y, _target.transform.position.z);
    }

    /// <summary>
    /// Handles the end of the dragging process.
    /// </summary>
    private void OnPointerUp(PointerUpEvent evt)
    {
        IsDragging = false;
        _target.ReleasePointer(evt.pointerId);
        target.pickingMode = _mode;

        // Cancel the scheduled mouse button state check
        _scheduledItem?.Pause();
        _scheduledItem = null;
    }
}