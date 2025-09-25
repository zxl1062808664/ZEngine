using System.Threading.Tasks;
using Framework.Core;

namespace Examples.Procedures
{
    public class InitProcedure : ProcedureBase
    {
        private float _progress = 0;

        public override void OnEnter(object userData)
        {
            LogModule.Log("进入初始化流程");
            StartInitialization();
        }

        private async void StartInitialization()
        {
            // 1. 检查资源更新
            _progress = 0.2f;
            // var updateSize = await GameFramework.ResourceModule.CheckForUpdatesAsync();
            //
            // if (updateSize > 0)
            // {
            //     LogModule.Log($"需要更新 {updateSize:F2} MB 的资源");
            //     await GameFramework.ResourceModule.UpdateResourcesAsync(progress => 
            //     {
            //         _progress = 0.2f + progress * 0.6f; // 更新进度占总进度的60%
            //     });
            // }

            // 2. 加载玩家数据
            _progress = 0.8f;
            var playerData = await GameFramework.Instance.DataPersistenceModule.LoadDataAsync<PlayerData>();
            LogModule.Log($"加载玩家数据: {playerData.playerName}, 最高分: {playerData.highScore}");

            // 3. 初始化完成，进入主菜单
            _progress = 1.0f;
            await Task.Delay(500); // 短暂延迟，让玩家看到完成状态
            GameFramework.Instance.ProcedureModule.ChangeProcedure<GamePlayProcedure>(playerData);
        }

        public override void OnUpdate(float deltaTime, float realDeltaTime)
        {
            // 这里可以更新进度条UI
        }

        public override void OnLeave(object userData)
        {
            LogModule.Log("离开初始化流程");
        }
    }
}