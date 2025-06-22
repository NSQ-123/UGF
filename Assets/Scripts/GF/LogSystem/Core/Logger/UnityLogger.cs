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

        public UnityLogger(bool enableFileOutput, string customFolder = "Default")
        {
            if (enableFileOutput)
            {
                _fileWriter = new LogFileWriter(this, customFolder);
            }
        }

        public UnityLogger(long playerID,bool enableFileOutput, string customFolder = "Default")
        {
            this.PlayerID = playerID;
            if (enableFileOutput)
            {
                _fileWriter = new LogFileWriter(this, customFolder);
            }
        }
        
        public long PlayerID { get; set; } = 0;
        public bool Enable { get; set; } = true;
        public LogLevel EnableLevel { get; set; } = LogLevel.Debug;

        [HideInCallstack]
        public void Log(LogLevel level, string message, params object[] args)
        {
            if (!CheckLogLevel(level))
                return;
            switch (level)
            {
                case LogLevel.Warning:
                    LogWaring(message, args);
                    break;
                case LogLevel.Error:
                    LogError(message, args);
                    break;
                case LogLevel.Debug:
                case LogLevel.Info:
                default:
                    LogInfo(message, args);
                    break;
            }
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

        [Conditional("LOG_ENABLE")]
        private void LogInfo(string message, params object[] args)
        {
            if (args is { Length: > 0 })
            {
                UnityEngine.Debug.LogFormat(message, args);
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
            // 写入文件（如果启用）
            _fileWriter?.WriteLog(LogLevel.Info, message, args);
        }

        [Conditional("LOG_ENABLE")]
        private void LogWaring(string message, params object[] args)
        {
            if (args is { Length: > 0 })
            {
                UnityEngine.Debug.LogWarningFormat(message, args);
            }
            else
            {
                UnityEngine.Debug.LogWarning(message);
            }
            // 写入文件（如果启用）
            _fileWriter?.WriteLog(LogLevel.Warning, message, args);
        }

        private void LogError(string message, params object[] args)
        {
            if (args is { Length: > 0 })
            {
                UnityEngine.Debug.LogErrorFormat(message, args);
            }
            else
            {
                UnityEngine.Debug.LogError(message);
            }
            // 写入文件（如果启用）
            _fileWriter?.WriteLog(LogLevel.Error, message, args);
        }

        [HideInCallstack]
        private bool CheckLogLevel(LogLevel level)
        {
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
