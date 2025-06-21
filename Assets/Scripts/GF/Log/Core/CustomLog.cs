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
        private readonly string _rawTag; // 原始tag名，不包含颜色标记
        private readonly bool _enable;
        private ILogger _logger;
        private readonly bool _enableFileWrite;
        private LogFileWriter _fileWriter; // 专属的文件写入器
        private bool _isShutdown;
        private static Dictionary<string, CustomLog> _allCustomLoggers;

        /// <summary>
        /// 延迟获取Logger实例，确保在使用时Logger已经初始化
        /// </summary>
        private ILogger Logger
        {
            get
            {
                if (!_isShutdown) return _logger ??= Log.Logger;
                UnityEngine.Debug.LogError("日志系统已关闭，请先调用Log.Initialize()重新初始化");
                return null;
            }
        }

        public static Dictionary<string, CustomLog> AllCustomLoggers
        {
            get => _allCustomLoggers;
            private set => _allCustomLoggers = value;
        }

        static CustomLog()
        {
            _allCustomLoggers = new Dictionary<string, CustomLog>();
        }

        public CustomLog(string tag, bool enable = false, string tagColorStr = "", bool enableFileWrite = true)
        {
            _rawTag = tag; // 保存原始tag名
            if (!string.IsNullOrEmpty(tagColorStr) && tagColorStr.StartsWith("#"))
                this._tag = $"<color={tagColorStr}>[{tag}]</color>";
            else
                this._tag = $"[{tag}]";
            this._enable = enable;
            this._enableFileWrite = enableFileWrite;

            _allCustomLoggers[tag.ToLower()] = this;
        }

        public CustomLog(string tag, bool enable = false, Color tagColor = default, bool enableFileWrite = true)
        {
            _rawTag = tag; // 保存原始tag名
            if (tagColor != default)
            {
                var colorStr = ColorUtility.ToHtmlStringRGB(tagColor);
                this._tag = $"<color=#{colorStr}>[{tag}]</color>";
            }
            else
                this._tag = $"[{tag}]";

            this._enable = enable;
            this._enableFileWrite = enableFileWrite;


            _allCustomLoggers[tag.ToLower()] = this;
        }

        /// <summary>
        /// 获取文件写入器（延迟创建，共享实例）
        /// </summary>
        private LogFileWriter FileWriter
        {
            get
            {
                if (!_enableFileWrite)
                    return null;

                return _fileWriter ??= SharedLogFileManager.Instance.GetOrCreateWriter(_rawTag, Logger);
            }
        }

        /// <summary>
        /// 写入日志到控制台和文件
        /// </summary>
        private void WriteLog(LogLevel level, string message, params object[] args)
        {
            if (Logger?.Enable == false || Logger?.EnableLevel > level)return;
            // 写入控制台
            this.Logger?.Log(level, $"{_tag}{message}", args);

            // 写入文件（如果启用）
            FileWriter?.WriteLog(level, message, args);
        }

        /// <summary>
        /// 写入异常日志到控制台和文件
        /// </summary>
        private void WriteException(Exception ex)
        {
            if (Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Error)return;
            // 写入控制台
            this.Logger?.Log(ex);

            // 写入文件（如果启用）
            FileWriter?.WriteException(ex);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Debug(string message, params object[] args)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Debug)
                return;
            WriteLog(LogLevel.Debug, message, args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, params object[] args)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Info)
                return;
            WriteLog(LogLevel.Info, message, args);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Info(string message, UnityEngine.Color color)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Info)
                return;
            var colorStr = ColorUtility.ToHtmlStringRGB(color);
            var msg = $"{_tag}<color=#{colorStr}>{message}</color>";
            WriteLog(LogLevel.Info, msg);
        }

        [HideInCallstack]
        [Conditional("LOG_ENABLE")]
        public void Warning(string message, params object[] args)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Warning)
                return;
            WriteLog(LogLevel.Warning, message, args);
        }

        [HideInCallstack]
        public void Error(string message, params object[] args)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Error)
                return;
            WriteLog(LogLevel.Error, message, args);
        }

        [HideInCallstack]
        public void Error(Exception e)
        {
            if (!_enable || Logger?.Enable == false || Logger?.EnableLevel > LogLevel.Error)
                return;
            WriteException(e);
        }

        /// <summary>
        /// 刷新文件缓冲区
        /// </summary>
        public void Flush()
        {
            FileWriter?.Flush();
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public string GetLogFilePath()
        {
            return FileWriter?.GetLogFilePath();
        }


        public void Initialize()
        {
            _isShutdown = false; // 重置关闭状态
            // 清除缓存的文件写入器，强制重新获取
            _fileWriter = null;
        }

        /// <summary>
        /// 关闭日志系统
        /// </summary>
        public void Shutdown()
        {
            Dispose();
            _logger = null;
            _isShutdown = true; // 设置关闭标志
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_enableFileWrite)
            {
                SharedLogFileManager.Instance.ReleaseWriter(_rawTag);
            }

            _fileWriter = null;
        }
    }
}