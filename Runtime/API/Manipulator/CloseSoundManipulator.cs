using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class CloseSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private Button Button => (Button)target;

    private readonly string _hoverSound;
    private readonly string _clickSound;

    public CloseSoundManipulator(
        string clickSound = "Play_ui_extended_button_select",
        string hoverSound = "Play_ui_extended_button_hover"
    )
    {
        _clickSound = clickSound;
        _hoverSound = hoverSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        if (_clickSound != null)
        {
            Button.clicked += PlayClickSound;
            Button.RegisterCallback<MouseEnterEvent>(PlayHoverSound);
        }
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        if (_clickSound != null)
        {
            Button.clicked -= PlayClickSound;
            Button.UnregisterCallback<MouseEnterEvent>(PlayHoverSound);
        }
    }

    private void PlayClickSound()
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_clickSound, target);
    }

    private void PlayHoverSound(MouseEnterEvent e)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_hoverSound, target);
    }
}
