using System.Linq;
using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 组合字符串使用示例
    /// </summary>
    public static class CompositeStringExample
    {
        /// <summary>
        /// 演示组合字符串的各种用法
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void RunExamples()
        {
            var optionService = OptionSystem.Service;
            
            Debug.Log("=== 组合字符串示例 ===");
            
            // 示例1：保存玩家装备列表
            Debug.Log("\n1. 玩家装备列表示例:");
            string[] equipment = { "剑", "盾牌", "头盔", "护甲" };
            optionService.SetCompositeString("PlayerEquipment", equipment);
            
            string[] loadedEquipment = optionService.GetCompositeString("PlayerEquipment");
            Debug.Log($"装备列表: [{string.Join(", ", loadedEquipment)}]");
            
            // 示例2：保存游戏设置（混合类型）
            Debug.Log("\n2. 游戏设置示例:");
            optionService.SetCompositeValues("GameSettings", "Hard", 100, 0.8f, true);
            
            string[] gameSettings = optionService.GetCompositeString("GameSettings");
            Debug.Log($"游戏设置: 难度={gameSettings[0]}, 生命值={gameSettings[1]}, 音量={gameSettings[2]}, 启用音效={gameSettings[3]}");
            
            // 示例3：使用指定分隔符
            Debug.Log("\n3. 指定分隔符示例:");
            string[] serverList = { "服务器1", "服务器2", "服务器3" };
            optionService.SetCompositeString("ServerList", CompositeStringHelper.Separators.Pipe, serverList);
            
            string[] loadedServers = optionService.GetCompositeString("ServerList", CompositeStringHelper.Separators.Pipe);
            Debug.Log($"服务器列表: [{string.Join(", ", loadedServers)}]");
            
            // 示例4：类型化解析
            Debug.Log("\n4. 类型化解析示例:");
            optionService.SetCompositeString("PlayerStats", CompositeStringHelper.Separators.Semicolon, "100", "50", "25", "75");
            
            int[] stats = optionService.GetCompositeValues<int>("PlayerStats", CompositeStringHelper.Separators.Semicolon);
            Debug.Log($"玩家属性: 生命={stats[0]}, 魔法={stats[1]}, 攻击={stats[2]}, 防御={stats[3]}");
            
            // 示例5：处理包含特殊字符的字符串
            Debug.Log("\n5. 特殊字符处理示例:");
            string[] specialStrings = { "包含|管道符的文本", "普通文本", "包含;分号的文本" };
            
            // 自动选择最佳分隔符
            char bestSeparator = CompositeStringHelper.SelectBestSeparator(specialStrings);
            Debug.Log($"为特殊字符串选择的分隔符: {CompositeStringHelper.GetSeparatorDescription(bestSeparator)}");
            
            optionService.SetCompositeString("SpecialStrings", bestSeparator, specialStrings);
            string[] loadedSpecialStrings = optionService.GetCompositeString("SpecialStrings", bestSeparator);
            Debug.Log($"特殊字符串列表: [{string.Join(", ", loadedSpecialStrings)}]");
            
            // 示例6：安全解析
            Debug.Log("\n6. 安全解析示例:");
            if (optionService.TryGetCompositeValues<float>("FloatList", CompositeStringHelper.Separators.Comma, out float[] floatValues))
            {
                Debug.Log($"浮点数列表解析成功: [{string.Join(", ", floatValues)}]");
            }
            else
            {
                Debug.Log("浮点数列表解析失败，可能是因为配置不存在或格式错误");
                
                // 设置一些浮点数进行测试
                optionService.SetCompositeValues("FloatList", CompositeStringHelper.Separators.Comma, 1.5f, 2.7f, 3.14f);
                
                if (optionService.TryGetCompositeValues<float>("FloatList", CompositeStringHelper.Separators.Comma, out floatValues))
                {
                    Debug.Log($"重新解析成功: [{string.Join(", ", floatValues)}]");
                }
            }
            
            // 示例7：推荐分隔符展示（INI安全）
            Debug.Log("\n7. 可用分隔符列表 (INI安全):");
            foreach (char separator in CompositeStringHelper.RecommendedSeparators)
            {
                string description = CompositeStringHelper.GetSeparatorDescription(separator);
                bool isIniSafe = !CompositeStringHelper.IsIniReservedChar(separator);
                Debug.Log($"  {description} {(isIniSafe ? "✓" : "⚠️")}");
            }
            
            // 示例8：INI冲突字符警告
            Debug.Log("\n8. INI文件冲突字符示例:");
            char[] dangerousChars = { '=', '#', '[', ']', '"', '\\' };
            foreach (char dangerousChar in dangerousChars)
            {
                Debug.Log($"  {CompositeStringHelper.GetSeparatorDescription(dangerousChar)}");
            }
            
            Debug.Log("\n=== 组合字符串示例完成 ===");
        }
        
        /// <summary>
        /// 实际使用场景示例：保存玩家的关卡进度
        /// </summary>
        public static void SavePlayerProgress(int[] levelStars, float[] levelTimes)
        {
            var optionService = OptionSystem.Service;
            
            // 保存关卡星级
            optionService.SetCompositeValues("LevelStars", CompositeStringHelper.Separators.Pipe, levelStars.Cast<object>().ToArray());
            
            // 保存关卡时间
            optionService.SetCompositeValues("LevelTimes", CompositeStringHelper.Separators.Pipe, levelTimes.Cast<object>().ToArray());
            
            Debug.Log($"保存了 {levelStars.Length} 个关卡的进度数据");
        }
        
        /// <summary>
        /// 实际使用场景示例：加载玩家的关卡进度
        /// </summary>
        public static (int[] stars, float[] times) LoadPlayerProgress()
        {
            var optionService = OptionSystem.Service;
            
            // 加载关卡星级
            int[] stars = optionService.GetCompositeValues<int>("LevelStars", CompositeStringHelper.Separators.Pipe);
            
            // 加载关卡时间
            float[] times = optionService.GetCompositeValues<float>("LevelTimes", CompositeStringHelper.Separators.Pipe);
            
            Debug.Log($"加载了 {stars.Length} 个关卡的星级数据和 {times.Length} 个关卡的时间数据");
            
            return (stars, times);
        }
    }
} 