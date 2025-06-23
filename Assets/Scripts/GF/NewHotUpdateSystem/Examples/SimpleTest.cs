using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 简单测试脚本
    /// </summary>
    public class SimpleTest : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private bool _autoTest = true;
        [SerializeField] private string _packageName = "DefaultPackage";
        [SerializeField] private EPlayMode _playMode = EPlayMode.HostPlayMode;

        private void Start()
        {
            if (_autoTest)
            {
                StartCoroutine(TestHotUpdate());
            }
        }

        private IEnumerator TestHotUpdate()
        {
            Debug.Log("开始测试新热更新系统...");
            
            // 1. 初始化YooAsset
            YooAssets.Initialize();
            Debug.Log("YooAsset 初始化完成");
            
            // 2. 创建热更新操作
            var operation = new PatchOperation(_packageName, _playMode);
            operation.SetCoroutineRunner(this);
            
            // 3. 注册事件
            operation.OnStageChanged += OnStageChanged;
            operation.OnProgressChanged += OnProgressChanged;
            operation.OnError += OnError;
            operation.OnCompleted += OnCompleted;
            
            // 4. 开始热更新
            YooAssets.StartOperation(operation);
            yield return operation;
            
            Debug.Log("测试完成");
        }

        private void OnStageChanged(object sender, HotUpdateEventArgs e)
        {
            Debug.Log($"[阶段变化] {e.PreviousStage} → {e.CurrentStage}: {e.Message}");
        }

        private void OnProgressChanged(object sender, HotUpdateEventArgs e)
        {
            if (e.Progress > 0)
            {
                Debug.Log($"[进度] {e.Progress * 100:F1}% - {e.Message}");
            }
        }

        private void OnError(object sender, HotUpdateEventArgs e)
        {
            Debug.LogError($"[错误] {e.ErrorMessage}");
        }

        private void OnCompleted(object sender, HotUpdateEventArgs e)
        {
            Debug.Log("[完成] 热更新完成");
        }

        [ContextMenu("测试工具类")]
        public void TestHelper()
        {
            StartCoroutine(TestHelperCoroutine());
        }

        private IEnumerator TestHelperCoroutine()
        {
            Debug.Log("测试工具类...");
            
            // 使用工具类的极简模式
            var operation = HotUpdateHelper.StartHotUpdate(_playMode);
            operation.SetCoroutineRunner(this);
            operation.OnCompleted += (sender, e) => Debug.Log("工具类测试完成");
            
            yield return operation;
        }
    }
} 