using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator;

public class DocumentSoundManipulator : UnityEngine.UIElements.Manipulator
{
    private enum SoundTargetType
    {
        None,
        Button,
        Close,
        Toggle,
        Hover,
        Slider
    }

    public const string SelectedClass = "selected";
    public const string SoundNoneClass = "ui-sound-none";
    public const string ButtonSoundClass = "ui-sound-button";
    public const string CloseSoundClass = "ui-sound-close";
    public const string OabCloseSoundClass = "oab-close-button";
    public const string ToggleSoundClass = "ui-sound-toggle";
    public const string HoverSoundClass = "ui-sound-hover";
    public const string SliderSoundClass = "ui-sound-slider";

    private readonly string _buttonClickSound;
    private readonly string _buttonClickSelectedSound;
    private readonly string _buttonHoverSound;
    private readonly string _closeClickSound;
    private readonly string _closeHoverSound;
    private readonly string _toggleClickSound;
    private readonly string _toggleHoverSound;
    private readonly string _sliderStartSound;
    private readonly string _sliderStopSound;

    private VisualElement _lastHoverTarget;
    public DocumentSoundManipulator(
        string buttonClickSound = "Play_ui_extended_toggle_ON",
        string buttonClickSelectedSound = "Play_ui_extended_toggle_OFF",
        string buttonHoverSound = "Play_ui_extended_toggle_hover",
        string closeClickSound = "Play_ui_extended_button_select",
        string closeHoverSound = "Play_ui_extended_button_hover",
        string toggleClickSound = "Play_ui_extended_stagging_add_select",
        string toggleHoverSound = "Play_ui_extended_toggle_hover",
        string sliderStartSound = "Play_ui_options_slider_generic",
        string sliderStopSound = "Play_ui_options_slider_generic_stop"
    )
    {
        _buttonClickSound = buttonClickSound;
        _buttonClickSelectedSound = buttonClickSelectedSound ?? buttonClickSound;
        _buttonHoverSound = buttonHoverSound;
        _closeClickSound = closeClickSound;
        _closeHoverSound = closeHoverSound;
        _toggleClickSound = toggleClickSound;
        _toggleHoverSound = toggleHoverSound;
        _sliderStartSound = sliderStartSound;
        _sliderStopSound = sliderStopSound;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
        target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        target.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged, TrickleDown.TrickleDown);
        target.RegisterCallback<PointerOverEvent>(OnPointerOver, TrickleDown.TrickleDown);
        target.RegisterCallback<PointerOutEvent>(OnPointerOut, TrickleDown.TrickleDown);
        target.RegisterCallback<MouseCaptureEvent>(OnMouseCapture, TrickleDown.TrickleDown);
        target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut, TrickleDown.TrickleDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        target.UnregisterCallback<ChangeEvent<bool>>(OnToggleChanged, TrickleDown.TrickleDown);
        target.UnregisterCallback<PointerOverEvent>(OnPointerOver, TrickleDown.TrickleDown);
        target.UnregisterCallback<PointerOutEvent>(OnPointerOut, TrickleDown.TrickleDown);
        target.UnregisterCallback<MouseCaptureEvent>(OnMouseCapture, TrickleDown.TrickleDown);
        target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut, TrickleDown.TrickleDown);
    }

    private void OnClick(ClickEvent evt)
    {
        if (ResolveCloseTarget(evt.target as VisualElement) != null)
        {
            return;
        }

        VisualElement element = ResolveClickTarget(evt.target as VisualElement, out _);
        if (element == null)
        {
            return;
        }

        Play(
            element.ClassListContains(SelectedClass)
                ? _buttonClickSelectedSound
                : _buttonClickSound,
            element
        );
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        VisualElement closeTarget = ResolveCloseTarget(evt.target as VisualElement);
        if (closeTarget != null)
        {
            Play(_closeClickSound, closeTarget);
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.target is Toggle or RadioButton)
        {
            return;
        }

        if (evt.target is not VisualElement targetElement || IsSuppressed(targetElement))
        {
            return;
        }

        VisualElement element = ResolveClosestExplicitTarget(
            targetElement,
            out _,
            SoundTargetType.Toggle
        );

        if (element != null && element is not Toggle and not RadioButton)
        {
            Play(_toggleClickSound, element);
        }
    }

    private void OnToggleChanged(ChangeEvent<bool> evt)
    {
        VisualElement element = ResolveToggleTarget(evt.target as VisualElement);
        if (element != null)
        {
            Play(_toggleClickSound, element);
        }
    }

    private void OnPointerOver(PointerOverEvent evt)
    {
        VisualElement element = ResolveHoverTarget(evt.target as VisualElement, out SoundTargetType soundType);
        if (element == null || ReferenceEquals(element, _lastHoverTarget))
        {
            return;
        }

        _lastHoverTarget = element;

        if (soundType == SoundTargetType.Close)
        {
            Play(_closeHoverSound, element);
        }
        else if (soundType == SoundTargetType.Toggle)
        {
            Play(_toggleHoverSound, element);
        }
        else if (soundType == SoundTargetType.Hover || soundType == SoundTargetType.Button)
        {
            Play(_buttonHoverSound, element);
        }
    }

    private void OnPointerOut(PointerOutEvent evt)
    {
        if (_lastHoverTarget != null && _lastHoverTarget.worldBound.Contains(evt.position))
        {
            return;
        }

        VisualElement element = ResolveHoverTarget(evt.target as VisualElement, out _);
        if (element == null || ReferenceEquals(element, _lastHoverTarget))
        {
            _lastHoverTarget = null;
        }
    }

    private void OnMouseCapture(MouseCaptureEvent evt)
    {
        VisualElement element = ResolveSliderTarget(evt.target as VisualElement, out _);
        if (element != null)
        {
            Play(_sliderStartSound, element);
        }
    }

    private void OnMouseCaptureOut(MouseCaptureOutEvent evt)
    {
        VisualElement element = ResolveSliderTarget(evt.target as VisualElement, out _);
        if (element != null)
        {
            Play(_sliderStopSound, element);
        }
    }

    private static void Play(string eventName, VisualElement element)
    {
        SoundManipulatorHelper.PostAKEventWithPositionalRTPC(eventName, element);
    }

    private VisualElement ResolveClickTarget(VisualElement element, out SoundTargetType soundType)
    {
        return ResolveClosestTarget(
            element,
            out soundType,
            SoundTargetType.Button
        );
    }

    private VisualElement ResolveCloseTarget(VisualElement element)
    {
        if (element == null || IsSuppressed(element))
        {
            return null;
        }

        return ResolveClosestExplicitTarget(element, out _, SoundTargetType.Close);
    }

    private VisualElement ResolveToggleTarget(VisualElement element)
    {
        return ResolveClosestTarget(element, out _, SoundTargetType.Toggle);
    }

    private VisualElement ResolveHoverTarget(VisualElement element, out SoundTargetType soundType)
    {
        return ResolveClosestTarget(
            element,
            out soundType,
            SoundTargetType.Close,
            SoundTargetType.Toggle,
            SoundTargetType.Hover,
            SoundTargetType.Button
        );
    }

    private VisualElement ResolveSliderTarget(VisualElement element, out SoundTargetType soundType)
    {
        return ResolveClosestTarget(element, out soundType, SoundTargetType.Slider);
    }

    private VisualElement ResolveClosestTarget(
        VisualElement element,
        out SoundTargetType soundType,
        params SoundTargetType[] allowedTypes
    )
    {
        soundType = SoundTargetType.None;

        if (element == null || IsSuppressed(element))
        {
            return null;
        }

        VisualElement explicitTarget = ResolveClosestExplicitTarget(element, out soundType, allowedTypes);
        if (explicitTarget != null)
        {
            return explicitTarget;
        }

        return ResolveClosestAutomaticTarget(element, out soundType, allowedTypes);
    }

    private VisualElement ResolveClosestExplicitTarget(
        VisualElement element,
        out SoundTargetType soundType,
        params SoundTargetType[] allowedTypes
    )
    {
        soundType = SoundTargetType.None;

        for (VisualElement current = element; current != null; current = current.parent)
        {
            foreach (SoundTargetType allowedType in allowedTypes)
            {
                if (HasSoundClass(current, allowedType))
                {
                    soundType = allowedType;
                    return current;
                }
            }

            if (ReferenceEquals(current, target))
            {
                break;
            }
        }

        return null;
    }

    private VisualElement ResolveClosestAutomaticTarget(
        VisualElement element,
        out SoundTargetType soundType,
        params SoundTargetType[] allowedTypes
    )
    {
        soundType = SoundTargetType.None;

        for (VisualElement current = element; current != null; current = current.parent)
        {
            foreach (SoundTargetType allowedType in allowedTypes)
            {
                if (IsAutomaticSoundTarget(current, allowedType))
                {
                    soundType = allowedType;
                    return current;
                }
            }

            if (ReferenceEquals(current, target))
            {
                break;
            }
        }

        return null;
    }

    private bool IsSuppressed(VisualElement element)
    {
        for (VisualElement current = element; current != null; current = current.parent)
        {
            if (current.ClassListContains(SoundNoneClass))
            {
                return true;
            }

            if (ReferenceEquals(current, target))
            {
                break;
            }
        }

        return false;
    }

    private static bool HasSoundClass(VisualElement element, SoundTargetType soundType)
    {
        return soundType switch
        {
            SoundTargetType.Close => element.ClassListContains(CloseSoundClass)
                || element.ClassListContains(OabCloseSoundClass),
            SoundTargetType.Button => element.ClassListContains(ButtonSoundClass),
            SoundTargetType.Toggle => element.ClassListContains(ToggleSoundClass),
            SoundTargetType.Hover => element.ClassListContains(HoverSoundClass),
            SoundTargetType.Slider => element.ClassListContains(SliderSoundClass),
            _ => false
        };
    }

    private static bool IsAutomaticSoundTarget(VisualElement element, SoundTargetType soundType)
    {
        return soundType switch
        {
            SoundTargetType.Button => element is Button,
            SoundTargetType.Toggle => element is Toggle or RadioButton,
            SoundTargetType.Slider => element is Slider or SliderInt or MinMaxSlider,
            _ => false
        };
    }
}
