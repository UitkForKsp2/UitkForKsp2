using ReduxLib.GameInterfaces;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

/// <summary>
/// Allows hiding a VisualElement with the F2 key.
/// </summary>
public class HideManipulator : IManipulator
{
    private bool _alreadySavedVisibility;
    private Visibility _originalVisibility;

    private VisualElement _target;

    /// <summary>
    /// The target element that will be hidden.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            if (!IInputManager.Instance.Ready)
            {
                UitkForKsp2Plugin.Logger.LogError("HideManipulator: GameManager.Instance is null.");
                return;
            }
            _originalVisibility = value.style.visibility.value;
            IInputManager.Instance.BindHideAction(ToggleHidden);

            _target = value;
        }
    }

    private void ToggleHidden(bool hide)
    {
        if (!hide)
        {
            _alreadySavedVisibility = false;
            _target.style.visibility = _originalVisibility;
        }
        else if (!_alreadySavedVisibility)
        {
            _alreadySavedVisibility = true;
            _originalVisibility = _target.style.visibility.value;
            Extensions.NotifyElementHidden(_target);
            _target.style.visibility = Visibility.Hidden;
            Extensions.NotifyElementHidden(_target);
        }
    }
}