using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        long PlayerID{get;set;}
        bool Enable { get; set; }
        LogLevel EnableLevel { get; set; }
        void Log(LogLevel level, string message, params object[] args);
        void Log(Exception exception);
    }
}
