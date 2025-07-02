#!/bin/bash

# Unity项目专用的Protobuf编译器启动脚本
# 适用于Mac系统，专门为Unity项目优化

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# 获取protoc路径
PROTOC_PATH="$SCRIPT_DIR/protoc"

# Unity项目默认配置
# 获取项目根目录
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
# 获取Unity Scripts目录
UNITY_SCRIPTS_DIR="$PROJECT_ROOT/Assets/Scripts"
# 获取Unity Proto目录
UNITY_PROTO_DIR="$PROJECT_ROOT/protos"
# 获取Unity Generated目录
UNITY_GENERATED_DIR="$UNITY_SCRIPTS_DIR/Generated"

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
    echo -e "${BLUE}Unity项目专用Protobuf编译器启动器${NC}"
    echo ""
    echo "用法: $0 [命令] [选项]"
    echo ""
    echo "命令:"
    echo "  generate    生成C#代码到Unity Scripts目录"
    echo "  clean       清理生成的代码"
    echo "  watch       监视proto文件变化并自动重新生成"
    echo "  test        测试proto文件语法"
    echo "  help        显示此帮助信息"
    echo "  version     显示protoc版本"
    echo ""
    echo "选项:"
    echo "  -p, --proto <目录>    指定proto文件目录 (默认: $UNITY_PROTO_DIR)"
    echo "  -o, --output <目录>   指定输出目录 (默认: $UNITY_GENERATED_DIR)"
    echo "  -f, --file <文件>     指定单个proto文件"
    echo "  -d, --dry-run         显示将要执行的命令但不执行"
    echo "  -v, --verbose         详细输出"
    echo ""
    echo "示例:"
    echo "  $0 generate                    # 生成所有proto文件的C#代码"
    echo "  $0 generate -f message.proto   # 生成单个文件的C#代码"
    echo "  $0 clean                       # 清理生成的代码"
    echo "  $0 watch                       # 监视文件变化"
    echo "  $0 test                        # 测试proto文件语法"
    echo ""
}

# 显示版本信息
show_version() {
    echo -e "${GREEN}Protoc版本信息:${NC}"
    $PROTOC_CMD --version
    echo ""
    echo -e "${GREEN}Unity项目配置:${NC}"
    echo "Proto目录: $UNITY_PROTO_DIR"
    echo "输出目录: $UNITY_GENERATED_DIR"
    echo "Scripts目录: $UNITY_SCRIPTS_DIR"
}

# 创建目录
create_directories() {
    local output_dir="$1"
    if [ ! -d "$output_dir" ]; then
        echo -e "${YELLOW}创建输出目录: $output_dir${NC}"
        mkdir -p "$output_dir"
    fi
    
    if [ ! -d "$UNITY_SCRIPTS_DIR" ]; then
        echo -e "${YELLOW}创建Scripts目录: $UNITY_SCRIPTS_DIR${NC}"
        mkdir -p "$UNITY_SCRIPTS_DIR"
    fi
}

