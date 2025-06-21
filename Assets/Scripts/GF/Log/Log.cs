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
        
        static Log()
        {
            UnityEngine.Debug.developerConsoleEnabled = false;
            UnityEngine.Debug.developerConsoleVisible = false;
        }

        private static ILogger _logger;
        public static ILogger Logger 
        { 
            get => _logger ??= new UnityLogger();
            private set => _logger = value;
        }

        
        public static bool Enable 
        { 
            get =>  Logger.Enable;
            set =>  Logger.Enable = value;
        }
        
        public static LogLevel EnableLevel 
        { 
            get =>  Logger.EnableLevel;
            set =>  Logger.EnableLevel = value;
        }

        /// <summary>
        /// 设置日志记录器
        /// </summary>
        /// <param name="logger">要设置的日志记录器</param>
        public static void SetLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
