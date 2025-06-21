using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GF.Log
{
    public class UnityLogger : ILogger, IDisposable
    {
        private LogFileWriter _fileWriter;

        public UnityLogger(bool enableFileOutput = false, string logFilePath = null) 
        { 
            if (enableFileOutput)
            {
                _fileWriter = new LogFileWriter(logFilePath);
            }
        }

        public bool Enable { get; set; } = true;
        public LogLevel EnableLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// 是否启用文件输出
        /// </summary>
        public bool FileOutputEnabled => _fileWriter != null;

        /// <summary>
        /// 启用文件输出
        /// </summary>
        /// <param name="logFilePath">日志文件路径</param>
        public void EnableFileOutput(string logFilePath = null)
        {
            if (_fileWriter == null)
            {
                _fileWriter = new LogFileWriter(logFilePath);
            }
        }

        /// <summary>
        /// 禁用文件输出
        /// </summary>
        public void DisableFileOutput()
        {
            _fileWriter?.Dispose();
            _fileWriter = null;
        }

        [HideInCallstack]
        public void Log(LogLevel level, string message, params object[] args)
        {
            if (!CheckLogLevel(level))
                return;

            // 写入Unity控制台
            // 如果有参数且需要格式化，先进行格式化
            if (args is { Length: > 0 })
            {
                if (StringFormatter.NeedsFormatting(message))
                {
                    // 使用高性能格式化器
                    var formattedMessage = StringFormatter.SafeFormat(message, args);
                    CallInternalLogger(level, formattedMessage);
                }
                else
                {
                    // 不需要自定义格式化，直接使用Unity的格式化
                    CallInternalLogger(level, message, args);
                }
            }
            else
            {
                // 没有参数，直接记录
                CallInternalLogger(level, message);
            }

            // 写入文件（如果启用）
            _fileWriter?.WriteLog(level, message, args);
        }

        private void CallInternalLogger(LogLevel level, string message, params object[] args)
        {
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
            if (args is { Length: > 0 })
                UnityEngine.Debug.LogFormat(message, args);
            else
                UnityEngine.Debug.Log(message);
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
            if (args is { Length: > 0 })
                UnityEngine.Debug.LogFormat(message, args);
            else
                UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        private void LogWarningInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Warning))
                return;
            if (args is { Length: > 0 })
                UnityEngine.Debug.LogWarningFormat(message, args);
            else
                UnityEngine.Debug.LogWarning(message);
        }

        /// <summary>
        /// 错误日志 - 始终编译（生产环境也需要错误日志）
        /// </summary>
        [HideInCallstack]
        private void LogErrorInternal(string message, params object[] args)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;
            if (args is { Length: > 0 })
                UnityEngine.Debug.LogErrorFormat(message, args);
            else
                UnityEngine.Debug.LogError(message);
        }

        [HideInCallstack]
        public void Log(Exception exception)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;
            
            // 写入Unity控制台
            UnityEngine.Debug.LogException(exception);
            
            // 写入文件（如果启用）
            _fileWriter?.WriteException(exception);
        }

        [HideInCallstack]
        private bool CheckLogLevel(LogLevel level)
        {
            //激活Log并且激活的LogLevel大于等于当前要打印的Level
            return Enable && EnableLevel <= level;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _fileWriter?.Dispose();
            _fileWriter = null;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~UnityLogger()
        {
            Dispose();
        }
    }
}
