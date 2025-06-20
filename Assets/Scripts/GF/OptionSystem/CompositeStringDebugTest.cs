using System.Linq;
using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 组合字符串调试测试
    /// </summary>
    public static class CompositeStringDebugTest
    {
        [RuntimeInitializeOnLoadMethod]
        public static void DebugTest()
        {
            Debug.Log("=== 组合字符串调试测试 ===");
            
            var optionService = OptionSystem.Service;
            
            // 测试1：基本设置和获取
            Debug.Log("\n1. 基本设置和获取测试:");
            string[] testValues = { "值1", "值2", "值3" };
            
            // 设置
            char separator = CompositeStringHelper.SelectBestSeparator(testValues);
            string compositeValue = CompositeStringHelper.Combine(separator, testValues);
            optionService.Set(new OptionKey("DebugTest"), compositeValue);
            Debug.Log($"设置的值: [{string.Join(", ", testValues)}]");
            Debug.Log($"使用的分隔符: '{separator}' (ASCII {(int)separator})");
            
            // 检查实际保存的字符串
            string rawValue = optionService.Get<string>(new OptionKey("DebugTest"));
            Debug.Log($"实际保存的字符串: '{rawValue}'");
            
            // 获取
            string[] retrievedValues = CompositeStringHelper.Parse(rawValue, separator);
            Debug.Log($"获取的值: [{string.Join(", ", retrievedValues)}]");
            Debug.Log($"获取的数组长度: {retrievedValues.Length}");
            
            // 测试2：指定分隔符
            Debug.Log("\n2. 指定分隔符测试:");
            char testSeparator = CompositeStringHelper.Separators.Pipe;
            string compositeValue2 = CompositeStringHelper.Combine(testSeparator, testValues);
            optionService.Set(new OptionKey("DebugTest2"), compositeValue2);
            
            string rawValue2 = optionService.Get<string>(new OptionKey("DebugTest2"));
            Debug.Log($"使用管道符保存的字符串: '{rawValue2}'");
            
            string[] retrievedValues2 = CompositeStringHelper.Parse(rawValue2, testSeparator);
            Debug.Log($"使用管道符获取的值: [{string.Join(", ", retrievedValues2)}]");
            
            // 测试3：自动检测分隔符
            Debug.Log("\n3. 自动检测分隔符测试:");
            // 尝试用推荐的分隔符解析
            string[] autoRetrieved = null;
            foreach (char sep in CompositeStringHelper.RecommendedSeparators)
            {
                if (rawValue2.Contains(sep))
                {
                    autoRetrieved = CompositeStringHelper.Parse(rawValue2, sep);
                    break;
                }
            }
            if (autoRetrieved == null) autoRetrieved = new string[] { rawValue2 };
            Debug.Log($"自动检测获取的值: [{string.Join(", ", autoRetrieved)}]");
            
            // 测试4：检查推荐分隔符列表
            Debug.Log("\n4. 推荐分隔符列表:");
            foreach (char item in CompositeStringHelper.RecommendedSeparators.Take(5))
            {
                Debug.Log($"  分隔符: '{item}' (ASCII {(int)item}) - {CompositeStringHelper.GetSeparatorDescription(item)}");
            }
            
            // 测试5：选择最佳分隔符
            Debug.Log("\n5. 分隔符选择测试:");
            char selectedSeparator = CompositeStringHelper.SelectBestSeparator(testValues);
            Debug.Log($"为测试值选择的分隔符: '{selectedSeparator}' (ASCII {(int)selectedSeparator})");
            Debug.Log($"分隔符描述: {CompositeStringHelper.GetSeparatorDescription(selectedSeparator)}");
            
            Debug.Log("\n=== 调试测试完成 ===");
        }
    }
} 