using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// A manipulator to make UI Toolkit elements draggable within the screen bounds.
/// </summary>
public class DragManipulator : IManipulator
{
    private const float DragThresholdPx = 6f;

    private VisualElement? _handle;

    private VisualElement? _dragTarget;

    private Vector2 _mouseOffsetInTarget;

    private PickingMode _mode;

    private Vector2 _pointerDownPos;
    private bool _pendingDrag;
    private int _pointerId;
    private bool _hasPointerCapture;

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
        get => _handle;
        set
        {
            if (_handle != null)
            {
                _handle.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                _handle.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                _handle.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                _handle.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
                _handle.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            }

            _handle = value;
            _dragTarget ??= value;
            _handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _handle.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            _handle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
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
    /// Creates a new instance of the <see cref="DragManipulator"/> class that moves a separate element.
    /// </summary>
    /// <param name="dragTarget">Element to move when the handle is dragged.</param>
    /// <param name="allowDraggingOffScreen">Allow dragging off screen?</param>
    public DragManipulator(VisualElement dragTarget, bool allowDraggingOffScreen = false)
        : this(allowDraggingOffScreen)
    {
        _dragTarget = dragTarget;
    }

    private static Type _textInput = Type.GetType("UnityEngine.UIElements.TextField+TextInput, UnityEngine.UIElementsModule")!;

    /// <summary>
    /// Handles the initiation of the dragging process.
    /// </summary>
    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!IsEnabled
            || IsDragBlockedTarget(evt.target))
        {
            return;
        }

        _pointerDownPos = evt.position;
        _pointerId = evt.pointerId;
        VisualElement dragTarget = _dragTarget ?? _handle;
        _mouseOffsetInTarget = (Vector2)evt.position - dragTarget.worldBound.position;

        _pendingDrag = true;
        _handle.CapturePointer(evt.pointerId);
        _hasPointerCapture = true;
    }

    /// <summary>
    /// Handles the movement of the draggable element.
    /// </summary>
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!IsEnabled)
        {
            EndInteraction(evt.pointerId, true);
            return;
        }

        if ((_pendingDrag || IsDragging) && evt.pointerId == _pointerId && evt.pressedButtons == 0)
        {
            EndInteraction(evt.pointerId, true);
            return;
        }

        if (_pendingDrag && !IsDragging)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            Vector2 delta = (Vector2)evt.position - _pointerDownPos;
            if (delta.sqrMagnitude < DragThresholdPx * DragThresholdPx)
            {
                return;
            }

            _pendingDrag = false;
            IsDragging = true;

            _mode = _handle.pickingMode;
            _handle.pickingMode = PickingMode.Ignore;
        }

        if (!IsDragging)
        {
            return;
        }

        VisualElement dragTarget = _dragTarget ?? _handle;
        Vector2 desiredTopLeftInPanel = (Vector2)evt.position - _mouseOffsetInTarget;

        Rect panelRect = dragTarget.panel?.visualTree.contentRect ?? new Rect(0, 0, Screen.width, Screen.height);

        Vector2 size = dragTarget.worldBound.size;

        if (!AllowDraggingOffScreen)
        {
            float maxX = Mathf.Max(0, panelRect.width  - size.x);
            float maxY = Mathf.Max(0, panelRect.height - size.y);
            desiredTopLeftInPanel.x = Mathf.Clamp(desiredTopLeftInPanel.x, panelRect.xMin, panelRect.xMin + maxX);
            desiredTopLeftInPanel.y = Mathf.Clamp(desiredTopLeftInPanel.y, panelRect.yMin, panelRect.yMin + maxY);
        }

        VisualElement? parent = dragTarget.parent ?? dragTarget.hierarchy.parent;
        if (parent == null)
        {
            return;
        }

        Vector2 parentLocal = parent.WorldToLocal(new Vector2(desiredTopLeftInPanel.x, desiredTopLeftInPanel.y));

        dragTarget.style.position = Position.Absolute;
        dragTarget.style.left = parentLocal.x;
        dragTarget.style.top  = parentLocal.y;

        dragTarget.transform.position = Vector3.zero;
    }

    /// <summary>
    /// Handles the end of the dragging process.
    /// </summary>
    private void OnPointerUp(PointerUpEvent evt)
    {
        EndInteraction(evt.pointerId, true);
    }

    private void OnPointerCancel(PointerCancelEvent evt)
    {
        EndInteraction(evt.pointerId, true);
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        if (evt.pointerId == _pointerId)
        {
            ResetInteractionState(true);
        }
    }

    private void EndInteraction(int pointerId, bool restorePickingMode)
    {
        if (pointerId != _pointerId || (!_pendingDrag && !IsDragging && !_hasPointerCapture))
        {
            return;
        }

        if (_hasPointerCapture && _handle.HasPointerCapture(pointerId))
        {
            _handle.ReleasePointer(pointerId);
        }

        ResetInteractionState(restorePickingMode);
    }

    private void ResetInteractionState(bool restorePickingMode)
    {
        if (IsDragging && restorePickingMode)
        {
            _handle.pickingMode = _mode;
        }

        _pendingDrag = false;
        IsDragging = false;
        _hasPointerCapture = false;
    }

    private bool IsDragBlockedTarget(object eventTarget)
    {
        if (eventTarget is not VisualElement element)
        {
            return false;
        }

        while (element != null && element != _handle)
        {
            if (element is Button
                or TextField
                or Toggle
                or Slider
                or SliderInt
                or MinMaxSlider
                or Scroller
                or ScrollView
                or ListView
                || element.GetType() == _textInput)
            {
                return true;
            }

            element = element.parent;
        }

        return false;
    }
}
