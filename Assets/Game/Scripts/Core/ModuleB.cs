using UnityEngine;

namespace Framework.Core
{
    public abstract class ModuleB : MonoBehaviour
    {
        protected GameFramework GameFramework { get; private set; }
        public bool IsInitialized { get; protected set; }
        public virtual int Priority => 0;

        internal void Initialize(GameFramework gameFramework)
        {
            GameFramework = gameFramework;
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        protected internal virtual void OnInitialize()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// 启动模块
        /// </summary>
        protected internal virtual void OnStart() { }

        /// <summary>
        /// 每帧更新
        /// </summary>
        protected internal virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// 固定时间间隔更新
        /// </summary>
        protected internal virtual void OnFixedUpdate(float fixedDeltaTime) { }

        /// <summary>
        /// 延迟更新
        /// </summary>
        protected internal virtual void OnLateUpdate() { }

        /// <summary>
        /// 关闭模块
        /// </summary>
        protected internal virtual void OnShutdown()
        {
            IsInitialized = false;
        }
    }
}