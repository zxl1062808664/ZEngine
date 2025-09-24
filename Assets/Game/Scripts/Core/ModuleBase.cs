using System;
using UnityEngine;

namespace Framework.Core
{
    /// <summary>
    /// 模块基类
    /// </summary>
    public abstract class ModuleBase : MonoBehaviour, IModule
    {
        protected GameFramework GameFramework { get; private set; }
        public bool IsInitialized { get; protected set; }
        public virtual int Priority => 0;

        public void SetGameFramework(GameFramework gameFramework)
        {
            GameFramework = gameFramework;
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        public virtual void OnInitialize()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// 固定时间间隔更新
        /// </summary>
        public virtual void OnFixedUpdate(float fixedDeltaTime) { }

        /// <summary>
        /// 延迟更新
        /// </summary>
        public virtual void OnLateUpdate(float deltaTime) { }

        /// <summary>
        /// 关闭模块
        /// </summary>
        public virtual void OnShutdown()
        {
            IsInitialized = false;
        }
    }
}