using System;
using UnityEngine.UI;
using System.ComponentModel;
using System.Collections.Generic;
using Framework.UI;
using TMPro;

public class PlayerInfoView : ViewBase<PlayerInfoViewModel>
{
    public TMP_Text playerNameText;
    public TMP_Text levelText;
    public Slider expSlider;
    public Button closeButton;
    public Button levelUpButton;

    // 用于管理所有绑定，方便统一释放
    private List<IDisposable> _bindings = new List<IDisposable>();

    protected override void InitializeBindings()
    {
        // 创建属性绑定
        _bindings.Add(new PropertyBinder<string>(ViewModel, () => ViewModel.PlayerName, name => playerNameText.text = name));
        _bindings.Add(new PropertyBinder<string>(ViewModel, () => ViewModel.LevelText, lvl => levelText.text = lvl));
        _bindings.Add(new PropertyBinder<float>(ViewModel, () => ViewModel.ExpProgress, progress => expSlider.value = progress));
        
        // 创建命令绑定
        _bindings.Add(new CommandBinder(ViewModel.CloseCommand, closeButton));
        _bindings.Add(new CommandBinder(ViewModel.LevelUpCommand, levelUpButton));
    }

    protected override void ReleaseBindings()
    {
        // 释放所有绑定
        foreach (var binding in _bindings)
        {
            binding.Dispose();
        }
        _bindings.Clear();
    }

    // 可选：如果希望更精细地控制更新，可以实现此方法
    // 否则，让PropertyBinder自动处理即可
    protected override void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // 例如：当经验值变化时，播放一个动画
        if (e.PropertyName == nameof(ViewModel.ExpProgress))
        {
            // PlayExpAnimation();
        }
    }
}