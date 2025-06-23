using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 清理缓存状态
    /// </summary>
    internal class FsmClearCacheBundle : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.ClearCacheBundle;
        
        protected override void OnEnterState()
        {
            var serverConfig = _machine.GetBlackboardValue<ServerConfig>("ServerConfig");
            
            if (serverConfig.AutoClearCache)
            {
                var packageName = _machine.GetBlackboardValue<string>("PackageName");
                var package = YooAssets.GetPackage(packageName);
                var operation = package.ClearCacheFilesAsync(serverConfig.CacheClearMode);
                operation.Completed += Operation_Completed;
            }
            else
            {
                _machine.ChangeState<FsmStartGame>();
            }
        }
        
        private void Operation_Completed(AsyncOperationBase obj)
        {
            _machine.ChangeState<FsmStartGame>();
        }
    }
} 