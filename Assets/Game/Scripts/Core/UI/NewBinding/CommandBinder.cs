using System.Windows.Input;

namespace Framework.UI
{
    // 用法：在View的InitializeBindings中调用
// binder.Bind(ViewModel.CloseCommand, closeButton);

    using UnityEngine.UI;
    using System;

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
        
            // 立即更新一次按钮状态
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