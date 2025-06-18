using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GF.Log
{
    public class FilteredLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private readonly List<ILogFilter> _filters;

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

        public FilteredLogger(ILogger innerLogger)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _filters = new List<ILogFilter>();
        }

        public void AddFilter(ILogFilter filter)
        {
            if (filter != null && !_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        public void RemoveFilter(ILogFilter filter)
        {
            _filters.Remove(filter);
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            var formattedMessage = args?.Length > 0 ? string.Format(message, args) : message;

            // 应用所有过滤器
            foreach (var filter in _filters)
            {
                if (!filter.ShouldLog(level, formattedMessage))
                    return;
            }

            _innerLogger.Log(level, message, args);
        }

        public void Log(Exception exception)
        {
            // 应用所有过滤器
            foreach (var filter in _filters)
            {
                if (!filter.ShouldLog(LogLevel.Error, exception.ToString()))
                    return;
            }

            _innerLogger.Log(exception);
        }
    }

    public interface ILogFilter
    {
        bool ShouldLog(LogLevel level, string message);
    }

    // 基于关键词的过滤器
    public class KeywordFilter : ILogFilter
    {
        private readonly HashSet<string> _excludeKeywords;
        private readonly HashSet<string> _includeKeywords;
        private readonly bool _isExcludeMode;

        public KeywordFilter(bool isExcludeMode = true)
        {
            _isExcludeMode = isExcludeMode;
            _excludeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _includeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddExcludeKeyword(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
                _excludeKeywords.Add(keyword);
        }

        public void AddIncludeKeyword(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
                _includeKeywords.Add(keyword);
        }

        public bool ShouldLog(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(message))
                return true;

            if (_isExcludeMode)
            {
                // 排除模式：如果消息包含排除关键词，则不记录
                foreach (var keyword in _excludeKeywords)
                {
                    if (message.Contains(keyword))
                        return false;
                }
                return true;
            }
            else
            {
                // 包含模式：只有消息包含包含关键词时才记录
                if (_includeKeywords.Count == 0)
                    return true;

                foreach (var keyword in _includeKeywords)
                {
                    if (message.Contains(keyword))
                        return true;
                }
                return false;
            }
        }
    }

    // 基于正则表达式的过滤器
    public class RegexFilter : ILogFilter
    {
        private readonly Regex _regex;
        private readonly bool _shouldMatch;

        public RegexFilter(string pattern, bool shouldMatch = false)
        {
            _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _shouldMatch = shouldMatch;
        }

        public bool ShouldLog(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(message))
                return true;

            bool matches = _regex.IsMatch(message);
            return _shouldMatch ? matches : !matches;
        }
    }

    // 基于频率限制的过滤器
    public class RateLimitFilter : ILogFilter
    {
        private readonly Dictionary<string, DateTime> _lastLogTimes;
        private readonly TimeSpan _minInterval;

        public RateLimitFilter(TimeSpan minInterval)
        {
            _minInterval = minInterval;
            _lastLogTimes = new Dictionary<string, DateTime>();
        }

        public bool ShouldLog(LogLevel level, string message)
        {
            var key = $"{level}:{message}";
            var now = DateTime.Now;

            if (!_lastLogTimes.ContainsKey(key))
            {
                _lastLogTimes[key] = now;
                return true;
            }

            if (now - _lastLogTimes[key] >= _minInterval)
            {
                _lastLogTimes[key] = now;
                return true;
            }

            return false;
        }
    }
}
