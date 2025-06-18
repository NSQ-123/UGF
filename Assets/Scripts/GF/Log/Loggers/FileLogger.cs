using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GF.Log
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();

        public bool Enable { get; set; } = true;
        public LogLevel EnableLevel { get; set; } = LogLevel.Debug;

        public FileLogger(string fileName = null)
        {
            var logDir = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            fileName = fileName ?? $"game_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            _logFilePath = Path.Combine(logDir, fileName);
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            if (!CheckLogLevel(level))
                return;

            var formattedMessage = args?.Length > 0 ? string.Format(message, args) : message;
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {formattedMessage}";

            WriteToFile(logEntry);
        }

        public void Log(Exception exception)
        {
            if (!CheckLogLevel(LogLevel.Error))
                return;

            var logEntry =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Exception: {exception}";
            WriteToFile(logEntry);
        }

        private bool CheckLogLevel(LogLevel level)
        {
            return Enable && (int)level >= (int)EnableLevel;
        }

        private void WriteToFile(string logEntry)
        {
            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"写入日志文件失败: {ex.Message}");
            }
        }
    }
}
