using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// A manipulator to resize UI Toolkit elements from the lower-right corner.
/// </summary>
public class ResizeManipulator : IManipulator
{
    private const float DefaultMinWidth = 160f;
    private const float DefaultMinHeight = 120f;

    private readonly ResizeOptions _options;
    private VisualElement? _target;
    private VisualElement? _handle;
    private readonly VisualElement? _providedHandle;
    private bool _callbacksRegistered;
    private bool _hasPointerCapture;
    private Vector2 _startPointerPosition;
    private Vector2 _startSize;
    private PickingMode _pickingMode;
    private int _pointerId;

    /// <summary>
    /// Indicates whether the element is currently being resized.
    /// </summary>
    public bool IsResizing { get; private set; }

    /// <summary>
    /// Enables or disables resizing.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The target element that will be made resizable.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            _target = value;
            EnsureHandle();
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ResizeManipulator"/> class.
    /// </summary>
    /// <param name="options">Resize behavior options.</param>
    public ResizeManipulator(ResizeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ResizeManipulator"/> class using an existing handle element.
    /// </summary>
    /// <param name="options">Resize behavior options.</param>
    /// <param name="handle">The handle element that starts resizing.</param>
    public ResizeManipulator(ResizeOptions options, VisualElement handle)
    {
        _options = options;
        _providedHandle = handle;
    }

    private void EnsureHandle()
    {
        if (_target == null)
        {
            return;
        }

        _handle ??= _providedHandle ?? new VisualElement { name = "window-resize-handle" };
        _handle.AddToClassList("window-resize-handle");
        EnsureHandleIcon(_handle);
        if (_handle.parent == null)
        {
            _target.Add(_handle);
        }

        if (_callbacksRegistered)
        {
            return;
        }

        _handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
        _handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
        _handle.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        _handle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        _callbacksRegistered = true;
    }

    private static void EnsureHandleIcon(VisualElement handle)
    {
        if (handle.Q<VisualElement>("window-resize-handle-icon") != null)
        {
            return;
        }

        var icon = new VisualElement
        {
            name = "window-resize-handle-icon",
            pickingMode = PickingMode.Ignore
        };
        icon.AddToClassList("window-resize-handle-icon");
        handle.Add(icon);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!IsEnabled || _target == null || _handle == null)
        {
            return;
        }

        _pointerId = evt.pointerId;
        _startPointerPosition = evt.position;
        _startSize = _target.worldBound.size;
        _pickingMode = _target.pickingMode;
        _target.pickingMode = PickingMode.Ignore;
        _handle.CapturePointer(evt.pointerId);
        _hasPointerCapture = true;
        IsResizing = true;
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (IsResizing && evt.pointerId == _pointerId && evt.pressedButtons == 0)
        {
            EndInteraction(evt.pointerId);
            evt.StopPropagation();
            return;
        }

        if (!IsResizing || evt.pointerId != _pointerId || _target == null)
        {
            return;
        }

        Vector2 delta = (Vector2)evt.position - _startPointerPosition;
        float width = Mathf.Max(GetMinWidth(), _startSize.x + delta.x);
        float height = Mathf.Max(GetMinHeight(), _startSize.y + delta.y);

        if (_options.CheckScreenBounds)
        {
            Rect panelRect = _target.panel?.visualTree.contentRect ?? new Rect(0, 0, Screen.width, Screen.height);
            width = Mathf.Min(width, Mathf.Max(GetMinWidth(), panelRect.xMax - _target.worldBound.xMin));
            height = Mathf.Min(height, Mathf.Max(GetMinHeight(), panelRect.yMax - _target.worldBound.yMin));
        }

        _target.style.width = width;
        _target.style.height = height;
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        EndInteraction(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerCancel(PointerCancelEvent evt)
    {
        EndInteraction(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        if (evt.pointerId == _pointerId)
        {
            ResetInteractionState();
        }
    }

    private void EndInteraction(int pointerId)
    {
        if (pointerId != _pointerId || !IsResizing || _target == null || _handle == null)
        {
            return;
        }

        if (_hasPointerCapture && _handle.HasPointerCapture(pointerId))
        {
            _handle.ReleasePointer(pointerId);
        }

        ResetInteractionState();
    }

    private void ResetInteractionState()
    {
        if (IsResizing && _target != null)
        {
            _target.pickingMode = _pickingMode;
        }

        IsResizing = false;
        _hasPointerCapture = false;
    }

    private float GetMinWidth()
    {
        if (_options.MinWidth > 0)
        {
            return _options.MinWidth;
        }

        return DefaultMinWidth;
    }

    private float GetMinHeight()
    {
        if (_options.MinHeight > 0)
        {
            return _options.MinHeight;
        }

        return DefaultMinHeight;
    }
}
