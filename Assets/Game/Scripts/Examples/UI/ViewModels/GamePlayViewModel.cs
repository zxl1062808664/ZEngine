using Examples.Procedures;
using Framework.UI.MVVM;
using Framework.UI.MVVM.Commands;
using Framework.Core;

namespace Examples.UI.ViewModels
{
    public class GamePlayViewModel : ViewModelBase
    {
        private int _score;
        private int _timeLeft;

        // 命令
        public ICommand AddScoreCommand { get; private set; }
        public ICommand ReturnToMenuCommand { get; private set; }

        public int Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }

        public int TimeLeft
        {
            get => _timeLeft;
            set => SetProperty(ref _timeLeft, value);
        }

        public GamePlayViewModel()
        {
            // 初始化命令
            AddScoreCommand = new RelayCommand<int>(AddScore);
            ReturnToMenuCommand = new RelayCommand(ReturnToMenu);
        }

        private void AddScore(int points)
        {
            Score += points;
        }

        private void ReturnToMenu(object parameter)
        {
            // 返回主菜单
            GameFramework.Instance.ProcedureModule.ChangeProcedure<MainMenuProcedure>();
        }
    }
}