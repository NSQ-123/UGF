#!/bin/bash

# Protobuf编译器启动脚本
# 适用于Mac系统

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROTOC_PATH="$SCRIPT_DIR/protoc"

# 检查protoc是否存在
if [ ! -f "$PROTOC_PATH" ]; then
    echo -e "${RED}错误: 找不到protoc编译器${NC}"
    echo "期望路径: $PROTOC_PATH"
    exit 1
fi

# 检查protoc是否可执行
if [ ! -x "$PROTOC_PATH" ]; then
    echo -e "${YELLOW}警告: protoc文件没有执行权限，正在添加执行权限...${NC}"
    chmod +x "$PROTOC_PATH"
fi

# 检查系统架构兼容性
check_architecture() {
    local system_arch=$(uname -m)
    local protoc_arch=$(file "$PROTOC_PATH" | grep -o "x86_64\|arm64" | head -1)
    
    if [ "$system_arch" = "arm64" ] && [ "$protoc_arch" = "x86_64" ]; then
        echo -e "${YELLOW}警告: 检测到架构不匹配${NC}"
        echo "系统架构: $system_arch (Apple Silicon)"
        echo "Protoc架构: $protoc_arch (Intel)"
        echo "建议: 下载ARM64版本的protoc以获得更好的性能"
        echo ""
        echo -e "${YELLOW}尝试使用Rosetta运行...${NC}"
        
        # 检查是否安装了Rosetta
        if ! arch -x86_64 /usr/bin/true >/dev/null 2>&1; then
            echo -e "${RED}错误: 需要安装Rosetta 2来运行x86_64版本的protoc${NC}"
            echo "请运行: softwareupdate --install-rosetta"
            exit 1
        fi
        
        # 使用Rosetta运行protoc
        PROTOC_CMD="arch -x86_64 $PROTOC_PATH"
    else
        PROTOC_CMD="$PROTOC_PATH"
    fi
}

# 检查架构兼容性
check_architecture

# 显示帮助信息
show_help() {
    echo -e "${BLUE}Protobuf编译器启动脚本${NC}"
    echo ""
    echo "用法: $0 [选项] <proto文件>"
    echo ""
    echo "选项:"
    echo "  -h, --help              显示此帮助信息"
    echo "  -v, --version           显示protoc版本"
    echo "  -o, --output <目录>      指定输出目录"
    echo "  -I, --proto_path <路径>  指定proto文件搜索路径"
    echo "  --cpp_out <目录>         生成C++代码"
    echo "  --csharp_out <目录>      生成C#代码"
    echo "  --java_out <目录>        生成Java代码"
    echo "  --python_out <目录>      生成Python代码"
    echo "  --js_out <目录>          生成JavaScript代码"
    echo "  --go_out <目录>          生成Go代码"
    echo ""
    echo "示例:"
    echo "  $0 --cpp_out=./generated message.proto"
    echo "  $0 --csharp_out=./generated --proto_path=./protos *.proto"
    echo "  $0 -v"
    echo ""
}

# 显示版本信息
show_version() {
    echo -e "${GREEN}Protoc版本信息:${NC}"
    $PROTOC_CMD --version
    echo ""
    echo -e "${GREEN}可用插件:${NC}"
    $PROTOC_CMD --help | grep -A 20 "Available plugins:"
}

# 主函数
main() {
    # 如果没有参数，显示帮助
    if [ $# -eq 0 ]; then
        show_help
        exit 0
    fi

    # 处理特殊参数
    case "$1" in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--version)
            show_version
            exit 0
            ;;
    esac

    # 执行protoc命令
    echo -e "${GREEN}执行protoc命令...${NC}"
    echo "命令: $PROTOC_CMD $*"
    echo ""
    
    # 执行protoc并捕获输出
    if $PROTOC_CMD "$@"; then
        echo -e "${GREEN}✓ protoc执行成功${NC}"
    else
        echo -e "${RED}✗ protoc执行失败${NC}"
        exit 1
    fi
}

# 运行主函数
main "$@" 