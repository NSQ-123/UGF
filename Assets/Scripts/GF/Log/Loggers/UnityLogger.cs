using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GF.Log
{
    public class UnityLogger : LoggerBase
    {
        public UnityLogger() { }

        public override bool Enable { get; set; } = true;
        public override LogLevel EnableLevel { get; set; } = LogLevel.Debug;

        [HideInCallstack]
        public override void Log(LogLevel level, string message, params object[] args)
        {
            if (!CheckLogLevel(level))
                return;

            switch (level)
            {
                case LogLevel.Debug:
                    LogDebugInternal(message, args);
                    break;
                case LogLevel.Info:
                    LogInfoInternal(message, args);
                    break;
                case LogLevel.Warning:
                    LogWarningInternal(message, args);
                    break;
                case LogLevel.Error:
                    LogErrorInternal(message, args);
                    break;
                default:
                    LogInfoInternal(message, args);
                    break;
            }
        }

        [HideInCallstack]
        public override void Log(Exception exception)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;
            UnityEngine.Debug.LogException(exception);
        }

        /// <summary>
        /// 调试日志 - 仅在定义了 LOG_DEBUG 时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_DEBUG")]
        [Conditional("LOG_ENABLE")]
        private void LogDebugInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Debug))
                return;
            UnityEngine.Debug.LogFormat(message, args);
        }

        /// <summary>
        /// 信息日志
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        private void LogInfoInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Info))
                return;
            UnityEngine.Debug.LogFormat(message, args);
        }

        /// <summary>
        /// 警告日志 - 仅在定义了 LOG_WARNING 或更高级别时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        private void LogWarningInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Warning))
                return;
            UnityEngine.Debug.LogWarningFormat(message, args);
        }

        /// <summary>
        /// 错误日志 - 始终编译（生产环境也需要错误日志）
        /// </summary>
        [HideInCallstack]
        private void LogErrorInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;
            UnityEngine.Debug.LogErrorFormat(message, args);
        }

        [HideInCallstack]
        private bool CheckLogLevel(LogLevel level)
        {
            return Enable && (int)EnableLevel >= (int)level;
        }
    }
}
