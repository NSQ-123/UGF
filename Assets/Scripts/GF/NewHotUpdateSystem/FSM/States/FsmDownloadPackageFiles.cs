using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 下载包文件状态
    /// </summary>
    public class FsmDownloadPackageFiles : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.DownloadPackageFiles;
        
        protected override void OnEnterState()
        {
            StartCoroutine(BeginDownload());
        }
        
        private IEnumerator BeginDownload()
        {
            var downloader = _machine.GetBlackboardValue<ResourceDownloaderOperation>("Downloader");
            
            // 设置下载回调
            downloader.DownloadErrorCallback = (errorData) =>
            {
                _operation?.SendError($"下载文件失败: {errorData.FileName} - {errorData.ErrorInfo}", HotUpdateStage.DownloadPackageFiles);
            };
            
            downloader.DownloadUpdateCallback = (updateData) =>
            {
                var downloadInfo = new DownloadInfo
                {
                    TotalCount = updateData.TotalDownloadCount,
                    CurrentCount = updateData.CurrentDownloadCount,
                    TotalSize = updateData.TotalDownloadBytes,
                    CurrentSize = updateData.CurrentDownloadBytes,
                    CurrentFileName = "" // DownloadUpdateData没有FileName属性，设为空字符串
                };
                
                float progress = (float)updateData.CurrentDownloadCount / updateData.TotalDownloadCount;
                string message = $"{updateData.CurrentDownloadCount}/{updateData.TotalDownloadCount} 文件";
                
                _operation?.SendProgress(progress, message, downloadInfo);
            };
            
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                _operation?.SendError("下载过程中发生错误", HotUpdateStage.DownloadPackageFiles);
                yield break;
            }

            _machine.ChangeState<FsmDownloadPackageOver>();
        }
    }
} 