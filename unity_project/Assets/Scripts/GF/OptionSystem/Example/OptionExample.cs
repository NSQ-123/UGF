using System;
using UnityEngine;
using GF.Option;

namespace GF.Option.Examples
{
    /// <summary>
    /// 音频配置类示例
    /// </summary>
    public class AudioConfig
    {
        [Option("MasterVolume", Description = "主音量")]
        [OptionValidator(nameof(ValidateVolume))]
        [OptionCallback(nameof(OnMasterVolumeChanged))]
        public float MasterVolume { get; set; } = 1.0f;

        [Option("MusicVolume", Description = "音乐音量")]
        [OptionValidator(nameof(ValidateVolume))]
        [OptionCallback(nameof(OnMusicVolumeChanged))]
        public float MusicVolume { get; set; } = 0.8f;

        [Option("SFXVolume", Description = "音效音量")]
        [OptionValidator(nameof(ValidateVolume))]
        public float SFXVolume { get; set; } = 1.0f;

        [Option("AudioEnabled", Description = "启用音频")]
        [OptionCallback(nameof(OnAudioEnabledChanged))]
        public bool AudioEnabled { get; set; } = true;

        public bool ValidateVolume(object value)
        {
            if (value is float volume)
                return volume >= 0f && volume <= 1f;
            return false;
        }

        public void OnMasterVolumeChanged(object value)
        {
            Debug.Log($"主音量变更为: {value}");
            AudioListener.volume = (float)value;
        }

        public void OnMusicVolumeChanged(object value)
        {
            Debug.Log($"音乐音量变更为: {value}");
            // 这里可以调用音乐管理器设置音量
        }

        public void OnAudioEnabledChanged(object value)
        {
            Debug.Log($"音频启用状态变更为: {value}");
            AudioListener.pause = !(bool)value;
        }
    }

    /// <summary>
    /// 图形配置类示例
    /// </summary>
    public class GraphicsConfig
    {
        [Option("Resolution", Description = "分辨率")]
        public string Resolution { get; set; } = "1920x1080";

        [Option("GraphicsQuality", Description = "图形质量")]
        [OptionValidator(nameof(ValidateQuality))]
        [OptionCallback(nameof(OnQualityChanged))]
        public int GraphicsQuality { get; set; } = 2;

        [Option("FullScreen", Description = "全屏模式")]
        [OptionCallback(nameof(OnFullScreenChanged))]
        public bool FullScreen { get; set; } = false;

        [Option("VSync", Description = "垂直同步")]
        [OptionCallback(nameof(OnVSyncChanged))]
        public bool VSync { get; set; } = true;

        [Option("FrameRateLimit", Description = "帧率限制")]
        [OptionValidator(nameof(ValidateFrameRate))]
        public int FrameRateLimit { get; set; } = 60;

        public bool ValidateQuality(object value)
        {
            if (value is int quality)
                return quality >= 0 && quality <= 3;
            return false;
        }

        public bool ValidateFrameRate(object value)
        {
            if (value is int frameRate)
                return frameRate == -1 || (frameRate >= 30 && frameRate <= 144);
            return false;
        }

        public void OnQualityChanged(object value)
        {
            Debug.Log($"图形质量变更为: {value}");
            QualitySettings.SetQualityLevel((int)value);
        }

        public void OnFullScreenChanged(object value)
        {
            Debug.Log($"全屏模式变更为: {value}");
            Screen.fullScreen = (bool)value;
        }

        public void OnVSyncChanged(object value)
        {
            Debug.Log($"垂直同步变更为: {value}");
            QualitySettings.vSyncCount = (bool)value ? 1 : 0;
        }
    }

    /// <summary>
    /// 游戏配置类示例
    /// </summary>
    public class GameConfig
    {
        [Option("Language", Description = "语言设置")]
        [OptionValidator(nameof(ValidateLanguage))]
        [OptionCallback(nameof(OnLanguageChanged))]
        public string Language { get; set; } = "zh-CN";

