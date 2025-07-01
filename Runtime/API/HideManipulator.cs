using ReduxLib.GameInterfaces;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Allows hiding a VisualElement with the F2 key.
/// </summary>
public class HideManipulator : IManipulator
{
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
            _target.style.visibility = _originalVisibility;
        }
        else
        {
            _originalVisibility = _target.style.visibility.value;
            _target.style.visibility = Visibility.Hidden;
        }
    }
}