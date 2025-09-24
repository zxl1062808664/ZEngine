using Examples.UI.ViewModels;
using Examples.UI.Views;
using Framework.Core;
using UnityEngine;

namespace Examples.Procedures
{
    public class MainMenuProcedure : ProcedureBase
    {
        private MainMenuView _mainMenuView;

        public override void OnEnter(object userData)
        {
            LogModule.Log("进入主菜单流程");
            
            // 显示主菜单UI
            var playerData = userData as PlayerData;
            GameFramework.Instance.UIModule.ShowViewAsync<MainMenuView>(AssetConst.Assets_Game_Res_Prefab_UI_MainMenuView_prefab).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    _mainMenuView = task.Result;
                    
                    // 传递玩家数据到视图模型
                    if (playerData != null && _mainMenuView.ViewModel is MainMenuViewModel vm)
                    {
                        vm.PlayerName = playerData.playerName;
                        vm.HighScore = playerData.highScore;
                    }
                }
            });
        }

        public override void OnUpdate(float deltaTime, float realDeltaTime)
        {
            // 主菜单更新逻辑
        }

        public override void OnLeave(object userData)
        {
            LogModule.Log("离开主菜单流程");
            
            // 关闭主菜单UI
            if (_mainMenuView != null)
            {
                GameFramework.Instance.UIModule.CloseView("UI/MainMenuView");
                _mainMenuView = null;
            }
        }
    }
}