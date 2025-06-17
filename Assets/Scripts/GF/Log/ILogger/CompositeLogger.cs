using System;
using System.Collections.Generic;
using System.Linq;

namespace GF.Log
{
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers;

        public bool Enable
        {
            get => _loggers.Any(l => l.Enable);
            set => _loggers.ForEach(l => l.Enable = value);
        }

        public LogLevel EnableLevel
        {
            get => _loggers.Count > 0 ? _loggers.Min(l => l.EnableLevel) : LogLevel.Debug;
            set => _loggers.ForEach(l => l.EnableLevel = value);
        }

        public CompositeLogger(params ILogger[] loggers)
        {
            _loggers = loggers?.ToList() ?? new List<ILogger>();
        }

        public void AddLogger(ILogger logger)
        {
            if (logger != null && !_loggers.Contains(logger))
            {
                _loggers.Add(logger);
            }
        }

        public void RemoveLogger(ILogger logger)
        {
            _loggers.Remove(logger);
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                try
                {
                    logger.Log(level, message, args);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"日志器执行失败: {ex.Message}");
                }
            }
        }

        public void Log(Exception exception)
        {
            foreach (var logger in _loggers)
            {
                try
                {
                    logger.Log(exception);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"日志器执行失败: {ex.Message}");
                }
            }
        }
    }
}
