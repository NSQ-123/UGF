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
        private readonly bool _enableFileReport;
        private readonly ILogger _logger;
        private static Dictionary<string, CustomLog> _allCustomLoggers;

        public static Dictionary<string, CustomLog> AllCustomLoggers
        {
            get => _allCustomLoggers;
            private set => _allCustomLoggers = value;
        }

        static CustomLog()
        {
            _allCustomLoggers = new Dictionary<string, CustomLog>();
        }

        public CustomLog(
            string tag,
            bool enable = false,
            string tagColorStr = "",
            bool enableFileReport = true
        )
        {
            if (!string.IsNullOrEmpty(tagColorStr) && tagColorStr.StartsWith("#"))
                this._tag = $"<color={tagColorStr}>[{tag}]</color>";
            else
                this._tag = $"[{tag}]";
            this._logger = Log.Logger;
            this._enable = enable;
            this._enableFileReport = enableFileReport;
            _allCustomLoggers[tag.ToLower()] = this;
        }

        public CustomLog(
            string tag,
            bool enable = false,
            Color tagColor = default,
            bool enableFileReport = true
        )
        {
            if (tagColor != default)
            {
                var colorStr = ColorUtility.ToHtmlStringRGB(tagColor);
                this._tag = $"<color=#{colorStr}>[{tag}]</color>";
            }
            else
                this._tag = $"[{tag}]";
            this._logger = Log.Logger;
            this._enable = enable;
            this._enableFileReport = enableFileReport;
            _allCustomLoggers[tag.ToLower()] = this;
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Debug(string message, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Debug($"{_tag}{message}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Info($"{_tag}{message}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, UnityEngine.Color color)
        {
            if (!_enable)
                return;
            var colorStr = ColorUtility.ToHtmlStringRGB(color);
            var showMsg = $"{_tag}<color=#{colorStr}>{message}</color>";
            this._logger.Info(showMsg);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void InfoFormat(string format, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Info($"{_tag}{format}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void DebugFormat(string format, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Debug($"{_tag}{format}", args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void WarningFormat(string format, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Warning($"{_tag}{format}", args);
        }

        [HideInCallstack]
        public void ErrorFormat(string format, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Error($"{_tag}{format}", args);
        }

        // 支持方法调用跟踪
        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Trace(
            string message = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            if (!_enable)
                return;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            var traceMsg = string.IsNullOrEmpty(message)
                ? $"[TRACE] {fileName}.{memberName}:{sourceLineNumber}"
                : $"[TRACE] {fileName}.{memberName}:{sourceLineNumber} - {message}";

            this._logger.Debug($"{_tag}{traceMsg}");
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Warning(string message, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Warning($"{_tag}{message}", args);
        }

        [HideInCallstack]
        public void Error(string message, params object[] args)
        {
            if (!_enable)
                return;
            this._logger.Error($"{_tag}{message}", args);
        }

        [HideInCallstack]
        public void Error(Exception e)
        {
            if (!_enable)
                return;
            this._logger.Error(e);
        }
    }
}
