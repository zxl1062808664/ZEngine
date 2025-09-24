using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Framework.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Framework.Core
{
    public class ResourceModule : IModule
    {
        private GameFramework _gameFramework;
        private ResourcePackage _defaultPackage;
        private bool _isInitialized = false;

        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        public string packageName = "DefaultPackage";
        public string hostServerIP = "http://192.168.1.239:8080/NetworkServer/AssetsFolder/Framework";
        public string sceneName = "";

        public void SetGameFramework(GameFramework gameFramework)
        {
            _gameFramework = gameFramework;
        }

        public void OnInitialize()
        {
            InitAsync().Forget();
        }

        public async UniTask InitAsync()
        {
            var initializePackage = new InitializePackage(playMode, packageName, hostServerIP);
            await initializePackage.StartAsync();
            _defaultPackage = initializePackage.GetPackage(packageName);

            if (!string.IsNullOrEmpty(sceneName))
            {
                YooAssets.LoadSceneSync(sceneName);
            }

            LogModule.Log("Resource module initialized successfully");
            _isInitialized = true;
            // EventModule.Instance.FireNow(this,InitializePackageFinish);
        }

        public async Task<bool> LoadSceneAsync(string assetPath, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return false;
            }

            var operation = YooAssets.LoadSceneAsync(assetPath, loadSceneMode);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                return true;
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return false;
            }
        }

        public bool LoadSceneSync(string assetPath, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return false;
            }

            var operation = YooAssets.LoadSceneSync(assetPath, loadSceneMode);

            if (operation.Status == EOperationStatus.Succeed)
            {
                return true;
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return false;
            }
        }

        // 加载资源
        public async Task<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return null;
            }

            var operation = YooAssets.LoadAssetAsync<T>(assetPath);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                return operation.AssetObject as T;
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return null;
            }
        }

        public async Task<AssetHandle> LoadAssetAsync(string assetPath)
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return null;
            }

            var operation = YooAssets.LoadAssetAsync(assetPath);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                return operation;
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return null;
            }
        }

        public T LoadAssetSync<T>(string assetPath) where T : UnityEngine.Object
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return null;
            }

            var operation = YooAssets.LoadAssetSync(assetPath);

            if (operation.Status == EOperationStatus.Succeed)
            {
                return operation.AssetObject as T;
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return null;
            }
        }

        public AssetInfo LoadAssetSync(string assetPath)
        {
            if (!_isInitialized)
            {
                LogModule.Error("Resource module not initialized");
                return null;
            }

            var operation = YooAssets.LoadAssetSync(assetPath);

            if (operation.Status == EOperationStatus.Succeed)
            {
                return operation.GetAssetInfo();
            }
            else
            {
                LogModule.Error($"Failed to load asset {assetPath}: {operation.LastError}");
                return null;
            }
        }

        // 加载并实例化预制体
        public async Task<GameObject> InstantiatePrefabAsync(string assetPath, Transform parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(assetPath);
            if (prefab == null)
                return null;

            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        // 释放资源
        public void UnloadAsset(AssetInfo asset)
        {
            if (asset == null) return;
            _defaultPackage.TryUnloadUnusedAsset(asset);
        }

        // 释放未使用的资源
        public void UnloadUnusedAssets()
        {
            _defaultPackage.UnloadUnusedAssetsAsync();
            Resources.UnloadUnusedAssets();
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public void OnLateUpdate(float deltaTime)
        {
        }

        public void OnShutdown()
        {
            UnloadUnusedAssets();
            LogModule.Log("Resource module shutdown");
        }
    }
}