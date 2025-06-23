using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 热更新工具类
    /// </summary>
    public static class HotUpdateHelper
    {
        private static Dictionary<string, ResourcePackage> _packages = new Dictionary<string, ResourcePackage>();
        
        /// <summary>
        /// 获取或创建Package
        /// </summary>
        public static ResourcePackage GetOrCreatePackage(string packageName)
        {
            if (!_packages.ContainsKey(packageName))
            {
                var package = YooAssets.TryGetPackage(packageName);
                if (package == null)
                    package = YooAssets.CreatePackage(packageName);
                _packages[packageName] = package;
            }
            return _packages[packageName];
        }
        
        /// <summary>
        /// 极简模式启动热更新（使用默认配置）
        /// </summary>
        public static PatchOperation StartHotUpdate(EPlayMode playMode)
        {
            var config = ServerConfig.CreateDefault();
            return CreateHotUpdateOperation("DefaultPackage", playMode, config);
        }
        
        /// <summary>
        /// 配置模式启动热更新
        /// </summary>
        public static PatchOperation StartHotUpdate(EPlayMode playMode,ServerConfig config)
        {
            return CreateHotUpdateOperation("DefaultPackage", playMode, config);
        }
        
        /// <summary>
        /// 参数模式启动热更新
        /// </summary>
        public static PatchOperation StartHotUpdate(string packageName, EPlayMode playMode, string serverURL = null)
        {
            ServerConfig config = null;
            if (!string.IsNullOrEmpty(serverURL))
            {
                config = ServerConfig.CreateDefault();
                config.MainServerURL = serverURL;
            }
            else
            {
                config = ServerConfig.CreateDefault();
            }
            
            return CreateHotUpdateOperation(packageName, playMode, config);
        }
        
        /// <summary>
        /// 完整参数启动热更新
        /// </summary>
        public static PatchOperation StartHotUpdateWithConfig(string packageName, EPlayMode playMode, ServerConfig serverConfig)
        {
            Debug.Log("StartHotUpdateWithConfig");
            return CreateHotUpdateOperation(packageName, playMode, serverConfig);
        }
        
        /// <summary>
        /// 创建热更新操作（不自动开始）
        /// </summary>
        public static PatchOperation CreateHotUpdateOperation(string packageName, EPlayMode playMode, ServerConfig serverConfig)
        {
            var operation = new PatchOperation(packageName, playMode, serverConfig);
            return operation;
        }
        
        /// <summary>
        /// 开始执行热更新操作
        /// </summary>
        public static void StartOperation(PatchOperation operation)
        {
            YooAssets.StartOperation(operation);
        }
        
        /// <summary>
        /// 批量更新多个Package
        /// </summary>
        public static IEnumerator UpdatePackages(string[] packageNames, EPlayMode playMode, ServerConfig serverConfig = null)
        {
            var operations = new List<PatchOperation>();
            
            foreach (var packageName in packageNames)
            {
                var operation = CreateHotUpdateOperation(packageName, playMode, serverConfig);
                operations.Add(operation);
                YooAssets.StartOperation(operation);
            }
            
            // 等待所有Package更新完成
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }
        
        /// <summary>
        /// 检查网络连接
        /// </summary>
        public static bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        /// <summary>
        /// 检查是否为WiFi连接
        /// </summary>
        public static bool IsWifiConnected()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
        
        /// <summary>
        /// 格式化文件大小
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024f * 1024f):F1} MB";
            return $"{bytes / (1024f * 1024f * 1024f):F1} GB";
        }
        
        /// <summary>
        /// 获取可用磁盘空间
        /// </summary>
        public static long GetAvailableDiskSpace()
        {
            string path = Application.persistentDataPath;
            if (System.IO.Directory.Exists(path))
            {
                var driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(path));
                return driveInfo.AvailableFreeSpace;
            }
            return 0;
        }
        
        /// <summary>
        /// 检查是否有足够的磁盘空间
        /// </summary>
        public static bool HasEnoughDiskSpace(long requiredBytes)
        {
            return GetAvailableDiskSpace() >= requiredBytes;
        }
        
        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            foreach (var package in _packages.Values)
            {
                var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
                operation.WaitForAsyncComplete();
            }
        }
        
        /// <summary>
        /// 获取包信息
        /// </summary>
        public static PackageInfo GetPackageInfo(string packageName)
        {
            var package = GetOrCreatePackage(packageName);
            if (package != null)
            {
                return new PackageInfo
                {
                    PackageName = packageName,
                    PackageVersion = package.GetPackageVersion(),
                    IsInitialized = true // 简化处理，假设包已初始化
                };
            }
            return null;
        }
        
        /// <summary>
        /// 加载ServerConfig资源
        /// </summary>
        public static ServerConfig LoadServerConfig(string configName = "ServerConfig")
        {
            var config = Resources.Load<ServerConfig>(configName);
            if (config == null)
            {
                Debug.LogWarning($"未找到ServerConfig资源: {configName}，使用默认配置");
                config = ServerConfig.CreateDefault();
            }
            return config;
        }
        
        /// <summary>
        /// 包信息
        /// </summary>
        public class PackageInfo
        {
            public string PackageName;
            public string PackageVersion;
            public bool IsInitialized;
            public int TotalFileCount;
            public int CachedFileCount;
        }
    }
} 