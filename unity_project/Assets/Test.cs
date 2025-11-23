using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

// @Author ：NiShiqiang
// @Created ：2025/11/19 23:25:47
namespace Game
{
    public class Test : MonoBehaviour
    {
        public string packageName = "HotUpdateTest";
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        private string packageVersion;

        IEnumerator Start()
        {
            // 初始化资源系统
            YooAssets.Initialize();

            yield return InitPackage();
            yield return UpdatePackageVersion();
            yield return UpdateManifest();

            var package = YooAssets.GetPackage("HotUpdateTest");
            YooAssets.SetDefaultPackage(package);

            // AssetHandle handle = package.LoadAssetAsync<GameObject>("Cube");
            // yield return handle;
            // GameObject go = handle.InstantiateSync();
            // Debug.Log($"Cube name is {go.name}");

            LoadCubeTest().Forget();
            LoadTextTest().Forget();
        }

        private async UniTask LoadCubeTest()
        {
            var (asset, handle) = await Load<GameObject>("Cube");
            var cubeGo = GameObject.Instantiate(asset);
            cubeGo.transform.position = new Vector3(0, 0, 0);
            Debug.Log($"Cube name is {cubeGo.name}");
        }

        private async UniTask LoadTextTest()
        {
            var (asset, handle) = await Load<TextAsset>("GamePlay.dll");
            Debug.Log(asset.text.Length);
        }

        private static async UniTask<(T, AssetHandle)> Load<T>(string assetLocation)
            where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetAsync<T>(assetLocation);
            await handle.Task.AsUniTask();
            var asset = handle.GetAssetObject<T>();
            return (asset, handle);
        }

        private IEnumerator InitPackage()
        {
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(
                    defaultHostServer,
                    fallbackHostServer
                );
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                var createParameters = new WebPlayModeParameters();
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
                IRemoteServices remoteServices = new RemoteServices(
                    defaultHostServer,
                    fallbackHostServer
                );
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

            yield return initializationOperation;
        }

        private IEnumerator UpdatePackageVersion()
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
            }
            else
            {
                packageVersion = operation.PackageVersion;
                Debug.Log($"Request package version : {operation.PackageVersion}");
            }
        }

        private IEnumerator UpdateManifest()
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                yield break;
            }
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = "http://127.0.0.1";
            string appVersion = "v1.0";

#if UNITY_EDITOR
            if (
                UnityEditor.EditorUserBuildSettings.activeBuildTarget
                == UnityEditor.BuildTarget.Android
            )
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            else if (
                UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS
            )
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            else if (
                UnityEditor.EditorUserBuildSettings.activeBuildTarget
                == UnityEditor.BuildTarget.WebGL
            )
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
    }
}
