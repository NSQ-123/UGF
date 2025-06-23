using System;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 更新类型
    /// </summary>
    public enum UpdateType
    {
        None,           // 无需更新
        Optional,       // 可选更新
        Recommended,    // 建议更新
        ForceUpdate     // 强制更新
    }

    /// <summary>
    /// 版本兼容性管理器
    /// </summary>
    public static class VersionCompatibilityManager
    {
        /// <summary>
        /// 检查版本兼容性
        /// </summary>
        public static VersionCheckResult CheckVersion(string localVersion, string serverVersion,
            ServerVersionPolicy serverPolicy = null)
        {
            var result = new VersionCheckResult();

            // 1. 基础检查（内置规则）
            if (!VersionCompatibilityChecker.CompatibilityRules.IsCompatible(localVersion, serverVersion))
            {
                result.IsCompatible = false;
                result.Reason = "版本不兼容（基础规则）";
                result.Recommendation = UpdateType.ForceUpdate;
                return result;
            }

            // 2. 服务器策略检查（如果有）
            if (serverPolicy != null)
            {
                if (!ServerVersionChecker.CheckCompatibility(localVersion, serverPolicy))
                {
                    result.IsCompatible = false;
                    result.Reason = "版本不兼容（服务器策略）";
                    result.Recommendation = UpdateType.ForceUpdate;
                    return result;
                }

                if (ServerVersionChecker.CheckForceUpdate(localVersion, serverPolicy))
                {
                    result.IsCompatible = true;
                    result.Reason = "需要强制更新";
                    result.Recommendation = UpdateType.ForceUpdate;
                    result.Message = serverPolicy.UpdateMessage;
                    return result;
                }
            }

            // 3. 获取更新建议
            var recommendation =
                VersionCompatibilityChecker.CompatibilityRules.GetUpdateRecommendation(localVersion, serverVersion);
            result.IsCompatible = true;
            result.Recommendation = recommendation.Type;
            result.Message = recommendation.Message;

            return result;
        }
    }

    /// <summary>
    /// 版本检查结果
    /// </summary>
    public class VersionCheckResult
    {
        public bool IsCompatible { get; set; }
        public string Reason { get; set; }
        public UpdateType Recommendation { get; set; }
        public string Message { get; set; }
    }
}