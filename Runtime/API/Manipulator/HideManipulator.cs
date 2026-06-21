using System;
using ReduxLib.GameInterfaces;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// Allows hiding a VisualElement with the F2 key (via the game-view hide action).
/// </summary>
public class HideManipulator : IManipulator
{
    private bool _bound;
    private bool _hidden;

    private VisualElement _target;

    /// <summary>
    /// The target element that will be hidden.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            // Unbind the previous target (e.g. when the manipulator is removed, target is set to null).
            Unbind();

            _target = value;

            // Stay bound for the element's whole lifetime: the game-view hide action (F2) detaches/re-attaches the
            // windows as part of switching to the hidden view, and we must still restore visibility afterward.
            // Unbinding on detach would strand the element hidden. The released-element case (the window was
            // destroyed) is handled defensively in ToggleHidden instead.
            if (_target != null && IInputManager.Instance.Ready)
            {
                IInputManager.Instance.BindHideAction(ToggleHidden);
                _bound = true;
            }
        }
    }

    private void Unbind()
    {
        if (_bound && IInputManager.Instance.Ready)
        {
            IInputManager.Instance.UnbindHideAction(ToggleHidden);
        }

        _bound = false;
        _hidden = false;
    }

    private void ToggleHidden(bool hide)
    {
        if (_target == null || hide == _hidden)
        {
            return;
        }

        try
        {
            if (hide)
            {
                Extensions.NotifyElementHidden(_target);
                _target.style.visibility = Visibility.Hidden;
                Extensions.NotifyElementHidden(_target);
            }
            else
            {
                // Clear our inline override rather than restoring a captured value: the element may have been
                // re-bound (or had another manipulator added) while transiently hidden, so a captured "original"
                // could itself be Hidden. Removing the override lets the element fall back to its natural visibility.
                _target.style.visibility = StyleKeyword.Null;
            }

            _hidden = hide;
        }
        catch (Exception)
        {
            // The target's backing UI data was released (its window was destroyed) while this handler was still
            // bound to the global hide action. Stop responding so released data is never dereferenced again.
            Unbind();
            _target = null;
        }
    }
}