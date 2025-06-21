using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GF.Log
{
    public partial class LogFileWriter
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        private static readonly string ROOT_DIR = Application.dataPath + "/../";
#else
        private static readonly string ROOT_DIR = Application.persistentDataPath + "/";
#endif
        private static string DATA_DIR => $"{ROOT_DIR}user_data/";

        /// <summary>
        /// 日志数据类
        /// </summary>
        private class LogEntry
        {
            public string text;
        }

        private static readonly LogLevel STLogLevel = LogLevel.Error;
    }

    /// <summary>
    /// 日志文件写入器 - 专门处理日志文件的写入操作
    /// </summary>
    public partial class LogFileWriter : IDisposable
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private StreamWriter _writer;
        private bool _disposed = false;
        private bool _enable = true;
        private readonly string _customFolder;

        // 缓存机制相关
        private readonly List<LogEntry> _poolList = new List<LogEntry>();
        private readonly List<LogEntry> _writeList = new List<LogEntry>();
        private readonly object _writeLock = new object();
        private bool _isProcessing = false;
        private readonly ILogger _logger;
        private long PlayerId => _logger.PlayerID;

        /// <summary>
        /// 是否启用文件写入
        /// </summary>
        public bool Enable
        {
            get => _enable;
            set => _enable = value;
        }

        public LogFileWriter(ILogger logger, string customFolder = "")
        {
            _logger = logger;
            _customFolder = string.IsNullOrEmpty(customFolder) ? "Default" : customFolder;
            _logFilePath = InitializeLogFile();
            // 启动处理协程（通过MonoBehaviour更新）
            LogFileProcessor.Instance.RegisterWriter(this);
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private string InitializeLogFile()
        {
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var startTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                // 文件结构: user_data/playerId/Logs/当天的日期/自定义的文件夹/启动时间.log
                var logDir = $"{DATA_DIR}{PlayerId}/Logs/{today}/{_customFolder}";

                // 确保目录存在
                if (!Directory.Exists(DATA_DIR))
                    Directory.CreateDirectory(DATA_DIR);

                var playerDir = $"{DATA_DIR}{PlayerId}";
                if (!Directory.Exists(playerDir))
                    Directory.CreateDirectory(playerDir);

                var logsDir = $"{playerDir}/Logs";
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

                var todayDir = $"{logsDir}/{today}";
                if (!Directory.Exists(todayDir))
                    Directory.CreateDirectory(todayDir);

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                var logFilePath = $"{logDir}/{startTime}.log";

                // 初始化StreamWriter
                lock (_lockObject)
                {
                    _writer = new StreamWriter(logFilePath, false, Encoding.UTF8);
                }

                return logFilePath;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize log file: {ex.Message}");
                return null;
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

            var formattedMessage = FormatLogMessage(level, message, true, args);
            WriteToCache(formattedMessage);
        }

        /// <summary>
        /// 写入异常日志到文件
        /// </summary>
        /// <param name="ex">异常对象</param>
        public void WriteException(Exception ex)
        {
            if (!_enable || _disposed)
                return;

            // 格式化异常信息
            var exceptionMessage = FormatExceptionMessage(ex);
            var formattedMessage = FormatLogMessage(LogLevel.Error, exceptionMessage, false);
            WriteToCache(formattedMessage);
        }

        /// <summary>
        /// 格式化异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>格式化后的异常信息</returns>
        private string FormatExceptionMessage(Exception ex)
        {
            var message = $"Exception: {ex.GetType().Name} - {ex.Message}";

            // 格式化异常详情
            var details = new List<string>();

            if (!string.IsNullOrEmpty(ex.Source))
                details.Add($"Source: {ex.Source}");

            if (ex.InnerException != null)
                details.Add(
                    $"InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}"
                );

            // 添加异常详情（如果有）
            if (details.Count > 0)
            {
                var indentedDetails = string.Join("\n", details.Select(detail => $"    {detail}"));
                message += $"\n    Exception Details:\n{indentedDetails}";
            }

            // 添加堆栈跟踪
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                var stackLines = ex.StackTrace.Split('\n');
                var indentedStack = string.Join(
                    "\n",
                    stackLines.Select(line =>
                        string.IsNullOrWhiteSpace(line) ? line : $"    {line.Trim()}"
                    )
                );
                message += $"\n    StackTrace:\n{indentedStack}";
            }

            return message;
        }

        /// <summary>
        /// 格式化日志消息
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">原始消息</param>
        /// <param name="needST">是否需要堆栈跟踪</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的消息</returns>
        private string FormatLogMessage(
            LogLevel level,
            string message,
            bool needST = true,
            params object[] args
        )
        {
            var timestamp = TimeStampCache.GetTimestamp();
            // 使用左对齐的完整级别名称，固定宽度为7个字符（WARNING是最长的）
            var levelStr = level.ToString().ToUpper().PadRight(7);

            var actualMessage = message;
            if (args is { Length: > 0 })
            {
                try
                {
                    actualMessage = string.Format(message, args);
                }
                catch (Exception)
                {
                    actualMessage = $"{message} [格式化失败, 参数: {string.Join(", ", args)}]";
                }
            }

            if (needST && level >= STLogLevel)
            {
                StackTrace st = new StackTrace(4, true);
                var stackLines = st.ToString().Split('\n');
                var indentedStack = string.Join(
                    "\n",
                    stackLines.Select(line =>
                        string.IsNullOrWhiteSpace(line) ? line : $"    {line.Trim()}"
                    )
                );
                actualMessage = $"{actualMessage}\n    StackTrace:\n{indentedStack}";
    
            }
            return $"[{timestamp}] [{levelStr}] {actualMessage}\n\n";
        }

        /// <summary>
        /// 写入到缓存中，避免频繁IO
        /// </summary>
        /// <param name="message">要写入的消息</param>
        private void WriteToCache(string message)
        {
            var logData = GetEmptyLogData();
            logData.text = message;

            lock (_writeLock)
            {
                _writeList.Add(logData);
            }
        }

        /// <summary>
        /// 获取空的LogData对象（对象池）
        /// </summary>
        /// <returns></returns>
        private LogEntry GetEmptyLogData()
        {
            lock (_poolList)
            {
                if (_poolList.Count > 0)
                {
                    var logEntry = _poolList[_poolList.Count - 1];
                    _poolList.RemoveAt(_poolList.Count - 1);
                    return logEntry;
                }
            }
            // 在锁外创建新对象，减少锁定时间
            return new LogEntry();
        }

        /// <summary>
        /// 处理缓存的日志数据（由LogFileProcessor调用）
        /// </summary>
        internal void ProcessCachedLogs()
        {
            // 先检查是否需要处理，避免不必要的锁竞争
            if (_disposed)
                return;

            List<LogEntry> logsToProcess = null;

            // 在锁内检查和设置处理状态
            lock (_writeLock)
            {
                if (_isProcessing || _writeList.Count == 0 || _disposed)
                    return;

                _isProcessing = true;

                // 交换列表，减少锁定时间
                logsToProcess = new List<LogEntry>(_writeList);
                _writeList.Clear();
            }

            try
            {
                // 文件写入使用单独的锁
                lock (_lockObject)
                {
                    if (_writer == null || _disposed)
                        return;

                    foreach (var logData in logsToProcess)
                    {
                        if (!string.IsNullOrEmpty(logData.text))
                        {
                            _writer.Write(logData.text);
                            logData.text = string.Empty; // 清空文本
                        }
                    }
                    _writer.Flush();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to write logs: {ex.Message}");
            }
            finally
            {
                // 将LogData返回到对象池
                if (logsToProcess != null)
                {
                    lock (_poolList)
                    {
                        _poolList.AddRange(logsToProcess);
                    }
                }

                // 重置处理状态
                lock (_writeLock)
                {
                    _isProcessing = false;
                }
            }
        }

        /// <summary>
        /// 刷新缓冲区
        /// </summary>
        public void Flush()
        {
            ProcessCachedLogs();
        }

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        /// <summary>
        /// 获取日志目录路径
        /// </summary>
        public string GetLogDirectory()
        {
            return string.IsNullOrEmpty(_logFilePath)
                ? string.Empty
                : Path.GetDirectoryName(_logFilePath);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            lock (_writeLock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            // 最后一次处理缓存的日志
            ProcessCachedLogs();

            // 注销处理器
            LogFileProcessor.Instance?.UnregisterWriter(this);

            lock (_lockObject)
            {
                try
                {
                    _writer?.Close();
                    _writer?.Dispose();
                    _writer = null;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error disposing LogFileWriter: {ex.Message}");
                }
            }

            // 清理缓存列表
            lock (_writeLock)
            {
                _writeList.Clear();
            }

            lock (_poolList)
            {
                _poolList.Clear();
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

    /// <summary>
    /// 日志文件处理器 - 单例，负责定期处理所有LogFileWriter的缓存
    /// </summary>
    public class LogFileProcessor : MonoBehaviour
    {
        private static LogFileProcessor _instance;
        public static LogFileProcessor Instance
        {
            get
            {
                if (_instance)
                    return _instance;
                var go = new GameObject("LogFileProcessor");
                _instance = go.AddComponent<LogFileProcessor>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        private readonly List<LogFileWriter> _writers = new List<LogFileWriter>();
        private readonly object _writersLock = new object();

        public void RegisterWriter(LogFileWriter writer)
        {
            lock (_writersLock)
            {
                if (!_writers.Contains(writer))
                {
                    _writers.Add(writer);
                }
            }
        }

        public void UnregisterWriter(LogFileWriter writer)
        {
            lock (_writersLock)
            {
                _writers.Remove(writer);
            }
        }

        private void Update()
        {
            lock (_writersLock)
            {
                foreach (var writer in _writers.Where(writer => writer != null))
                {
                    writer.ProcessCachedLogs();
                }
            }
        }

        private void OnDestroy()
        {
            // 清理所有writers
            lock (_writersLock)
            {
                foreach (var writer in _writers)
                {
                    writer?.Flush();
                }
                _writers.Clear();
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
