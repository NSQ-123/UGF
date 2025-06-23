using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 可配置热更新启动器
    /// 提供完整的配置选项，适合项目使用
    /// </summary>
    public class ConfigurableHotUpdateStarter : MonoBehaviour
    {
        [Header("包配置")]
        [SerializeField] private string _packageName = "DefaultPackage";
        [SerializeField] private EPlayMode _playMode = EPlayMode.HostPlayMode;
        
        [Header("服务器配置")]
        [Tooltip("服务器配置文件，如果为空则使用默认配置")]
        [SerializeField] private ServerConfig _serverConfig;
        
        [Header("启动配置")]
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _checkNetworkBeforeStart = true;
        [SerializeField] private bool _checkDiskSpaceBeforeStart = true;
        [SerializeField] private long _requiredDiskSpaceMB = 100; // 需要的磁盘空间（MB）
        
        [Header("调试信息")]
        [SerializeField] private bool _showDebugLog = true;
        
        /// <summary>
        /// 热更新完成事件
        /// </summary>
        public System.Action OnHotUpdateCompleted;
        
        /// <summary>
        /// 热更新失败事件
        /// </summary>
        public System.Action<string> OnHotUpdateFailed;
        
        /// <summary>
        /// 阶段变化事件
        /// </summary>
        public System.Action<HotUpdateStage, HotUpdateStage> OnStageChanged;
        
        /// <summary>
        /// 进度更新事件
        /// </summary>
        public System.Action<float, string, DownloadInfo> OnProgressUpdated;

        private void Start()
        {
            if (_autoStart)
            {
                StartHotUpdate();
            }
        }

        /// <summary>
        /// 开始热更新
        /// </summary>
        [ContextMenu("开始热更新")]
        public void StartHotUpdate()
        {
            StartCoroutine(HotUpdateCoroutine());
        }

        private IEnumerator HotUpdateCoroutine()
        {
            if (_showDebugLog)
                Debug.Log($"开始热更新流程... Package: {_packageName}, PlayMode: {_playMode}");

            // 1. 预检查
            if (!PreCheck())
            {
                yield break;
            }

            // 2. 初始化YooAsset
            YooAssets.Initialize();
            
            // 3. 获取配置
            var config = GetServerConfig();
            
            // 4. 创建热更新操作
            var operation = HotUpdateHelper.StartHotUpdateWithConfig(_packageName, _playMode, config);
            
            // 5. 设置协程运行器（必须在开始操作之前）
            operation.SetCoroutineRunner(this);
            
            // 6. 注册事件
            operation.OnStageChanged += OnStageChangedEvent;
            operation.OnProgressChanged += OnProgressChangedEvent;
            operation.OnError += OnErrorEvent;
            operation.OnCompleted += OnCompletedEvent;
            
            // 7. 开始热更新操作
            HotUpdateHelper.StartOperation(operation);
            
            // 8. 等待完成
            yield return operation;
        }

        /// <summary>
        /// 预检查
        /// </summary>
        private bool PreCheck()
        {
            // 检查网络连接
            if (_checkNetworkBeforeStart && !HotUpdateHelper.IsNetworkAvailable())
            {
                string error = "网络连接不可用，请检查网络设置";
                if (_showDebugLog)
                    Debug.LogError(error);
                OnHotUpdateFailed?.Invoke(error);
                return false;
            }

            // 检查磁盘空间
            if (_checkDiskSpaceBeforeStart)
            {
                long requiredBytes = _requiredDiskSpaceMB * 1024 * 1024; // 转换为字节
                if (!HotUpdateHelper.HasEnoughDiskSpace(requiredBytes))
                {
                    string error = $"磁盘空间不足，需要 {_requiredDiskSpaceMB}MB 可用空间";
                    if (_showDebugLog)
                        Debug.LogError(error);
                    OnHotUpdateFailed?.Invoke(error);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        private ServerConfig GetServerConfig()
        {
            // 优先使用Inspector中配置的ServerConfig
            if (_serverConfig != null)
            {
                if (_showDebugLog)
                    Debug.Log("使用Inspector配置的ServerConfig");
                return _serverConfig;
            }
            
            // 尝试从Resources加载
            var config = HotUpdateHelper.LoadServerConfig();
            if (_showDebugLog)
                Debug.Log("使用Resources加载的ServerConfig");
            
            return config;
        }

        private void OnStageChangedEvent(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
                Debug.Log($"阶段变化: {e.PreviousStage} → {e.CurrentStage}: {e.Message}");
            
            OnStageChanged?.Invoke(e.PreviousStage, e.CurrentStage);
        }

        private void OnProgressChangedEvent(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
            {
                string progressText = $"{e.Progress * 100:F1}%";
                if (e.DownloadInfo != null)
                {
                    string sizeText = $"{HotUpdateHelper.FormatFileSize(e.DownloadInfo.CurrentSize)}/{HotUpdateHelper.FormatFileSize(e.DownloadInfo.TotalSize)}";
                    Debug.Log($"进度: {progressText} - {e.DownloadInfo.CurrentCount}/{e.DownloadInfo.TotalCount} 文件 - {sizeText}");
                }
                else
                {
                    Debug.Log($"进度: {progressText} - {e.Message}");
                }
            }
            
            OnProgressUpdated?.Invoke(e.Progress, e.Message, e.DownloadInfo);
        }

        private void OnErrorEvent(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
                Debug.LogError($"热更新失败: {e.ErrorMessage}");
            
            OnHotUpdateFailed?.Invoke(e.ErrorMessage);
        }

        private void OnCompletedEvent(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
                Debug.Log("热更新完成！");
            
            // 设置默认包
            var gamePackage = YooAssets.GetPackage(_packageName);
            YooAssets.SetDefaultPackage(gamePackage);
            
            OnHotUpdateCompleted?.Invoke();
            
            //TODO:切换到主页面场景
            YooAssets.LoadSceneAsync("scene_home");
        }

        /// <summary>
        /// 获取当前配置信息
        /// </summary>
        public string GetConfigInfo()
        {
            var config = GetServerConfig();
            return $"包名: {_packageName}\n" +
                   $"运行模式: {_playMode}\n" +
                   $"服务器: {config.GetFullServerURL()}\n" +
                   $"版本: {config.AppVersion}\n" +
                   $"并发下载: {config.MaxConcurrentDownloads}\n" +
                   $"重试次数: {config.DownloadRetryCount}";
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateConfig()
        {
            var config = GetServerConfig();
            return config.Validate();
        }
        
        /// <summary>
        /// 设置服务器配置
        /// </summary>
        public void SetServerConfig(ServerConfig config)
        {
            _serverConfig = config;
        }
        
        /// <summary>
        /// 从Resources加载服务器配置
        /// </summary>
        [ContextMenu("从Resources加载配置")]
        public void LoadConfigFromResources()
        {
            _serverConfig = HotUpdateHelper.LoadServerConfig();
            if (_showDebugLog)
                Debug.Log("已从Resources加载ServerConfig");
        }
    }
} 