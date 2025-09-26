using System;
using System.Collections;
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
                Instantiate(gameFrameworkPrefab);
            }
        }

        IEnumerator Start()
        {
            // 等待框架初始化
            while (!GameFramework.Instance.isInitialized)
            {
                yield return null;
            }

            InitializeAssetsFinishEvent();
        }

        private void OnDestroy()
        {
        }

        private void InitializeAssetsFinishEvent()
        {
            // 注册流程
            var procedureModule = GameFramework.Instance.ProcedureModule;
            procedureModule.RegisterProcedure<InitProcedure>();
            procedureModule.RegisterProcedure<GamePlayProcedure>();

            // 启动初始流程
            procedureModule.ChangeProcedure<InitProcedure>();
        }
    }
}