using UnityEngine;

namespace GF.Option
{
    /// <summary>
    /// 简单的组合字符串测试
    /// </summary>
    public static class SimpleCompositeTest
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Test()
        {
            Debug.Log("=== 简单组合字符串测试 ===");
            
            // 测试基本功能
            string[] values = { "苹果", "香蕉", "橙子" };
            
            // 1. 选择分隔符
            char separator = CompositeStringHelper.SelectBestSeparator(values);
            Debug.Log("选择的分隔符: ASCII " + (int)separator + " - " + CompositeStringHelper.GetSeparatorDescription(separator));
            
            // 2. 组合字符串
            string combined = CompositeStringHelper.Combine(separator, values);
            Debug.Log("组合后的字符串: '" + combined + "'");
            
            // 3. 解析字符串
            string[] parsed = CompositeStringHelper.Parse(combined, separator);
            Debug.Log("解析后的数组长度: " + parsed.Length);
            for (int i = 0; i < parsed.Length; i++)
            {
                Debug.Log("  [" + i + "] = '" + parsed[i] + "'");
            }
            
            // 4. 测试配置系统
            var optionService = OptionSystem.Service;
            optionService.Set(new OptionKey("TestComposite"), combined);
            
            string retrieved = optionService.Get<string>(new OptionKey("TestComposite"));
            Debug.Log("从配置系统获取的字符串: '" + retrieved + "'");
            
            string[] finalParsed = CompositeStringHelper.Parse(retrieved, separator);
            Debug.Log("最终解析结果: " + finalParsed.Length + " 个元素");
            
            Debug.Log("=== 测试完成 ===");
        }
    }
} 