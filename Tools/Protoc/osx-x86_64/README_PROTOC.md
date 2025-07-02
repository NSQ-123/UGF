# Protobuf编译器启动程序使用说明

本项目提供了两个在Mac系统下运行protoc编译器的启动程序。

## 文件说明

### 1. `run_protoc.sh` - 基础启动脚本
这是一个简单的protoc启动脚本，直接传递参数给protoc编译器。

**特点：**
- 自动检测protoc路径
- 自动添加执行权限
- 彩色输出
- 错误处理

**使用方法：**
```bash
# 显示帮助
./run_protoc.sh --help

# 显示版本
./run_protoc.sh --version

# 生成C#代码
./run_protoc.sh --csharp_out=./generated message.proto

# 生成C++代码
./run_protoc.sh --cpp_out=./generated --proto_path=./protos *.proto
```

### 2. `protoc_launcher.sh` - 高级启动器
这是一个功能更强大的启动器，支持预设配置和批量处理。

**特点：**
- 预设配置（csharp, cpp, java, python, js, go, all）
- 批量处理
- 自动创建输出目录
- 清理功能
- 干运行模式
- 递归处理

### 3. `unity_protoc.sh` - Unity专用启动器
这是专门为Unity项目优化的启动器，提供更便捷的工作流程。

**特点：**
- 专门为Unity项目设计
- 自动生成到Unity Scripts目录
- 文件监视和自动重新生成
- 语法检查功能
- 清理功能
- 详细输出选项

**使用方法：**

#### 基本用法
```bash
# 显示帮助
./protoc_launcher.sh

# 生成C#代码（Unity项目）
./protoc_launcher.sh csharp

# 生成C++代码
./protoc_launcher.sh cpp

# 生成所有支持的代码
./protoc_launcher.sh all
```

#### 高级用法
```bash
# 指定输出目录
./protoc_launcher.sh csharp -o ./Assets/Scripts/Generated

# 指定proto文件目录
./protoc_launcher.sh csharp -p ./protos

# 处理单个文件
./protoc_launcher.sh csharp -f message.proto

# 清理输出目录后生成
./protoc_launcher.sh csharp -c

# 显示将要执行的命令但不执行
./protoc_launcher.sh csharp -d
```

#### Unity专用启动器用法
```bash
# 生成C#代码到Unity Scripts目录
./unity_protoc.sh generate

# 生成单个文件的C#代码
./unity_protoc.sh generate -f example.proto

# 清理生成的代码
./unity_protoc.sh clean

# 测试proto文件语法
./unity_protoc.sh test

# 监视文件变化并自动重新生成
./unity_protoc.sh watch

# 显示详细输出
./unity_protoc.sh generate -v

# 显示将要执行的命令但不执行
./unity_protoc.sh generate -d
```

## 目录结构

```
protobuf/
├── Tools/
│   └── protoc-31.1-osx-x86_64/
│       ├── bin/
│       │   └── protoc          # protoc编译器
│       └── include/            # 标准proto文件
├── protos/                     # proto文件目录
│   └── example.proto          # 示例proto文件
├── Assets/
│   └── Scripts/
│       └── Generated/         # 生成的C#代码
├── run_protoc.sh              # 基础启动脚本
├── protoc_launcher.sh         # 高级启动器
├── unity_protoc.sh            # Unity专用启动器
└── README_PROTOC.md           # 本说明文档
```

## 预设说明

| 预设 | 描述 | 输出目录 |
|------|------|----------|
| `csharp` | 生成C#代码（适用于Unity） | `./generated` |
| `cpp` | 生成C++代码 | `./generated` |
| `java` | 生成Java代码 | `./generated` |
| `python` | 生成Python代码 | `./generated` |
| `js` | 生成JavaScript代码 | `./generated` |
| `go` | 生成Go代码 | `./generated` |
| `all` | 生成所有支持的代码 | `./generated/[语言]` |

## 选项说明

| 选项 | 长选项 | 描述 |
|------|--------|------|
| `-h` | `--help` | 显示帮助信息 |
| `-v` | `--version` | 显示protoc版本 |
| `-o` | `--output` | 指定输出目录 |
| `-p` | `--proto` | 指定proto文件目录 |
| `-f` | `--file` | 指定单个proto文件 |
| `-r` | `--recursive` | 递归处理子目录 |
| `-c` | `--clean` | 清理输出目录 |
| `-d` | `--dry-run` | 显示将要执行的命令但不执行 |

## 使用示例

### Unity项目中使用
```bash
# 使用Unity专用启动器（推荐）
./unity_protoc.sh generate

# 使用高级启动器
./protoc_launcher.sh csharp -o ./Assets/Scripts/Generated -p ./protos

# 清理后重新生成
./unity_protoc.sh clean && ./unity_protoc.sh generate

# 监视文件变化并自动重新生成
./unity_protoc.sh watch
```

### 多语言项目中使用
```bash
# 生成所有语言的代码
./protoc_launcher.sh all -p ./protos -o ./generated

# 这将创建以下目录结构：
# ./generated/
# ├── csharp/
# ├── cpp/
# ├── java/
# ├── python/
# ├── js/
# └── go/
```

### 测试命令
```bash
# 查看将要执行的命令
./protoc_launcher.sh csharp -d

# 查看protoc版本和可用插件
./protoc_launcher.sh -v
```

## 注意事项

1. **权限问题**：脚本会自动为protoc文件添加执行权限
2. **路径问题**：脚本会自动检测protoc的路径，无需手动配置
3. **输出目录**：脚本会自动创建不存在的输出目录
4. **错误处理**：如果protoc执行失败，脚本会显示错误信息并退出

## 故障排除

### 找不到protoc编译器
确保`Tools/protoc-31.1-osx-x86_64/bin/protoc`文件存在。

### 权限被拒绝
脚本会自动添加执行权限，如果仍有问题，手动执行：
```bash
chmod +x Tools/protoc-31.1-osx-x86_64/bin/protoc
```

### 找不到proto文件
确保proto文件存在于指定的目录中，或使用`-p`选项指定正确的proto文件目录。 