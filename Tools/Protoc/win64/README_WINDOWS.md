# Windows版本Protobuf启动器使用说明

本目录包含适用于Windows系统的protoc启动器，使用批处理文件(.bat)实现。

## 文件说明

### 1. `run_protoc.bat` - 基础启动脚本
这是一个简单的protoc启动脚本，直接传递参数给protoc编译器。

**特点：**
- 自动检测protoc路径
- 彩色输出
- 错误处理
- 支持所有protoc原生参数

**使用方法：**
```cmd
REM 显示帮助
run_protoc.bat --help

REM 显示版本
run_protoc.bat --version

REM 生成C#代码
run_protoc.bat --csharp_out=./generated message.proto

REM 生成C++代码
run_protoc.bat --cpp_out=./generated --proto_path=./protos *.proto
```

### 2. `protoc_launcher.bat` - 高级启动器
这是一个功能更强大的启动器，支持预设配置和批量处理。

**特点：**
- 预设配置（csharp, cpp, java, python, js, go, all）
- 批量处理
- 自动创建输出目录
- 清理功能
- 干运行模式

**使用方法：**

#### 基本用法
```cmd
REM 显示帮助
protoc_launcher.bat

REM 生成C#代码（Unity项目）
protoc_launcher.bat csharp

REM 生成C++代码
protoc_launcher.bat cpp

REM 生成所有支持的代码
protoc_launcher.bat all
```

#### 高级用法
```cmd
REM 指定输出目录
protoc_launcher.bat csharp -o ./Assets/Scripts/Generated

REM 指定proto文件目录
protoc_launcher.bat csharp -p ./protos

REM 处理单个文件
protoc_launcher.bat csharp -f message.proto

REM 清理输出目录后生成
protoc_launcher.bat csharp -c

REM 显示将要执行的命令但不执行
protoc_launcher.bat csharp -d
```

### 3. `unity_protoc.bat` - Unity专用启动器
这是专门为Unity项目优化的启动器，提供更便捷的工作流程。

**特点：**
- 专门为Unity项目设计
- 自动生成到Unity Scripts目录
- 语法检查功能
- 清理功能
- 详细输出选项
- 命令式接口（generate, clean, test）

**使用方法：**

#### 基本用法
```cmd
REM 生成C#代码到Unity Scripts目录
unity_protoc.bat generate

REM 测试proto文件语法
unity_protoc.bat test

REM 清理生成的代码
unity_protoc.bat clean

REM 显示帮助
unity_protoc.bat help
```

#### 高级用法
```cmd
REM 生成单个文件的C#代码
unity_protoc.bat generate -f example.proto

REM 指定输出目录
unity_protoc.bat generate -o ./CustomOutput

REM 指定proto文件目录
unity_protoc.bat generate -p ./CustomProtos

REM 显示详细输出
unity_protoc.bat generate -v

REM 显示将要执行的命令但不执行
unity_protoc.bat generate -d
```

## 目录结构

```
protobuf/
├── Tools/
│   └── Protoc/
│       ├── osx-x86_64/           # macOS版本
│       │   ├── protoc
│       │   ├── run_protoc.sh
│       │   ├── protoc_launcher.sh
│       │   └── unity_protoc.sh
│       └── win64/                # Windows版本
│           ├── protoc.exe
│           ├── run_protoc.bat
│           ├── protoc_launcher.bat
│           ├── unity_protoc.bat
│           └── README_WINDOWS.md
├── protos/                       # proto文件目录
│   └── example.proto
├── Assets/
│   └── Scripts/
│       └── Generated/           # 生成的C#代码
└── ...
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
```cmd
REM 使用Unity专用启动器（推荐）
unity_protoc.bat generate

REM 使用高级启动器
protoc_launcher.bat csharp -o ./Assets/Scripts/Generated -p ./protos

REM 清理后重新生成
unity_protoc.bat clean
unity_protoc.bat generate
```

### 多语言项目中使用
```cmd
REM 生成所有语言的代码
protoc_launcher.bat all -p ./protos -o ./generated

REM 这将创建以下目录结构：
REM ./generated/
REM ├── csharp/
REM ├── cpp/
REM ├── java/
REM ├── python/
REM ├── js/
REM └── go/
```

### 测试命令
```cmd
REM 查看将要执行的命令
unity_protoc.bat generate -d

REM 查看protoc版本和可用插件
unity_protoc.bat version
```

## 注意事项

1. **路径问题**：脚本会自动检测protoc的路径，无需手动配置
2. **输出目录**：脚本会自动创建不存在的输出目录
3. **错误处理**：如果protoc执行失败，脚本会显示错误信息并退出
4. **文件监视**：Windows版本暂不支持自动文件监视功能，需要手动重新生成
5. **编码问题**：确保proto文件使用UTF-8编码

## 故障排除

### 找不到protoc编译器
确保`Tools/Protoc/win64/protoc.exe`文件存在。

### 找不到proto文件
确保proto文件存在于指定的目录中，或使用`-p`选项指定正确的proto文件目录。

### 权限问题
确保有足够的权限创建和写入输出目录。

### 编码问题
如果遇到中文乱码，确保proto文件使用UTF-8编码保存。

## 与macOS版本的区别

1. **文件格式**：Windows使用.bat批处理文件，macOS使用.sh shell脚本
2. **路径分隔符**：Windows使用反斜杠`\`，macOS使用正斜杠`/`
3. **文件监视**：Windows版本暂不支持自动文件监视功能
4. **架构兼容性**：Windows版本不需要处理架构兼容性问题

---

这些启动程序为您提供了完整的Protocol Buffers工作流程，特别针对Unity项目和Windows系统进行了优化。 