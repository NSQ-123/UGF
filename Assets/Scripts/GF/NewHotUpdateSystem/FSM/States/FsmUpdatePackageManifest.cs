using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 更新包清单状态
    /// </summary>
    public class FsmUpdatePackageManifest : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.UpdatePackageManifest;
        
        protected override void OnEnterState()
        {
            StartCoroutine(UpdateManifest());
        }
        
        private IEnumerator UpdateManifest()
        {
            var packageName = _machine.GetBlackboardValue<string>("PackageName");
            var packageVersion = _machine.GetBlackboardValue<string>("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                _operation?.SendError($"更新包清单失败: {operation.Error}", HotUpdateStage.UpdatePackageManifest);
                yield break;
            }
            else
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
} 