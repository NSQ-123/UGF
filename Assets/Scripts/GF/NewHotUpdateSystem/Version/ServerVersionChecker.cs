using System;
using System.Linq;

namespace GF.HotUpdateSystem.New
{
    
    /// <summary>
    /// 服务器版本策略
    /// </summary>
    [Serializable]
    public class ServerVersionPolicy
    {
        public string MinSupportedVersion; // 最低支持版本
        public string ForceUpdateVersion; // 强制更新版本
        public bool AllowDowngrade; // 是否允许降级
        public string[] CompatibleVersions; // 兼容版本列表
        public string UpdateMessage; // 更新提示信息
    }

    /// <summary>
    /// 版本检查器（使用服务器策略）
    /// </summary>
    public static class ServerVersionChecker
    {
        /// <summary>
        /// 检查版本兼容性（基于服务器策略）
        /// </summary>
        public static bool CheckCompatibility(string localVersion, ServerVersionPolicy policy)
        {
            if (policy == null)
                return true; // 如果没有策略，默认允许更新

            var local = VersionParser.ParseVersion(localVersion);

            // 检查最低支持版本
            if (!string.IsNullOrEmpty(policy.MinSupportedVersion))
            {
                var minSupported = VersionParser.ParseVersion(policy.MinSupportedVersion);
                if (local.CompareTo(minSupported) < 0)
                    return false;
            }

            // 检查兼容版本列表
            if (policy.CompatibleVersions != null && policy.CompatibleVersions.Length > 0)
            {
                return policy.CompatibleVersions.Any(v => VersionParser.IsValidVersion(v) &&
                                                          VersionParser.ParseVersion(v).CompareTo(local) == 0);
            }

            return true;
        }

        /// <summary>
        /// 检查是否需要强制更新
        /// </summary>
        public static bool CheckForceUpdate(string localVersion, ServerVersionPolicy policy)
        {
            if (policy == null || string.IsNullOrEmpty(policy.ForceUpdateVersion))
                return false;

            var local = VersionParser.ParseVersion(localVersion);
            var forceUpdate = VersionParser.ParseVersion(policy.ForceUpdateVersion);

            return local.CompareTo(forceUpdate) < 0;
        }
    }
}