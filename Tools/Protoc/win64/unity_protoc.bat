@echo off
setlocal enabledelayedexpansion

REM Unity项目专用的Protobuf编译器启动脚本
REM 适用于Windows系统，专门为Unity项目优化

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

REM Unity项目默认配置
REM 获取项目根目录
for %%i in ("%SCRIPT_DIR%..\..\..") do set "PROJECT_ROOT=%%~fi"
set "UNITY_SCRIPTS_DIR=%PROJECT_ROOT%\Assets\Scripts"
set "UNITY_PROTO_DIR=%PROJECT_ROOT%\protos"
set "UNITY_GENERATED_DIR=%UNITY_SCRIPTS_DIR%\Generated"

REM 检查protoc是否存在
if not exist "%PROTOC_PATH%" (
    echo %RED%错误: 找不到protoc编译器%NC%
    echo 期望路径: %PROTOC_PATH%
    exit /b 1
)

REM 显示帮助信息
if "%1"=="" goto show_help
if "%1"=="--help" goto show_help
if "%1"=="-h" goto show_help

REM 显示版本信息
if "%1"=="--version" goto show_version
if "%1"=="-v" goto show_version

REM 解析参数
set "command="
set "output_dir=%UNITY_GENERATED_DIR%"
set "proto_dir=%UNITY_PROTO_DIR%"
set "proto_file="
set "recursive=false"
set "clean=false"
set "dry_run=false"
set "verbose=false"

:parse_args
if "%1"=="" goto end_parse
if "%1"=="generate" set "command=generate" & shift & goto parse_args
if "%1"=="clean" set "command=clean" & shift & goto parse_args
if "%1"=="watch" set "command=watch" & shift & goto parse_args
if "%1"=="test" set "command=test" & shift & goto parse_args
if "%1"=="help" set "command=help" & shift & goto parse_args
if "%1"=="version" set "command=version" & shift & goto parse_args
if "%1"=="-p" set "proto_dir=%2" & shift & shift & goto parse_args
if "%1"=="--proto" set "proto_dir=%2" & shift & shift & goto parse_args
if "%1"=="-o" set "output_dir=%2" & shift & shift & goto parse_args
if "%1"=="--output" set "output_dir=%2" & shift & shift & goto parse_args
if "%1"=="-f" set "proto_file=%2" & shift & shift & goto parse_args
if "%1"=="--file" set "proto_file=%2" & shift & shift & goto parse_args
if "%1"=="-r" set "recursive=true" & shift & goto parse_args
if "%1"=="--recursive" set "recursive=true" & shift & goto parse_args
if "%1"=="-c" set "clean=true" & shift & goto parse_args
if "%1"=="--clean" set "clean=true" & shift & goto parse_args
if "%1"=="-d" set "dry_run=true" & shift & goto parse_args
if "%1"=="--dry-run" set "dry_run=true" & shift & goto parse_args
if "%1"=="-v" set "verbose=true" & shift & goto parse_args
if "%1"=="--verbose" set "verbose=true" & shift & goto parse_args
echo %RED%未知参数: %1%NC%
goto show_help

:end_parse

REM 处理命令
if "%command%"=="generate" goto do_generate
if "%command%"=="clean" goto do_clean
if "%command%"=="watch" goto do_watch
if "%command%"=="test" goto do_test
if "%command%"=="help" goto show_help
if "%command%"=="version" goto show_version
if "%command%"=="" goto show_help

echo %RED%未知命令: %command%%NC%
goto show_help

:show_help
echo %BLUE%Unity项目专用Protobuf编译器启动器%NC%
echo.
echo 用法: %~nx0 [命令] [选项]
echo.
echo 命令:
echo   generate    生成C#代码到Unity Scripts目录
echo   clean       清理生成的代码
echo   watch       监视proto文件变化并自动重新生成
echo   test        测试proto文件语法
echo   help        显示此帮助信息
echo   version     显示protoc版本
echo.
echo 选项:
echo   -p, --proto ^<目录^>    指定proto文件目录 (默认: %UNITY_PROTO_DIR%)
echo   -o, --output ^<目录^>   指定输出目录 (默认: %UNITY_GENERATED_DIR%)
echo   -f, --file ^<文件^>     指定单个proto文件
echo   -d, --dry-run         显示将要执行的命令但不执行
echo   -v, --verbose         详细输出
echo.
echo 示例:
echo   %~nx0 generate                    # 生成所有proto文件的C#代码
echo   %~nx0 generate -f message.proto   # 生成单个文件的C#代码
echo   %~nx0 clean                       # 清理生成的代码
echo   %~nx0 watch                       # 监视文件变化
echo   %~nx0 test                        # 测试proto文件语法
echo.
goto :eof

