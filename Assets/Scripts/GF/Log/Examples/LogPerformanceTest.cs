using System;
using System.Diagnostics;
using UnityEngine;
using GF.Log;

namespace GF.Log.Examples
{
    /// <summary>
    /// 日志系统性能测试工具
    /// </summary>
    public class LogPerformanceTest : MonoBehaviour
    {
        [Header("测试配置")]
        public int testCount = 10000;
        public bool testBuffered = true;
        public bool testFiltered = true;
        public bool testFormatter = true;
        
        private void Start()
        {
            RunPerformanceTests();
        }

        [ContextMenu("运行性能测试")]
        public void RunPerformanceTests()
        {
            UnityEngine.Debug.Log("=== 日志系统性能测试开始 ===");
            
            // 测试LogFormatter性能
            if (testFormatter)
            {
                TestFormatter();
            }
            
            // 测试基础UnityLogger
            TestLogger("基础UnityLogger", new UnityLogger());
            
            // 测试优化后的FileLogger
            var fileLogger = new FileLogger("performance_test.log");
            TestLogger("FileLogger(已优化)", fileLogger);
            
            // 测试BufferedLogger
            if (testBuffered)
            {
                var bufferedLogger = new BufferedLogger(new UnityLogger(), 100, 5000);
                TestLogger("BufferedLogger", bufferedLogger);
                bufferedLogger.Dispose();
            }
            
            // 测试FilteredLogger
            if (testFiltered)
            {
                var filteredLogger = new FilteredLogger(new UnityLogger());
                filteredLogger.AddFilter(new KeywordFilter(true)); // 空过滤器
                TestLogger("FilteredLogger(空过滤)", filteredLogger);
                
                // 添加一些过滤规则
                var keywordFilter = new KeywordFilter(true);
                keywordFilter.AddExcludeKeyword("exclude");
                filteredLogger.AddFilter(keywordFilter);
                TestLogger("FilteredLogger(有过滤)", filteredLogger);
            }
            
            // 测试组合日志器
            var compositeLogger = new CompositeLogger(
                new UnityLogger(),
                new FileLogger("composite_test.log")
            );
            TestLogger("CompositeLogger", compositeLogger);
            
            UnityEngine.Debug.Log("=== 日志系统性能测试完成 ===");
        }

        private void TestLogger(string loggerName, ILogger logger)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 禁用实际输出以专注于性能测试
            var originalEnable = logger.Enable;
            logger.Enable = false;
            
            try
            {
                // 测试不同类型的日志调用
                for (int i = 0; i < testCount; i++)
                {
                    logger.Log(LogLevel.Info, "测试消息 {0}: {1}", i, "性能测试");
                    
                    if (i % 4 == 0)
                        logger.Log(LogLevel.Debug, "调试消息 " + i);
                    else if (i % 4 == 1)
                        logger.Log(LogLevel.Warning, "警告消息 {0}", i);
                    else if (i % 4 == 2)
                        logger.Log(LogLevel.Error, "错误消息: {0} - {1}", i, DateTime.Now);
                    else
                        logger.Log(LogLevel.Info, "简单消息");
                }
            }
            finally
            {
                logger.Enable = originalEnable;
            }
            
            stopwatch.Stop();
            
            var totalTime = stopwatch.ElapsedMilliseconds;
            var avgTime = (double)totalTime / testCount;
            var logsPerSecond = testCount / (stopwatch.ElapsedMilliseconds / 1000.0);
            
            UnityEngine.Debug.Log($"[{loggerName}] " +
                                $"总时间: {totalTime}ms, " +
                                $"平均: {avgTime:F4}ms/log, " +
                                $"吞吐量: {logsPerSecond:F0} logs/s");
        }

        [ContextMenu("内存分配测试")]
        public void TestMemoryAllocation()
        {
            UnityEngine.Debug.Log("=== 内存分配测试 ===");
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            // 测试大量日志分配 - 使用优化后的UnityLogger
            var logger = new UnityLogger();
            logger.Enable = false; // 禁用输出
            
            for (int i = 0; i < 1000; i++)
            {
                logger.Log(LogLevel.Info, "内存测试 {0}: {1} - {2}", i, DateTime.Now, "长字符串测试内容");
            }
            
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedMemory = finalMemory - initialMemory;
            
            UnityEngine.Debug.Log($"分配内存: {allocatedMemory} bytes ({allocatedMemory / 1024.0:F2} KB)");
            UnityEngine.Debug.Log("测试完成 - 使用了LogFormatter优化的格式化");
        }

