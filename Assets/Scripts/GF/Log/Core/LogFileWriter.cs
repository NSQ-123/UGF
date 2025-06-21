using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 日志文件写入器 - 专门处理日志文件的写入操作
    /// </summary>
    public class LogFileWriter : IDisposable
    {
        private readonly string _logFilePath;
        private readonly long _maxFileSize;
        private readonly int _maxBackupFiles;
        private readonly object _lockObject = new object();
        private StreamWriter _writer;
        private bool _disposed = false;
        private bool _enable = true;

        /// <summary>
        /// 是否启用文件写入
        /// </summary>
        public bool Enable 
        { 
            get => _enable; 
            set => _enable = value; 
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logFilePath">日志文件路径，如果为空则使用默认路径</param>
        /// <param name="maxFileSize">最大文件大小（字节），默认10MB</param>
        /// <param name="maxBackupFiles">最大备份文件数量，默认5个</param>
        public LogFileWriter(string logFilePath = null, long maxFileSize = 10 * 1024 * 1024, int maxBackupFiles = 5)
        {
            _maxFileSize = maxFileSize;
            _maxBackupFiles = maxBackupFiles;
            
            if (string.IsNullOrEmpty(logFilePath))
            {
                // 使用默认路径：Application.persistentDataPath/Logs/
                var logDir = Path.Combine(Application.persistentDataPath, "GameLogs");
                var fileName = $"game_{DateTime.Now:yyyyMMdd}.log";
                _logFilePath = Path.Combine(logDir, fileName);
            }
            else
            {
                _logFilePath = logFilePath;
            }

            InitializeLogFile();
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private void InitializeLogFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(directory))
                {
                    if (directory != null) Directory.CreateDirectory(directory);
                    else
                    {
                        return;
                    }
                }

                // 检查文件大小，如果需要则进行轮转
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length > _maxFileSize)
                    {
                        RotateLogFile();
                    }
                }

                _writer = new StreamWriter(_logFilePath, true, Encoding.UTF8)
                {
                    AutoFlush = true
                };

                // 写入启动标记
                _writer.WriteLine($"=== Log Session Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 轮转日志文件
        /// </summary>
        private void RotateLogFile()
        {
            try
            {
                _writer?.Close();
                _writer?.Dispose();
                _writer = null;

                var directory = Path.GetDirectoryName(_logFilePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(_logFilePath);
                var extension = Path.GetExtension(_logFilePath);

                // 轮转现有的备份文件
                for (int i = _maxBackupFiles - 1; i >= 1; i--)
                {
                    var oldBackupPath = Path.Combine(directory, $"{fileNameWithoutExt}.{i}{extension}");
                    var newBackupPath = Path.Combine(directory, $"{fileNameWithoutExt}.{i + 1}{extension}");
                    
                    if (File.Exists(oldBackupPath))
                    {
                        if (i == _maxBackupFiles - 1)
                        {
                            File.Delete(oldBackupPath); // 删除最老的文件
                        }
                        else
                        {
                            if (File.Exists(newBackupPath))
                                File.Delete(newBackupPath);
                            File.Move(oldBackupPath, newBackupPath);
                        }
                    }
                }

                // 将当前日志文件重命名为 .1
                var firstBackupPath = Path.Combine(directory, $"{fileNameWithoutExt}.1{extension}");
                if (File.Exists(firstBackupPath))
                    File.Delete(firstBackupPath);
                File.Move(_logFilePath, firstBackupPath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to rotate log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入日志到文件
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        public void WriteLog(LogLevel level, string message, params object[] args)
        {
            if (!_enable || _disposed)
                return;

            var formattedMessage = FormatLogMessage(level, message, args);
            WriteToFile(formattedMessage);
        }

        /// <summary>
        /// 写入异常日志到文件
        /// </summary>
        /// <param name="exception">异常对象</param>
        public void WriteException(Exception exception)
        {
            if (!_enable || _disposed)
                return;

            var formattedMessage = FormatLogMessage(LogLevel.Error, 
                $"Exception: {exception.Message}\nStackTrace:\n{exception.StackTrace}");
            WriteToFile(formattedMessage);
        }

        /// <summary>
        /// 异步写入日志到文件
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        public async Task WriteLogAsync(LogLevel level, string message, params object[] args)
        {
            if (!_enable || _disposed)
                return;

            var formattedMessage = FormatLogMessage(level, message, args);
            await WriteToFileAsync(formattedMessage);
        }

        /// <summary>
        /// 格式化日志消息
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">原始消息</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的消息</returns>
        private string FormatLogMessage(LogLevel level, string message, params object[] args)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper();
            
            var actualMessage = message;
            if (args != null && args.Length > 0)
            {
                try
                {
                    if (StringFormatter.NeedsFormatting(message))
                    {
                        actualMessage = StringFormatter.SafeFormat(message, args);
                    }
                    else
                    {
                        actualMessage = string.Format(message, args);
                    }
                }
                catch (Exception)
                {
                    actualMessage = $"{message} [格式化失败, 参数: {string.Join(", ", args)}]";
                }
            }

            return $"[{timestamp}] [{levelStr}] {actualMessage}";
        }

        /// <summary>
        /// 同步写入文件
        /// </summary>
        /// <param name="message">要写入的消息</param>
        private void WriteToFile(string message)
        {
            lock (_lockObject)
            {
                try
                {
                    if (_writer == null || _disposed)
                        return;

                    _writer.WriteLine(message);

                    // 检查文件大小，如果需要则进行轮转
                    if (_writer.BaseStream.Length > _maxFileSize)
                    {
                        RotateLogFile();
                        InitializeLogFile();
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 异步写入文件
        /// </summary>
        /// <param name="message">要写入的消息</param>
        private async Task WriteToFileAsync(string message)
        {
            await Task.Run(() => WriteToFile(message));
        }

        /// <summary>
        /// 刷新缓冲区
        /// </summary>
        public void Flush()
        {
            lock (_lockObject)
            {
                try
                {
                    _writer?.Flush();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to flush log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lockObject)
            {
                if (_writer != null)
                {
                    _writer.WriteLine($"=== Log Session Ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~LogFileWriter()
        {
            Dispose();
        }
    }
} 