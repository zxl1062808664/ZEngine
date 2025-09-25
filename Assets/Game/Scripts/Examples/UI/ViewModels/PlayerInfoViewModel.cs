using Framework.UI;
using ICommand = System.Windows.Input.ICommand;

public class PlayerInfoViewModel : ViewModelBase
{
    private readonly PlayerModel _model;

    // 属性直接暴露给View
    private string _playerName;
    public string PlayerName { get => _playerName; set => Set(ref _playerName, value); }

    private string _levelText;
    public string LevelText { get => _levelText; set => Set(ref _levelText, value); }
    
    private float _expProgress;
    public float ExpProgress { get => _expProgress; set => Set(ref _expProgress, value); }

    // 命令
    public ICommand CloseCommand { get; }
    public ICommand LevelUpCommand { get; }

    public PlayerInfoViewModel(PlayerModel model)
    {
        _model = model;
        
        // 初始化数据转换
        UpdateData();

        // 初始化命令
        // CloseCommand = new RelayCommand(() => UIManager.Instance.CloseView<PlayerInfoViewModel>());
        LevelUpCommand = new RelayCommand(ExecuteLevelUp, CanExecuteLevelUp);
        
        // 实际项目中，这里应该订阅全局事件，如 PlayerDataUpdateEvent
    }

    private void UpdateData()
    {
        PlayerName = _model.Name;
        LevelText = $"Lv. {_model.Level}";
        ExpProgress = (float)_model.Exp / _model.MaxExp;
        
        // 手动通知LevelUpCommand的状态可能已改变
        (LevelUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool CanExecuteLevelUp()
    {
        return _model.Level < 100; // 假设满级100
    }

    private void ExecuteLevelUp()
    {
        _model.Level++;
        _model.Exp = 0;
        _model.MaxExp += 100;
        // 业务逻辑应在Model层或Service层处理，这里仅为演示
        
        UpdateData(); // 更新ViewModel数据，会自动通知View
    }
}