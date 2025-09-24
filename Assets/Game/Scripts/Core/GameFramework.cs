using UnityEngine;
using System.Collections.Generic;
using Framework.Core.Events;
using Framework.DataPersistence;

namespace Framework.Core
{
    public class GameFramework : MonoBehaviour
    {
        // 单例实例
        public static GameFramework Instance { get; private set; }

        // 模块列表
        private readonly Dictionary<System.Type, IModule> _modules = new Dictionary<System.Type, IModule>();

        // 模块属性访问器
        public UIModule UIModule => GetModule<UIModule>();
        public ResourceModule ResourceModule => GetModule<ResourceModule>();
        public DataPersistenceModule DataPersistenceModule => GetModule<DataPersistenceModule>();
        public ProcedureModule ProcedureModule => GetModule<ProcedureModule>();
        public LogModule LogModule => GetModule<LogModule>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化核心模块
            InitializeModules();
        }

        private void InitializeModules()
        {
            // 注册并初始化所有核心模块
            RegisterModule(new LogModule());
            RegisterModule(new EventModule());
            RegisterModule(new DataPersistenceModule());
            RegisterModule(new ResourceModule());
            RegisterModule(new UIModule());
            RegisterModule(new ProcedureModule());

            // 初始化所有模块
            foreach (var module in _modules.Values)
            {
                module.OnInitialize();
            }
        }

        // 注册模块
        public void RegisterModule(IModule module)
        {
            var type = module.GetType();
            if (!_modules.ContainsKey(type))
            {
                _modules.Add(type, module);
                module.SetGameFramework(this);
                LogModule.Log($"Registered module: {type.Name}");
            }
            else
            {
                LogModule.Warning($"Module {type.Name} already registered");
            }
        }

        // 获取模块
        public T GetModule<T>() where T : class, IModule
        {
            var type = typeof(T);
            if (_modules.TryGetValue(type, out var module))
            {
                return module as T;
            }
            LogModule.Error($"Module {type.Name} not found");
            return null;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var module in _modules.Values)
            {
                module.OnUpdate(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            foreach (var module in _modules.Values)
            {
                module.OnFixedUpdate(fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            foreach (var module in _modules.Values)
            {
                module.OnLateUpdate(deltaTime);
            }
        }

        private void OnDestroy()
        {
            foreach (var module in _modules.Values)
            {
                module.OnShutdown();
            }
            _modules.Clear();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    // 模块接口
    public interface IModule
    {
        void SetGameFramework(GameFramework gameFramework);
        void OnInitialize();
        void OnUpdate(float deltaTime);
        void OnFixedUpdate(float fixedDeltaTime);
        void OnLateUpdate(float deltaTime);
        void OnShutdown();
    }
}
