using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GF.Log
{
    public class UnityLogger : ILogger
    {
        public UnityLogger() { }

        public bool Enable { get; set; } = true;
        public LogLevel EnableLevel { get; set; } = LogLevel.Debug;

        [HideInCallstack]
        public void Log(LogLevel level, string message, params object[] args)
        {
            if (!CheckLogLevel(level))
                return;

            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(message, args);
                    break;
                case LogLevel.Info:
                    LogInfo(message, args);
                    break;
                case LogLevel.Warning:
                    LogWarning(message, args);
                    break;
                case LogLevel.Error:
                    LogError(message, args);
                    break;
            }
        }

        [HideInCallstack]
        public void Log(Exception exception)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;
            Debug.LogException(exception);
        }

        //============================
        [HideInCallstack]
        private bool CheckLogLevel(LogLevel level)
        {
            return Enable && (int)EnableLevel >= (int)level;
        }

        [HideInCallstack]
        private void LogDebug(string message, params object[] args)
        {
#if UNITY_EDITOR
            Debug.LogFormat(message, args);
#endif
        }

        [HideInCallstack]
        private void LogInfo(string message, params object[] args)
        {
            Debug.LogFormat(message, args);
        }

        [HideInCallstack]
        private void LogWarning(string message, params object[] args)
        {
            Debug.LogWarningFormat(message, args);
        }

        [HideInCallstack]
        private void LogError(string message, params object[] args)
        {
            Debug.LogErrorFormat(message, args);
        }
    }
}
