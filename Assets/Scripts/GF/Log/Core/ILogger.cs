using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GF.Log
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    public interface ILogger
    {
        bool Enable { get; set; }
        LogLevel EnableLevel { get; set; }
        void Log(LogLevel level, string message, params object[] args);
        void Log(Exception exception);

        void Debug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
        void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);
        void Warning(string message, params object[] args) => Log(LogLevel.Warning, message, args);
        void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);
        void Error(Exception e) => Log(e);
    }
}
