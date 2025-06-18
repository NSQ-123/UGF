using System;
using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 日志系统使用示例
    /// 展示了优化后的日志系统的各种功能
    /// </summary>
    public class LogExample : MonoBehaviour
    {
        private CustomLog _gameLog;
        private CustomLog _networkLog;

        void Start()
        {
            // 初始化自定义日志器
            _gameLog = new CustomLog("游戏", true, "#00FF00");
            _networkLog = new CustomLog("网络", true, "#FF6600");

            // 演示各种日志功能
            DemonstrateBasicLogging();
            DemonstrateCustomLogging();
            DemonstrateConfigManagement();
            DemonstrateAdvancedFeatures();
        }

        /// <summary>
        /// 演示基础日志功能
        /// </summary>
        void DemonstrateBasicLogging()
        {
            Debug.Log("=== 基础日志功能演示 ===");

            // 使用全局日志
            Log.Debug("这是一条调试信息");
            Log.Info("玩家登录成功，用户ID: {0}", 12345);
            Log.Warning("内存使用率较高: {0}%", 85.5f);
            Log.Error("加载资源失败: {0}", "texture_missing.png");

            // 异常日志
            try
            {
                throw new InvalidOperationException("这是一个测试异常");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// 演示自定义日志功能
        /// </summary>
        void DemonstrateCustomLogging()
        {
            Debug.Log("=== 自定义日志功能演示 ===");

            // 使用带标签的日志
            _gameLog.Info("游戏开始，关卡: {0}", "Level_01");
            _gameLog.Debug("玩家位置更新: ({0}, {1}, {2})", 10.5f, 0.0f, 20.3f);
            _gameLog.Warning("生命值较低: {0}/100", 15);

            _networkLog.Info("连接服务器成功");
            _networkLog.Error("网络超时，尝试重连...");

            // 使用颜色化信息
            _gameLog.Info("关卡完成！", Color.green);

            // 使用方法跟踪
            _gameLog.Trace("进入游戏主循环");
            _gameLog.Trace("检查点保存", "CheckpointManager.SaveCheckpoint");
        }

        /// <summary>
        /// 演示配置管理
        /// </summary>
        void DemonstrateConfigManagement()
        {
            Debug.Log("=== 配置管理演示 ===");

            // 显示当前配置
            LogConfigEditor.ShowCurrentConfig();

            // 动态调整日志级别
            LogConfigEditor.SetLogLevel(LogLevel.Warning);
            Log.Debug("这条调试信息不会显示"); // 因为级别设置为Warning
            Log.Warning("这条警告信息会显示");

            // 恢复调试级别
            LogConfigEditor.SetLogLevel(LogLevel.Debug);

            // 添加过滤关键词
            LogConfigEditor.AddExcludeKeyword("广告");
            Log.Info("显示广告 - 这条消息会被过滤");
            Log.Info("游戏正常运行 - 这条消息正常显示");

            // 移除过滤关键词
            LogConfigEditor.RemoveExcludeKeyword("广告");
        }

        /// <summary>
        /// 演示高级功能
        /// </summary>
        void DemonstrateAdvancedFeatures()
        {
            Debug.Log("=== 高级功能演示 ===");

            // 创建自定义日志器组合
            var customUnityLogger = new UnityLogger();
            var customFileLogger = new FileLogger("custom_demo.log");
            var compositeLogger = new CompositeLogger(customUnityLogger, customFileLogger);

            // 添加过滤器
            var filteredLogger = new FilteredLogger(compositeLogger);
            var keywordFilter = new KeywordFilter(true);
            keywordFilter.AddExcludeKeyword("垃圾信息");
            filteredLogger.AddFilter(keywordFilter);

            // 添加频率限制
            var rateLimitFilter = new RateLimitFilter(TimeSpan.FromSeconds(1));
            filteredLogger.AddFilter(rateLimitFilter);

            // 使用自定义日志器
            filteredLogger.Log(LogLevel.Info,"这是自定义日志器的信息");
            filteredLogger.Log(LogLevel.Info,"垃圾信息 - 会被过滤");

            // 测试频率限制
            for (int i = 0; i < 5; i++)
            {
                filteredLogger.Log(LogLevel.Info,"重复消息 - 只有第一条会显示");
            }

            // 刷新缓冲区
            Log.FlushBuffers();

            Debug.Log("日志系统功能演示完成！");
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // 应用暂停时刷新日志缓冲区
            if (pauseStatus)
            {
                Log.FlushBuffers();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            // 应用失去焦点时刷新日志缓冲区
            if (!hasFocus)
            {
                Log.FlushBuffers();
            }
        }

        void OnDestroy()
        {
            // 销毁时确保所有日志都被写入
            Log.FlushBuffers();
        }
    }
}
