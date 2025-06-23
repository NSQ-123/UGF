using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 简单热更新启动器
    /// 提供最基础的热更新启动功能，适合快速集成
    /// </summary>
    public class SimpleHotUpdateStarter : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private string _packageName = "DefaultPackage";
        [SerializeField] private EPlayMode _playMode = EPlayMode.HostPlayMode;
        [SerializeField] private bool _autoStart = true;
        
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
        /// 进度更新事件
        /// </summary>
        public System.Action<float, string> OnProgressUpdated;

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
                Debug.Log("开始热更新流程...");

            // 1. 初始化YooAsset
            YooAssets.Initialize();
            
            // 2. 创建热更新操作
            var operation = HotUpdateHelper.StartHotUpdate(_playMode);
            
            // 3. 设置协程运行器（必须在开始操作之前）
            operation.SetCoroutineRunner(this);
            
            // 4. 注册事件
            operation.OnProgressChanged += OnProgressChanged;
            operation.OnError += OnError;
            operation.OnCompleted += OnCompleted;
            
            // 5. 开始热更新操作
            HotUpdateHelper.StartOperation(operation);
            
            // 6. 等待完成
            yield return operation;
        }

        private void OnProgressChanged(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
                Debug.Log($"进度: {e.Progress * 100:F1}% - {e.Message}");
            
            OnProgressUpdated?.Invoke(e.Progress, e.Message);
        }

        private void OnError(object sender, HotUpdateEventArgs e)
        {
            if (_showDebugLog)
                Debug.LogError($"热更新失败: {e.ErrorMessage}");
            
            OnHotUpdateFailed?.Invoke(e.ErrorMessage);
        }

        private void OnCompleted(object sender, HotUpdateEventArgs e)
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
        /// 检查网络连接
        /// </summary>
        public bool IsNetworkAvailable()
        {
            return HotUpdateHelper.IsNetworkAvailable();
        }

        /// <summary>
        /// 检查磁盘空间
        /// </summary>
        public bool HasEnoughDiskSpace(long requiredBytes)
        {
            return HotUpdateHelper.HasEnoughDiskSpace(requiredBytes);
        }
    }
} 