# 清理生成的代码
clean_generated() {
    local output_dir="$1"
    if [ -d "$output_dir" ]; then
        echo -e "${YELLOW}清理输出目录: $output_dir${NC}"
        rm -rf "$output_dir"/*
        echo -e "${GREEN}✓ 清理完成${NC}"
    else
        echo -e "${YELLOW}输出目录不存在: $output_dir${NC}"
    fi
}

# 生成C#代码
generate_csharp() {
    local proto_dir="$1"
    local output_dir="$2"
    local proto_file="$3"
    local dry_run="$4"
    local verbose="$5"
    
    echo -e "${CYAN}生成C#代码...${NC}"
    
    if [ "$verbose" = "true" ]; then
        echo "Proto目录: $proto_dir"
        echo "输出目录: $output_dir"
        echo "Proto文件: $proto_file"
    fi
    
    create_directories "$output_dir"
    
    if [ -n "$proto_file" ]; then
        # 处理单个文件
        local cmd="$PROTOC_CMD --proto_path=$proto_dir --csharp_out=$output_dir $proto_file"
        if [ "$dry_run" = "true" ]; then
            echo "命令: $cmd"
        else
            if $cmd; then
                echo -e "${GREEN}✓ C#代码生成成功: $proto_file${NC}"
            else
                echo -e "${RED}✗ C#代码生成失败: $proto_file${NC}"
                return 1
            fi
        fi
    else
        # 处理所有proto文件
        local cmd="$PROTOC_CMD --proto_path=$proto_dir --csharp_out=$output_dir *.proto"
        if [ "$dry_run" = "true" ]; then
            echo "命令: $cmd"
        else
            if $cmd; then
                echo -e "${GREEN}✓ C#代码生成成功${NC}"
            else
                echo -e "${RED}✗ C#代码生成失败${NC}"
                return 1
            fi
        fi
    fi
}

# 测试proto文件语法
test_proto_files() {
    local proto_dir="$1"
    local proto_file="$2"
    local verbose="$3"
    
    echo -e "${CYAN}测试proto文件语法...${NC}"
    
    if [ -n "$proto_file" ]; then
        # 测试单个文件
        local cmd="$PROTOC_CMD --proto_path=$proto_dir --cpp_out=/tmp $proto_file"
        if $cmd >/dev/null 2>&1; then
            echo -e "${GREEN}✓ 语法正确: $proto_file${NC}"
        else
            echo -e "${RED}✗ 语法错误: $proto_file${NC}"
            return 1
        fi
    else
        # 测试所有文件
        local has_errors=false
        for file in "$proto_dir"/*.proto; do
            if [ -f "$file" ]; then
                local filename=$(basename "$file")
                local cmd="$PROTOC_CMD --proto_path=$proto_dir --cpp_out=/tmp $filename"
                if $cmd >/dev/null 2>&1; then
                    echo -e "${GREEN}✓ 语法正确: $filename${NC}"
                else
                    echo -e "${RED}✗ 语法错误: $filename${NC}"
                    has_errors=true
                fi
            fi
        done
        
        if [ "$has_errors" = "true" ]; then
            return 1
        fi
    fi
}

# 监视文件变化
watch_proto_files() {
    local proto_dir="$1"
    local output_dir="$2"
    
    echo -e "${CYAN}开始监视proto文件变化...${NC}"
    echo "监视目录: $proto_dir"
    echo "输出目录: $output_dir"
    echo "按 Ctrl+C 停止监视"
    echo ""
    
    # 检查是否安装了fswatch
    if ! command -v fswatch >/dev/null 2>&1; then
        echo -e "${RED}错误: 需要安装fswatch来监视文件变化${NC}"
        echo "请运行: brew install fswatch"
        exit 1
    fi
    
    # 初始生成
    generate_csharp "$proto_dir" "$output_dir" "" "false" "false"
    
    # 监视文件变化
    fswatch -o "$proto_dir" | while read f; do
        echo -e "${YELLOW}检测到文件变化，重新生成...${NC}"
        generate_csharp "$proto_dir" "$output_dir" "" "false" "false"
        echo -e "${GREEN}✓ 重新生成完成${NC}"
        echo ""
    done
}

# 主函数
main() {
    # 检查protoc
    check_protoc
    
    # 默认参数
    local command=""
    local proto_dir="$UNITY_PROTO_DIR"
    local output_dir="$UNITY_GENERATED_DIR"
    local proto_file=""
    local dry_run=false
    local verbose=false
    
    # 解析参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            generate|clean|watch|test|help|version)
                command="$1"
                shift
                ;;
            -p|--proto)
                proto_dir="$2"
                shift 2
                ;;
            -o|--output)
                output_dir="$2"
                shift 2
                ;;
            -f|--file)
                proto_file="$2"
                shift 2
                ;;
            -d|--dry-run)
                dry_run=true
                shift
                ;;
            -v|--verbose)
                verbose=true
                shift
                ;;
            *)
                echo -e "${RED}未知参数: $1${NC}"
                show_help
                exit 1
                ;;
        esac
    done
    
    # 处理命令
    case "$command" in
        generate)
            if [ ! -d "$proto_dir" ]; then
                echo -e "${RED}错误: 找不到proto目录 $proto_dir${NC}"
                exit 1
            fi
            
            cd "$proto_dir"
            generate_csharp "." "$output_dir" "$proto_file" "$dry_run" "$verbose"
            ;;
        clean)
            clean_generated "$output_dir"
            ;;
        watch)
            if [ ! -d "$proto_dir" ]; then
                echo -e "${RED}错误: 找不到proto目录 $proto_dir${NC}"
                exit 1
            fi
            
            watch_proto_files "$proto_dir" "$output_dir"
            ;;
        test)
            if [ ! -d "$proto_dir" ]; then
                echo -e "${RED}错误: 找不到proto目录 $proto_dir${NC}"
                exit 1
            fi
            
            cd "$proto_dir"
            test_proto_files "." "$proto_file" "$verbose"
            ;;
        help)
            show_help
            ;;
        version)
            show_version
            ;;
        "")
            show_help
            ;;
        *)
            echo -e "${RED}未知命令: $command${NC}"
            show_help
            exit 1
            ;;
    esac
}

# 运行主函数
main "$@" 