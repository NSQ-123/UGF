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

        private static bool _isShutdown = false;
        private static ILogger _logger;
        public static ILogger Logger
        {
            get
            {
                if (!_isShutdown)
                    return _logger ??= new UnityLogger(PlayerId,true, "_LogAll");
                UnityEngine.Debug.LogWarning("日志系统已关闭，请先调用Log.Initialize()重新初始化");
                return null;
            }
        }

        public static bool Enable
        {
            get => Logger.Enable;
            set => Logger.Enable = value;
        }

        public static LogLevel EnableLevel
        {
            get => Logger.EnableLevel;
            set => Logger.EnableLevel = value;
        }

        /// <summary>
        /// 当前玩家ID
        /// </summary>
        public static long PlayerId { get; set; }

        /// <summary>
        /// 设置日志记录器
        /// </summary>
        /// <param name="logger">要设置的日志记录器</param>
        public static void SetLogger(ILogger logger)
        {
            // 先清理旧的logger
            if (_logger is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isShutdown = false; // 重置状态
        }

        public static void Initialize()
        {
            // 重置关闭状态
            _isShutdown = false; 
            // 重新初始化所有CustomLog
            foreach (var customLog in CustomLog.AllCustomLoggers.Values)
            {
                customLog.Initialize();
            }
        }

        /// <summary>
        /// 关闭日志系统
        /// </summary>
        public static void Shutdown()
        {
            if (_logger is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _logger = null;
            _isShutdown = true; // 设置关闭标志

            foreach (var item in CustomLog.AllCustomLoggers)
            {
                item.Value.Shutdown();
            }
        }

        /// <summary>
        /// 调试日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Debug(string message, params object[] args)
        {
            if (Logger?.Enable == false || Logger.EnableLevel > LogLevel.Debug)return;
            Logger?.Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// 信息日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Info(string message, params object[] args)
        {
            if (Logger?.Enable == false || Logger.EnableLevel > LogLevel.Info)return;
            Logger?.Log(LogLevel.Info, message, args);
        }

        /// <summary>
        /// 警告日志 - 仅在LOG_ENABLE定义时编译
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Warning(string message, params object[] args)
        {
            if (Logger?.Enable == false || Logger.EnableLevel > LogLevel.Warning)return;
            Logger?.Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// 错误日志 - 始终可用，不受LOG_ENABLE影响
        /// </summary>
        [HideInCallstack]
        public static void Error(string message, params object[] args)
        {
            if (Logger?.Enable == false || Logger.EnableLevel > LogLevel.Error)return;
            Logger?.Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// 异常日志 - 始终可用，不受LOG_ENABLE影响
        /// </summary>
        [HideInCallstack]
        public static void Error(Exception exception)
        {
            if (Logger?.Enable == false || Logger.EnableLevel > LogLevel.Error)return;
            Logger?.Log(exception);
        }
    }
}
