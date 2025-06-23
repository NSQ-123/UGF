using UnityEngine;
using UnityEditor;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 创建默认ServerConfig的编辑器工具
    /// </summary>
    public class CreateDefaultServerConfig
    {
        /// <summary>
        /// 创建默认的ServerConfig资源
        /// </summary>
        [MenuItem("GF/HotUpdateSystem/创建默认ServerConfig")]
        public static void CreateDefaultConfig()
        {
            // 创建Resources文件夹（如果不存在）
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // 创建ServerConfig资源
            var config = ScriptableObject.CreateInstance<ServerConfig>();
            config.name = "ServerConfig";
            
            // 保存到Resources文件夹
            AssetDatabase.CreateAsset(config, "Assets/Resources/ServerConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 选中创建的资源
            Selection.activeObject = config;
            
            Debug.Log("已创建默认ServerConfig资源: Assets/Resources/ServerConfig.asset");
        }
        
        /// <summary>
        /// 创建开发环境ServerConfig
        /// </summary>
        [MenuItem("GF/HotUpdateSystem/创建开发环境ServerConfig")]
        public static void CreateDevelopmentConfig()
        {
            CreateConfigWithSettings("DevelopmentServerConfig", "http://127.0.0.1", "v1.0");
        }
        
        /// <summary>
        /// 创建测试环境ServerConfig
        /// </summary>
        [MenuItem("GF/HotUpdateSystem/创建测试环境ServerConfig")]
        public static void CreateTestConfig()
        {
            CreateConfigWithSettings("TestServerConfig", "http://test-server.com", "v1.0");
        }
        
        /// <summary>
        /// 创建生产环境ServerConfig
        /// </summary>
        [MenuItem("GF/HotUpdateSystem/创建生产环境ServerConfig")]
        public static void CreateProductionConfig()
        {
            CreateConfigWithSettings("ProductionServerConfig", "http://prod-server.com", "v1.0");
        }
        
        /// <summary>
        /// 创建指定设置的ServerConfig
        /// </summary>
        private static void CreateConfigWithSettings(string configName, string serverURL, string appVersion)
        {
            // 创建Resources文件夹（如果不存在）
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // 创建ServerConfig资源
            var config = ScriptableObject.CreateInstance<ServerConfig>();
            config.name = configName;
            
            // 设置配置
            config.MainServerURL = serverURL;
            config.FallbackServerURL = serverURL;
            config.AppVersion = appVersion;
            
            // 保存到Resources文件夹
            AssetDatabase.CreateAsset(config, $"Assets/Resources/{configName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 选中创建的资源
            Selection.activeObject = config;
            
            Debug.Log($"已创建{configName}资源: Assets/Resources/{configName}.asset");
        }
        
        /// <summary>
        /// 验证所有ServerConfig资源
        /// </summary>
        [MenuItem("GF/HotUpdateSystem/验证所有ServerConfig")]
        public static void ValidateAllConfigs()
        {
            var configs = Resources.LoadAll<ServerConfig>("");
            
            if (configs.Length == 0)
            {
                Debug.LogWarning("未找到任何ServerConfig资源！");
                return;
            }
            
            Debug.Log($"找到 {configs.Length} 个ServerConfig资源:");
            
            foreach (var config in configs)
            {
                if (config.Validate())
                {
                    Debug.Log($"✅ {config.name}: 配置有效");
                }
                else
                {
                    Debug.LogError($"❌ {config.name}: 配置无效");
                }
            }
        }
    }
} 