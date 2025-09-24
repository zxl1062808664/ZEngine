using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Framework.Core;
using Examples.Procedures;
using Framework.Core.Events;

namespace Examples
{
    public class GameLauncher : MonoBehaviour
    {
        [Tooltip("框架核心组件")] public GameFramework gameFrameworkPrefab;

        private void Awake()
        {
            // 确保框架实例存在
            if (GameFramework.Instance == null && gameFrameworkPrefab != null)
            {
                // Instantiate(gameFrameworkPrefab);
            }
        }

        private void Start()
        {
            EventModule.Instance.Subscribe(InitializeAssetsFinishEventArgs.EventID, InitializeAssetsFinishEvent);
            // 等待框架初始化
        }

        private void OnDestroy()
        {
            EventModule.Instance.Unsubscribe(InitializeAssetsFinishEventArgs.EventID, InitializeAssetsFinishEvent);
        }

        private void InitializeAssetsFinishEvent(object sender, GameEventArgs e)
        {
            var args = e as InitializeAssetsFinishEventArgs;
            if (!args.isFinished)
            {
                Debug.LogError("GameFramework instance not found!");
                return;
            }

            // 注册流程
            var procedureModule = GameFramework.Instance.ProcedureModule;
            procedureModule.RegisterProcedure<InitProcedure>();
            procedureModule.RegisterProcedure<MainMenuProcedure>();
            procedureModule.RegisterProcedure<GamePlayProcedure>();

            // 启动初始流程
            procedureModule.ChangeProcedure<InitProcedure>();
        }
    }
}