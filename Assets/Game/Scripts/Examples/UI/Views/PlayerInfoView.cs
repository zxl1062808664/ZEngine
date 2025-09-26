using System;
using System.Collections.Generic;
using System.ComponentModel;
using Framework.UI;
using TMPro;
using UnityEngine.UI;

namespace Game.UI
{
    public class PlayerInfoView : ViewBase<PlayerInfoViewModel>
    {
        public TMP_Text nameText;
        public TMP_Text levelText;
        public Slider hpSlider;
        public Button closeButton;
        public Button levelUpButton;

        private readonly List<IDisposable> _bindings = new List<IDisposable>();

        protected override void InitializeBindings()
        {
            // 属性绑定
            _bindings.Add(new PropertyBinder<string>(ViewModel, () => ViewModel.PlayerName, v => nameText.text = v));
            _bindings.Add(new PropertyBinder<int>(ViewModel, () => ViewModel.Level, v => levelText.text = $"Lv.{v}"));
            _bindings.Add(new PropertyBinder<int>(ViewModel, () => ViewModel.HP, v => hpSlider.value = v));

            // 命令绑定（假设你有这些命令）
            if (ViewModel.CloseCommand != null)
                _bindings.Add(new CommandBinder(ViewModel.CloseCommand, closeButton));

            if (ViewModel.LevelUpCommand != null)
                _bindings.Add(new CommandBinder(ViewModel.LevelUpCommand, levelUpButton));
        }

        protected override void ReleaseBindings()
        {
            foreach (var binding in _bindings)
            {
                binding.Dispose();
            }
            _bindings.Clear();
        }

        protected override void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 如果需要额外逻辑可以写在这里
            if (e.PropertyName == nameof(ViewModel.HP))
            {
                // 举例：播放血量掉落动画
                // PlayHpAnim();
            }
        }
    }
}