using System;

namespace UitkForKsp2.MVVM.Commands
{
    /// <summary>
    /// Concrete parameterized command backed by delegates.
    /// T is typically a UI event type or a value extracted from ChangeEvent&lt;T&gt;.
    /// </summary>
    public class RelayCommand<T> : ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<bool> _canExecute;

        public event Action CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute() => _canExecute?.Invoke() ?? true;

        public void Execute(T parameter)
        {
            if (CanExecute())
            {
                _execute(parameter);
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke();
    }
}