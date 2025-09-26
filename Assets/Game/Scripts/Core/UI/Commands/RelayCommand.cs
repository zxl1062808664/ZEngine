using System;
using System.Windows.Input;

namespace Framework.UI
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = p => execute();
            if (canExecute != null)
            {
                _canExecute = p => canExecute();
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = p => execute();
            if (canExecute != null)
            {
                _canExecute = p => canExecute();
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            if (parameter == null)
            {
                return !typeof(T).IsValueType;
            }

            if (parameter is T tParam)
            {
                return _canExecute(tParam);
            }

            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                throw new ArgumentException($"Parameter cannot be null for value type {typeof(T).Name}");
            }

            if (parameter is T tParam)
            {
                _execute(tParam);
            }
            else
            {
                throw new ArgumentException($"Invalid parameter type. Expected {typeof(T).Name}");
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
