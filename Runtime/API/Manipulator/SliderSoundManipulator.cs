using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class SliderSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private readonly string _startSound;
    private readonly string _endSound;

    public SliderSoundManipulator(
        string startSound = "Play_ui_options_slider_generic",
        string stopSound = "Play_ui_options_slider_generic_stop"
    )
    {
        _startSound = startSound;
        _endSound = stopSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseCaptureEvent>(BeginPlayingSound);
        target.RegisterCallback<MouseCaptureOutEvent>(StopPlayingSound);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseCaptureEvent>(BeginPlayingSound);
        target.UnregisterCallback<MouseCaptureOutEvent>(StopPlayingSound);
    }

    private void BeginPlayingSound(MouseCaptureEvent evt)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_startSound, target);
    }

    private void StopPlayingSound(MouseCaptureOutEvent evt)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(_endSound, target);
    }
}
