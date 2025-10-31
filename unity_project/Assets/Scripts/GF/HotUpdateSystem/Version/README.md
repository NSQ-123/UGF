# 版本检查系统

本系统提供了完整的版本兼容性检查功能，支持多种版本号格式和灵活的检查策略。

## 核心组件

### 1. VersionParser - 版本解析器
- 支持多种版本号格式：`v1.0.0`、`1.0.0`、`v1.0.0.123`、`v1.0.0-alpha`
- 提供版本号解析、验证和格式化功能

### 2. VersionCompatibilityChecker - 内置兼容性检查器
- 提供内置的版本兼容性规则
- 支持主版本、次版本差异检查
- 自动检测强制更新需求

### 3. ServerVersionChecker - 服务器策略检查器
- 支持服务器端版本策略
- 可配置最低支持版本和强制更新版本
- 支持兼容版本列表

### 4. VersionCompatibilityManager - 版本兼容性管理器
- 统一管理所有版本检查逻辑
- 结合内置规则和服务器策略
- 提供完整的版本检查结果

## 使用方法

### 基础版本检查

```csharp
// 解析版本号
var versionInfo = VersionParser.ParseVersion("v1.0.0");

// 检查版本兼容性
var isCompatible = VersionParser.IsVersionCompatible("v1.0.0", "v1.1.0");

// 检查是否需要强制更新
var requiresForceUpdate = VersionCompatibilityChecker.CompatibilityRules.RequiresForceUpdate("v1.0.0", "v2.0.0");
```

### 完整版本检查

```csharp
// 创建服务器策略（可选）
var serverPolicy = new ServerVersionPolicy
{
    MinSupportedVersion = "v1.0.0",
    ForceUpdateVersion = "v2.0.0",
    UpdateMessage = "请更新到最新版本"
};

// 执行完整版本检查
var result = VersionCompatibilityManager.CheckVersion("v1.0.0", "v1.1.0", serverPolicy);

if (!result.IsCompatible)
{
    Debug.LogError($"版本不兼容: {result.Reason}");
    return;
}

if (result.Recommendation == UpdateType.ForceUpdate)
{
    Debug.LogError($"需要强制更新: {result.Message}");
    return;
}

Debug.Log($"版本检查通过: {result.Recommendation} - {result.Message}");
```

### 在热更新流程中使用

```csharp
// 在FsmRequestPackageVersion中
private IEnumerator UpdatePackageVersion()
{
    var operation = package.RequestPackageVersionAsync();
    yield return operation;

    if (operation.Status != EOperationStatus.Succeed)
    {
        _operation?.SendError($"请求包版本失败: {operation.Error}", HotUpdateStage.RequestPackageVersion);
        yield break;
    }
    
    var serverVersion = operation.PackageVersion;
    var localVersion = package.GetPackageVersion();
    
    // 获取服务器版本策略（可选）
    var serverPolicy = _machine.GetBlackboardValue<ServerVersionPolicy>("ServerVersionPolicy");

    // 检查版本兼容性
    var versionResult = VersionCompatibilityManager.CheckVersion(localVersion, serverVersion, serverPolicy);

    if (!versionResult.IsCompatible)
    {
        _operation?.SendError($"版本不兼容: {versionResult.Reason}", HotUpdateStage.RequestPackageVersion);
        yield break;
    }

    if (versionResult.Recommendation == UpdateType.ForceUpdate)
    {
        var message = string.IsNullOrEmpty(versionResult.Message) ? 
            "需要强制更新，请前往应用商店下载最新版本" : versionResult.Message;
        _operation?.SendError(message, HotUpdateStage.RequestPackageVersion);
        yield break;
    }
    
    // 版本检查通过，继续热更新流程
    _machine.SetBlackboardValue("PackageVersion", serverVersion);
    _machine.ChangeState<FsmUpdatePackageManifest>();
}
```

## 版本号格式

支持以下版本号格式：

- `v1.0.0` - 标准格式
- `1.0.0` - 无前缀格式
- `v1.0.0.123` - 包含构建号
- `v1.0.0-alpha` - 包含后缀
- `v1.0.0-beta.1` - 包含数字后缀

## 更新类型

- `None` - 无需更新
- `Optional` - 可选更新
- `Recommended` - 建议更新
- `ForceUpdate` - 强制更新

## 内置规则

- **最低支持版本**: `v1.0.0`
- **最大主版本差异**: 1
- **最大次版本差异**: 2
- **主版本不同**: 需要强制更新

## 服务器策略

可以通过`ServerVersionPolicy`配置：

- `MinSupportedVersion` - 最低支持版本
- `ForceUpdateVersion` - 强制更新版本
- `AllowDowngrade` - 是否允许降级
- `CompatibleVersions` - 兼容版本列表
- `UpdateMessage` - 更新提示信息

## 测试

使用`VersionUsageExample`组件进行测试：

1. 在场景中添加`VersionUsageExample`组件
2. 设置测试参数
3. 使用Context Menu执行测试：
   - 测试版本检查
   - 测试版本格式
   - 测试更新建议 