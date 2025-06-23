using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 热更新使用示例
    /// </summary>
    public class HotUpdateExample : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private ServerConfig _serverConfig;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private string _packageName = "DefaultPackage";
        [SerializeField] private EPlayMode _playMode = EPlayMode.HostPlayMode;

        [Header("调试信息")]
        [SerializeField] private bool _showDebugInfo = true;

        private void Start()
        {
            if (_autoStart)
            {
                StartCoroutine(InitializeAndStartHotUpdate());
            }
        }

        /// <summary>
        /// 初始化并开始热更新
        /// </summary>
        private IEnumerator InitializeAndStartHotUpdate()
        {
            // 1. 初始化YooAsset
            YooAssets.Initialize();

            // 2. 创建热更新操作
            var operation = new PatchOperation(_packageName, _playMode, _serverConfig);
            
            // 3. 设置协程运行器
            operation.SetCoroutineRunner(this);
            
            // 4. 注册事件监听
            operation.OnStageChanged += OnStageChanged;
            operation.OnProgressChanged += OnProgressChanged;
            operation.OnError += OnError;
            operation.OnCompleted += OnCompleted;

            // 5. 检查网络连接
            if (!HotUpdateHelper.IsNetworkAvailable())
            {
                Debug.LogWarning("Network not available!");
            }

            // 6. 开始热更新
            YooAssets.StartOperation(operation);
            yield return operation;

            // 7. 设置默认包
            var gamePackage = YooAssets.GetPackage(_packageName);
            YooAssets.SetDefaultPackage(gamePackage);

            Debug.Log("Hot update completed!");
        }

        /// <summary>
        /// 手动开始热更新
        /// </summary>
        [ContextMenu("Start Hot Update")]
        public void StartHotUpdate()
        {
            StartCoroutine(InitializeAndStartHotUpdate());
        }

        /// <summary>
        /// 使用工具类启动热更新
        /// </summary>
        [ContextMenu("Start Hot Update with Helper")]
        public void StartHotUpdateWithHelper()
        {
            StartCoroutine(StartHotUpdateWithHelperCoroutine());
        }

        private IEnumerator StartHotUpdateWithHelperCoroutine()
        {
            // 使用工具类的极简模式
            var operation = HotUpdateHelper.StartHotUpdate(_playMode);
            
            // 注册事件
            operation.OnStageChanged += OnStageChanged;
            operation.OnProgressChanged += OnProgressChanged;
            operation.OnError += OnError;
            operation.OnCompleted += OnCompleted;
            
            // 设置协程运行器
            operation.SetCoroutineRunner(this);
            
            yield return operation;
        }

        /// <summary>
        /// 批量更新多个包
        /// </summary>
        [ContextMenu("Update Multiple Packages")]
        public void UpdateMultiplePackages()
        {
            StartCoroutine(UpdateMultiplePackagesCoroutine());
        }

        private IEnumerator UpdateMultiplePackagesCoroutine()
        {
            string[] packageNames = { "DefaultPackage", "UI", "Audio" };
            yield return HotUpdateHelper.UpdatePackages(packageNames, _playMode, _serverConfig);
            Debug.Log("Multiple packages updated!");
        }

        /// <summary>
        /// 阶段变化事件
        /// </summary>
        private void OnStageChanged(object sender, HotUpdateEventArgs e)
        {
            if (!_showDebugInfo) return;

            if (e.PreviousStage != HotUpdateStage.None)
            {
                Debug.Log($"[{e.PackageName}] 阶段变化: {GetStageDescription(e.PreviousStage)} → {GetStageDescription(e.CurrentStage)}");
            }
            else
            {
                Debug.Log($"[{e.PackageName}] 进入阶段: {GetStageDescription(e.CurrentStage)}");
            }
        }

        /// <summary>
        /// 进度变化事件
        /// </summary>
        private void OnProgressChanged(object sender, HotUpdateEventArgs e)
        {
            if (!_showDebugInfo) return;

            string stageName = GetStageDescription(e.CurrentStage);
            string progressText = $"{e.Progress * 100:F1}%";
            
            if (e.DownloadInfo != null)
            {
                string sizeText = $"{HotUpdateHelper.FormatFileSize(e.DownloadInfo.CurrentSize)}/{HotUpdateHelper.FormatFileSize(e.DownloadInfo.TotalSize)}";
                Debug.Log($"[{e.PackageName}] {stageName} - {progressText} - {e.DownloadInfo.CurrentCount}/{e.DownloadInfo.TotalCount} 文件 - {sizeText}");
            }
            else
            {
                Debug.Log($"[{e.PackageName}] {stageName} - {progressText} - {e.Message}");
            }
        }

        /// <summary>
        /// 错误事件
        /// </summary>
        private void OnError(object sender, HotUpdateEventArgs e)
        {
            string stageName = GetStageDescription(e.CurrentStage);
            Debug.LogError($"[{e.PackageName}] {stageName} 发生错误: {e.ErrorMessage}");
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        private void OnCompleted(object sender, HotUpdateEventArgs e)
        {
            Debug.Log($"[{e.PackageName}] 热更新完成");
        }

        /// <summary>
        /// 获取阶段描述
        /// </summary>
        private string GetStageDescription(HotUpdateStage stage)
        {
            return stage switch
            {
                HotUpdateStage.None => "未开始",
                HotUpdateStage.InitializePackage => "初始化资源包",
                HotUpdateStage.RequestPackageVersion => "请求资源版本",
                HotUpdateStage.UpdatePackageManifest => "更新资源清单",
                HotUpdateStage.CreateDownloader => "创建资源下载器",
                HotUpdateStage.DownloadPackageFiles => "下载资源文件",
                HotUpdateStage.DownloadPackageOver => "资源文件下载完毕",
                HotUpdateStage.ClearCacheBundle => "清理未使用的缓存文件",
                HotUpdateStage.StartGame => "开始游戏",
                HotUpdateStage.Completed => "完成",
                HotUpdateStage.Failed => "失败",
                _ => "未知阶段"
            };
        }

        private void OnGUI()
        {
            if (!_showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Hot Update Debug Info", GUI.skin.box);

            if (GUILayout.Button("Start Hot Update"))
            {
                StartHotUpdate();
            }

            if (GUILayout.Button("Start with Helper"))
            {
                StartHotUpdateWithHelper();
            }

            if (GUILayout.Button("Update Multiple Packages"))
            {
                UpdateMultiplePackages();
            }

            GUILayout.EndArea();
        }
    }
} 