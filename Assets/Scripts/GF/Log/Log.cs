using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace GF.Log
{
    public static partial class Log
    {
        public static ILogger Logger { get; private set; } = new UnityLogger();

        static Log()
        {
            UnityEngine.Debug.developerConsoleEnabled = false;
            UnityEngine.Debug.developerConsoleVisible = false;
        }
        

        public static void SetLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static void SetEnable(bool enable)
        {
            if (Logger != null)
                Logger.Enable = enable;
        }

        public static void SetEnableLevel(LogLevel level)
        {
            if (Logger != null)
                Logger.EnableLevel = level;
        }

        /// <summary>
        /// 调试日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Debug(string message, params object[] args)
        {
            Logger?.Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// 信息日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Info(string message, params object[] args)
        {
            Logger?.Log(LogLevel.Info, message, args);
        }

        /// <summary>
        /// 警告日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Warning(string message, params object[] args)
        {
            Logger?.Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// 错误日志 - 始终可用，不受LOG_ENABLE影响
        /// </summary>
        [HideInCallstack]
        public static void Error(string message, params object[] args)
        {
            Logger?.Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// 异常日志 - 始终可用，不受LOG_ENABLE影响
        /// </summary>
        [HideInCallstack]
        public static void Error(Exception exception)
        {
            Logger?.Log(exception);
        }
    }
}
