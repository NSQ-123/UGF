using System;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 初始化包状态
    /// </summary>
    internal class FsmInitializePackage : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.InitializePackage;
        
        protected override void OnEnterState()
        {
            StartCoroutine(InitPackage());
        }
        
        private IEnumerator InitPackage()
        {
            Debug.Log("FsmInitializePackage : InitPackage");
            var playMode = _machine.GetBlackboardValue<YooAsset.EPlayMode>("PlayMode");
            var packageName = _machine.GetBlackboardValue<string>("PackageName");
            var serverConfig = _machine.GetBlackboardValue<ServerConfig>("ServerConfig");

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 根据运行模式初始化
            InitializationOperation initializationOperation = null;
            
            switch (playMode)
            {
                case YooAsset.EPlayMode.EditorSimulateMode:
                    initializationOperation = InitEditorSimulateMode(package, packageName);
                    break;
                case YooAsset.EPlayMode.OfflinePlayMode:
                    initializationOperation = InitOfflinePlayMode(package);
                    break;
                case YooAsset.EPlayMode.HostPlayMode:
                    initializationOperation = InitHostPlayMode(package, serverConfig);
                    break;
                case YooAsset.EPlayMode.WebPlayMode:
                    initializationOperation = InitWebPlayMode(package, serverConfig);
                    break;
                default:
                    _operation?.SendError($"不支持的运行模式: {playMode}", HotUpdateStage.InitializePackage);
                    yield break;
            }

            yield return initializationOperation;

            // 检查初始化结果
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                _operation?.SendError($"初始化失败: {initializationOperation.Error}", HotUpdateStage.InitializePackage);
                yield break;
            }
            else
            {
                Debug.Log("FsmInitializePackage->FsmRequestPackageVersion");
                _machine.ChangeState<FsmRequestPackageVersion>();
            }
        }
        
        private InitializationOperation InitEditorSimulateMode(ResourcePackage package, string packageName)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            return package.InitializeAsync(createParameters);
        }
        
        private InitializationOperation InitOfflinePlayMode(ResourcePackage package)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            return package.InitializeAsync(createParameters);
        }
        
        private InitializationOperation InitHostPlayMode(ResourcePackage package, ServerConfig serverConfig)
        {
            string defaultHostServer = serverConfig.GetFullServerURL(false);
            string fallbackHostServer = serverConfig.GetFullServerURL(true);
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            
            return package.InitializeAsync(createParameters);
        }
        
        private InitializationOperation InitWebPlayMode(ResourcePackage package, ServerConfig serverConfig)
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
            string defaultHostServer = serverConfig.GetFullServerURL(false);
            string fallbackHostServer = serverConfig.GetFullServerURL(true);
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            return package.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            return package.InitializeAsync(createParameters);
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
    }
} 