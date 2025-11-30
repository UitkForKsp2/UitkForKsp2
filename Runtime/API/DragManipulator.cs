using System;
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
    private Vector2 _mouseOffsetInTarget;
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

    private static Type _textInput = Type.GetType("UnityEngine.UIElements.TextField+TextInput, UnityEngine.UIElementsModule")!;

    /// <summary>
    /// Handles the initiation of the dragging process.
    /// </summary>
    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!IsEnabled
            || evt.target is TextField
            || evt.target.GetType() == _textInput
            || (evt.target is TextElement te && te.parent?.GetType() == _textInput))
            return;

        _mouseOffsetInTarget = (Vector2)evt.position - _target.worldBound.position;

        _mode = _target.pickingMode;
        _target.pickingMode = PickingMode.Ignore;

        IsDragging = true;
        _target.CapturePointer(evt.pointerId);
        evt.StopImmediatePropagation();
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

        Vector2 desiredTopLeftInPanel = (Vector2)evt.position - _mouseOffsetInTarget;

        Rect panelRect = _target.panel?.visualTree.contentRect ?? new Rect(0, 0, Screen.width, Screen.height);

        Vector2 size = _target.worldBound.size;

        if (!AllowDraggingOffScreen)
        {
            float maxX = Mathf.Max(0, panelRect.width  - size.x);
            float maxY = Mathf.Max(0, panelRect.height - size.y);
            desiredTopLeftInPanel.x = Mathf.Clamp(desiredTopLeftInPanel.x, panelRect.xMin, panelRect.xMin + maxX);
            desiredTopLeftInPanel.y = Mathf.Clamp(desiredTopLeftInPanel.y, panelRect.yMin, panelRect.yMin + maxY);
        }

        VisualElement? parent = _target.parent ?? _target.hierarchy.parent;
        if (parent != null)
        {
            Vector2 parentLocal = parent.WorldToLocal(new Vector2(desiredTopLeftInPanel.x, desiredTopLeftInPanel.y));

            _target.style.position = Position.Absolute;
            _target.style.left = parentLocal.x;
            _target.style.top  = parentLocal.y;

            _target.transform.position = Vector3.zero;
        }

        evt.StopImmediatePropagation();
    }

    /// <summary>
    /// Handles the end of the dragging process.
    /// </summary>
    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!IsDragging)
        {
            return;
        }

        IsDragging = false;
        _target.ReleasePointer(evt.pointerId);
        _target.pickingMode = _mode;
        evt.StopImmediatePropagation();
    }
}