using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class ProcedureModule : IModule
    {
        private GameFramework _gameFramework;
        private readonly Dictionary<Type, ProcedureBase> _procedures = new Dictionary<Type, ProcedureBase>();
        private ProcedureBase _currentProcedure;

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            LogModule.Log("Procedure module initialized");
        }

        // 注册流程
        public void RegisterProcedure<T>() where T : ProcedureBase, new()
        {
            Type procedureType = typeof(T);
            if (!_procedures.ContainsKey(procedureType))
            {
                T procedure = new T();
                procedure.Initialize(_gameFramework);
                _procedures.Add(procedureType, procedure);
                LogModule.Log($"Registered procedure: {procedureType.Name}");
            }
            else
            {
                LogModule.Warning($"Procedure {procedureType.Name} already registered");
            }
        }

        // 切换流程
        public void ChangeProcedure<T>(object userData = null) where T : ProcedureBase
        {
            Type targetType = typeof(T);
            if (!_procedures.TryGetValue(targetType, out var targetProcedure))
            {
                LogModule.Error($"Procedure {targetType.Name} not registered");
                return;
            }

            // 退出当前流程
            if (_currentProcedure != null)
            {
                LogModule.Log($"Leaving procedure: {_currentProcedure.GetType().Name}");
                _currentProcedure.OnLeave(userData);
            }

            // 进入新流程
            _currentProcedure = targetProcedure;
            LogModule.Log($"Entering procedure: {targetType.Name}");
            _currentProcedure.OnEnter(userData);
        }

        public void OnUpdate(float deltaTime)
        {
            _currentProcedure?.OnUpdate(deltaTime, Time.unscaledDeltaTime);
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            _currentProcedure?.OnFixedUpdate(fixedDeltaTime);
        }

        public void OnLateUpdate(float deltaTime)
        {
            _currentProcedure?.OnLateUpdate(deltaTime);
        }

        public void OnShutdown()
        {
            if (_currentProcedure != null)
            {
                _currentProcedure.OnLeave(null);
                _currentProcedure = null;
            }

            foreach (var procedure in _procedures.Values)
            {
                procedure.Shutdown();
            }
            _procedures.Clear();

            LogModule.Log("Procedure module shutdown");
        }
    }

    // 流程基类
    public abstract class ProcedureBase
    {
        protected GameFramework GameFramework { get; private set; }

        public void Initialize(GameFramework gameFramework)
        {
            GameFramework = gameFramework;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public abstract void OnEnter(object userData);

        public abstract void OnUpdate(float deltaTime, float realDeltaTime);

        public virtual void OnFixedUpdate(float fixedDeltaTime) { }

        public virtual void OnLateUpdate(float deltaTime) { }

        public abstract void OnLeave(object userData);

        public virtual void Shutdown() { }
    }
}
