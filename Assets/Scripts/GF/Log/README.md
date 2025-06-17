# GF 日志系统 (GF Log System)

一个功能强大、高性能的Unity日志系统，支持多种输出方式、智能过滤、性能优化和灵活配置。

## 🚀 功能特性

### 核心功能
- **多日志级别**: Debug、Info、Warning、Error
- **多输出目标**: Unity控制台、文件、自定义输出
- **标签系统**: 支持彩色标签和分类日志
- **条件编译**: 支持LOG_ENABLE宏控制日志开关

### 高级功能
- **缓冲机制**: 批量写入提升性能
- **智能过滤**: 关键词、正则表达式、频率限制
- **组合输出**: 同时向多个目标输出日志
- **配置管理**: 运行时动态配置，持久化保存
- **方法跟踪**: 自动记录调用位置信息

### 性能优化
- **异步写入**: 避免阻塞主线程
- **内存池**: 减少GC压力
- **智能刷新**: 自动和手动缓冲区管理

## 📦 系统架构

```
GF.Log
├── 核心接口
│   └── ILogger                    # 日志接口定义
├── 基础实现
│   ├── UnityLogger               # Unity控制台输出
│   ├── FileLogger                # 文件输出
│   └── CustomLog                 # 自定义标签日志
├── 高级功能
│   ├── CompositeLogger           # 组合日志器
│   ├── BufferedLogger            # 缓冲日志器
│   └── FilteredLogger            # 过滤日志器
├── 配置管理
│   ├── LogConfig                 # 配置数据
│   └── LogConfigEditor           # 配置编辑器
└── 入口点
    └── Log                       # 静态日志类
```

## 🏁 快速开始

### 基础使用

```csharp
using GF.Log;

// 基础日志输出
Log.Debug("调试信息");
Log.Info("游戏开始，玩家ID: {0}", playerId);
Log.Warning("内存使用率: {0}%", memoryUsage);
Log.Error("加载失败: {0}", fileName);

// 异常日志
try 
{
    // 可能出错的代码
}
catch (Exception ex)
{
    Log.Error(ex);
}
```

### 自定义标签日志

```csharp
// 创建带标签的日志器
var gameLog = new CustomLog("游戏", true, "#00FF00");
var netLog = new CustomLog("网络", true, "#FF6600", true);

// 使用自定义日志器
gameLog.Info("关卡开始: {0}", levelName);
netLog.Error("连接超时");

// 方法跟踪
gameLog.Trace("进入战斗循环");
// 输出: [游戏][TRACE] GameManager.StartBattle:123 - 进入战斗循环
```

## ⚙️ 配置系统

### 配置参数

```csharp
// 查看当前配置
LogConfigEditor.ShowCurrentConfig();

// 动态调整日志级别
LogConfigEditor.SetLogLevel(LogLevel.Warning);

// 启用/禁用日志
LogConfigEditor.SetEnableLogging(false);

// 过滤设置
LogConfigEditor.AddExcludeKeyword("广告");
LogConfigEditor.RemoveExcludeKeyword("广告");
```

### 配置文件结构

```json
{
    "enableLogging": true,
    "globalLogLevel": 0,
    "enableFileLogging": true,
    "logFileName": "game_log_20231201.log",
    "maxLogFileSize": 10,
    "maxLogFiles": 5,
    "enableBuffering": true,
    "bufferSize": 100,
    "flushIntervalMs": 1000,
    "enableFiltering": false,
    "excludeKeywords": ["广告", "调试"],
    "includeKeywords": [],
    "enableRateLimit": false,
    "rateLimitIntervalMs": 1000,
    "showUnityConsole": false,
    "enableStackTrace": true
}
```

## 🔧 详细使用说明

### 0. 日志级别系统

日志系统支持4个级别，按重要性从低到高排列：

| 级别 | 数值 | 说明 | 使用场景 |
|------|------|------|----------|
| `Debug` | 0 | 调试信息 | 开发调试，仅在Editor中显示 |
| `Info` | 1 | 一般信息 | 游戏流程、状态变化 |
| `Warning` | 2 | 警告信息 | 非致命问题、性能警告 |
| `Error` | 3 | 错误信息 | 严重错误、异常情况 |

