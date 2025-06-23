using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 热更新操作
    /// </summary>
    public class PatchOperation : GameAsyncOperation
    {
        private enum ESteps
        {
            None,
            Update,
            Done,
        }

        private readonly StateMachine _machine;
        private readonly string _packageName;
        private readonly YooAsset.EPlayMode _playMode;
        private readonly ServerConfig _serverConfig;
        private ESteps _steps = ESteps.None;
        
        // 阶段管理
        private HotUpdateStage _currentStage = HotUpdateStage.None;
        private HotUpdateStage _previousStage = HotUpdateStage.None;
        
        // 协程管理
        private MonoBehaviour _coroutineRunner;
        
        /// <summary>
        /// 当前阶段
        /// </summary>
        public HotUpdateStage CurrentStage => _currentStage;
        
        /// <summary>
        /// 上一个阶段
        /// </summary>
        public HotUpdateStage PreviousStage => _previousStage;
        
        /// <summary>
        /// 阶段变化事件
        /// </summary>
        public event HotUpdateEventHandler OnStageChanged;
        
        /// <summary>
        /// 进度变化事件
        /// </summary>
        public event HotUpdateEventHandler OnProgressChanged;
        
        /// <summary>
        /// 错误事件
        /// </summary>
        public event HotUpdateEventHandler OnError;
        
        /// <summary>
        /// 完成事件
        /// </summary>
        public event HotUpdateEventHandler OnCompleted;

        public PatchOperation(string packageName, YooAsset.EPlayMode playMode, ServerConfig serverConfig = null)
        {
            _packageName = packageName;
            _playMode = playMode;
            _serverConfig = serverConfig ?? ServerConfig.CreateDefault();

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmDownloadPackageOver>();
            _machine.AddNode<FsmClearCacheBundle>();
            _machine.AddNode<FsmStartGame>();

            // 设置状态机数据
            _machine.SetBlackboardValue("PackageName", packageName);
            _machine.SetBlackboardValue("PlayMode", playMode);
            _machine.SetBlackboardValue("ServerConfig", _serverConfig);
        }
        
        protected override void OnStart()
        {
            _steps = ESteps.Update;
            Debug.Log("PatchOperation->FsmInitializePackage");
            _machine.Run<FsmInitializePackage>();
        }
        
        protected override void OnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Update)
            {
                _machine.Update();
            }
        }
        
        protected override void OnAbort()
        {
            // 可以在这里添加中止逻辑
        }

        /// <summary>
        /// 设置协程运行器
        /// </summary>
        public void SetCoroutineRunner(MonoBehaviour runner)
        {
            _coroutineRunner = runner;
        }
        
        /// <summary>
        /// 启动协程
        /// </summary>
        public void StartCoroutine(IEnumerator routine)
        {
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(routine);
            }
            else
            {
                Debug.LogWarning("CoroutineRunner is not set, cannot start coroutine!");
            }
        }
        
        /// <summary>
        /// 设置当前阶段
        /// </summary>
        public void SetStage(HotUpdateStage newStage, string message = null)
        {
            _previousStage = _currentStage;
            _currentStage = newStage;
            
            var args = new HotUpdateEventArgs
            {
                PackageName = _packageName,
                CurrentStage = _currentStage,
                PreviousStage = _previousStage,
                Message = message ?? GetStageDescription(_currentStage)
            };
            
            OnStageChanged?.Invoke(this, args);
        }
        
        /// <summary>
        /// 发送进度事件
        /// </summary>
        public void SendProgress(float progress, string message = null, DownloadInfo downloadInfo = null)
        {
            var args = new HotUpdateEventArgs
            {
                PackageName = _packageName,
                CurrentStage = _currentStage,
                Progress = progress,
                Message = message,
                DownloadInfo = downloadInfo
            };
            
            OnProgressChanged?.Invoke(this, args);
        }
        
        /// <summary>
        /// 发送错误事件
        /// </summary>
        public void SendError(string error, HotUpdateStage stage = HotUpdateStage.None)
        {
            var args = new HotUpdateEventArgs
            {
                PackageName = _packageName,
                CurrentStage = stage != HotUpdateStage.None ? stage : _currentStage,
                IsError = true,
                ErrorMessage = error,
                Message = $"阶段 {GetStageDescription(stage != HotUpdateStage.None ? stage : _currentStage)} 发生错误: {error}"
            };
            
            OnError?.Invoke(this, args);
        }
        
        /// <summary>
        /// 发送完成事件
        /// </summary>
        public void SendCompleted()
        {
            SetStage(HotUpdateStage.Completed, "热更新完成");
            OnCompleted?.Invoke(this, new HotUpdateEventArgs
            {
                PackageName = _packageName,
                CurrentStage = HotUpdateStage.Completed,
                Message = "热更新完成"
            });
        }
        
        /// <summary>
        /// 设置完成状态
        /// </summary>
        public void SetFinish()
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Succeed;
            Debug.Log($"Package {_packageName} patch done!");
        }
        
        /// <summary>
        /// 设置失败状态
        /// </summary>
        public void SetFailed(string error)
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            SendError(error);
            Debug.LogError($"Package {_packageName} patch failed: {error}");
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
    }
} 