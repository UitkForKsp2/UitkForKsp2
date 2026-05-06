using UitkForKsp2.Controls;
using UitkForKsp2.MVVM.Converters;
using UnityEngine;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator
{
    public class TooltipManipulator : UnityEngine.UIElements.Manipulator
    {
        private readonly Tooltip tooltip;
        private IVisualElementScheduledItem? showTask;
        private int hoverVersion;

        public int? HoverDelay { get; set; }
        public int? FadeInDuration { get; set; }
        public int? FadeOutDuration { get; set; }

        public TooltipManipulator(
            Tooltip tooltip,
            int? hoverDelay = null,
            int? fadeInDuration = null,
            int? fadeOutDuration = null
        )
        {
            this.tooltip = tooltip;
            HoverDelay = hoverDelay;
            FadeInDuration = fadeInDuration;
            FadeOutDuration = fadeOutDuration;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseIn);
            target.RegisterCallback<MouseLeaveEvent>(MouseOut);
            target.RegisterCallback<TooltipEvent>(SuppressNativeTooltip);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            CancelPendingShow();
            tooltip.Close(target, GetFadeOutDuration());
            target.UnregisterCallback<MouseEnterEvent>(MouseIn);
            target.UnregisterCallback<MouseLeaveEvent>(MouseOut);
            target.UnregisterCallback<TooltipEvent>(SuppressNativeTooltip);
        }

        private void MouseIn(MouseEnterEvent e)
        {
            int version = ++hoverVersion;
            CancelPendingShow();

            int hoverDelay = GetHoverDelay();
            if (hoverDelay <= 0)
            {
                ShowTooltip(version);
                return;
            }

            showTask = target.schedule.Execute(() => ShowTooltip(version))
                .StartingIn(hoverDelay);
        }

        private void MouseOut(MouseLeaveEvent e)
        {
            hoverVersion++;
            CancelPendingShow();
            tooltip.Close(target, GetFadeOutDuration());
        }

        private void ShowTooltip(int version)
        {
            if (version != hoverVersion)
            {
                return;
            }

            showTask = null;
            Tooltip.ParseTooltip(target, out char hint, out string message);
            tooltip.Show(
                target,
                LocalizationConverter.Translate(message),
                hint,
                0,
                GetFadeInDuration(),
                GetFadeOutDuration()
            );
        }

        private void CancelPendingShow()
        {
            showTask?.Pause();
            showTask = null;
        }

        private int GetHoverDelay()
        {
            return Mathf.Max(0, HoverDelay ?? tooltip.Delay);
        }

        private int GetFadeInDuration()
        {
            return Mathf.Max(0, FadeInDuration ?? tooltip.FadeInDuration);
        }

        private int GetFadeOutDuration()
        {
            return Mathf.Max(0, FadeOutDuration ?? tooltip.FadeOutDuration);
        }

        private void SuppressNativeTooltip(TooltipEvent e)
        {
            e.StopImmediatePropagation();
        }

        // ============================================================================================================
    }
}
