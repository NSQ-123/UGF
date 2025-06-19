# INI安全的组合字符串分隔符指南

## 🚨 重要说明
由于配置系统使用INI文件格式进行持久化存储，组合字符串的分隔符**不能使用**INI文件的特殊字符，否则会导致配置文件解析错误。

## 🚫 禁用的INI保留字符

以下字符在INI文件中有特殊含义，**绝对不能**用作分隔符：

| 字符 | 用途 | 说明 |
|------|------|------|
| `=` | 键值分隔符 | 用于分隔配置项的键和值 |
| `[` `]` | 节标记 | 用于标记配置节（如`[Default]`、`[Player_123]`） |
| `#` | 注释符 | 用于标记注释行 |
| `"` | 字符串包装 | 用于包装字符串值 |
| `\` | 转义字符 | 用于转义特殊字符 |
| `\n` `\r` | 换行符 | 用于行分隔 |

## ✅ 推荐的安全分隔符

按安全性从高到低排序：

### 🥇 最安全（ASCII控制字符）
```csharp
CompositeStringHelper.Separators.UnitSeparator     // \x1F (ASCII 31)
CompositeStringHelper.Separators.RecordSeparator   // \x1E (ASCII 30)  
CompositeStringHelper.Separators.GroupSeparator    // \x1D (ASCII 29)
CompositeStringHelper.Separators.FileSeparator     // \x1C (ASCII 28)
CompositeStringHelper.Separators.InfoSeparator1    // \x1B (ASCII 27)
CompositeStringHelper.Separators.InfoSeparator2    // \x1A (ASCII 26)
```

### 🥈 常用安全字符
```csharp
CompositeStringHelper.Separators.Pipe        // |
CompositeStringHelper.Separators.Tilde       // ~  
CompositeStringHelper.Separators.At          // @
CompositeStringHelper.Separators.Caret       // ^
```

### 🥉 较安全字符
```csharp
CompositeStringHelper.Separators.Dollar      // $
CompositeStringHelper.Separators.Percent     // %
CompositeStringHelper.Separators.Ampersand   // &
CompositeStringHelper.Separators.Asterisk    // *
CompositeStringHelper.Separators.Plus        // +
CompositeStringHelper.Separators.Question    // ?
CompositeStringHelper.Separators.Underscore  // _
CompositeStringHelper.Separators.Hyphen      // -
```

### ⚠️ 需谨慎使用
```csharp
CompositeStringHelper.Separators.Comma       // , (可能与CSV格式冲突)
CompositeStringHelper.Separators.Colon       // : (可能与时间格式冲突)
CompositeStringHelper.Separators.Semicolon   // ; (可能与某些INI实现的注释冲突)
```

## 💡 使用建议

### 1. 自动选择（推荐）
```csharp
// 系统会自动选择最安全的分隔符
optionService.SetCompositeString("PlayerEquipment", "剑", "盾牌", "头盔");
```

### 2. 手动指定安全分隔符
```csharp
// 使用最安全的ASCII控制字符
optionService.SetCompositeString("PlayerStats", 
    CompositeStringHelper.Separators.UnitSeparator, 
    "100", "50", "25", "75");

// 使用常见的安全字符
optionService.SetCompositeString("ServerList", 
    CompositeStringHelper.Separators.Pipe, 
    "服务器1", "服务器2", "服务器3");
```

### 3. 安全性检查
```csharp
char separator = '=';  // 危险字符
if (CompositeStringHelper.IsIniReservedChar(separator))
{
    Debug.LogWarning($"分隔符 '{separator}' 与INI格式冲突，请选择其他字符");
}
```

## 📝 最佳实践

1. **优先使用ASCII控制字符**：它们专门设计用于数据分隔，不会与任何可见文本冲突
2. **让系统自动选择**：`SelectBestSeparator()`会自动避开INI冲突字符
3. **避免常见符号**：如果数据中可能包含常见符号，优先选择不常见的分隔符
4. **使用安全检查**：在手动指定分隔符时，使用`IsIniReservedChar()`进行检查

## 🔍 实际示例

### ✅ 正确用法
```csharp
// 游戏装备列表 - 使用管道符（安全）
optionService.SetCompositeString("Equipment", 
    CompositeStringHelper.Separators.Pipe, 
    "传说之剑", "龙鳞盾", "魔法头盔");

// 玩家属性 - 使用ASCII控制字符（最安全）
optionService.SetCompositeString("PlayerStats", 
    CompositeStringHelper.Separators.UnitSeparator, 
    "100", "50", "25", "75");
```

### ❌ 错误用法
```csharp
// 错误：使用等号作为分隔符（会破坏INI格式）
optionService.SetCompositeString("BadExample", '=', "value1", "value2");

// 错误：使用方括号作为分隔符（会破坏INI节标记）
optionService.SetCompositeString("BadExample", '[', "value1", "value2");
```

## 🎯 配置文件示例

使用安全分隔符后，INI文件内容如下：

```ini
[Default]
PlayerEquipment="传说之剑|龙鳞盾|魔法头盔"
PlayerStats="100☟50☟25☟75"
ServerList="亚洲服务器~欧洲服务器~美洲服务器"

[Player_12345]
LevelStars="3|2|1|3|2"
LevelTimes="45.2|67.8|123.4|89.1|56.7"
```

注：`☟`代表ASCII 31 (UnitSeparator)字符，实际文件中不可见。 