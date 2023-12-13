using KSP.Game;
using KSP.Input;
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
            if (GameManager.Instance is not { Game: { InputManager: not null, Input: not null } game })
            {
                UitkForKsp2Plugin.Logger.LogError("HideManipulator: GameManager.Instance is null.");
                return;
            }

            if (game.InputManager.TryGetInputDefinition<GlobalInputDefinition>(out var definition))
            {
                definition.BindAction(game.Input.Global.ToggleUIVisibility.name, ToggleHidden);
            }

            _target = value;
        }
    }

    private void ToggleHidden()
    {
        if (_isHidden)
        {
            _isHidden = false;
            _target.style.display = _originalDisplayStyle;
        }
        else
        {
            _isHidden = true;
            _originalDisplayStyle = _target.style.display.value;
            _target.style.display = DisplayStyle.None;
        }
    }
}