**级别过滤规则**: 当设置 `EnableLevel = Warning` 时，只会显示Warning (2) 和 Error (3) 级别的日志。

```csharp
// 设置日志级别为Warning
Log.SetEnableLevel(LogLevel.Warning);

Log.Debug("这条不会显示");    // Debug (0) < Warning (2)
Log.Info("这条不会显示");     // Info (1) < Warning (2)  
Log.Warning("这条会显示");    // Warning (2) >= Warning (2)
Log.Error("这条会显示");      // Error (3) >= Warning (2)
```

### 1. 基础日志器 (UnityLogger)

Unity控制台输出，支持颜色标记和级别过滤。

```csharp
var logger = new UnityLogger();
logger.EnableLevel = LogLevel.Info;  // 只显示Info及以上级别
logger.Enable = true;               // 启用日志

logger.Log(LogLevel.Info, "消息内容: {0}", value);
```

### 2. 文件日志器 (FileLogger)

将日志写入文件，支持自定义文件名和路径。

```csharp
// 使用默认文件名 (game_yyyyMMdd_HHmmss.log)
var fileLogger = new FileLogger();

// 使用自定义文件名
var customFileLogger = new FileLogger("my_game.log");

customFileLogger.Log(LogLevel.Error, "严重错误: {0}", errorMsg);
```

**文件位置**: `Application.persistentDataPath/Logs/`

### 3. 组合日志器 (CompositeLogger)

同时向多个日志器输出，支持动态添加/移除。

```csharp
var unityLogger = new UnityLogger();
var fileLogger = new FileLogger();
var compositeLogger = new CompositeLogger(unityLogger, fileLogger);

// 动态添加新的日志器
var customLogger = new MyCustomLogger();
compositeLogger.AddLogger(customLogger);

// 移除日志器
compositeLogger.RemoveLogger(fileLogger);
```

### 4. 缓冲日志器 (BufferedLogger)

批量处理日志以提升性能，特别适合高频日志场景。

```csharp
var fileLogger = new FileLogger();
var bufferedLogger = new BufferedLogger(
    fileLogger, 
    bufferSize: 200,        // 缓冲区大小
    flushIntervalMs: 2000   // 自动刷新间隔(毫秒)
);

// 手动刷新缓冲区
bufferedLogger.Flush();
```

### 5. 过滤日志器 (FilteredLogger)

支持多种过滤策略，灵活控制日志输出。

```csharp
var baseLogger = new UnityLogger();
var filteredLogger = new FilteredLogger(baseLogger);

// 关键词过滤
var keywordFilter = new KeywordFilter(isExcludeMode: true);
keywordFilter.AddExcludeKeyword("广告");
keywordFilter.AddExcludeKeyword("统计");
filteredLogger.AddFilter(keywordFilter);

// 正则表达式过滤
var regexFilter = new RegexFilter(@"\d{4}-\d{2}-\d{2}", shouldMatch: false);
filteredLogger.AddFilter(regexFilter);

// 频率限制过滤
var rateLimitFilter = new RateLimitFilter(TimeSpan.FromSeconds(1));
filteredLogger.AddFilter(rateLimitFilter);
```

### 6. 自定义日志 (CustomLog)

带标签的日志器，支持颜色标记和增强功能。

```csharp
// 基础创建
var gameLog = new CustomLog("游戏", true, "#00FF00");

// 使用Color对象
var netLog = new CustomLog("网络", true, Color.red, true);

// 各种日志方法
gameLog.Debug("调试信息: {0}", debugData);
gameLog.Info("普通信息: {0}", info);
gameLog.Warning("警告信息: {0}", warning);
gameLog.Error("错误信息: {0}", error);

// 格式化方法
gameLog.InfoFormat("玩家{0}获得{1}经验", playerName, exp);

// 彩色信息
gameLog.Info("恭喜升级！", Color.yellow);

// 方法跟踪
gameLog.Trace();                           // 自动记录当前方法
gameLog.Trace("自定义跟踪信息");            // 带自定义信息
```

