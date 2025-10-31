using System.Collections.Generic;
using UnityEngine;

namespace GF.RedPoint
{
    /// <summary>
    /// 红点数据
    /// </summary>
    public class RedPointData
    {
        /// <summary>
        /// 红点key
        /// </summary>
        public string RedPointKey { get; set; }

        /// <summary>
        /// 获取红点信息Func
        /// </summary>
        public System.Func<int> CheckFunc { get; set; }

        /// <summary>
        /// 计数数量
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 获取当前红点数量
        /// </summary>
        /// <returns></returns>
        public int GetCurrentNum()
        {
            if (IsDisposed || CheckFunc == null)
                return 0;

            try
            {
                int result = CheckFunc();
                return result > 0 ? result : 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RedPoint CheckFunc Error for key '{RedPointKey}': {ex.Message}");
                return 0;
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            CheckFunc = null;
            RedPointKey = null;
            Num = 0;
            IsDisposed = true;
        }
    }
}
