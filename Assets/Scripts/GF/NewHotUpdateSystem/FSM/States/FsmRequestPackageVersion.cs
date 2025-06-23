using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 请求包版本状态
    /// </summary>
    internal class FsmRequestPackageVersion : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.RequestPackageVersion;
        
        protected override void OnEnterState()
        {
            StartCoroutine(UpdatePackageVersion());
        }
        
        private IEnumerator UpdatePackageVersion()
        {
            var packageName = _machine.GetBlackboardValue<string>("PackageName");
            var playMode = _machine.GetBlackboardValue<YooAsset.EPlayMode>("PlayMode");
            var serverConfig = _machine.GetBlackboardValue<ServerConfig>("ServerConfig");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                _operation?.SendError($"请求包版本失败: {operation.Error}", HotUpdateStage.RequestPackageVersion);
                yield break;
            }
            
            var serverVersion = operation.PackageVersion;
            if (playMode is  EPlayMode.HostPlayMode or EPlayMode.WebPlayMode)
            {
                var localVersion = package.GetPackageVersion();
                Debug.Log($"版本对比检查: 本地={localVersion}, 服务器={serverVersion}");

                // 2. 尝试获取版本策略（非阻塞）
                ServerVersionPolicy serverPolicy = null;
                if (serverConfig != null && !string.IsNullOrEmpty(serverConfig.MainServerURL))
                {
                    // 使用非阻塞方式请求策略，如果失败就使用内置规则
                    yield return VersionPolicyHelper.GetVersionPolicy(serverConfig.MainServerURL,
                        (policy) => serverPolicy = policy,
                        (error) => Debug.LogWarning($"版本策略请求失败，使用内置规则: {error}"));
                }

                // 3. 检查版本兼容性
                var versionResult = VersionCompatibilityManager.CheckVersion(localVersion, serverVersion, serverPolicy);

                if (!versionResult.IsCompatible)
                {
                    _operation?.SendError($"版本不兼容: {versionResult.Reason}", HotUpdateStage.RequestPackageVersion);
                    yield break;
                }

                if (versionResult.Recommendation == UpdateType.ForceUpdate)
                {
                    var message = string.IsNullOrEmpty(versionResult.Message) ? 
                        "需要强制更新，请前往应用商店下载最新版本" : versionResult.Message;
                    _operation?.SendError(message, HotUpdateStage.RequestPackageVersion);
                    yield break;
                }
                Debug.Log($"版本检查通过: {versionResult.Recommendation} - {versionResult.Message}");
            }
            
            _machine.SetBlackboardValue("PackageVersion", serverVersion);
            _machine.ChangeState<FsmUpdatePackageManifest>();
            
        }
        
    }
} 