:show_version
echo %GREEN%Protoc版本信息:%NC%
%PROTOC_PATH% --version
echo.
echo %GREEN%Unity项目配置:%NC%
echo Proto目录: %UNITY_PROTO_DIR%
echo 输出目录: %UNITY_GENERATED_DIR%
echo Scripts目录: %UNITY_SCRIPTS_DIR%
goto :eof

:do_clean
call :clean_generated "%output_dir%"
goto :eof

:do_test
if not exist "%proto_dir%" (
    echo %RED%错误: 找不到proto目录 %proto_dir%%NC%
    exit /b 1
)
cd /d "%proto_dir%"
call :test_proto_files "." "%proto_file%" "%verbose%"
goto :eof

:do_watch
if not exist "%proto_dir%" (
    echo %RED%错误: 找不到proto目录 %proto_dir%%NC%
    exit /b 1
)
call :watch_proto_files "%proto_dir%" "%output_dir%"
goto :eof

:do_generate
if not exist "%proto_dir%" (
    echo %RED%错误: 找不到proto目录 %proto_dir%%NC%
    exit /b 1
)
cd /d "%proto_dir%"
call :generate_csharp "." "%output_dir%" "%proto_file%" "%dry_run%" "%verbose%"
goto :eof

:clean_generated
if exist "%~1" (
    echo %YELLOW%清理输出目录: %~1%NC%
    rmdir /s /q "%~1"
    echo %GREEN%✓ 清理完成%NC%
) else (
    echo %YELLOW%输出目录不存在: %~1%NC%
)
goto :eof

:create_directories
if not exist "%~1" (
    echo %YELLOW%创建输出目录: %~1%NC%
    mkdir "%~1"
)
if not exist "%UNITY_SCRIPTS_DIR%" (
    echo %YELLOW%创建Scripts目录: %UNITY_SCRIPTS_DIR%%NC%
    mkdir "%UNITY_SCRIPTS_DIR%"
)
goto :eof

:generate_csharp
echo %CYAN%生成C#代码...%NC%
if "%5"=="true" (
    echo Proto目录: %~1
    echo 输出目录: %~2
    echo Proto文件: %~3
)
call :create_directories "%~2"
if not "%~3"=="" (
    REM 处理单个文件
    set "cmd=%PROTOC_PATH% --proto_path=%~1 --csharp_out=%~2 %~3"
    if "%4"=="true" (
        echo 命令: %cmd%
    ) else (
        %cmd%
        if %ERRORLEVEL% EQU 0 (
            echo %GREEN%✓ C#代码生成成功: %~3%NC%
        ) else (
            echo %RED%✗ C#代码生成失败: %~3%NC%
            exit /b 1
        )
    )
) else (
    REM 处理所有proto文件
    set "cmd=%PROTOC_PATH% --proto_path=%~1 --csharp_out=%~2 *.proto"
    if "%4"=="true" (
        echo 命令: %cmd%
    ) else (
        %cmd%
        if %ERRORLEVEL% EQU 0 (
            echo %GREEN%✓ C#代码生成成功%NC%
        ) else (
            echo %RED%✗ C#代码生成失败%NC%
            exit /b 1
        )
    )
)
goto :eof

:test_proto_files
echo %CYAN%测试proto文件语法...%NC%
if not "%~2"=="" (
    REM 测试单个文件
    set "cmd=%PROTOC_PATH% --proto_path=%~1 --cpp_out=%TEMP% %~2"
    %cmd% >nul 2>&1
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ 语法正确: %~2%NC%
    ) else (
        echo %RED%✗ 语法错误: %~2%NC%
        exit /b 1
    )
) else (
    REM 测试所有文件
    set "has_errors=false"
    for %%f in ("%~1\*.proto") do (
        set "filename=%%~nxf"
        set "cmd=%PROTOC_PATH% --proto_path=%~1 --cpp_out=%TEMP% !filename!"
        !cmd! >nul 2>&1
        if !ERRORLEVEL! EQU 0 (
            echo %GREEN%✓ 语法正确: !filename!%NC%
        ) else (
            echo %RED%✗ 语法错误: !filename!%NC%
            set "has_errors=true"
        )
    )
    if "%has_errors%"=="true" exit /b 1
)
goto :eof

:watch_proto_files
echo %CYAN%开始监视proto文件变化...%NC%
echo 监视目录: %~1
echo 输出目录: %~2
echo 按 Ctrl+C 停止监视
echo.
echo %YELLOW%注意: Windows版本暂不支持自动监视功能%NC%
echo 请手动运行 generate 命令来重新生成代码
echo.
REM 初始生成
call :generate_csharp "%~1" "%~2" "" "false" "false"
goto :eof 