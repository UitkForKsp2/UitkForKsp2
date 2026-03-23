using System;

namespace UitkForKsp2.MVVM.Commands
{
    /// <summary>
    /// Concrete parameterless command backed by delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event Action CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute() => _canExecute?.Invoke() ?? true;

        public void Execute()
        {
            if (CanExecute())
            {
                _execute();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke();
    }
}
