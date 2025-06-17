using System;
using System.Collections.Generic;
using UnityEngine;

namespace GF.Log
{
    [Serializable]
    public class LogConfig
    {
        [Header("基础设置")]
        public bool enableLogging = true;
        public LogLevel globalLogLevel = LogLevel.Debug;

        [Header("文件日志设置")]
        public bool enableFileLogging = true;
        public string logFileName = "";
        public int maxLogFileSize = 10; // MB
        public int maxLogFiles = 5;

        [Header("缓冲设置")]
        public bool enableBuffering = true;
        public int bufferSize = 100;
        public int flushIntervalMs = 1000;

        [Header("过滤设置")]
        public bool enableFiltering = false;
        public List<string> excludeKeywords = new List<string>();
        public List<string> includeKeywords = new List<string>();

        [Header("性能设置")]
        public bool enableRateLimit = false;
        public int rateLimitIntervalMs = 1000;

        [Header("调试设置")]
        public bool showUnityConsole = false;
        public bool enableStackTrace = true;

        // 单例实例
        private static LogConfig _instance;
        public static LogConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadConfig();
                }
                return _instance;
            }
        }

        public static void LoadConfig()
        {
            try
            {
                // 尝试从 PlayerPrefs 加载配置
                var jsonData = PlayerPrefs.GetString("LogConfig", "");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    _instance = JsonUtility.FromJson<LogConfig>(jsonData);
                }
                else
                {
                    _instance = CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载日志配置失败: {ex.Message}");
                _instance = CreateDefaultConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var jsonData = JsonUtility.ToJson(_instance, true);
                PlayerPrefs.SetString("LogConfig", jsonData);
                PlayerPrefs.Save();
                Debug.Log("日志配置已保存");
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存日志配置失败: {ex.Message}");
            }
        }

        private static LogConfig CreateDefaultConfig()
        {
            return new LogConfig
            {
                enableLogging = true,
                globalLogLevel = LogLevel.Debug,
                enableFileLogging = true,
                logFileName = $"game_log_{DateTime.Now:yyyyMMdd}.log",
                maxLogFileSize = 10,
                maxLogFiles = 5,
                enableBuffering = true,
                bufferSize = 100,
                flushIntervalMs = 1000,
                enableFiltering = false,
                excludeKeywords = new List<string>(),
                includeKeywords = new List<string>(),
                enableRateLimit = false,
                rateLimitIntervalMs = 1000,
                showUnityConsole = false,
                enableStackTrace = true,
            };
        }

        // 应用配置到日志系统
        public void ApplyToLogSystem()
        {
            // 设置全局日志级别
            Log.SetEnable(enableLogging);
            Log.SetEnableLevel(globalLogLevel);

            // 设置Unity控制台显示
            Debug.developerConsoleEnabled = showUnityConsole;
            Debug.developerConsoleVisible = showUnityConsole;

            Debug.Log($"日志配置已应用 - 启用: {enableLogging}, 级别: {globalLogLevel}");
        }
    }

    // 日志配置编辑器（用于运行时调整）
    public static class LogConfigEditor
    {
        public static void SetLogLevel(LogLevel level)
        {
            LogConfig.Instance.globalLogLevel = level;
            Log.SetEnableLevel(level);
            LogConfig.SaveConfig();
        }

        public static void SetEnableLogging(bool enable)
        {
            LogConfig.Instance.enableLogging = enable;
            Log.SetEnable(enable);
            LogConfig.SaveConfig();
        }

        public static void AddExcludeKeyword(string keyword)
        {
            if (!LogConfig.Instance.excludeKeywords.Contains(keyword))
            {
                LogConfig.Instance.excludeKeywords.Add(keyword);
                LogConfig.SaveConfig();
            }
        }

        public static void RemoveExcludeKeyword(string keyword)
        {
            LogConfig.Instance.excludeKeywords.Remove(keyword);
            LogConfig.SaveConfig();
        }

        public static void ShowCurrentConfig()
        {
            var config = LogConfig.Instance;
            Debug.Log(
                $"=== 当前日志配置 ===\n"
                    + $"启用日志: {config.enableLogging}\n"
                    + $"日志级别: {config.globalLogLevel}\n"
                    + $"文件日志: {config.enableFileLogging}\n"
                    + $"缓冲功能: {config.enableBuffering}\n"
                    + $"过滤功能: {config.enableFiltering}\n"
                    + $"频率限制: {config.enableRateLimit}"
            );
        }
    }
}
