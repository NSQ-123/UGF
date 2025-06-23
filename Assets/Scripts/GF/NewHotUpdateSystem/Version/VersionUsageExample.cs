using System.Collections;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 版本检查使用示例
    /// </summary>
    public class VersionUsageExample : MonoBehaviour
    {
        [Header("版本检查示例")]
        [SerializeField] private string localVersion = "v1.0.0";
        [SerializeField] private string serverVersion = "v1.1.0";
        
        [Header("服务器策略示例")]
        [SerializeField] private ServerVersionPolicy serverPolicy;
        
        [ContextMenu("测试版本检查")]
        public void TestVersionCheck()
        {
            // 1. 基础版本解析
            var versionInfo = VersionParser.ParseVersion(localVersion);
            Debug.Log($"解析版本: {localVersion} -> {versionInfo}");
            
            // 2. 版本兼容性检查
            var isCompatible = VersionParser.IsVersionCompatible(localVersion, serverVersion);
            Debug.Log($"版本兼容性: {isCompatible}");
            
            // 3. 使用内置规则检查
            var builtinCompatible = VersionCompatibilityChecker.CompatibilityRules.IsCompatible(localVersion, serverVersion);
            Debug.Log($"内置规则兼容性: {builtinCompatible}");
            
            // 4. 使用完整管理器检查
            var result = VersionCompatibilityManager.CheckVersion(localVersion, serverVersion, serverPolicy);
            Debug.Log($"完整检查结果: 兼容={result.IsCompatible}, 建议={result.Recommendation}, 消息={result.Message}");
            
            // 5. 检查是否需要强制更新
            var requiresForceUpdate = VersionCompatibilityChecker.CompatibilityRules.RequiresForceUpdate(localVersion, serverVersion);
            Debug.Log($"需要强制更新: {requiresForceUpdate}");
        }
        
        [ContextMenu("测试版本格式")]
        public void TestVersionFormats()
        {
            string[] testVersions = {
                "v1.0.0",
                "1.0.0",
                "v1.0.0.123",
                "v1.0.0-alpha",
                "v1.0.0-beta.1",
                "invalid-version",
                ""
            };
            
            foreach (var version in testVersions)
            {
                var isValid = VersionParser.IsValidVersion(version);
                var parsed = VersionParser.ParseVersion(version);
                Debug.Log($"版本: {version} -> 有效: {isValid}, 解析: {parsed}");
            }
        }
        
        [ContextMenu("测试更新建议")]
        public void TestUpdateRecommendations()
        {
            string[][] testCases = {
                new[] { "v1.0.0", "v1.0.0" },      // 相同版本
                new[] { "v1.0.0", "v1.0.1" },      // 补丁更新
                new[] { "v1.0.0", "v1.1.0" },      // 次版本更新
                new[] { "v1.0.0", "v2.0.0" },      // 主版本更新
                new[] { "v1.0.0", "v0.9.0" }       // 降级
            };
            
            foreach (var testCase in testCases)
            {
                var local = testCase[0];
                var server = testCase[1];
                var recommendation = VersionCompatibilityChecker.CompatibilityRules.GetUpdateRecommendation(local, server);
                Debug.Log($"本地: {local}, 服务器: {server} -> 建议: {recommendation.Type}, 消息: {recommendation.Message}");
            }
        }
    }
} 