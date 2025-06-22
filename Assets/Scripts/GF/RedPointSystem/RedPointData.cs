using System.Collections.Generic;
using UnityEngine;


namespace GF.RedPoint
{
    /// <summary>
    /// 红点类型枚举
    /// </summary>
    public enum ERedPointType
    {
        /// <summary>
        /// 计数型红点
        /// 记录每级红点数量，传播时计数为对应count
        /// </summary>
        Count,

        /// <summary>
        /// 状态型红点
        /// 记录是否有红点，传播时计数为0或1
        /// </summary>
        State,
    }

    /// <summary>
    /// 红点数据
    /// </summary>
    public class RedPointData
    {
        /// <summary>
        /// 红点key
        /// </summary>
        public string RedPointKey;

        /// <summary>
        /// 获取红点信息Func
        /// </summary>
        public System.Func<int> CheckFunc;


        /// <summary>
        /// 计数数量
        /// 计数型为对应count
        /// 状态型计数为0或1
        /// </summary>
        public int Num;

        public void Dispose()
        {
            CheckFunc = null;
        }
    }
}