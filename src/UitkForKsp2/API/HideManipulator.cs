using KSP.Game;
using KSP.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API;

/// <summary>
/// Allows hiding a VisualElement with the F2 key.
/// </summary>
public class HideManipulator : IManipulator
{
    private bool _isHidden;
    private DisplayStyle _originalDisplayStyle;

    private VisualElement _target;

    /// <summary>
    /// The target element that will be hidden.
    /// </summary>
    public VisualElement target
    {
        get => _target;
        set
        {
            var game = GameManager.Instance.Game;
            if (game.InputManager.TryGetInputDefinition<GlobalInputDefinition>(out var definition))
            {
                Debug.Log("HideManipulator: Binding ToggleHidden to GlobalInputDefinition");
                definition.BindAction(game.Input.Global.ToggleUIVisibility.name, ToggleHidden);
            }
            _target = value;
        }
    }

    private void ToggleHidden()
    {
        if (_isHidden)
        {
            Debug.Log("HideManipulator: Unhiding.");
            _isHidden = false;
            _target.style.display = _originalDisplayStyle;
        }
        else
        {
            Debug.Log("HideManipulator: Hiding.");
            _isHidden = true;
            _originalDisplayStyle = _target.style.display.value;
            _target.style.display = DisplayStyle.None;
        }
    }
}