using System;
using System.Windows.Input;

namespace GTweak.Core
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public RelayCommand(Action<object> _execute, Func<object, bool> _canExecute = null)
        {
            execute = _execute;
            canExecute = _canExecute;
        }
        public bool CanExecute(object _parameter) => canExecute == null || canExecute(_parameter);
        public void Execute(object _parameter) => execute(_parameter);
    }
}
