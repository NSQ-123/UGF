using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GF.Log
{
    public class CustomLog
    {
        private readonly string _tag;
        private readonly bool _enable;
        private ILogger _logger;
        private static Dictionary<string, CustomLog> _allCustomLoggers;

        /// <summary>
        /// 延迟获取Logger实例，确保在使用时Logger已经初始化
        /// </summary>
        private ILogger Logger => _logger ??= Log.Logger;

        public static Dictionary<string, CustomLog> AllCustomLoggers
        {
            get => _allCustomLoggers;
            private set => _allCustomLoggers = value;
        }

        static CustomLog()
        {
            _allCustomLoggers = new Dictionary<string, CustomLog>();
        }

        public CustomLog(string tag, bool enable = false, string tagColorStr = "")
        {
            if (!string.IsNullOrEmpty(tagColorStr) && tagColorStr.StartsWith("#"))
                this._tag = $"<color={tagColorStr}>[{tag}]</color>";
            else
                this._tag = $"[{tag}]";
            this._enable = enable;
            _allCustomLoggers[tag.ToLower()] = this;
        }

        public CustomLog(string tag, bool enable = false, Color tagColor = default)
        {
            if (tagColor != default)
            { 
                var colorStr = ColorUtility.ToHtmlStringRGB(tagColor);
                this._tag = $"<color=#{colorStr}>[{tag}]</color>";
            }
            else
                this._tag = $"[{tag}]";

            this._enable = enable;
            _allCustomLoggers[tag.ToLower()] = this;
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Debug(string message, params object[] args)
        {
            if (!_enable)
                return;
            this.Logger.Log(LogLevel.Debug, $"{_tag}{message}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, params object[] args)
        {
            if (!_enable)
                return;
            this.Logger.Log(LogLevel.Info, $"{_tag}{message}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, UnityEngine.Color color)
        {
            if (!_enable)
                return;
            var colorStr = ColorUtility.ToHtmlStringRGB(color);
            var showMsg = $"{_tag}<color=#{colorStr}>{message}</color>";
            this.Logger.Log(LogLevel.Info, showMsg);
        }

        // 支持方法调用跟踪
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Trace(
            string message = null,
            [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber]
            int sourceLineNumber = 0
        )
        {
            if (!_enable)
                return;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            var traceMsg = string.IsNullOrEmpty(message)
                ? $"[TRACE] {fileName}.{memberName}:{sourceLineNumber}"
                : $"[TRACE] {fileName}.{memberName}:{sourceLineNumber} - {message}";

            this.Logger.Log(LogLevel.Info, $"{_tag}{traceMsg}");
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Warning(string message, params object[] args)
        {
            if (!_enable)
                return;
            this.Logger.Log(LogLevel.Warning, $"{_tag}{message}", args);
        }

        [HideInCallstack]
        public void Error(string message, params object[] args)
        {
            if (!_enable)
                return;
            this.Logger.Log(LogLevel.Error, $"{_tag}{message}", args);
        }

        [HideInCallstack]
        public void Error(Exception e)
        {
            if (!_enable)
                return;
            this.Logger.Log(e);
        }
    }
}