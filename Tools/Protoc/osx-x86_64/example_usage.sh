#!/bin/bash

# Protobuf启动程序使用示例
# 展示如何使用不同的启动程序

echo "=== Protobuf启动程序使用示例 ==="
echo ""

# 显示版本信息
echo "1. 显示protoc版本信息："
echo "   ./run_protoc.sh --version"
echo "   ./protoc_launcher.sh -v"
echo "   ./unity_protoc.sh version"
echo ""

# 基础用法
echo "2. 基础用法："
echo "   # 生成C#代码"
echo "   ./run_protoc.sh --csharp_out=./generated example.proto"
echo ""

# 高级启动器用法
echo "3. 高级启动器用法："
echo "   # 生成C#代码"
echo "   ./protoc_launcher.sh csharp"
echo ""
echo "   # 生成所有语言的代码"
echo "   ./protoc_launcher.sh all"
echo ""
echo "   # 指定输出目录"
echo "   ./protoc_launcher.sh csharp -o ./Assets/Scripts/Generated"
echo ""

# Unity专用启动器用法
echo "4. Unity专用启动器用法："
echo "   # 生成C#代码到Unity Scripts目录"
echo "   ./unity_protoc.sh generate"
echo ""
echo "   # 测试proto文件语法"
echo "   ./unity_protoc.sh test"
echo ""
echo "   # 清理生成的代码"
echo "   ./unity_protoc.sh clean"
echo ""
echo "   # 监视文件变化并自动重新生成"
echo "   ./unity_protoc.sh watch"
echo ""

# 实际运行示例
echo "=== 实际运行示例 ==="
echo ""

echo "显示protoc版本："
./run_protoc.sh --version
echo ""

echo "测试proto文件语法："
./unity_protoc.sh test
echo ""

echo "生成C#代码到Unity Scripts目录："
./unity_protoc.sh generate
echo ""

echo "检查生成的代码："
if [ -f "Assets/Scripts/Generated/Example.cs" ]; then
    echo "✓ 成功生成C#代码：Assets/Scripts/Generated/Example.cs"
    echo "  文件大小: $(ls -lh Assets/Scripts/Generated/Example.cs | awk '{print $5}')"
else
    echo "✗ 未找到生成的代码文件"
fi
echo ""

echo "=== 使用建议 ==="
echo ""
echo "1. 对于Unity项目，推荐使用 unity_protoc.sh"
echo "2. 对于多语言项目，推荐使用 protoc_launcher.sh"
echo "3. 对于简单的一次性任务，可以使用 run_protoc.sh"
echo ""
echo "4. 开发时可以使用 watch 模式自动重新生成："
echo "   ./unity_protoc.sh watch"
echo ""
echo "5. 在提交代码前记得清理和重新生成："
echo "   ./unity_protoc.sh clean && ./unity_protoc.sh generate"
echo "" 