using System;

namespace UitkForKsp2.MVVM.Commands
{
    /// <summary>
    /// Generic command that receives a parameter of type T.
    /// T can be a UI event type (e.g. KeyDownEvent) or a value type
    /// extracted from ChangeEvent&lt;T&gt; by EventBinding.
    /// </summary>
    public interface ICommand<in T>
    {
        bool CanExecute();
        void Execute(T parameter);
        event Action CanExecuteChanged;
    }
}
