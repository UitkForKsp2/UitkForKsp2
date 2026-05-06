using UitkForKsp2.Controls;
using UnityEngine.UIElements;

namespace UitkForKsp2.API.Manipulator
{
    public class TooltipManipulator : UnityEngine.UIElements.Manipulator
    {
        private Tooltip tooltip;

        public TooltipManipulator(Tooltip tooltip)
        {
            this.tooltip = tooltip;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseIn);
            target.RegisterCallback<MouseOutEvent>(MouseOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseIn);
            target.UnregisterCallback<MouseOutEvent>(MouseOut);
        }

        private void MouseIn(MouseEnterEvent e)
        {
            tooltip.Show(target);
        }

        private void MouseOut(MouseOutEvent e)
        {
            tooltip.Close();
        }

        // ============================================================================================================
    }
}