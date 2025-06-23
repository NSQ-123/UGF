using System.Collections;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 下载完成状态
    /// </summary>
    internal class FsmDownloadPackageOver : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.DownloadPackageOver;
        
        protected override void OnEnterState()
        {
            _machine.ChangeState<FsmClearCacheBundle>();
        }
    }
} 