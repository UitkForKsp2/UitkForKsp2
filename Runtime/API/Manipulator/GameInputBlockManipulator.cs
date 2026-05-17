using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// Blocks gameplay input while the pointer is interacting with a UI Toolkit window.
/// </summary>
public class GameInputBlockManipulator : IManipulator
{
    private VisualElement? _target;
    private IVisualElementScheduledItem? _stateCheck;
    private bool _hasLock;
    private bool _isPointerOver;
    private bool _isPointerDown;
    private int _pointerId;

    /// <summary>
    /// The target element whose pointer bounds block gameplay input.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            if (_target != null)
            {
                UnregisterCallbacks(_target);
            }

            ResetInteractionState();
            _target = value;
            RegisterCallbacks();
        }
    }

    private void RegisterCallbacks()
    {
        if (_target == null)
        {
            return;
        }

        _target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        _target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        _target.RegisterCallback<PointerDownEvent>(OnPointerDownTrickle, TrickleDown.TrickleDown);
        _target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        _target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        _target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        _target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        _target.RegisterCallback<WheelEvent>(OnWheelTrickle, TrickleDown.TrickleDown);
        _target.RegisterCallback<WheelEvent>(OnWheel);
        _target.RegisterCallback<ClickEvent>(OnClick);
        _target.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        _target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        Extensions.ElementHidden += OnElementHidden;
        StartStateCheck();
    }

    private void UnregisterCallbacks(VisualElement target)
    {
        target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        target.UnregisterCallback<PointerDownEvent>(OnPointerDownTrickle, TrickleDown.TrickleDown);
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        target.UnregisterCallback<WheelEvent>(OnWheelTrickle, TrickleDown.TrickleDown);
        target.UnregisterCallback<WheelEvent>(OnWheel);
        target.UnregisterCallback<ClickEvent>(OnClick);
        target.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        Extensions.ElementHidden -= OnElementHidden;
        _stateCheck?.Pause();
        _stateCheck = null;
    }

    private void OnPointerEnter(PointerEnterEvent evt)
    {
        CheckPointerState();
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        _isPointerOver = false;
        if (!_isPointerDown)
        {
            ReleaseLock();
        }
    }

    private void OnPointerDownTrickle(PointerDownEvent evt)
    {
        AcquireLockIfPointerOverTarget();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!AcquireLockIfPointerOverTarget())
        {
            _isPointerDown = false;
            return;
        }

        _pointerId = evt.pointerId;
        _isPointerDown = true;
        evt.StopPropagation();
        evt.PreventDefault();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        bool wasPointerDown = _isPointerDown && evt.pointerId == _pointerId;
        if (wasPointerDown)
        {
            _isPointerDown = false;
            if (!_isPointerOver)
            {
                ReleaseLock();
            }
        }

        if (wasPointerDown || IsPointerOverTarget(_target))
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnPointerCancel(PointerCancelEvent evt)
    {
        bool wasPointerDown = _isPointerDown && evt.pointerId == _pointerId;
        if (wasPointerDown)
        {
            _isPointerDown = false;
            if (!_isPointerOver)
            {
                ReleaseLock();
            }
        }

        if (wasPointerDown || IsPointerOverTarget(_target))
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        if (evt.pointerId == _pointerId)
        {
            _isPointerDown = false;
            if (!_isPointerOver)
            {
                ReleaseLock();
            }
        }
    }

    private void OnWheelTrickle(WheelEvent evt)
    {
        AcquireLockIfPointerOverTarget();
    }

    private void OnWheel(WheelEvent evt)
    {
        if (!AcquireLockIfPointerOverTarget())
        {
            return;
        }

        evt.StopPropagation();
        evt.PreventDefault();
        _target?.schedule.Execute(() =>
        {
            if (!_isPointerOver && !_isPointerDown)
            {
                ReleaseLock();
            }
        });
    }

    private void OnClick(ClickEvent evt)
    {
        if (IsPointerOverTarget(_target))
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        StartStateCheck();
        CheckPointerState();
    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
        ResetInteractionState();
    }

    private void OnElementHidden(VisualElement hiddenElement)
    {
        if (_target != null && Extensions.IsSameElementOrAncestor(hiddenElement, _target))
        {
            ResetInteractionState();
        }
    }

    private void StartStateCheck()
    {
        _stateCheck?.Pause();
        VisualElement? scheduleTarget = _target?.panel?.visualTree ?? _target;
        _stateCheck = scheduleTarget?.schedule.Execute(CheckPointerState).Every(100);
    }

    private void CheckPointerState()
    {
        if (_target == null || !IsTargetAvailable(_target))
        {
            ResetInteractionState();
            return;
        }

        _isPointerOver = IsPointerOverTarget(_target);

        if (_isPointerDown && !IsAnyMouseButtonPressed())
        {
            _isPointerDown = false;
        }

        if (_isPointerOver || _isPointerDown)
        {
            AcquireLock();
            return;
        }

        ReleaseLock();
    }

    private bool AcquireLockIfPointerOverTarget()
    {
        if (_target == null || !IsTargetAvailable(_target))
        {
            ResetInteractionState();
            return false;
        }

        _isPointerOver = IsPointerOverTarget(_target);
        if (!_isPointerOver)
        {
            if (!_isPointerDown)
            {
                ReleaseLock();
            }

            return false;
        }

        AcquireLock();
        return true;
    }

    private void ResetInteractionState()
    {
        _isPointerDown = false;
        _isPointerOver = false;
        ReleaseLock();
    }

    private static bool IsAnyMouseButtonPressed()
    {
        return Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
    }

    private static bool IsPointerOverTarget(VisualElement? target)
    {
        if (target == null || !IsTargetAvailable(target))
        {
            return false;
        }

        IPanel panel = target.panel;
        if (panel == null)
        {
            return false;
        }

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, Input.mousePosition);
        if (!target.worldBound.Contains(panelPosition))
        {
            return false;
        }

        VisualElement? pickedElement = panel.Pick(panelPosition);
        if (pickedElement == target)
        {
            return HasVisibleSurface(target);
        }

        return pickedElement != null && Extensions.IsSameElementOrAncestor(target, pickedElement);
    }

    private static bool HasVisibleSurface(VisualElement element)
    {
        IResolvedStyle style = element.resolvedStyle;
        return style.backgroundColor.a > 0f ||
               !style.backgroundImage.IsEmpty() ||
               style.borderBottomWidth > 0f && style.borderBottomColor.a > 0f ||
               style.borderLeftWidth > 0f && style.borderLeftColor.a > 0f ||
               style.borderRightWidth > 0f && style.borderRightColor.a > 0f ||
               style.borderTopWidth > 0f && style.borderTopColor.a > 0f;
    }

    private static bool IsTargetAvailable(VisualElement target)
    {
        if (target.panel == null || target.worldBound.width <= 0 || target.worldBound.height <= 0)
        {
            return false;
        }

        VisualElement? element = target;
        while (element != null)
        {
            if (element.resolvedStyle.display == DisplayStyle.None ||
                element.resolvedStyle.visibility == Visibility.Hidden)
            {
                return false;
            }

            element = element.parent;
        }

        return true;
    }

    private void AcquireLock()
    {
        if (_hasLock)
        {
            return;
        }

        _hasLock = Extensions.SetGameInputDisabled(this, true);
    }

    private void ReleaseLock()
    {
        if (!_hasLock)
        {
            return;
        }

        _hasLock = false;
        Extensions.SetGameInputDisabled(this, false);
    }
}