## 🎛️ 高级配置

### 系统初始化配置

在系统启动时，Log系统会自动根据配置初始化：

```csharp
// 配置会自动加载并应用
var config = LogConfig.Instance;

// 手动重新初始化 (通常不需要)
// Log系统会在静态构造函数中自动初始化
```

### 自定义初始化

如果需要完全自定义的日志系统：

```csharp
// 创建自定义组合
var unityLogger = new UnityLogger();
var fileLogger = new FileLogger("custom.log");
var bufferedFileLogger = new BufferedLogger(fileLogger, 50, 500);

var compositeLogger = new CompositeLogger(unityLogger, bufferedFileLogger);

// 添加过滤
var filteredLogger = new FilteredLogger(compositeLogger);
var filter = new KeywordFilter();
filter.AddExcludeKeyword("噪音");
filteredLogger.AddFilter(filter);

// 设置为全局日志器
Log.SetLogger(filteredLogger);
```

## 📋 API 参考

### Log 静态类

| 方法 | 描述 |
|------|------|
| `Debug(string, params object[])` | 调试日志 (仅Editor显示) |
| `Info(string, params object[])` | 信息日志 |
| `Warning(string, params object[])` | 警告日志 |
| `Error(string, params object[])` | 错误日志 |
| `Error(Exception)` | 异常日志 |
| `SetLogger(ILogger)` | 设置日志器 |
| `SetEnable(bool)` | 启用/禁用日志 |
| `SetEnableLevel(LogLevel)` | 设置日志级别 |
| `AddLogger(ILogger)` | 添加日志器到默认组合 |
| `RemoveLogger(ILogger)` | 从默认组合移除日志器 |
| `FlushBuffers()` | 刷新所有缓冲区 |

### CustomLog 类

| 方法 | 描述 |
|------|------|
| `Debug/Info/Warning/Error(string, params object[])` | 基础日志方法 |
| `DebugFormat/InfoFormat/WarningFormat/ErrorFormat` | 格式化日志方法 |
| `Info(string, Color)` | 彩色信息日志 |
| `Trace(string, string, string, int)` | 方法跟踪 |
| `Error(Exception)` | 异常日志 |

### LogConfigEditor 静态类

| 方法 | 描述 |
|------|------|
| `SetLogLevel(LogLevel)` | 设置日志级别 |
| `SetEnableLogging(bool)` | 启用/禁用日志 |
| `AddExcludeKeyword(string)` | 添加排除关键词 |
| `RemoveExcludeKeyword(string)` | 移除排除关键词 |
| `ShowCurrentConfig()` | 显示当前配置 |

## 🔥 最佳实践

### 1. 性能优化

```csharp
// ✅ 推荐：使用条件编译
[Conditional("LOG_ENABLE")]
public void LogPlayerAction(string action)
{
    Log.Info("玩家行为: {0}", action);
}

// ✅ 推荐：避免复杂的字符串构建
Log.Info("玩家位置: ({0}, {1})", player.x, player.y);

// ❌ 不推荐：复杂字符串操作
Log.Info("玩家位置: " + player.GetDetailedPosition());
```

### 2. 日志分类

```csharp
// 为不同模块创建专用日志器
public static class GameLogs
{
    public static readonly CustomLog Player = new CustomLog("玩家", true, "#00FF00");
    public static readonly CustomLog AI = new CustomLog("AI", true, "#FF6600");
    public static readonly CustomLog Network = new CustomLog("网络", true, "#0066FF");
    public static readonly CustomLog Resource = new CustomLog("资源", true, "#FF00FF");
}

// 使用
GameLogs.Player.Info("玩家升级到 {0} 级", newLevel);
GameLogs.AI.Debug("AI决策: {0}", decision);
```

### 3. 错误处理

