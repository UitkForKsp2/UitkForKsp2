using UitkForKsp2;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A manipulator to make UI Toolkit elements draggable within the screen bounds.
/// </summary>
public class DragManipulator : IManipulator
{
    private VisualElement _target;
    private Vector3 _offset;
    private PickingMode _mode;

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
        if (!IsEnabled || evt.target is TextElement { parent: TextField.TextInput })
        {
            return;
        }

        _mode = target.pickingMode;
        target.pickingMode = PickingMode.Ignore;
        IsDragging = true;
        _offset = evt.localPosition;
        _target.CapturePointer(evt.pointerId);
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

        var delta = evt.localPosition - _offset;
        var newPosition = target.transform.position + delta;

        if (!AllowDraggingOffScreen)
        {
            newPosition.x = Mathf.Clamp(
                newPosition.x,
                0,
                Configuration.CurrentScreenWidth - _target.resolvedStyle.width
            );
            newPosition.y = Mathf.Clamp(
                newPosition.y,
                0,
                Configuration.CurrentScreenHeight - _target.resolvedStyle.height
            );
        }

        _target.transform.position = newPosition;
    }

    /// <summary>
    /// Handles the end of the dragging process.
    /// </summary>
    private void OnPointerUp(PointerUpEvent evt)
    {
        IsDragging = false;
        _target.ReleasePointer(evt.pointerId);
        target.pickingMode = _mode;
    }
}