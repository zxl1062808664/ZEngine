using System;
using System.Windows.Input;

namespace Framework.UI
{
    using System;
    using System.Windows.Input;

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

        // 为了方便，也可以提供一个无参版本
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

        // 手动触发CanExecuteChanged事件，用于更新命令的可执行状态（例如按钮的interactable）
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

        // 为了方便，也可以提供一个无参版本
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
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        // 手动触发CanExecuteChanged事件，用于更新命令的可执行状态（例如按钮的interactable）
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}