// using Examples.UI.ViewModels;
// using Examples.UI.Views;
using Framework.Core;
using UnityEngine;

namespace Examples.Procedures
{
    public class GamePlayProcedure : ProcedureBase
    {
        private PlayerInfoView _gamePlayView;
        private float _gameTime = 0;

        public override void OnEnter(object userData)
        {
            LogModule.Log("进入游戏流程");
            var model = new PlayerModel();
            var viewModel = new PlayerInfoViewModel(model);
            
            // 显示游戏UI
            GameFramework.Instance.UIModule.ShowViewAsync<PlayerInfoView>(AssetConst.Assets_Game_Res_Prefab_UI_GamePlayView_prefab,viewModel).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    _gamePlayView = task.Result;
                    
                    // 初始化游戏数据
                    if (_gamePlayView.ViewModel is GamePlayViewModel vm)
                    {
                        vm.Score = 0;
                        vm.TimeLeft = 60; // 60秒游戏时间
                    }
                }
            });

            _gameTime = 0;
        }

        public override void OnUpdate(float deltaTime, float realDeltaTime)
        {
            _gameTime += deltaTime;
            
            // 更新游戏时间
            // if (_gamePlayView != null && _gamePlayView.ViewModel is GamePlayViewModel vm)
            // {
            //     vm.TimeLeft = Mathf.Max(0, 60 - (int)_gameTime);
            //     
            //     // 游戏结束条件
            //     if (vm.TimeLeft <= 0)
            //     {
            //         // 保存分数
            //         SaveGameScore(vm.Score);
            //         
            //         // 返回主菜单
            //         GameFramework.Instance.ProcedureModule.ChangeProcedure<MainMenuProcedure>();
            //     }
            // }
        }

        private async void SaveGameScore(int score)
        {
            if (score <= 0) return;
            
            // 加载并更新玩家数据
            var playerData = await GameFramework.Instance.DataPersistenceModule.LoadDataAsync<PlayerData>();
            if (score > playerData.highScore)
            {
                playerData.highScore = score;
                await GameFramework.Instance.DataPersistenceModule.SaveDataAsync(playerData);
                LogModule.Log($"新的最高分: {score}");
            }
        }

        public override void OnLeave(object userData)
        {
            LogModule.Log("离开游戏流程");
            
            // 关闭游戏UI
            if (_gamePlayView != null)
            {
                GameFramework.Instance.UIModule.CloseView("UI/GamePlayView");
                _gamePlayView = null;
            }
        }
    }
}
