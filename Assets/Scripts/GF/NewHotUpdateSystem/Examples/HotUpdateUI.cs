using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 热更新UI界面
    /// 提供完整的热更新进度显示和交互功能
    /// </summary>
    public class HotUpdateUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private GameObject _completedPanel;
        
        [Header("进度显示")]
        [SerializeField] private Text _stageText;
        [SerializeField] private Text _progressText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private Text _messageText;
        [SerializeField] private Text _speedText;
        [SerializeField] private Text _sizeText;
        
        [Header("错误处理")]
        [SerializeField] private Text _errorText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _skipButton;
        
        [Header("完成界面")]
        [SerializeField] private Text _completedText;
        [SerializeField] private Button _startGameButton;
        
        [Header("配置")]
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private float _autoStartDelay = 2f;
        [SerializeField] private string _mainSceneName = "MainScene";
        
        [Header("调试")]
        [SerializeField] private bool _showDebugInfo = true;
        
        private ConfigurableHotUpdateStarter _hotUpdateStarter;
        private float _lastProgressTime;
        private long _lastDownloadedBytes;
        private string _currentSpeed = "";

        private void Start()
        {
            InitializeUI();
            
            if (_autoStart)
            {
                StartCoroutine(AutoStartHotUpdate());
            }
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 初始化面板状态
            if (_loadingPanel != null) _loadingPanel.SetActive(true);
            if (_errorPanel != null) _errorPanel.SetActive(false);
            if (_completedPanel != null) _completedPanel.SetActive(false);
            
            // 初始化进度条
            if (_progressSlider != null) _progressSlider.value = 0f;
            if (_progressText != null) _progressText.text = "0%";
            if (_stageText != null) _stageText.text = "准备中...";
            if (_messageText != null) _messageText.text = "";
            if (_speedText != null) _speedText.text = "";
            if (_sizeText != null) _sizeText.text = "";
            
            // 绑定按钮事件
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);
            if (_startGameButton != null) _startGameButton.onClick.AddListener(OnStartGameClicked);
            
            // 创建热更新启动器
            _hotUpdateStarter = gameObject.AddComponent<ConfigurableHotUpdateStarter>();
            _hotUpdateStarter.OnHotUpdateCompleted += OnHotUpdateCompleted;
            _hotUpdateStarter.OnHotUpdateFailed += OnHotUpdateFailed;
            _hotUpdateStarter.OnStageChanged += OnStageChanged;
            _hotUpdateStarter.OnProgressUpdated += OnProgressUpdated;
        }

        /// <summary>
        /// 自动开始热更新
        /// </summary>
        private IEnumerator AutoStartHotUpdate()
        {
            yield return new WaitForSeconds(0.5f);
            StartHotUpdate();
        }

        /// <summary>
        /// 开始热更新
        /// </summary>
        [ContextMenu("开始热更新")]
        public void StartHotUpdate()
        {
            if (_showDebugInfo)
                Debug.Log("UI开始热更新...");
            
            // 重置UI状态
            ResetUI();
            
            // 开始热更新
            _hotUpdateStarter.StartHotUpdate();
        }

        /// <summary>
        /// 重置UI状态
        /// </summary>
        private void ResetUI()
        {
            if (_loadingPanel != null) _loadingPanel.SetActive(true);
            if (_errorPanel != null) _errorPanel.SetActive(false);
            if (_completedPanel != null) _completedPanel.SetActive(false);
            
            if (_progressSlider != null) _progressSlider.value = 0f;
            if (_progressText != null) _progressText.text = "0%";
            if (_stageText != null) _stageText.text = "准备中...";
            if (_messageText != null) _messageText.text = "";
            if (_speedText != null) _speedText.text = "";
            if (_sizeText != null) _sizeText.text = "";
            
            _lastProgressTime = Time.time;
            _lastDownloadedBytes = 0;
            _currentSpeed = "";
        }

        /// <summary>
        /// 阶段变化事件
        /// </summary>
        private void OnStageChanged(HotUpdateStage previousStage, HotUpdateStage currentStage)
        {
            if (_stageText != null)
            {
                _stageText.text = GetStageDescription(currentStage);
            }
            
            if (_showDebugInfo)
                Debug.Log($"阶段变化: {previousStage} → {currentStage}");
        }

        /// <summary>
        /// 进度更新事件
        /// </summary>
        private void OnProgressUpdated(float progress, string message, DownloadInfo downloadInfo)
        {
            // 更新进度条
            if (_progressSlider != null)
            {
                _progressSlider.value = progress;
            }
            
            // 更新进度文本
            if (_progressText != null)
            {
                _progressText.text = $"{progress * 100:F1}%";
            }
            
            // 更新消息
            if (_messageText != null)
            {
                _messageText.text = message;
            }
            
            // 更新下载信息
            if (downloadInfo != null)
            {
                UpdateDownloadInfo(downloadInfo);
            }
        }

        /// <summary>
        /// 更新下载信息
        /// </summary>
        private void UpdateDownloadInfo(DownloadInfo downloadInfo)
        {
            // 计算下载速度
            float currentTime = Time.time;
            float timeDiff = currentTime - _lastProgressTime;
            
            if (timeDiff > 0)
            {
                long bytesDiff = downloadInfo.CurrentSize - _lastDownloadedBytes;
                float speedBytesPerSecond = bytesDiff / timeDiff;
                _currentSpeed = HotUpdateHelper.FormatFileSize((long)speedBytesPerSecond) + "/s";
                
                if (_speedText != null)
                {
                    _speedText.text = $"速度: {_currentSpeed}";
                }
            }
            
            // 更新大小信息
            if (_sizeText != null)
            {
                string sizeText = $"{HotUpdateHelper.FormatFileSize(downloadInfo.CurrentSize)}/{HotUpdateHelper.FormatFileSize(downloadInfo.TotalSize)}";
                _sizeText.text = $"大小: {sizeText}";
            }
            
            _lastProgressTime = currentTime;
            _lastDownloadedBytes = downloadInfo.CurrentSize;
        }

        /// <summary>
        /// 热更新完成事件
        /// </summary>
        private void OnHotUpdateCompleted()
        {
            if (_showDebugInfo)
                Debug.Log("热更新完成！");
            
            // 显示完成界面
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
            if (_completedPanel != null) _completedPanel.SetActive(true);
            
            if (_completedText != null)
            {
                _completedText.text = "热更新完成！\n准备开始游戏...";
            }
            
            // 自动开始游戏
            StartCoroutine(AutoStartGame());
        }

        /// <summary>
        /// 热更新失败事件
        /// </summary>
        private void OnHotUpdateFailed(string error)
        {
            if (_showDebugInfo)
                Debug.LogError($"热更新失败: {error}");
            
            // 显示错误界面
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
            if (_errorPanel != null) _errorPanel.SetActive(true);
            
            if (_errorText != null)
            {
                _errorText.text = $"热更新失败:\n{error}";
            }
        }

        /// <summary>
        /// 重试按钮点击
        /// </summary>
        private void OnRetryClicked()
        {
            if (_showDebugInfo)
                Debug.Log("重试热更新...");
            
            StartHotUpdate();
        }

        /// <summary>
        /// 跳过按钮点击
        /// </summary>
        private void OnSkipClicked()
        {
            if (_showDebugInfo)
                Debug.Log("跳过热更新...");
            
            StartCoroutine(AutoStartGame());
        }

        /// <summary>
        /// 开始游戏按钮点击
        /// </summary>
        private void OnStartGameClicked()
        {
            StartCoroutine(LoadMainScene());
        }

        /// <summary>
        /// 自动开始游戏
        /// </summary>
        private IEnumerator AutoStartGame()
        {
            yield return new WaitForSeconds(_autoStartDelay);
            yield return LoadMainScene();
        }

        /// <summary>
        /// 加载主场景
        /// </summary>
        private IEnumerator LoadMainScene()
        {
            if (_showDebugInfo)
                Debug.Log($"加载主场景: {_mainSceneName}");
            
            // 这里可以添加场景切换效果
            yield return new WaitForSeconds(0.5f);
            
            // 加载主场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(_mainSceneName);
        }

        /// <summary>
        /// 获取阶段描述
        /// </summary>
        private string GetStageDescription(HotUpdateStage stage)
        {
            return stage switch
            {
                HotUpdateStage.None => "准备中...",
                HotUpdateStage.InitializePackage => "初始化资源包",
                HotUpdateStage.RequestPackageVersion => "请求资源版本",
                HotUpdateStage.UpdatePackageManifest => "更新资源清单",
                HotUpdateStage.CreateDownloader => "创建下载器",
                HotUpdateStage.DownloadPackageFiles => "下载资源文件",
                HotUpdateStage.DownloadPackageOver => "下载完成",
                HotUpdateStage.ClearCacheBundle => "清理缓存",
                HotUpdateStage.StartGame => "准备开始游戏",
                HotUpdateStage.Completed => "完成",
                HotUpdateStage.Failed => "失败",
                _ => "未知阶段"
            };
        }

        /// <summary>
        /// 设置配置信息
        /// </summary>
        public void SetConfig(string serverURL, string appVersion = "v1.0")
        {
            if (_hotUpdateStarter != null)
            {
                // 这里可以通过反射或其他方式设置配置
                // 为了简化，我们直接修改组件
                var configStarter = _hotUpdateStarter as ConfigurableHotUpdateStarter;
                if (configStarter != null)
                {
                    // 可以通过序列化字段或其他方式设置
                }
            }
        }

        private void OnDestroy()
        {
            // 清理事件绑定
            if (_hotUpdateStarter != null)
            {
                _hotUpdateStarter.OnHotUpdateCompleted -= OnHotUpdateCompleted;
                _hotUpdateStarter.OnHotUpdateFailed -= OnHotUpdateFailed;
                _hotUpdateStarter.OnStageChanged -= OnStageChanged;
                _hotUpdateStarter.OnProgressUpdated -= OnProgressUpdated;
            }
        }
    }
} 