#!/bin/bash

# 高级Protobuf编译器启动器
# 适用于Mac系统，支持预设配置和批量处理

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROTOC_PATH="$SCRIPT_DIR/protoc"
INCLUDE_PATH="$SCRIPT_DIR/include"

# 默认配置
DEFAULT_OUTPUT_DIR="./generated"
DEFAULT_PROTO_DIR="./protos"

# 检查protoc是否存在
check_protoc() {
    if [ ! -f "$PROTOC_PATH" ]; then
        echo -e "${RED}错误: 找不到protoc编译器${NC}"
        echo "期望路径: $PROTOC_PATH"
        exit 1
    fi

    if [ ! -x "$PROTOC_PATH" ]; then
        echo -e "${YELLOW}警告: protoc文件没有执行权限，正在添加执行权限...${NC}"
        chmod +x "$PROTOC_PATH"
    fi
    
    # 检查系统架构兼容性
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

# 显示帮助信息
show_help() {
    echo -e "${BLUE}高级Protobuf编译器启动器${NC}"
    echo ""
    echo "用法: $0 [预设] [选项] <proto文件>"
    echo ""
    echo "预设:"
    echo "  csharp    生成C#代码 (Unity项目)"
    echo "  cpp       生成C++代码"
    echo "  java      生成Java代码"
    echo "  python    生成Python代码"
    echo "  js        生成JavaScript代码"
    echo "  go        生成Go代码"
    echo "  all       生成所有支持的代码"
    echo ""
    echo "选项:"
    echo "  -h, --help              显示此帮助信息"
    echo "  -v, --version           显示protoc版本"
    echo "  -o, --output <目录>      指定输出目录 (默认: $DEFAULT_OUTPUT_DIR)"
    echo "  -p, --proto <目录>       指定proto文件目录 (默认: $DEFAULT_PROTO_DIR)"
    echo "  -f, --file <文件>        指定单个proto文件"
    echo "  -r, --recursive         递归处理子目录"
    echo "  -c, --clean             清理输出目录"
    echo "  -d, --dry-run           显示将要执行的命令但不执行"
    echo ""
    echo "示例:"
    echo "  $0 csharp                    # 生成C#代码"
    echo "  $0 csharp -o ./Scripts       # 生成C#代码到Scripts目录"
    echo "  $0 all -p ./protos -r        # 递归处理protos目录，生成所有代码"
    echo "  $0 cpp -f message.proto      # 处理单个文件"
    echo "  $0 -v                        # 显示版本信息"
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

# 清理输出目录
clean_output_dir() {
    local output_dir="$1"
    if [ -d "$output_dir" ]; then
        echo -e "${YELLOW}清理输出目录: $output_dir${NC}"
        rm -rf "$output_dir"/*
    fi
}

# 创建输出目录
create_output_dir() {
    local output_dir="$1"
    if [ ! -d "$output_dir" ]; then
        echo -e "${YELLOW}创建输出目录: $output_dir${NC}"
        mkdir -p "$output_dir"
    fi
}

# 生成C#代码
generate_csharp() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成C#代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --csharp_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ C#代码生成成功${NC}"
        else
            echo -e "${RED}✗ C#代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成C++代码
generate_cpp() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成C++代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --cpp_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ C++代码生成成功${NC}"
        else
            echo -e "${RED}✗ C++代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成Java代码
generate_java() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成Java代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --java_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ Java代码生成成功${NC}"
        else
            echo -e "${RED}✗ Java代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成Python代码
generate_python() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成Python代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --python_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ Python代码生成成功${NC}"
        else
            echo -e "${RED}✗ Python代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成JavaScript代码
generate_js() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成JavaScript代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --js_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ JavaScript代码生成成功${NC}"
        else
            echo -e "${RED}✗ JavaScript代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成Go代码
generate_go() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成Go代码...${NC}"
    local cmd="$PROTOC_CMD --proto_path=$proto_path --go_out=$output_dir"
    
    if [ "$dry_run" = "true" ]; then
        echo "命令: $cmd *.proto"
    else
        create_output_dir "$output_dir"
        if $cmd *.proto; then
            echo -e "${GREEN}✓ Go代码生成成功${NC}"
        else
            echo -e "${RED}✗ Go代码生成失败${NC}"
            return 1
        fi
    fi
}

# 生成所有代码
generate_all() {
    local proto_path="$1"
    local output_dir="$2"
    local dry_run="$3"
    
    echo -e "${CYAN}生成所有支持的代码...${NC}"
    
    generate_csharp "$proto_path" "$output_dir/csharp" "$dry_run" || return 1
    generate_cpp "$proto_path" "$output_dir/cpp" "$dry_run" || return 1
    generate_java "$proto_path" "$output_dir/java" "$dry_run" || return 1
    generate_python "$proto_path" "$output_dir/python" "$dry_run" || return 1
    generate_js "$proto_path" "$output_dir/js" "$dry_run" || return 1
    generate_go "$proto_path" "$output_dir/go" "$dry_run" || return 1
    
    echo -e "${GREEN}✓ 所有代码生成完成${NC}"
}

# 处理单个文件
process_single_file() {
    local proto_file="$1"
    local output_dir="$2"
    local language="$3"
    local dry_run="$4"
    
    echo -e "${CYAN}处理文件: $proto_file${NC}"
    
    case "$language" in
        csharp)
            generate_csharp "." "$output_dir" "$dry_run"
            ;;
        cpp)
            generate_cpp "." "$output_dir" "$dry_run"
            ;;
        java)
            generate_java "." "$output_dir" "$dry_run"
            ;;
        python)
            generate_python "." "$output_dir" "$dry_run"
            ;;
        js)
            generate_js "." "$output_dir" "$dry_run"
            ;;
        go)
            generate_go "." "$output_dir" "$dry_run"
            ;;
        *)
            echo -e "${RED}未知的语言类型: $language${NC}"
            return 1
            ;;
    esac
}

# 主函数
main() {
    # 检查protoc
    check_protoc
    
    # 默认参数
    local preset=""
    local output_dir="$DEFAULT_OUTPUT_DIR"
    local proto_dir="$DEFAULT_PROTO_DIR"
    local proto_file=""
    local recursive=false
    local clean=false
    local dry_run=false
    
    # 解析参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            csharp|cpp|java|python|js|go|all)
                preset="$1"
                shift
                ;;
            -h|--help)
                show_help
                exit 0
                ;;
            -v|--version)
                show_version
                exit 0
                ;;
            -o|--output)
                output_dir="$2"
                shift 2
                ;;
            -p|--proto)
                proto_dir="$2"
                shift 2
                ;;
            -f|--file)
                proto_file="$2"
                shift 2
                ;;
            -r|--recursive)
                recursive=true
                shift
                ;;
            -c|--clean)
                clean=true
                shift
                ;;
            -d|--dry-run)
                dry_run=true
                shift
                ;;
            *)
                echo -e "${RED}未知参数: $1${NC}"
                show_help
                exit 1
                ;;
        esac
    done
    
    # 如果没有预设，显示帮助
    if [ -z "$preset" ]; then
        show_help
        exit 0
    fi
    
    # 清理输出目录
    if [ "$clean" = "true" ]; then
        clean_output_dir "$output_dir"
    fi
    
    # 处理单个文件
    if [ -n "$proto_file" ]; then
        if [ ! -f "$proto_file" ]; then
            echo -e "${RED}错误: 找不到文件 $proto_file${NC}"
            exit 1
        fi
        
        # 切换到文件所在目录
        local file_dir="$(dirname "$proto_file")"
        local file_name="$(basename "$proto_file")"
        cd "$file_dir"
        
        process_single_file "$file_name" "$output_dir" "$preset" "$dry_run"
        exit $?
    fi
    
    # 检查proto目录
    if [ ! -d "$proto_dir" ]; then
        echo -e "${RED}错误: 找不到proto目录 $proto_dir${NC}"
        exit 1
    fi
    
    # 切换到proto目录
    cd "$proto_dir"
    
    # 根据预设生成代码
    case "$preset" in
        csharp)
            generate_csharp "." "$output_dir" "$dry_run"
            ;;
        cpp)
            generate_cpp "." "$output_dir" "$dry_run"
            ;;
        java)
            generate_java "." "$output_dir" "$dry_run"
            ;;
        python)
            generate_python "." "$output_dir" "$dry_run"
            ;;
        js)
            generate_js "." "$output_dir" "$dry_run"
            ;;
        go)
            generate_go "." "$output_dir" "$dry_run"
            ;;
        all)
            generate_all "." "$output_dir" "$dry_run"
            ;;
        *)
            echo -e "${RED}未知的预设: $preset${NC}"
            exit 1
            ;;
    esac
    
    exit $?
}

# 运行主函数
main "$@" 