        [ContextMenu("并发测试")]
        public void TestConcurrency()
        {
            UnityEngine.Debug.Log("=== 并发测试 ===");
            
            var bufferedLogger = new BufferedLogger(new UnityLogger(), 50, 1000);
            bufferedLogger.Enable = false;
            
            var tasks = new System.Threading.Tasks.Task[4];
            
            for (int t = 0; t < tasks.Length; t++)
            {
                int taskId = t;
                tasks[t] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        bufferedLogger.Log(LogLevel.Info, "任务 {0} 消息 {1}", taskId, i);
                    }
                });
            }
            
            System.Threading.Tasks.Task.WaitAll(tasks);
            bufferedLogger.Flush();
            bufferedLogger.Dispose();
            
            UnityEngine.Debug.Log("并发测试完成 - 4个任务各发送1000条日志");
        }

        [ContextMenu("测试格式化器性能")]
        public void TestFormatter()
        {
            UnityEngine.Debug.Log("=== LogFormatter 性能测试 ===");
            
            var testMessages = new[]
            {
                "简单消息",
                "格式化消息 {0}",
                "多参数消息 {0}: {1} - {2}",
                "复杂格式 {0} at {1} with value {2} and status {3}"
            };
            
            var testArgs = new object[]
            {
                42,
                DateTime.Now,
                "test string",
                true
            };
            
            // 测试LogFormatter
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++)
            {
                var msg = testMessages[i % testMessages.Length];
                var args = new object[] { testArgs[0], testArgs[1] };
                LogFormatter.Format(msg, args);
            }
            stopwatch.Stop();
            
            var formatterTime = stopwatch.ElapsedMilliseconds;
            
            // 测试标准string.Format
            stopwatch.Restart();
            for (int i = 0; i < testCount; i++)
            {
                var msg = testMessages[i % testMessages.Length];
                var args = new object[] { testArgs[0], testArgs[1] };
                try
                {
                    string.Format(msg, args);
                }
                catch
                {
                    // 忽略格式化错误
                }
            }
            stopwatch.Stop();
            
            var standardTime = stopwatch.ElapsedMilliseconds;
            var improvement = ((double)(standardTime - formatterTime) / standardTime) * 100;
            
            UnityEngine.Debug.Log($"LogFormatter: {formatterTime}ms");
            UnityEngine.Debug.Log($"string.Format: {standardTime}ms");
            UnityEngine.Debug.Log($"性能提升: {improvement:F1}%");
        }

        [ContextMenu("测试日志级别过滤")]
        public void TestLogLevelFiltering()
        {
            UnityEngine.Debug.Log("=== 日志级别过滤测试 ===");
            
            var unityLogger = new UnityLogger();
            var fileLogger = new FileLogger("level_test.log");
            
            // 测试不同的EnableLevel设置
            var testLevels = new[] { LogLevel.Debug, LogLevel.Info, LogLevel.Warning, LogLevel.Error };
            
            foreach (var enableLevel in testLevels)
            {
                UnityEngine.Debug.Log($"\n--- 设置EnableLevel为: {enableLevel} ---");
                
                unityLogger.EnableLevel = enableLevel;
                fileLogger.EnableLevel = enableLevel;
                
                // 测试每个级别的日志
                foreach (var logLevel in testLevels)
                {
                    // 预期结果：只有当 logLevel >= enableLevel 时才应该记录
                    bool shouldLog = (int)logLevel >= (int)enableLevel;
                    
                    // 测试UnityLogger（禁用实际输出）
                    unityLogger.Enable = false;
                    unityLogger.Log(logLevel, $"测试消息 - 级别: {logLevel}");
                    
                    // 测试FileLogger（禁用实际输出）
                    fileLogger.Enable = false;
                    fileLogger.Log(logLevel, $"测试消息 - 级别: {logLevel}");
                    
                    UnityEngine.Debug.Log($"  {logLevel}: 应该记录={shouldLog}");
                }
                
                unityLogger.Enable = true;
                fileLogger.Enable = true;
            }
            
            UnityEngine.Debug.Log("=== 日志级别过滤测试完成 ===");
        }
    }
} 