        [Option("AutoSave", Description = "自动保存")]
        public bool AutoSave { get; set; } = true;

        [Option("GameDifficulty", Description = "游戏难度")]
        [OptionValidator(nameof(ValidateDifficulty))]
        public int GameDifficulty { get; set; } = 1;

        [Option("ShowTutorial", Description = "显示教程")]
        public bool ShowTutorial { get; set; } = true;

        public bool ValidateLanguage(object value)
        {
            if (value is string lang)
            {
                string[] supportedLanguages = { "zh-CN", "en-US", "ja-JP", "ko-KR" };
                return Array.Exists(supportedLanguages, l => l == lang);
            }
            return false;
        }

        public bool ValidateDifficulty(object value)
        {
            if (value is int difficulty)
                return difficulty >= 0 && difficulty <= 2; // 0=简单, 1=普通, 2=困难
            return false;
        }

        public void OnLanguageChanged(object value)
        {
            Debug.Log($"语言设置变更为: {value}");
            // 这里可以调用本地化管理器切换语言
        }
    }

    /// <summary>
    /// 玩家数据配置类示例
    /// </summary>
    public class PlayerDataConfig
    {
        [Option("PlayerLevel", Description = "玩家等级", IsPlayerUnique = true)]
        [OptionValidator(nameof(ValidateLevel))]
        public int PlayerLevel { get; set; } = 1;

        [Option("TotalPlayTime", Description = "总游戏时间(秒)", IsPlayerUnique = true)]
        [OptionValidator(nameof(ValidatePlayTime))]
        public long TotalPlayTime { get; set; } = 0;

        [Option("LastLoginTime", Description = "最后登录时间", IsPlayerUnique = true)]
        public string LastLoginTime { get; set; } = "";

        [Option("TutorialCompleted", Description = "教程完成状态", IsPlayerUnique = true)]
        public bool TutorialCompleted { get; set; } = false;

        [Option("CurrentStage", Description = "当前关卡", IsPlayerUnique = true)]
        [OptionValidator(nameof(ValidateStage))]
        public int CurrentStage { get; set; } = 1;

        [Option("UnlockedFeatures", Description = "已解锁功能", IsPlayerUnique = true)]
        public string UnlockedFeatures { get; set; } = "";

        [Option("TotalScore", Description = "总分数", IsPlayerUnique = true)]
        [OptionValidator(nameof(ValidateScore))]
        public long TotalScore { get; set; } = 0;

        public bool ValidateLevel(object value)
        {
            return value is int level && level >= 1 && level <= 100;
        }

        public bool ValidatePlayTime(object value)
        {
            return value is long time && time >= 0;
        }

        public bool ValidateStage(object value)
        {
            return value is int stage && stage >= 1;
        }

        public bool ValidateScore(object value)
        {
            return value is long score && score >= 0;
        }
    }

    /// <summary>
    /// 配置系统使用示例
    /// </summary>
    public static class OptionExample
    {
        private static IOptionService s_optionService;

        /// <summary>
        /// 初始化配置系统
        /// </summary>
        public static void InitializeOptionSystem()
        {
            Debug.Log("=== 初始化配置系统 ===");

            // 创建配置服务
            s_optionService = new EnhancedOptions();

            // 注册配置类
            OptionRegistry.RegisterFromType<AudioConfig>();
            OptionRegistry.RegisterFromType<GraphicsConfig>();
            OptionRegistry.RegisterFromType<GameConfig>();
            OptionRegistry.RegisterFromType<PlayerDataConfig>();

            // 初始化服务
            s_optionService.Initialize();

            Debug.Log("配置系统初始化完成！");

            // 运行示例
            RunExamples();
        }

