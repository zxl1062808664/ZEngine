using Framework.UI.MVVM;
using Framework.UI.MVVM.Commands;
using Examples.Procedures;
using Framework.Core;

namespace Examples.UI.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private string _playerName;
        private int _highScore;

        // 命令
        public ICommand StartGameCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public int HighScore
        {
            get => _highScore;
            set => SetProperty(ref _highScore, value);
        }

        public MainMenuViewModel()
        {
            // 初始化命令
            StartGameCommand = new RelayCommand(OnStartGame);
            ExitCommand = new RelayCommand(OnExit);
        }

        private void OnStartGame(object parameter)
        {
            // 切换到游戏流程
            GameFramework.Instance.ProcedureModule.ChangeProcedure<GamePlayProcedure>();
        }

        private void OnExit(object parameter)
        {
            // 退出游戏
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}