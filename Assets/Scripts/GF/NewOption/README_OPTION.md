# GF.Option 配置系统使用指南

## 概述

GF.Option 是一个基于反射的配置管理系统，支持：
- 通过属性自动注册配置项
- 类型安全的配置访问
- 配置值验证和变更回调
- 全局配置和玩家特定配置
- 多种存储后端支持

## 快速开始

### 1. 定义配置类

```csharp
using GF.Option;

public class GameSettings
{
    [Option("SoundVolume", Description = "主音量")]
    [OptionValidator(nameof(ValidateVolume))]
    [OptionCallback(nameof(OnVolumeChanged))]
    public float SoundVolume { get; set; } = 1.0f;

    [Option("PlayerName", Description = "玩家名称")]
    public string PlayerName { get; set; } = "Player";

    [Option("IsFullscreen", Description = "全屏模式")]
    public bool IsFullscreen { get; set; } = false;

    // 验证方法
    public bool ValidateVolume(object value)
    {
        return value is float volume && volume >= 0f && volume <= 1f;
    }

    // 回调方法
    public void OnVolumeChanged(object value)
    {
        Debug.Log($"音量已更改为: {value}");
        AudioListener.volume = (float)value;
    }
}
```

### 2. 注册和初始化

```csharp
public class GameManager : MonoBehaviour
{
    private IOptionService _optionService;

    void Start()
    {
        // 创建配置服务
        _optionService = new EnhancedOptions();
        
        // 自动注册配置类
        OptionRegistry.RegisterFromType<GameSettings>();
        
        // 初始化服务
        _optionService.Initialize();
    }
}
```

### 3. 使用配置

```csharp
// 方式1：通过OptionKey
var soundVolumeKey = new OptionKey("SoundVolume");
float volume = _optionService.Get<float>(soundVolumeKey);
_optionService.Set(soundVolumeKey, 0.8f, true);

// 方式2：通过字符串键（扩展方法）
volume = _optionService.Get<float>("SoundVolume");
_optionService.Set("SoundVolume", 0.8f, true);

// 方式3：使用类型化扩展方法
volume = _optionService.GetFloat(new OptionKey("SoundVolume"));
_optionService.SetFloat(new OptionKey("SoundVolume"), 0.8f, true);
```

## 配置属性详解

### @Option 属性

标记属性或字段为配置项：

```csharp
[Option("ConfigKey", Description = "配置描述", IsPlayerUnique = false, IsReadOnly = false)]
public float ConfigValue { get; set; } = 1.0f;
```

**参数说明：**
- `key`: 配置键名（必须）
- `Description`: 配置描述（可选）
- `IsPlayerUnique`: 是否为玩家特定配置（可选，默认false）
- `IsReadOnly`: 是否只读（可选，默认false）

### @OptionValidator 属性

设置配置值验证器：

```csharp
[Option("Quality")]
[OptionValidator(nameof(ValidateQuality))]
public int Quality { get; set; } = 2;

public bool ValidateQuality(object value)
{
    return value is int quality && quality >= 0 && quality <= 3;
}
```

### @OptionCallback 属性

设置配置值变更回调：

```csharp
[Option("Volume")]
[OptionCallback(nameof(OnVolumeChanged))]
public float Volume { get; set; } = 1.0f;

public void OnVolumeChanged(object value)
{
    Debug.Log($"音量变更为: {value}");
    // 应用变更逻辑
}
```

## 玩家特定配置

对于需要按玩家存储的配置，设置 `IsPlayerUnique = true`：

```csharp
public class PlayerData
{
    [Option("PlayerLevel", IsPlayerUnique = true)]
    public int PlayerLevel { get; set; } = 1;

    [Option("UnlockedLevels", IsPlayerUnique = true)]  
    public string UnlockedLevels { get; set; } = "1";
}

// 使用方式
long playerId = 12345;
var levelKey = new OptionKey("PlayerLevel", true);
int level = _optionService.GetPlayerUniqueConfig<int>(playerId, levelKey);
_optionService.SetPlayerUniqueConfig(playerId, levelKey, 5);
```

## 手动注册配置

除了自动注册，也支持手动注册：

```csharp
OptionRegistry.Register(new OptionKey("CustomOption"), "default_value")
    .WithDescription("自定义配置项")
    .WithValidator(v => v is string s && s.Length > 0)
    .WithOnValueChanged(v => Debug.Log($"配置变更: {v}"))
    .AsReadOnly() // 设为只读
    .Build();
```

## 完整示例

### 音频配置类

```csharp
public class AudioConfig
{
    [Option("MasterVolume", Description = "主音量")]
    [OptionValidator(nameof(ValidateVolume))]
    [OptionCallback(nameof(OnMasterVolumeChanged))]
    public float MasterVolume { get; set; } = 1.0f;

    [Option("MusicVolume", Description = "音乐音量")]
    [OptionValidator(nameof(ValidateVolume))]
    public float MusicVolume { get; set; } = 0.8f;

    [Option("SFXVolume", Description = "音效音量")]
    [OptionValidator(nameof(ValidateVolume))]
    public float SFXVolume { get; set; } = 1.0f;

    [Option("AudioEnabled", Description = "启用音频")]
    public bool AudioEnabled { get; set; } = true;

    public bool ValidateVolume(object value)
    {
        return value is float v && v >= 0f && v <= 1f;
    }

    public void OnMasterVolumeChanged(object value)
    {
        AudioListener.volume = (float)value;
    }
}
```

### 图形配置类

```csharp
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

    public bool ValidateQuality(object value)
    {
        return value is int q && q >= 0 && q <= 3;
    }

    public void OnQualityChanged(object value)
    {
        QualitySettings.SetQualityLevel((int)value);
    }

    public void OnFullScreenChanged(object value)
    {
        Screen.fullScreen = (bool)value;
    }

    public void OnVSyncChanged(object value)
    {
        QualitySettings.vSyncCount = (bool)value ? 1 : 0;
    }
}
```

## 配置文件格式

配置会自动保存为INI格式：

```ini
[Default]
MasterVolume=1.0
Resolution="1920x1080"
GraphicsQuality=2
FullScreen=false

[Player_12345]
PlayerLevel=5
UnlockedLevels="1,2,3,4,5"
```

## 最佳实践

1. **配置类分组** - 按功能模块分组配置类（音频、图形、游戏等）
2. **验证器使用** - 为重要配置添加验证器防止无效值
3. **回调应用** - 在回调中立即应用配置变更
4. **默认值设置** - 为所有配置项设置合理的默认值
5. **键名规范** - 使用清晰的键名，避免冲突

## 系统架构

- **OptionKey**: 配置项标识符（支持字符串键）
- **OptionAttribute**: 配置项标记属性
- **OptionRegistry**: 配置注册管理器（支持反射自动注册）
- **IOptionService**: 配置服务接口
- **EnhancedOptions**: 增强配置服务实现
- **IConfigStorage**: 存储后端接口
- **IniConfigStorage**: INI格式存储实现

## 优势

1. **自动化** - 无需手动枚举定义和注册代码
2. **类型安全** - 编译时类型检查
3. **验证机制** - 自动值验证
4. **回调系统** - 配置变更自动回调
5. **扩展性** - 支持插件化存储后端
6. **易维护** - 配置定义集中在配置类中 