        /// <summary>
        /// 运行各种使用示例
        /// </summary>
        private static void RunExamples()
        {
            Debug.Log("\n=== 配置系统使用示例 ===");

            // 示例1：基本配置操作
            BasicConfigExample();

            // 示例2：玩家特定配置
            PlayerUniqueConfigExample();

            // 示例3：手动注册配置
            ManualRegistrationExample();

            // 示例4：配置验证和回调
            ValidationAndCallbackExample();

            // 示例5：不同数据类型示例
            DataTypeExample();
        }

        /// <summary>
        /// 基本配置操作示例
        /// </summary>
        private static void BasicConfigExample()
        {
            Debug.Log("\n--- 基本配置操作示例 ---");

            // 方式1：通过OptionKey
            var volumeKey = new OptionKey("MasterVolume");
            float volume = s_optionService.Get<float>(volumeKey);
            Debug.Log($"当前主音量: {volume}");

            s_optionService.Set(volumeKey, 0.8f, true);
            Debug.Log($"设置主音量为: 0.8");

            // 方式2：通过字符串键（扩展方法）
            volume = s_optionService.Get<float>("MasterVolume");
            Debug.Log($"获取主音量: {volume}");

            // 方式3：使用类型化扩展方法
            s_optionService.SetFloat(new OptionKey("MusicVolume"), 0.6f, true);
            float musicVolume = s_optionService.GetFloat(new OptionKey("MusicVolume"));
            Debug.Log($"音乐音量设置为: {musicVolume}");

            // 布尔类型配置
            s_optionService.SetBool(new OptionKey("AudioEnabled"), false);
            bool audioEnabled = s_optionService.GetBool(new OptionKey("AudioEnabled"));
            Debug.Log($"音频启用状态: {audioEnabled}");
        }

        /// <summary>
        /// 玩家特定配置示例
        /// </summary>
        private static void PlayerUniqueConfigExample()
        {
            Debug.Log("\n--- 玩家特定配置示例 ---");

            long playerId = 12345;
            var levelKey = new OptionKey("PlayerLevel", true);
            var playTimeKey = new OptionKey("TotalPlayTime", true);

            // 获取玩家等级
            int playerLevel = s_optionService.GetPlayerUniqueConfig<int>(playerId, levelKey);
            Debug.Log($"玩家 {playerId} 等级: {playerLevel}");

            // 设置玩家等级
            s_optionService.SetPlayerUniqueConfig(playerId, levelKey, 5);
            playerLevel = s_optionService.GetPlayerUniqueConfig<int>(playerId, levelKey);
            Debug.Log($"玩家 {playerId} 新等级: {playerLevel}");

            // 设置游戏时间
            s_optionService.SetPlayerUniqueConfig(playerId, playTimeKey, 3600L);
            long playTime = s_optionService.GetPlayerUniqueConfig<long>(playerId, playTimeKey);
            Debug.Log($"玩家 {playerId} 游戏时间: {playTime}秒");

            // 另一个玩家的数据
            long playerId2 = 67890;
            int player2Level = s_optionService.GetPlayerUniqueConfig<int>(playerId2, levelKey);
            Debug.Log($"玩家 {playerId2} 等级: {player2Level}");
        }

        /// <summary>
        /// 手动注册配置示例
        /// </summary>
        private static void ManualRegistrationExample()
        {
            Debug.Log("\n--- 手动注册配置示例 ---");

            // 手动注册一个临时配置项
            OptionRegistry.Register(new OptionKey("TempConfig"), "临时值")
                .WithDescription("临时配置项")
                .WithValidator(v => v is string s && s.Length > 0)
                .WithOnValueChanged(v => Debug.Log($"临时配置变更: {v}"))
                .Build();

            // 使用手动注册的配置
            string tempValue = s_optionService.Get<string>("TempConfig");
            Debug.Log($"临时配置当前值: {tempValue}");

            s_optionService.Set("TempConfig", "新的临时值", true);
        }

