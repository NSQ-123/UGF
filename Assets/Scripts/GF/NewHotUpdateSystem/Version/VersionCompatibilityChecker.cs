using System;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 版本兼容性检查器
    /// </summary>
    /// 内置版本检查
    public static class VersionCompatibilityChecker
    {
        /// <summary>
        /// 版本兼容性规则
        /// </summary>
        public static class CompatibilityRules
        {
            // 这些规则是内置的，不依赖配置文件
            public const int MAX_MAJOR_VERSION_DIFF = 1; // 最大主版本差异
            public const int MAX_MINOR_VERSION_DIFF = 2; // 最大次版本差异
            public const string MIN_SUPPORTED_VERSION = "v1.0.0"; // 最低支持版本

            /// <summary>
            /// 检查版本兼容性
            /// </summary>
            public static bool IsCompatible(string localVersion, string serverVersion)
            {
                var local = VersionParser.ParseVersion(localVersion);
                var server = VersionParser.ParseVersion(serverVersion);

                // 检查最低支持版本
                var minSupported = VersionParser.ParseVersion(MIN_SUPPORTED_VERSION);
                if (local.CompareTo(minSupported) < 0)
                {
                    Debug.LogWarning($"本地版本过低: {localVersion} < {MIN_SUPPORTED_VERSION}");
                    return false;
                }

                // 检查主版本差异
                if (Math.Abs(local.Major - server.Major) > MAX_MAJOR_VERSION_DIFF)
                {
                    Debug.LogWarning($"主版本差异过大: {localVersion} vs {serverVersion}");
                    return false;
                }

                // 检查次版本差异（仅当主版本相同时）
                if (local.Major == server.Major && Math.Abs(local.Minor - server.Minor) > MAX_MINOR_VERSION_DIFF)
                {
                    Debug.LogWarning($"次版本差异过大: {localVersion} vs {serverVersion}");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 检查是否需要强制更新
            /// </summary>
            public static bool RequiresForceUpdate(string localVersion, string serverVersion)
            {
                var local = VersionParser.ParseVersion(localVersion);
                var server = VersionParser.ParseVersion(serverVersion);

                // 主版本不同，需要强制更新
                if (local.Major != server.Major)
                    return true;

                // 服务器版本比本地版本低太多，可能需要强制更新
                if (server.CompareTo(local) < 0 && Math.Abs(local.Major - server.Major) > 0)
                    return true;

                return false;
            }

            /// <summary>
            /// 获取更新建议
            /// </summary>
            public static UpdateRecommendation GetUpdateRecommendation(string localVersion, string serverVersion)
            {
                var local = VersionParser.ParseVersion(localVersion);
                var server = VersionParser.ParseVersion(serverVersion);

                if (local.CompareTo(server) == 0)
                    return new UpdateRecommendation(UpdateType.None, "版本已是最新");

                if (local.Major != server.Major)
                    return new UpdateRecommendation(UpdateType.ForceUpdate, "主版本更新，需要强制更新");

                if (local.Minor != server.Minor)
                    return new UpdateRecommendation(UpdateType.Recommended, "次版本更新，建议更新");

                return new UpdateRecommendation(UpdateType.Optional, "补丁更新，可选更新");
            }
        }

        /// <summary>
        /// 更新建议
        /// </summary>
        public class UpdateRecommendation
        {
            public UpdateType Type { get; }
            public string Message { get; }

            public UpdateRecommendation(UpdateType type, string message)
            {
                Type = type;
                Message = message;
            }
        }
    }
}