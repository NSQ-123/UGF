1.安装UnityTask的地址： https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask

## 符号链接创建指南

### 基本命令格式
```cmd
mklink [选项] <链接路径/链接名> <目标路径>
```

### 链接类型对比

| 类型 | 命令 | 权限要求 | 特点 |
|------|------|----------|------|
| **Symbolic Link** | `mklink /D` | 需要管理员权限 | 真正的符号链接，支持跨驱动器 |
| **Junction** | `mklink /J` | 不需要管理员权限 | 连接点，仅限同一NTFS卷 |
| **Hard Link** | `mklink /H` | 不需要管理员权限 | 仅适用于文件 |

### 实际使用示例

#### 1. 创建Junction（推荐）
```powershell
# 不需要管理员权限
mklink /J "D:\Projects\MyLink" "C:\Target\Folder"
```

#### 2. 创建Symbolic Link
```powershell
# 需要管理员权限
mklink /D "D:\Projects\MyLink" "C:\Target\Folder"
```

### 注意事项
- 链接名可以指定相对路径或绝对路径
- 包含空格的路径需要用引号括起来
- Junction不支持跨驱动器，但不需要管理员权限
- 创建链接前确保目标路径存在
- 链接创建后，修改链接中的文件会直接影响源文件

## 项目结构

- [脚本文件夹](Scripts/) - 项目脚本文件
- [预制体文件夹](Prefabs/) - Unity预制体
- [场景文件夹](Scenes/) - Unity场景文件
- [材质文件夹](Materials/) - 材质和纹理
- [文档](../Documentation/) - 项目文档

## 重要文件

- [主要配置文件](../ProjectSettings/ProjectSettings.asset)
- [包管理文件](../Packages/manifest.json)

## 外部资源链接

- [外部共享资源](../../SharedAssets/) - 项目外部的共享资源文件夹
- [外部文档库](../../../Documentation/) - 外部文档文件夹
- [本地资源库](file:///C:/ExternalAssets/) - 本地外部资源（Windows路径示例）
- [网络共享文件夹](\\\\ServerName\\SharedFolder\\) - 网络共享文件夹

