using System;

namespace UitkForKsp2.MVVM.Commands
{
    /// <summary>
    /// Parameterless command interface. Use for actions where no event data is needed:
    /// button clicks, pointer enter/leave, focus/blur, submit, etc.
    /// </summary>
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
        event Action CanExecuteChanged;
    }
}
