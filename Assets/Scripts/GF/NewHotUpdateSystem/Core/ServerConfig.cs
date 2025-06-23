using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 服务器配置 - ScriptableObject版本
    /// 可以在Inspector中直接配置和保存
    /// </summary>
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "GF/HotUpdateSystem/ServerConfig")]
    public class ServerConfig : ScriptableObject
    {
        [Header("服务器配置")]
        [Tooltip("主服务器地址")]
        public string MainServerURL = "http://127.0.0.1";
        
        [Tooltip("备用服务器地址")]
        public string FallbackServerURL = "http://127.0.0.1";
        
        [Tooltip("应用版本号")]
        public string AppVersion = "v1.0";
        
        [Header("平台CDN配置")]
        [Tooltip("Android平台CDN路径")]
        public string AndroidCDN = "/CDN/Android/{0}";
        
        [Tooltip("iOS平台CDN路径")]
        public string IOSCDN = "/CDN/IPhone/{0}";
        
        [Tooltip("WebGL平台CDN路径")]
        public string WebGLCDN = "/CDN/WebGL/{0}";
        
        [Tooltip("PC平台CDN路径")]
        public string PCCDN = "/CDN/PC/{0}";
        
        [Header("下载配置")]
        [Tooltip("最大并发下载数")]
        [Range(1, 20)]
        public int MaxConcurrentDownloads = 10;
        
        [Tooltip("下载重试次数")]
        [Range(0, 10)]
        public int DownloadRetryCount = 3;
        
        [Header("缓存配置")]
        [Tooltip("是否自动清理缓存")]
        public bool AutoClearCache = true;
        
        [Tooltip("缓存清理模式")]
        public EFileClearMode CacheClearMode = EFileClearMode.ClearUnusedBundleFiles;
        
        /// <summary>
        /// 获取平台CDN路径
        /// </summary>
        public string GetPlatformCDN()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return string.Format(AndroidCDN, AppVersion);
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return string.Format(IOSCDN, AppVersion);
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return string.Format(WebGLCDN, AppVersion);
            else
                return string.Format(PCCDN, AppVersion);
#else
            if (Application.platform == RuntimePlatform.Android)
                return string.Format(AndroidCDN, AppVersion);
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return string.Format(IOSCDN, AppVersion);
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                return string.Format(WebGLCDN, AppVersion);
            else
                return string.Format(PCCDN, AppVersion);
#endif
        }
        
        /// <summary>
        /// 获取完整的服务器URL
        /// </summary>
        public string GetFullServerURL(bool useFallback = false)
        {
            string baseURL = useFallback ? FallbackServerURL : MainServerURL;
            return baseURL + GetPlatformCDN();
        }
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static ServerConfig CreateDefault()
        {
            var config = CreateInstance<ServerConfig>();
            config.name = "DefaultServerConfig";
            return config;
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(MainServerURL))
            {
                Debug.LogError("MainServerURL cannot be empty!");
                return false;
            }
            
            if (string.IsNullOrEmpty(AppVersion))
            {
                Debug.LogError("AppVersion cannot be empty!");
                return false;
            }
            
            if (MaxConcurrentDownloads <= 0)
            {
                Debug.LogError("MaxConcurrentDownloads must be greater than 0!");
                return false;
            }
            
            if (DownloadRetryCount < 0)
            {
                Debug.LogError("DownloadRetryCount cannot be negative!");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取配置信息字符串
        /// </summary>
        public string GetConfigInfo()
        {
            return $"服务器配置信息:\n" +
                   $"主服务器: {MainServerURL}\n" +
                   $"备用服务器: {FallbackServerURL}\n" +
                   $"应用版本: {AppVersion}\n" +
                   $"平台CDN: {GetPlatformCDN()}\n" +
                   $"并发下载: {MaxConcurrentDownloads}\n" +
                   $"重试次数: {DownloadRetryCount}\n" +
                   $"自动清理缓存: {AutoClearCache}";
        }
        
        /// <summary>
        /// 复制配置到另一个ServerConfig
        /// </summary>
        public void CopyTo(ServerConfig target)
        {
            if (target == null) return;
            
            target.MainServerURL = MainServerURL;
            target.FallbackServerURL = FallbackServerURL;
            target.AppVersion = AppVersion;
            target.AndroidCDN = AndroidCDN;
            target.IOSCDN = IOSCDN;
            target.WebGLCDN = WebGLCDN;
            target.PCCDN = PCCDN;
            target.MaxConcurrentDownloads = MaxConcurrentDownloads;
            target.DownloadRetryCount = DownloadRetryCount;
            target.AutoClearCache = AutoClearCache;
            target.CacheClearMode = CacheClearMode;
        }
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        [ContextMenu("重置为默认值")]
        public void ResetToDefault()
        {
            MainServerURL = "http://127.0.0.1";
            FallbackServerURL = "http://127.0.0.1";
            AppVersion = "v1.0";
            AndroidCDN = "/CDN/Android/{0}";
            IOSCDN = "/CDN/IPhone/{0}";
            WebGLCDN = "/CDN/WebGL/{0}";
            PCCDN = "/CDN/PC/{0}";
            MaxConcurrentDownloads = 10;
            DownloadRetryCount = 3;
            AutoClearCache = true;
            CacheClearMode = EFileClearMode.ClearUnusedBundleFiles;
        }
        
        /// <summary>
        /// 验证并打印配置信息
        /// </summary>
        [ContextMenu("验证配置")]
        public void ValidateAndLog()
        {
            if (Validate())
            {
                Debug.Log("配置验证通过！\n" + GetConfigInfo());
            }
            else
            {
                Debug.LogError("配置验证失败！");
            }
        }
    }
} 