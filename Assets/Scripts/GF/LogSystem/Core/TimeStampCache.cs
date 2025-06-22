using System;
using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 时间戳缓存 - 减少DateTime.Now调用
    /// </summary>
    public static class TimeStampCache
    {
        private static DateTime _lastTime;
        private static string _cachedTimestamp;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// 获取缓存的时间戳（精确到毫秒）
        /// </summary>
        public static string GetTimestamp()
        {
            var now = DateTime.Now;
            
            lock (_lock)
            {
                // 如果时间差小于1毫秒，使用缓存
                if ((now - _lastTime).TotalMilliseconds < 1 && _cachedTimestamp != null)
                {
                    return _cachedTimestamp;
                }
                
                _lastTime = now;
                _cachedTimestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return _cachedTimestamp;
            }
        }
        
        /// <summary>
        /// 获取日期字符串（用于文件路径）
        /// </summary>
        public static string GetDateString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
        
        /// <summary>
        /// 获取启动时间字符串（用于文件名）
        /// </summary>
        public static string GetStartTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
} 