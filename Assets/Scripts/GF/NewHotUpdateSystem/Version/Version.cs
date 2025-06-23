using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 版本号结构
    /// </summary>
    [Serializable]
    public struct VersionInfo
    {
        public int Major;      // 主版本号
        public int Minor;      // 次版本号
        public int Patch;      // 补丁版本号
        public int Build;      // 构建号
        public string Suffix;  // 后缀（如alpha、beta、rc等）
        
        public VersionInfo(int major, int minor = 0, int patch = 0, int build = 0, string suffix = "")
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
            Suffix = suffix ?? "";
        }
        
        public override string ToString()
        {
            var version = $"{Major}.{Minor}.{Patch}";
            if (Build > 0)
                version += $".{Build}";
            if (!string.IsNullOrEmpty(Suffix))
                version += $"-{Suffix}";
            return version;
        }
        
        /// <summary>
        /// 比较版本号
        /// </summary>
        public int CompareTo(VersionInfo other)
        {
            if (Major != other.Major) return Major.CompareTo(other.Major);
            if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
            if (Patch != other.Patch) return Patch.CompareTo(other.Patch);
            if (Build != other.Build) return Build.CompareTo(other.Build);
            return string.Compare(Suffix, other.Suffix, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// 检查是否为兼容版本
        /// </summary>
        public bool IsCompatibleWith(VersionInfo other, VersionCompatibilityMode mode = VersionCompatibilityMode.MajorMinor)
        {
            switch (mode)
            {
                case VersionCompatibilityMode.MajorOnly:
                    return Major == other.Major;
                    
                case VersionCompatibilityMode.MajorMinor:
                    return Major == other.Major && Minor == other.Minor;
                    
                case VersionCompatibilityMode.MajorMinorPatch:
                    return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
                    
                case VersionCompatibilityMode.All:
                    return CompareTo(other) == 0;
                    
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// 版本兼容性模式
    /// </summary>
    public enum VersionCompatibilityMode
    {
        MajorOnly,        // 只检查主版本号
        MajorMinor,       // 检查主版本号和次版本号（推荐）
        MajorMinorPatch,  // 检查主版本号、次版本号和补丁版本号
        All              // 检查所有版本号
    }
    
    /// <summary>
    /// 版本解析器
    /// </summary>
    public static class VersionParser
    {
        // 支持多种版本号格式的正则表达式
        private static readonly Regex VersionRegex = new Regex(
            @"^v?(\d+)\.(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:-([a-zA-Z0-9]+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        
        /// <summary>
        /// 解析版本号字符串
        /// </summary>
        /// <param name="versionString">版本号字符串</param>
        /// <returns>版本信息结构</returns>
        public static VersionInfo ParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return new VersionInfo(0, 0, 0, 0, "invalid");
            
            var match = VersionRegex.Match(versionString.Trim());
            if (!match.Success)
            {
                Debug.LogWarning($"无法解析版本号: {versionString}");
                return new VersionInfo(0, 0, 0, 0, "invalid");
            }
            
            try
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var patch = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
                var build = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
                var suffix = match.Groups[5].Success ? match.Groups[5].Value : "";
                
                return new VersionInfo(major, minor, patch, build, suffix);
            }
            catch (Exception ex)
            {
                Debug.LogError($"解析版本号时发生错误: {versionString}, {ex.Message}");
                return new VersionInfo(0, 0, 0, 0, "error");
            }
        }
        
        /// <summary>
        /// 检查版本兼容性
        /// </summary>
        /// <param name="localVersion">本地版本</param>
        /// <param name="serverVersion">服务器版本</param>
        /// <param name="mode">兼容性检查模式</param>
        /// <returns>是否兼容</returns>
        public static bool IsVersionCompatible(string localVersion, string serverVersion, VersionCompatibilityMode mode = VersionCompatibilityMode.MajorMinor)
        {
            var local = ParseVersion(localVersion);
            var server = ParseVersion(serverVersion);
            
            // 检查是否为无效版本
            if (local.Suffix == "invalid" || local.Suffix == "error" || 
                server.Suffix == "invalid" || server.Suffix == "error")
            {
                Debug.LogWarning($"版本号无效: local={localVersion}, server={serverVersion}");
                return false;
            }
            
            return local.IsCompatibleWith(server, mode);
        }
        
        /// <summary>
        /// 检查是否需要强制更新
        /// </summary>
        /// <param name="localVersion">本地版本</param>
        /// <param name="serverVersion">服务器版本</param>
        /// <param name="minRequiredVersion">最低要求版本</param>
        /// <returns>是否需要强制更新</returns>
        public static bool RequiresForceUpdate(string localVersion, string serverVersion, string minRequiredVersion = null)
        {
            var local = ParseVersion(localVersion);
            var server = ParseVersion(serverVersion);
            
            // 如果服务器版本比本地版本低，可能需要强制更新
            if (server.CompareTo(local) < 0)
            {
                Debug.LogWarning($"服务器版本低于本地版本: local={localVersion}, server={serverVersion}");
                return true;
            }
            
            // 检查最低要求版本
            if (!string.IsNullOrEmpty(minRequiredVersion))
            {
                var minRequired = ParseVersion(minRequiredVersion);
                if (local.CompareTo(minRequired) < 0)
                {
                    Debug.LogWarning($"本地版本低于最低要求版本: local={localVersion}, minRequired={minRequiredVersion}");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 格式化版本号显示
        /// </summary>
        /// <param name="versionString">版本号字符串</param>
        /// <returns>格式化后的版本号</returns>
        public static string FormatVersion(string versionString)
        {
            var version = ParseVersion(versionString);
            return version.ToString();
        }
        
        /// <summary>
        /// 验证版本号格式
        /// </summary>
        /// <param name="versionString">版本号字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return false;
            
            var match = VersionRegex.Match(versionString.Trim());
            return match.Success;
        }
    }
}