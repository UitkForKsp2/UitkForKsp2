using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class HoverSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private readonly string _hoverSound;

    public HoverSoundManipulator(string hoverSound = "Play_ui_extended_toggle_hover")
    {
        _hoverSound = hoverSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(PlayHoverSound);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(PlayHoverSound);
    }

    private void PlayHoverSound(MouseEnterEvent e)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_hoverSound, target);
    }
}
