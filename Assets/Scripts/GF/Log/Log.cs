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
            try
            {
                if (Logger is BufferedLogger bufferedLogger)
                {
                    bufferedLogger.Flush();
                }
                else if (Logger is CompositeLogger compositeLogger)
                {
                    // 使用更安全的方式访问私有字段
                    FlushCompositeLogger(compositeLogger);
                }
                else if (Logger is FilteredLogger filteredLogger)
                {
                    // 检查FilteredLogger内部是否有BufferedLogger
                    FlushNestedBufferedLoggers(filteredLogger);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"刷新日志缓冲区时发生错误: {ex.Message}");
            }
        }

        private static void FlushCompositeLogger(CompositeLogger compositeLogger)
        {
            // 使用反射更安全地访问私有字段
            var loggerField = typeof(CompositeLogger).GetField(
                "_loggers",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            
            var loggerFieldValue = loggerField?.GetValue(compositeLogger);
            if (loggerFieldValue == null)
                return;

            // 更安全的类型转换，支持IEnumerable<ILogger>
            if (loggerFieldValue is IEnumerable<ILogger> loggers)
            {
                foreach (var logger in loggers)
                {
                    if (logger is BufferedLogger bl)
                        bl.Flush();
                    else if (logger is IDisposable disposable && logger.GetType().Name.Contains("Buffered"))
                    {
                        // 尝试调用Flush方法
                        var flushMethod = logger.GetType().GetMethod("Flush");
                        flushMethod?.Invoke(logger, null);
                    }
                }
            }
        }

        private static void FlushNestedBufferedLoggers(object logger)
        {
            // 递归检查嵌套的日志器
            var innerLoggerField = logger
                .GetType()
                .GetField(
                    "_innerLogger",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

            if (innerLoggerField?.GetValue(logger) is ILogger innerLogger)
            {
                if (innerLogger is BufferedLogger buffered)
                    buffered.Flush();
                else if (innerLogger is CompositeLogger composite)
                    FlushCompositeLogger(composite);
                else
                    FlushNestedBufferedLoggers(innerLogger);
            }
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

        /// <summary>
        /// 释放日志系统资源
        /// </summary>
        public static void Dispose()
        {
            try
            {
                FlushBuffers();

                if (Logger is IDisposable disposable)
                    disposable.Dispose();

                _defaultCompositeLogger = null;
                Logger = null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"释放日志系统时发生错误: {ex.Message}");
            }
        }
    }
}
