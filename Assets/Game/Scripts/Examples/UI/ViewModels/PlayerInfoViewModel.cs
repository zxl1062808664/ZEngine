using Framework.UI;
using System.Windows.Input;
using Framework.Core;

namespace Game.UI
{
    public class PlayerInfoViewModel : ViewModelBase
    {
        private string _playerName;
        private int _level;
        private int _hp;

        public string PlayerName
        {
            get => _playerName;
            set => Set(ref _playerName, value); // 自动触发 PropertyChanged
        }

        public int Level
        {
            get => _level;
            set => Set(ref _level, value);
        }

        public int HP
        {
            get => _hp;
            set => Set(ref _hp, value);
        }

        // 命令
        public ICommand CloseCommand { get; }
        public ICommand LevelUpCommand { get; }

        public PlayerInfoViewModel()
        {
            CloseCommand = new RelayCommand(OnClose);
            LevelUpCommand = new RelayCommand(OnLevelUp);
        }

        private void OnClose()
        {
            // 这里写关闭逻辑，比如调用 UIModule.CloseView("UI/PlayerInfoView");
            LogModule.Log("CloseCommand executed!");
            GameFramework.Instance.UIModule.CloseView(this);
        }

        private void OnLevelUp()
        {
            Level++;
            HP += 10; // 假设升级回血
            LogModule.Log($"LevelUpCommand executed! Now Level={Level}, HP={HP}");
        }
    }
}