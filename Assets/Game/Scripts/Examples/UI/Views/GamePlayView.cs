using UnityEngine;
using UnityEngine.UI;
using Framework.Core;
using Framework.UI.MVVM.Binding;
using Examples.UI.ViewModels;
using TMPro;

namespace Examples.UI.Views
{
    public class GamePlayView : UIView
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _timeLeftText;
        [SerializeField] private Button _addScoreButton;
        [SerializeField] private Button _returnButton;

        protected override void BindViewModel()
        {
            var vm = _viewModel as GamePlayViewModel;
            if (vm == null) return;

            // 绑定属性
            BindingManager.Instance.BindProperty(vm, nameof(vm.Score), _scoreText, nameof(_scoreText.text));
            BindingManager.Instance.BindProperty(vm, nameof(vm.TimeLeft), _timeLeftText, nameof(_timeLeftText.text));

            // 绑定命令（带参数）
            BindingManager.Instance.BindCommand(vm, nameof(vm.AddScoreCommand), _addScoreButton, nameof(_addScoreButton.onClick));
            BindingManager.Instance.BindCommand(vm, nameof(vm.ReturnToMenuCommand), _returnButton, nameof(_returnButton.onClick));
        }

        // 按钮点击时传递参数的示例方法
        public void OnAddScoreButtonClick()
        {
            var vm = _viewModel as GamePlayViewModel;
            vm?.AddScoreCommand.Execute(10); // 每次点击增加10分
        }
    }
}