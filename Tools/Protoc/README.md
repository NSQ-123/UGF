# Protobuf启动器 - 跨平台版本

本项目提供了适用于macOS和Windows系统的protoc启动器，专门为Unity项目优化。

## 目录结构

```
Tools/Protoc/
├── osx-x86_64/           # macOS版本
│   ├── protoc            # protoc编译器 (x86_64)
│   ├── run_protoc.sh     # 基础启动脚本
│   ├── protoc_launcher.sh # 高级启动器
│   ├── unity_protoc.sh   # Unity专用启动器
│   ├── example_usage.sh  # 使用示例
│   ├── README_PROTOC.md  # macOS使用说明
│   └── SUMMARY.md        # 总结文档
└── win64/                # Windows版本
    ├── protoc.exe        # protoc编译器
    ├── run_protoc.bat    # 基础启动脚本
    ├── protoc_launcher.bat # 高级启动器
    ├── unity_protoc.bat  # Unity专用启动器
    ├── example_usage.bat # 使用示例
    └── README_WINDOWS.md # Windows使用说明
```

## 启动器类型

### 1. 基础启动脚本
- **macOS**: `osx-x86_64/run_protoc.sh`
- **Windows**: `win64/run_protoc.bat`
- **用途**: 简单的protoc命令包装器
- **特点**: 自动路径检测、错误处理、彩色输出

### 2. 高级启动器
- **macOS**: `osx-x86_64/protoc_launcher.sh`
- **Windows**: `win64/protoc_launcher.bat`
- **用途**: 功能完整的protoc启动器
- **特点**: 预设配置、批量处理、多语言支持

### 3. Unity专用启动器 ⭐
- **macOS**: `osx-x86_64/unity_protoc.sh`
- **Windows**: `win64/unity_protoc.bat`
- **用途**: 专门为Unity项目优化
- **特点**: 自动生成到Unity Scripts目录、语法检查、清理功能

## 快速开始

### macOS用户
```bash
# 生成C#代码到Unity Scripts目录
./Tools/Protoc/osx-x86_64/unity_protoc.sh generate

# 测试proto文件语法
./Tools/Protoc/osx-x86_64/unity_protoc.sh test

# 监视文件变化（需要安装fswatch）
./Tools/Protoc/osx-x86_64/unity_protoc.sh watch
```

### Windows用户
```cmd
REM 生成C#代码到Unity Scripts目录
Tools\Protoc\win64\unity_protoc.bat generate

REM 测试proto文件语法
Tools\Protoc\win64\unity_protoc.bat test

REM 清理生成的代码
Tools\Protoc\win64\unity_protoc.bat clean
```

## 主要功能

### 1. 自动路径检测
- 自动找到protoc编译器
- 自动设置正确的路径
- 无需手动配置

### 2. 架构兼容性 (macOS)
- 支持Intel Mac (x86_64)
- 支持Apple Silicon (ARM64)
- 自动使用Rosetta 2

### 3. Unity项目集成
- 自动生成到Unity Scripts目录
- 自动创建必要的目录结构
- 与Unity项目结构完美匹配

### 4. 开发便利性
- 语法检查功能
- 清理功能
- 干运行模式
- 详细输出选项

## 使用建议

### 对于Unity项目
1. **推荐使用Unity专用启动器**
2. **开发时使用监视模式** (macOS)
3. **提交前清理并重新生成**

### 对于多语言项目
1. **使用高级启动器**
2. **选择需要的语言预设**
3. **使用all预设生成所有语言**

### 对于简单任务
1. **使用基础启动脚本**
2. **直接传递protoc参数**

## 平台差异

| 功能 | macOS | Windows |
|------|-------|---------|
| 文件格式 | .sh (Shell脚本) | .bat (批处理文件) |
| 路径分隔符 | `/` | `\` |
| 文件监视 | ✅ 支持 (fswatch) | ❌ 暂不支持 |
| 架构兼容性 | ✅ 自动处理 | ❌ 不需要 |
| 颜色输出 | ✅ 支持 | ✅ 支持 |

## 故障排除

### macOS常见问题
1. **权限问题**: `chmod +x Tools/Protoc/osx-x86_64/*.sh`
2. **Rosetta问题**: `softwareupdate --install-rosetta`
3. **fswatch问题**: `brew install fswatch`

### Windows常见问题
1. **路径问题**: 使用反斜杠`\`
2. **编码问题**: 确保proto文件使用UTF-8编码
3. **权限问题**: 以管理员身份运行

## 版本信息

- **protoc版本**: 31.1
- **支持语言**: C#, C++, Java, Python, JavaScript, Go
- **Unity支持**: 2020.3 LTS及以上版本
- **操作系统**: macOS 10.15+, Windows 10+

## 下一步

1. 根据需要修改proto文件
2. 使用相应的启动器生成代码
3. 在Unity中导入生成的C#代码
4. 开始使用Protocol Buffers进行数据序列化

---

这些启动程序为您提供了完整的Protocol Buffers工作流程，支持跨平台开发，特别针对Unity项目进行了优化。 