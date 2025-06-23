using System.Collections;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 状态节点基类
    /// </summary>
    public abstract class BaseStateNode : IStateNode
    {
        protected StateMachine _machine;
        protected PatchOperation _operation;
        
        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
            _operation = machine.Owner as PatchOperation;
        }
        
        void IStateNode.OnEnter()
        {
            // 设置当前阶段
            _operation?.SetStage(GetStage(), GetStageMessage());
            OnEnterState();
        }
        
        void IStateNode.OnUpdate()
        {
            OnUpdateState();
        }
        
        void IStateNode.OnExit()
        {
            OnExitState();
        }
        
        /// <summary>
        /// 获取当前阶段
        /// </summary>
        protected abstract HotUpdateStage GetStage();
        
        /// <summary>
        /// 获取阶段消息
        /// </summary>
        protected virtual string GetStageMessage()
        {
            return GetStageDescription(GetStage());
        }
        
        /// <summary>
        /// 进入状态
        /// </summary>
        protected abstract void OnEnterState();
        
        /// <summary>
        /// 更新状态
        /// </summary>
        protected virtual void OnUpdateState() { }
        
        /// <summary>
        /// 退出状态
        /// </summary>
        protected virtual void OnExitState() { }
        
        /// <summary>
        /// 获取阶段描述
        /// </summary>
        protected string GetStageDescription(HotUpdateStage stage)
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
        
        /// <summary>
        /// 启动协程
        /// </summary>
        protected void StartCoroutine(IEnumerator routine)
        {
            if (_operation != null)
            {
                _operation.StartCoroutine(routine);
            }
        }
    }
} 