```csharp
// ✅ 推荐：详细的错误信息
try 
{
    LoadGameData(fileName);
}
catch (FileNotFoundException ex)
{
    Log.Error("游戏数据文件未找到: {0}, 路径: {1}", fileName, ex.FileName);
}
catch (Exception ex)
{
    Log.Error("加载游戏数据时发生未知错误: {0}", ex);
    GameLogs.Resource.Error("资源加载失败", ex);
}
```

### 4. 调试和跟踪

```csharp
public class GameManager : MonoBehaviour
{
    private static readonly CustomLog _log = new CustomLog("GameManager", true, "#FFFF00");
    
    void Start()
    {
        _log.Trace("游戏管理器启动");
        InitializeGame();
        _log.Info("游戏初始化完成");
    }
    
    private void InitializeGame()
    {
        _log.Trace("开始初始化游戏");
        // 初始化逻辑
        _log.Debug("加载了 {0} 个关卡", levelCount);
    }
}
```

### 5. 发布版本优化

```csharp
// 在项目设置中定义宏
// Development Build: LOG_ENABLE
// Release Build: (不定义LOG_ENABLE)

#if LOG_ENABLE
    Log.Debug("这只在开发版本中显示");
#endif

// 或使用条件编译属性
[Conditional("LOG_ENABLE")]
public static void DebugInfo(string info)
{
    Log.Debug(info);
}
```

## 🐛 故障排除

### 常见问题

#### Q: 日志没有输出到文件
**A**: 检查以下几点：
1. 确认 `LogConfig.enableFileLogging = true`
2. 检查文件写入权限
3. 调用 `Log.FlushBuffers()` 确保缓冲区已刷新
4. 查看Unity Console是否有文件写入错误

#### Q: Debug日志在发布版本中仍然显示
**A**: 
1. 确认项目中没有定义 `LOG_ENABLE` 宏
2. 检查 `UnityLogger` 的 `LogDebug` 方法只在 `UNITY_EDITOR` 中执行

#### Q: 日志过多影响性能
**A**: 
1. 启用缓冲机制: `LogConfig.enableBuffering = true`
2. 提高日志级别: `LogConfigEditor.SetLogLevel(LogLevel.Warning)`
3. 使用过滤器排除不必要的日志
4. 在发布版本中禁用调试日志

#### Q: 自定义日志标签颜色不显示
**A**: 
1. 确认在Unity Editor中查看
2. 检查颜色代码格式 (如: "#FF0000")
3. 某些Unity版本可能不支持富文本

### 调试技巧

```csharp
// 检查当前日志配置
LogConfigEditor.ShowCurrentConfig();

// 测试日志系统
Log.Debug("测试Debug日志");
Log.Info("测试Info日志");  
Log.Warning("测试Warning日志");
Log.Error("测试Error日志");

// 手动刷新确保日志写入
Log.FlushBuffers();

// 检查文件日志位置
Debug.Log($"日志文件路径: {Application.persistentDataPath}/Logs/");
```

## 📄 更新日志

### v2.0.0 (优化版本)
- ✨ 新增文件日志输出支持
- ✨ 新增缓冲机制提升性能
- ✨ 新增多种过滤器支持
- ✨ 新增配置管理系统
- ✨ 新增方法跟踪功能
- 🐛 修复日志级别判断逻辑 (`EnableLevel >= level` 而不是 `level >= EnableLevel`)
- 🔧 增强CustomLog功能
- 📚 完善文档和示例

> **重要修复**: 日志级别判断逻辑已修正。现在当设置 `EnableLevel = Warning` 时，只会显示Warning和Error级别的日志，Debug和Info级别会被正确过滤。

### v1.0.0 (原始版本)
- ✨ 基础日志功能
- ✨ Unity控制台输出
- ✨ 自定义标签支持
- ✨ 条件编译支持

## 📞 技术支持

如果您在使用过程中遇到问题，请：

1. 首先查看本README的故障排除部分
2. 运行 `LogExample.cs` 测试基础功能
3. 检查Unity Console中的错误信息
4. 确认相关宏定义和配置设置

---

**GF Log System** - 让日志记录更简单、更高效！ 🚀 