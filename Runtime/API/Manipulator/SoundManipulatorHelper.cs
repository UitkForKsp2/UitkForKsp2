using ReduxLib.GameInterfaces;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public static class SoundManipulatorHelper
{
    public static void PostAKEventWithPositionalRTPC(string eventToPost, VisualElement target)
    {
        if (string.IsNullOrEmpty(eventToPost) || target == null)
        {
            return;
        }

        IUISoundPlayer.Instance?.PostAKEventWithPositionalRTPC(eventToPost, target);
    }
}
