using UnityEngine;
using UnityEngine.UI;
using Framework.Core;
using Framework.UI.MVVM.Binding;
using Examples.UI.ViewModels;
using TMPro;

namespace Examples.UI.Views
{
    public class MainMenuView : UIView
    {
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        protected override void BindViewModel()
        {
            var vm = _viewModel as MainMenuViewModel;
            if (vm == null) return;

            // 绑定属性
            BindingManager.Instance.BindProperty(vm, nameof(vm.PlayerName), _playerNameText,
                nameof(_playerNameText.text));
            BindingManager.Instance.BindProperty(vm, nameof(vm.HighScore), _highScoreText, nameof(_highScoreText.text));

            // 绑定命令
            BindingManager.Instance.BindCommand(vm, nameof(vm.StartGameCommand), _startButton,
                nameof(_startButton.onClick));
            BindingManager.Instance.BindCommand(vm, nameof(vm.ExitCommand), _exitButton, nameof(_exitButton.onClick));
        }

        private void ClickStartButton()
        {
            LogModule.Log("Click Start");
        }

        private void ClickExitButton()
        {
            LogModule.Log("Click Exit");
        }

        public override void OnShow(object data)
        {
            base.OnShow(data);
            gameObject.SetActive(true);
        }

        public override void OnHide()
        {
            base.OnHide();
            gameObject.SetActive(false);
        }
    }
}