        /// <summary>
        /// 配置验证和回调示例
        /// </summary>
        private static void ValidationAndCallbackExample()
        {
            Debug.Log("\n--- 配置验证和回调示例 ---");

            // 尝试设置无效的音量值（超出范围）
            Debug.Log("尝试设置无效音量值 1.5（应该失败）:");
            s_optionService.Set("MasterVolume", 1.5f, true);

            // 尝试设置无效的图形质量值
            Debug.Log("尝试设置无效图形质量 5（应该失败）:");
            s_optionService.Set("GraphicsQuality", 5, true);

            // 设置有效值（会触发回调）
            Debug.Log("设置有效的图形质量值 3:");
            s_optionService.Set("GraphicsQuality", 3, true);
        }

        /// <summary>
        /// 不同数据类型示例
        /// </summary>
        private static void DataTypeExample()
        {
            Debug.Log("\n--- 不同数据类型示例 ---");

            // 字符串类型
            s_optionService.SetString(new OptionKey("Language"), "en-US", true);
            string language = s_optionService.GetString(new OptionKey("Language"));
            Debug.Log($"语言设置: {language}");

            // 整数类型
            s_optionService.SetInt(new OptionKey("GameDifficulty"), 2);
            int difficulty = s_optionService.GetInt(new OptionKey("GameDifficulty"));
            Debug.Log($"游戏难度: {difficulty}");

            // 布尔类型
            s_optionService.SetBool(new OptionKey("AutoSave"), false);
            bool autoSave = s_optionService.GetBool(new OptionKey("AutoSave"));
            Debug.Log($"自动保存: {autoSave}");

            // 检查配置是否存在
            bool hasLanguage = s_optionService.Has(new OptionKey("Language"));
            bool hasNonExistent = s_optionService.Has(new OptionKey("NonExistentConfig"));
            Debug.Log($"Language配置存在: {hasLanguage}");
            Debug.Log($"不存在的配置存在: {hasNonExistent}");
        }

        /// <summary>
        /// 获取配置服务实例（供外部使用）
        /// </summary>
        public static IOptionService GetOptionService()
        {
            return s_optionService;
        }

        /// <summary>
        /// 打印所有已注册的配置项
        /// </summary>
        public static void PrintAllRegisteredOptions()
        {
            Debug.Log("\n=== 所有已注册的配置项 ===");
            
            foreach (var optionKey in OptionRegistry.GetAllOptionKeys())
            {
                var metadata = OptionRegistry.GetMetadata(optionKey);
                string playerUnique = optionKey.IsPlayerUnique ? " [玩家特定]" : "";
                string readOnly = metadata?.IsReadOnly == true ? " [只读]" : "";
                string description = !string.IsNullOrEmpty(metadata?.Description) ? $" - {metadata.Description}" : "";
                
                Debug.Log($"  {optionKey.Key}{playerUnique}{readOnly}{description}");
            }
        }
    }

    /// <summary>
    /// Unity MonoBehaviour 示例组件
    /// </summary>
    public class OptionExampleComponent : MonoBehaviour
    {
        [Header("配置系统测试")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool printAllOptions = true;

        private void Start()
        {
            if (initializeOnStart)
            {
                OptionExample.InitializeOptionSystem();
                
                if (printAllOptions)
                {
                    OptionExample.PrintAllRegisteredOptions();
                }
            }
        }

        [ContextMenu("初始化配置系统")]
        public void InitializeOptions()
        {
            OptionExample.InitializeOptionSystem();
        }

        [ContextMenu("打印所有配置项")]
        public void PrintAllOptions()
        {
            OptionExample.PrintAllRegisteredOptions();
        }

        [ContextMenu("测试音量设置")]
        public void TestVolumeSettings()
        {
            var optionService = OptionExample.GetOptionService();
            if (optionService != null)
            {
                optionService.SetFloat(new OptionKey("MasterVolume"), 0.5f, true);
                optionService.SetFloat(new OptionKey("MusicVolume"), 0.3f, true);
                Debug.Log("音量设置测试完成");
            }
        }
    }
} 