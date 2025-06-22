using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 共享日志文件管理器 - 减少文件句柄数量
    /// </summary>
    public class SharedLogFileManager : IDisposable
    {
        private static SharedLogFileManager _instance;
        public static SharedLogFileManager Instance => _instance ??= new SharedLogFileManager();

        private readonly Dictionary<string, LogFileWriter> _writers = new Dictionary<string, LogFileWriter>();
        private readonly object _writersLock = new object();
        private bool _disposed = false;

        private SharedLogFileManager()
        {
        }

        /// <summary>
        /// 获取或创建指定标签的文件写入器
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="logger">Logger实例</param>
        /// <returns>文件写入器</returns>
        public LogFileWriter GetOrCreateWriter(string tag, ILogger logger)
        {
            if (_disposed)
                return null;

            lock (_writersLock)
            {
                if (_writers.TryGetValue(tag, out var existingWriter))
                {
                    return existingWriter;
                }

                var newWriter = new LogFileWriter(logger, tag);
                _writers[tag] = newWriter;
                return newWriter;
            }
        }

        /// <summary>
        /// 刷新所有文件缓冲区
        /// </summary>
        public void FlushAll()
        {
            lock (_writersLock)
            {
                foreach (var writer in _writers.Values)
                {
                    writer?.Flush();
                }
            }
        }

        /// <summary>
        /// 释放指定标签的写入器
        /// </summary>
        /// <param name="tag">标签</param>
        public void ReleaseWriter(string tag)
        {
            lock (_writersLock)
            {
                if (_writers.TryGetValue(tag, out var writer))
                {
                    writer?.Dispose();
                    _writers.Remove(tag);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            lock (_writersLock)
            {
                foreach (var writer in _writers.Values)
                {
                    writer?.Dispose();
                }

                _writers.Clear();
            }
        }
    }
}