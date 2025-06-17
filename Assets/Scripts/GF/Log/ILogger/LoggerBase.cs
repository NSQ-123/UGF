using System;
using System.Diagnostics;
using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 日志器基类，为不支持接口默认实现的Unity版本提供便捷方法
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        public abstract bool Enable { get; set; }
        public abstract LogLevel EnableLevel { get; set; }
        public abstract void Log(LogLevel level, string message, params object[] args);
        public abstract void Log(Exception exception);
        
        [HideInCallstack]
        public virtual void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        [HideInCallstack]
        public virtual void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        [HideInCallstack]
        public virtual void Warning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        [HideInCallstack]
        public virtual void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        [HideInCallstack]
        public virtual void Error(Exception e)
        {
            Log(e);
        }
    }
} 