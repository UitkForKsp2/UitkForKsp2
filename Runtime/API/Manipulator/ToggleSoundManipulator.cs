using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class ToggleSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private readonly string _hoverSound;
    private readonly string _toggleSound;

    public ToggleSoundManipulator(
        string hoverSound = "Play_ui_extended_toggle_hover",
        string toggleSound = "Play_ui_extended_stagging_add_select"
    )
    {
        _hoverSound = hoverSound;
        _toggleSound = toggleSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(PlayHoverSound);
        target.RegisterCallback<MouseUpEvent>(PlayClickSound);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(PlayHoverSound);
        target.UnregisterCallback<MouseUpEvent>(PlayClickSound);
    }

    private void PlayHoverSound(MouseEnterEvent e)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_hoverSound, target);
    }

    private void PlayClickSound(MouseUpEvent e)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_toggleSound, target);
    }
}
