using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Log;

namespace GF.Log
{
    /// <summary>
    /// 日志系统使用示例
    /// </summary>
    public class LogExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            // 设置玩家ID
            Log.PlayerId = 12345;
            
            // 测试按tag分类的日志写入
            TestTaggedLogging();
            
            // 开始定期测试
            //StartCoroutine(PeriodicLogging());
        }
        
        void TestTaggedLogging()
        {
            // 测试主日志系统
            Log.Debug("主系统调试信息");
            Log.Info("游戏开始，玩家ID：{0}");
            Log.Warning("主系统警告信息");
            Log.Error("主系统错误信息");
            
            // 测试Game tag日志 - 这些会写入到 user_data/12345/Logs/2024-01-15/Game/ 目录
            Log.Game.Info("游戏逻辑开始执行");
            Log.Game.Debug("角色生成完成");
            Log.Game.Warning("背包空间不足");
            Log.Game.Error("关卡加载失败");
            
            // 测试Net tag日志 - 这些会写入到 user_data/12345/Logs/2024-01-15/Net/ 目录
            Log.Net.Info("网络连接建立");
            Log.Net.Debug("发送心跳包");
            Log.Net.Warning("网络延迟较高");
            Log.Net.Error("连接超时");
            
            // 测试异常日志
            try
            {
                throw new System.Exception("测试Game异常");
            }
            catch (System.Exception ex)
            {
                Log.Game.Error(ex);
            }
            
            try
            {
                throw new System.InvalidOperationException("测试Net异常");
            }
            catch (System.Exception ex)
            {
                Log.Net.Error(ex);
            }
        }
        
        IEnumerator PeriodicLogging()
        {
            int counter = 0;
            while (true)
            {
                yield return new WaitForSeconds(3f);
                
                // 轮流写入不同tag的日志
                switch (counter % 3)
                {
                    case 0:
                        Log.Game.Info("游戏帧更新 #{0}", counter);
                        break;
                    case 1:
                        Log.Net.Info("网络状态检查 #{0}", counter);
                        break;
                    case 2:
                        Log.Info("主系统定期日志 #{0}", counter);
                        break;
                }
                
                counter++;
                if (counter >= 15)
                {
                    Log.Info("停止定期日志");
                    break;
                }
            }
        }
        
        void OnDestroy()
        {
            // 清理所有tag的LogFileWriter
            Log.Shutdown();
        }
        
        // 提供GUI按钮来测试
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            
            GUILayout.Label($"日志系统状态:");
            GUILayout.Label($"PlayerId: {Log.PlayerId}");
            GUILayout.Label($"当前时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            GUILayout.Space(10);
            GUILayout.Label("=== 主系统日志测试 ===");
            
            if (GUILayout.Button("主系统Debug日志"))
            {
                Log.Debug("手动触发的主系统Debug日志");
            }
            
            if (GUILayout.Button("主系统Info日志"))
            {
                Log.Info("手动触发的主系统Info日志");
            }
            
            if (GUILayout.Button("主系统Warning日志"))
            {
                Log.Warning("手动触发的主系统Warning日志");
            }
            
            if (GUILayout.Button("主系统Error日志"))
            {
                Log.Error("手动触发的主系统Error日志");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== Game Tag 日志测试 ===");
            
            if (GUILayout.Button("Game Info日志"))
            {
                Log.Game.Info("手动触发的游戏信息日志 - 时间: {0}", System.DateTime.Now);
            }
            
            if (GUILayout.Button("Game Warning日志"))
            {
                Log.Game.Warning("手动触发的游戏警告日志");
            }
            
            if (GUILayout.Button("Game Error日志"))
            {
                Log.Game.Error("手动触发的游戏错误日志");
            }
            
            if (GUILayout.Button("Game 异常日志"))
            {
                try
                {
                    throw new System.InvalidOperationException("手动触发的Game异常测试");
                }
                catch (System.Exception ex)
                {
                    Log.Game.Error(ex);
                }
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== Net Tag 日志测试 ===");
            
            if (GUILayout.Button("Net Info日志"))
            {
                Log.Net.Info("手动触发的网络信息日志 - 连接状态: {0}", "正常");
            }
            
            if (GUILayout.Button("Net Warning日志"))
            {
                Log.Net.Warning("手动触发的网络警告日志 - 延迟: {0}ms", UnityEngine.Random.Range(100, 500));
            }
            
            if (GUILayout.Button("Net Error日志"))
            {
                Log.Net.Error("手动触发的网络错误日志");
            }
            
            if (GUILayout.Button("Net 异常日志"))
            {
                try
                {
                    throw new System.TimeoutException("手动触发的Net超时异常");
                }
                catch (System.Exception ex)
                {
                    Log.Net.Error(ex);
                }
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== 系统控制 ===");
            
            if (GUILayout.Button("刷新所有日志缓冲区"))
            {
                Log.Game.Flush();
                Log.Net.Flush();
                Log.Info("所有日志缓冲区已刷新");
            }
            
            if (GUILayout.Button("查看日志文件路径"))
            {
                var gamePath = Log.Game.GetLogFilePath();
                var netPath = Log.Net.GetLogFilePath();
                UnityEngine.Debug.Log($"Game日志路径: {gamePath}");
                UnityEngine.Debug.Log($"Net日志路径: {netPath}");
            }
            
            if (GUILayout.Button("重新设置PlayerId"))
            {
                Log.PlayerId = UnityEngine.Random.Range(10000, 99999);
                Log.Info("PlayerId已重设为: {0}", Log.PlayerId);
            }
            
            GUILayout.Space(10);
            GUILayout.Label("日志文件目录结构:");
            GUILayout.Label("user_data/");
            GUILayout.Label($"  └─ {Log.PlayerId}/");
            GUILayout.Label("      └─ Logs/");
            GUILayout.Label($"          └─ {System.DateTime.Now:yyyy-MM-dd}/");
            GUILayout.Label("              ├─ Game/");
            GUILayout.Label("              ├─ Net/");
            GUILayout.Label("              └─ Default/");
            
            GUILayout.EndArea();
        }
    }
} 