using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GF.Log
{
    public class BufferedLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly Timer _flushTimer;
        private readonly int _bufferSize;
        private readonly int _flushInterval;

        public bool Enable
        {
            get => _innerLogger.Enable;
            set => _innerLogger.Enable = value;
        }

        public LogLevel EnableLevel
        {
            get => _innerLogger.EnableLevel;
            set => _innerLogger.EnableLevel = value;
        }

        public BufferedLogger(ILogger innerLogger, int bufferSize = 100, int flushIntervalMs = 1000)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _logQueue = new ConcurrentQueue<LogEntry>();
            _bufferSize = bufferSize;
            _flushInterval = flushIntervalMs;

            _flushTimer = new Timer(FlushLogs, null, _flushInterval, _flushInterval);
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            _logQueue.Enqueue(
                new LogEntry
                {
                    Level = level,
                    Message = message,
                    Args = args,
                    Timestamp = DateTime.Now,
                }
            );

            if (_logQueue.Count >= _bufferSize)
            {
                FlushLogs(null);
            }
        }

        public void Log(Exception exception)
        {
            _logQueue.Enqueue(new LogEntry { Exception = exception, Timestamp = DateTime.Now });

            if (_logQueue.Count >= _bufferSize)
            {
                FlushLogs(null);
            }
        }

        private void FlushLogs(object state)
        {
            try
            {
                while (_logQueue.TryDequeue(out var entry))
                {
                    if (entry.Exception != null)
                    {
                        _innerLogger.Log(entry.Exception);
                    }
                    else
                    {
                        _innerLogger.Log(entry.Level, entry.Message, entry.Args);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"刷新日志缓冲区时出错: {ex.Message}");
            }
        }

        public void Flush()
        {
            FlushLogs(null);
        }

        ~BufferedLogger()
        {
            _flushTimer?.Dispose();
            FlushLogs(null);
        }

        private class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public object[] Args { get; set; }
            public Exception Exception { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
