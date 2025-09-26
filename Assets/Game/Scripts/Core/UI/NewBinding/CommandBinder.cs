using System;
using System.Windows.Input;
using UnityEngine.UI;

namespace Framework.UI
{
    public class CommandBinder : IDisposable
    {
        private readonly ICommand _command;
        private readonly Button _button;

        public CommandBinder(ICommand command, Button button, object commandParameter = null)
        {
            _command = command;
            _button = button;

            _button.onClick.AddListener(() =>
            {
                if (_command.CanExecute(commandParameter))
                {
                    _command.Execute(commandParameter);
                }
            });

            _command.CanExecuteChanged += OnCanExecuteChanged;

            OnCanExecuteChanged(null, EventArgs.Empty);
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            _button.interactable = _command.CanExecute(null);
        }

        public void Dispose()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
            if (_command != null)
            {
                _command.CanExecuteChanged -= OnCanExecuteChanged;
            }
        }
    }
}