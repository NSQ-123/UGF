@echo off
setlocal enabledelayedexpansion

REM Protobuf编译器启动脚本
REM 适用于Windows系统

REM 设置颜色输出
set "RED=[91m"
set "GREEN=[92m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "CYAN=[96m"
set "NC=[0m"

REM 获取脚本所在目录
set "SCRIPT_DIR=%~dp0"
set "PROTOC_PATH=%SCRIPT_DIR%protoc.exe"

REM 检查protoc是否存在
if not exist "%PROTOC_PATH%" (
    echo %RED%错误: 找不到protoc编译器%NC%
    echo 期望路径: %PROTOC_PATH%
    exit /b 1
)

REM 显示帮助信息
if "%1"=="--help" goto show_help
if "%1"=="-h" goto show_help
if "%1"=="" goto show_help

REM 显示版本信息
if "%1"=="--version" goto show_version
if "%1"=="-v" goto show_version

REM 执行protoc命令
echo %GREEN%执行protoc命令...%NC%
echo 命令: %PROTOC_PATH% %*
echo.

%PROTOC_PATH% %*
if %ERRORLEVEL% EQU 0 (
    echo %GREEN%✓ protoc执行成功%NC%
) else (
    echo %RED%✗ protoc执行失败%NC%
    exit /b 1
)
goto :eof

:show_help
echo %BLUE%Protobuf编译器启动脚本%NC%
echo.
echo 用法: %~nx0 [选项] ^<proto文件^>
echo.
echo 选项:
echo   -h, --help              显示此帮助信息
echo   -v, --version           显示protoc版本
echo   -o, --output ^<目录^>      指定输出目录
echo   -I, --proto_path ^<路径^>  指定proto文件搜索路径
echo   --cpp_out ^<目录^>         生成C++代码
echo   --csharp_out ^<目录^>      生成C#代码
echo   --java_out ^<目录^>        生成Java代码
echo   --python_out ^<目录^>      生成Python代码
echo   --js_out ^<目录^>          生成JavaScript代码
echo   --go_out ^<目录^>          生成Go代码
echo.
echo 示例:
echo   %~nx0 --cpp_out=./generated message.proto
echo   %~nx0 --csharp_out=./generated --proto_path=./protos *.proto
echo   %~nx0 -v
echo.
goto :eof

:show_version
echo %GREEN%Protoc版本信息:%NC%
%PROTOC_PATH% --version
echo.
echo %GREEN%可用插件:%NC%
%PROTOC_PATH% --help | findstr /C:"Available plugins:"
goto :eof 