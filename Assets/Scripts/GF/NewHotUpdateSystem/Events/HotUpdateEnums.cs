using System;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 热更新阶段枚举
    /// </summary>
    public enum HotUpdateStage
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 初始化包
        /// </summary>
        InitializePackage = 1,
        
        /// <summary>
        /// 请求包版本
        /// </summary>
        RequestPackageVersion = 2,
        
        /// <summary>
        /// 更新包清单
        /// </summary>
        UpdatePackageManifest = 3,
        
        /// <summary>
        /// 创建下载器
        /// </summary>
        CreateDownloader = 4,
        
        /// <summary>
        /// 下载包文件
        /// </summary>
        DownloadPackageFiles = 5,
        
        /// <summary>
        /// 下载完成
        /// </summary>
        DownloadPackageOver = 6,
        
        /// <summary>
        /// 清理缓存
        /// </summary>
        ClearCacheBundle = 7,
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        StartGame = 8,
        
        /// <summary>
        /// 完成
        /// </summary>
        Completed = 9,
        
        /// <summary>
        /// 失败
        /// </summary>
        Failed = 10
    }

    /// <summary>
    /// 热更新事件参数
    /// </summary>
    [Serializable]
    public class HotUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; set; }
        
        /// <summary>
        /// 当前阶段
        /// </summary>
        public HotUpdateStage CurrentStage { get; set; }
        
        /// <summary>
        /// 上一个阶段（阶段变化时有效）
        /// </summary>
        public HotUpdateStage PreviousStage { get; set; }
        
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 进度 (0-1)
        /// </summary>
        public float Progress { get; set; }
        
        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 下载信息（下载阶段有效）
        /// </summary>
        public DownloadInfo DownloadInfo { get; set; }
    }

    /// <summary>
    /// 下载信息
    /// </summary>
    [Serializable]
    public class DownloadInfo
    {
        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 当前下载文件数
        /// </summary>
        public int CurrentCount { get; set; }
        
        /// <summary>
        /// 总大小（字节）
        /// </summary>
        public long TotalSize { get; set; }
        
        /// <summary>
        /// 当前下载大小（字节）
        /// </summary>
        public long CurrentSize { get; set; }
        
        /// <summary>
        /// 当前下载文件名
        /// </summary>
        public string CurrentFileName { get; set; }
    }

    /// <summary>
    /// 热更新事件委托
    /// </summary>
    public delegate void HotUpdateEventHandler(object sender, HotUpdateEventArgs e);
} 