using System;
using Cysharp.Threading.Tasks;
using Framework.Core.Events;
using UnityEngine;
using YooAsset;

namespace Framework.Core
{
    public class InitializePackage
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        private EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        private string PackageName = "DefaultPackage";

        public ResourcePackage GetPackage(string packageName)
        {
            return YooAssets.GetPackage(packageName);
        }

        public InitializePackage(EPlayMode playMode, string packageName, string hostServerIP)
        {
            PlayMode = playMode;
            PackageName = packageName;
            this.hostServerIP = hostServerIP;
        }

        public async UniTask StartAsync()
        {
            LogModule.Log($"资源系统运行模式：{PlayMode}");
            Application.targetFrameRate = 60;
            Application.runInBackground = true;

            // 初始化资源系统
            YooAssets.Initialize();

            // 加载更新页面
            var go = Resources.Load<GameObject>("PatchWindow");
            var ins = GameObject.Instantiate(go);

#if ENABLE_WEBGL || PLATFORM_WEBGL
            YooAssets.SetCacheSystemDisableCacheOnWebGL();
#endif
            EventModule.Instance.Subscribe(EventIds.UpdatePackageCallbackID, UpdatePackageCallbackEvent);

            // 开始补丁更新流程
            // var operation = new PatchOperation("DefaultPackage", PlayMode);
            // YooAssets.StartOperation(operation);
            await InitPackage();

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);

            // 切换到主页面场景
            // SceneEventDefine.ChangeToHomeScene.SendEventMessage();
            // YooAssets.LoadSceneSync(SceneName);
            ins.SetActive(false);
            LogModule.Log("完成");
            EventModule.Instance.FireNow(this, InitializeAssetsFinishEventArgs.Create(this, true));
        }

        private void UpdatePackageCallbackEvent(object sender, GameEventArgs e)
        {
            var args = e as UpdatePackageCallbackEventArgs;
            switch (args.callbackType)
            {
                case UpdatePackageCallbackType.再次初始化资源包:
                    this.InitPackage().Forget();
                    break;
                case UpdatePackageCallbackType.开始下载网络文件:
                    this.FsmDownloadPackageFiles().Forget();
                    break;
                case UpdatePackageCallbackType.再次更新静态版本:
                    this.UpdatePackageVersion().Forget();
                    break;
                case UpdatePackageCallbackType.再次更新补丁清单:
                    this.FsmUpdatePackageManifest().Forget();
                    break;
                case UpdatePackageCallbackType.再次下载网络文件:
                    this.FsmDownloadPackageFiles().Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region FsmInitializePackage

        private async UniTask InitPackage()
        {
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(PackageName);
            if (package == null)
                package = YooAssets.CreatePackage(PackageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(PackageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (PlayMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (PlayMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                LogModule.Log(defaultHostServer);
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (PlayMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
 WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
#endif
            }

            await initializationOperation.Task;

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                LogModule.Warning($"{initializationOperation.Error}");
                // PatchEventDefine.InitializeFailed.SendEventMessage();
                EventModule.Instance.FireNow(this, InitializeFailedEventArgs.Create(this));
            }
            else
            {
                // _machine.ChangeState<FsmRequestPackageVersion>();
                await UpdatePackageVersion();
            }
        }

        string hostServerIP = "http://127.0.0.1";

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            // string hostServerIP = "http://127.0.0.1";
            string hostServerIP = "https://kiosk-assets-bundle.oss-cn-hongkong.aliyuncs.com/Framework";
            string appVersion = "v1.0";

#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            else
                return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }

        #endregion

        #region FsmRequestPackageVersion

        private async UniTask UpdatePackageVersion()
        {
            LogModule.Log("请求资源版本 !");
            var package = YooAssets.GetPackage(PackageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                LogModule.Warning(operation.Error);
                EventModule.Instance.FireNow(this, PackageVersionUpdateFailedEventArgs.Create(this));
            }
            else
            {
                LogModule.Log($"Request package version : {operation.PackageVersion}");
                packageVersion = operation.PackageVersion;
                await FsmUpdatePackageManifest();
            }
        }

        string packageVersion;

        #endregion

        #region FsmUpdatePackageManifest

        private async UniTask FsmUpdatePackageManifest()
        {
            LogModule.Log("更新资源清单！");
            var package = YooAssets.GetPackage(PackageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                LogModule.Warning(operation.Error);
                EventModule.Instance.FireNow(this, PatchManifestUpdateFailedEventArgs.Create(this));
                return;
            }
            else
            {
                await FsmCreateDownloader();
            }
        }

        #endregion

        #region FsmCreateDownloader

        private async UniTask FsmCreateDownloader()
        {
            await UniTask.Yield();
            LogModule.Log("创建资源下载器！");
            CreateDownloader();
        }

        ResourceDownloaderOperation downloader;

        void CreateDownloader()
        {
            var package = YooAssets.GetPackage(PackageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            // _machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                LogModule.Log("Not found any download files !");
                // _machine.ChangeState<FsmStartGame>();
                UniTask.Run(FsmStartGame);
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                EventModule.Instance.FireNow(this,
                    FoundUpdateFilesEventArgs.Create(this, totalDownloadCount, totalDownloadBytes));
            }
        }

        #endregion

        #region FsmDownloadPackageFiles

        private async UniTask FsmDownloadPackageFiles()
        {
            await UniTask.Yield();
            LogModule.Log("开始下载资源文件！");
            downloader.DownloadErrorCallback = DownloadErrorCallback;
            downloader.DownloadUpdateCallback = DownloadProgressCallback;
            downloader.BeginDownload();
            await downloader.Task;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            await FsmDownloadPackageOver();

            void DownloadErrorCallback(DownloadErrorData data)
            {
                LogModule.Log($"下载补丁出错！{data.ErrorInfo}");
                EventModule.Instance.FireNow(this,
                    WebFileDownloadFailedEventArgs.Create(this, data.FileName, data.ErrorInfo));
            }

            void DownloadProgressCallback(DownloadUpdateData data)
            {
                LogModule.Log(
                    $"下载进度：{data.CurrentDownloadCount}/{data.TotalDownloadCount} , {data.CurrentDownloadBytes}/{data.TotalDownloadBytes}！");
                EventModule.Instance.FireNow(this,
                    DownloadProgressUpdateEventArgs.Create(this, data.CurrentDownloadCount, data.TotalDownloadCount,
                        data.CurrentDownloadBytes, data.TotalDownloadBytes));
            }
        }

        #endregion

        #region FsmDownloadPackageOver

        private async UniTask FsmDownloadPackageOver()
        {
            await UniTask.Yield();
            LogModule.Log("资源文件下载完毕！");
            await FsmClearCacheBundle();
        }

        #endregion

        #region FsmClearCacheBundle

        private async UniTask FsmClearCacheBundle()
        {
            await UniTask.Yield();
            LogModule.Log("清理未使用的缓存文件！");
            var package = YooAssets.GetPackage(PackageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += Operation_Completed;
        }

        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            UniTask.Run(FsmStartGame);
        }

        #endregion

        #region FsmStartGame

        private async UniTask FsmStartGame()
        {
            await UniTask.Yield();
            LogModule.Log("开始游戏！");
        }

        #endregion
    }
}