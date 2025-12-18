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

    private VisualElement? _target;

    private Vector2 _mouseOffsetInTarget;

    private PickingMode _mode;

    private Vector2 _pointerDownPos;
    private bool _pendingDrag;
    private int _pointerId;

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
            || (evt.target is TextElement te && te.parent?.GetType() == _textInput)
            || (evt.target is TextElement { parent: VisualElement } te2 && te2.parent.parent?.GetType() == _textInput))
        {
            return;
        }

        _pointerDownPos = evt.position;
        _pointerId = evt.pointerId;
        _mouseOffsetInTarget = (Vector2)evt.position - _target.worldBound.position;

        _pendingDrag = true;
    }

    /// <summary>
    /// Handles the movement of the draggable element.
    /// </summary>
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!IsEnabled)
        {
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

            _mode = _target.pickingMode;
            _target.pickingMode = PickingMode.Ignore;

            _target.CapturePointer(evt.pointerId);
        }

        if (!IsDragging)
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
    }

    /// <summary>
    /// Handles the end of the dragging process.
    /// </summary>
    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_pendingDrag)
        {
            _pendingDrag = false;
            return;
        }

        if (!IsDragging)
        {
            return;
        }

        IsDragging = false;
        _target.ReleasePointer(evt.pointerId);
        _target.pickingMode = _mode;
    }
}