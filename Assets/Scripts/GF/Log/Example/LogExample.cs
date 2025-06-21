using UnityEngine;

namespace GF.Log
{
    /// <summary>
    /// 日志系统使用示例
    /// </summary>
    public class LogExample : MonoBehaviour
    {
        void Start()
        {
            // 示例1：使用默认UnityLogger（仅控制台输出）
            Log.Info("默认日志输出到控制台");

            // 示例2：启用文件输出
            var unityLogger = Log.Logger as UnityLogger;
            unityLogger?.EnableFileOutput(); // 使用默认路径
            
            Log.Info("这条日志会同时输出到控制台和文件");
            Log.Warning("警告信息", "参数1", 123);
            Log.Error("错误信息");
            return;
            // 示例3：自定义文件路径
            var customLogger = new UnityLogger(true, "Logs/custom_log.log");
            Log.SetLogger(customLogger);
            
            Log.Info("使用自定义文件路径的日志");

            // 示例4：直接使用LogFileWriter
            var fileWriter = new LogFileWriter("Logs/direct_file.log");
            fileWriter.WriteLog(LogLevel.Info, "直接写入文件的日志");
            fileWriter.WriteLog(LogLevel.Debug, "格式化日志: {0} + {1} = {2}", 1, 2, 3);
            
            // 记得释放资源
            fileWriter.Dispose();

            // 示例5：使用CustomLog
            Log.Game.Info("游戏日志信息");
            Log.Net.Warning("网络警告");
        }

        void OnDestroy()
        {
            // 释放日志资源
            if (Log.Logger is UnityLogger logger)
            {
                logger.Dispose();
            }
        }
    }
} 