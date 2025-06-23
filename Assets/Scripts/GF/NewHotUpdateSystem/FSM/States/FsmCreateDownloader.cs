using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 创建下载器状态
    /// </summary>
    public class FsmCreateDownloader : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.CreateDownloader;
        
        protected override void OnEnterState()
        {
            CreateDownloader();
        }
        
        void CreateDownloader()
        {
            var packageName = _machine.GetBlackboardValue<string>("PackageName");
            var serverConfig = _machine.GetBlackboardValue<ServerConfig>("ServerConfig");
            var package = YooAssets.GetPackage(packageName);

            int downloadingMaxNum = serverConfig.MaxConcurrentDownloads;
            int failedTryAgain = serverConfig.DownloadRetryCount;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            _machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files!");
                _machine.ChangeState<FsmStartGame>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                
                var downloadInfo = new DownloadInfo
                {
                    TotalCount = totalDownloadCount,
                    TotalSize = totalDownloadBytes
                };
                
                _operation?.SendProgress(0f, $"发现 {totalDownloadCount} 个更新文件", downloadInfo);
                _machine.ChangeState<FsmDownloadPackageFiles>();
            }
        }
    }
} 