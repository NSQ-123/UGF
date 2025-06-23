using System.Collections;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 开始游戏状态
    /// </summary>
    internal class FsmStartGame : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.StartGame;
        
        protected override void OnEnterState()
        {
            _operation?.SendCompleted();
            _operation?.SetFinish();
        }
    }
} 