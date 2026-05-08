using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class ButtonSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private Button Button => (Button)target;

    private readonly string _clickSound;
    private readonly string _clickSoundSelected;

    public ButtonSoundManipulator(
        string clickSound = "Play_ui_extended_toggle_ON",
        string clickSoundSelected = "Play_ui_extended_toggle_OFF"
    )
    {
        _clickSound = clickSound;
        _clickSoundSelected = clickSoundSelected ?? clickSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        if (_clickSound != null)
        {
            Button.clicked += PlayClickSound;
        }
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        if (_clickSound != null)
        {
            Button.clicked -= PlayClickSound;
        }
    }

    private void PlayClickSound()
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(
            Button.ClassListContains(DocumentSoundManipulator.SelectedClass)
                ? _clickSoundSelected
                : _clickSound,
            target
        );
    }
}
