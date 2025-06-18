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
        private static CompositeLogger _defaultCompositeLogger;

        static Log()
        {
            InitializeLogSystem();
            UnityEngine.Debug.developerConsoleEnabled = false;
            UnityEngine.Debug.developerConsoleVisible = false;
        }

        private static void InitializeLogSystem()
        {
            // 加载配置
            var config = LogConfig.Instance;

            var unityLogger = new UnityLogger();
            var loggers = new List<ILogger> { unityLogger };

            // 根据配置添加文件日志器
            if (config.enableFileLogging)
            {
                var fileLogger = new FileLogger(config.logFileName);

                // 根据配置决定是否使用缓冲
                if (config.enableBuffering)
                {
                    var bufferedFileLogger = new BufferedLogger(
                        fileLogger,
                        config.bufferSize,
                        config.flushIntervalMs
                    );
                    loggers.Add(bufferedFileLogger);
                }
                else
                {
                    loggers.Add(fileLogger);
                }
            }

            _defaultCompositeLogger = new CompositeLogger(loggers.ToArray());

            // 根据配置应用过滤器
            if (
                config.enableFiltering
                && (config.excludeKeywords.Count > 0 || config.includeKeywords.Count > 0)
            )
            {
                var filteredLogger = new FilteredLogger(_defaultCompositeLogger);
                var keywordFilter = new KeywordFilter(true); // 排除模式

                foreach (var keyword in config.excludeKeywords)
                {
                    keywordFilter.AddExcludeKeyword(keyword);
                }

                foreach (var keyword in config.includeKeywords)
                {
                    keywordFilter.AddIncludeKeyword(keyword);
                }

                filteredLogger.AddFilter(keywordFilter);

                // 如果启用频率限制
                if (config.enableRateLimit)
                {
                    var rateLimitFilter = new RateLimitFilter(
                        TimeSpan.FromMilliseconds(config.rateLimitIntervalMs)
                    );
                    filteredLogger.AddFilter(rateLimitFilter);
                }

                Logger = filteredLogger;
            }
            else
            {
                Logger = _defaultCompositeLogger;
            }

            // 应用配置
            config.ApplyToLogSystem();
        }

        public static void AddLogger(ILogger logger)
        {
            _defaultCompositeLogger?.AddLogger(logger);
        }

        public static void RemoveLogger(ILogger logger)
        {
            _defaultCompositeLogger?.RemoveLogger(logger);
        }

        public static void FlushBuffers()
        {
            if (Logger is BufferedLogger bufferedLogger)
            {
                bufferedLogger.Flush();
            }
            else if (Logger is CompositeLogger compositeLogger)
            {
                // 刷新所有缓冲的子日志器
                var loggerField = typeof(CompositeLogger).GetField(
                    "_loggers",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                if (
                    loggerField?.GetValue(compositeLogger)
                    is System.Collections.Generic.List<ILogger> loggers
                )
                {
                    foreach (var logger in loggers)
                    {
                        if (logger is BufferedLogger bl)
                            bl.Flush();
                    }
                }
            }
        }

        public static void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        public static void SetEnable(bool enable)
        {
            Logger.Enable = enable;
        }

        public static void SetEnableLevel(LogLevel level)
        {
            Logger.EnableLevel = level;
        }

        /// <summary>
        /// 只会在Editor中打印
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Debug(string message, params object[] args)
        {
            Logger.Log(LogLevel.Debug,message, args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Info(string message, params object[] args)
        {
            Logger.Log(LogLevel.Info,message, args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public static void Warning(string message, params object[] args)
        {
            Logger.Log(LogLevel.Warning,message, args);
        }

        [HideInCallstack]
        public static void Error(string message, params object[] args)
        {
            Logger.Log(LogLevel.Error,message, args);
        }

        [HideInCallstack]
        public static void Error(Exception exception)
        {
            Logger.Log(exception);
        }
    }
}
