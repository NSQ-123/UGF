@echo off
setlocal enabledelayedexpansion

REM 高级Protobuf编译器启动器
REM 适用于Windows系统，支持预设配置和批量处理

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

REM 默认配置
set "DEFAULT_OUTPUT_DIR=./generated"
set "DEFAULT_PROTO_DIR=./protos"

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
set "preset="
set "output_dir=%DEFAULT_OUTPUT_DIR%"
set "proto_dir=%DEFAULT_PROTO_DIR%"
set "proto_file="
set "recursive=false"
set "clean=false"
set "dry_run=false"

:parse_args
if "%1"=="" goto end_parse
if "%1"=="csharp" set "preset=csharp" & shift & goto parse_args
if "%1"=="cpp" set "preset=cpp" & shift & goto parse_args
if "%1"=="java" set "preset=java" & shift & goto parse_args
if "%1"=="python" set "preset=python" & shift & goto parse_args
if "%1"=="js" set "preset=js" & shift & goto parse_args
if "%1"=="go" set "preset=go" & shift & goto parse_args
if "%1"=="all" set "preset=all" & shift & goto parse_args
if "%1"=="-o" set "output_dir=%2" & shift & shift & goto parse_args
if "%1"=="--output" set "output_dir=%2" & shift & shift & goto parse_args
if "%1"=="-p" set "proto_dir=%2" & shift & shift & goto parse_args
if "%1"=="--proto" set "proto_dir=%2" & shift & shift & goto parse_args
if "%1"=="-f" set "proto_file=%2" & shift & shift & goto parse_args
if "%1"=="--file" set "proto_file=%2" & shift & shift & goto parse_args
if "%1"=="-r" set "recursive=true" & shift & goto parse_args
if "%1"=="--recursive" set "recursive=true" & shift & goto parse_args
if "%1"=="-c" set "clean=true" & shift & goto parse_args
if "%1"=="--clean" set "clean=true" & shift & goto parse_args
if "%1"=="-d" set "dry_run=true" & shift & goto parse_args
if "%1"=="--dry-run" set "dry_run=true" & shift & goto parse_args
echo %RED%未知参数: %1%NC%
goto show_help

:end_parse

REM 如果没有预设，显示帮助
if "%preset%"=="" goto show_help

REM 清理输出目录
if "%clean%"=="true" call :clean_output_dir "%output_dir%"

REM 处理单个文件
if not "%proto_file%"=="" (
    if not exist "%proto_file%" (
        echo %RED%错误: 找不到文件 %proto_file%%NC%
        exit /b 1
    )
    
    REM 切换到文件所在目录
    for %%F in ("%proto_file%") do set "file_dir=%%~dpF"
    for %%F in ("%proto_file%") do set "file_name=%%~nxF"
    cd /d "%file_dir%"
    
    call :process_single_file "%file_name%" "%output_dir%" "%preset%" "%dry_run%"
    exit /b %ERRORLEVEL%
)

REM 检查proto目录
if not exist "%proto_dir%" (
    echo %RED%错误: 找不到proto目录 %proto_dir%%NC%
    exit /b 1
)

REM 切换到proto目录
cd /d "%proto_dir%"

REM 根据预设生成代码
if "%preset%"=="csharp" call :generate_csharp "." "%output_dir%" "%dry_run%"
if "%preset%"=="cpp" call :generate_cpp "." "%output_dir%" "%dry_run%"
if "%preset%"=="java" call :generate_java "." "%output_dir%" "%dry_run%"
if "%preset%"=="python" call :generate_python "." "%output_dir%" "%dry_run%"
if "%preset%"=="js" call :generate_js "." "%output_dir%" "%dry_run%"
if "%preset%"=="go" call :generate_go "." "%output_dir%" "%dry_run%"
if "%preset%"=="all" call :generate_all "." "%output_dir%" "%dry_run%"

exit /b %ERRORLEVEL%

:show_help
echo %BLUE%高级Protobuf编译器启动器%NC%
echo.
echo 用法: %~nx0 [预设] [选项] ^<proto文件^>
echo.
echo 预设:
echo   csharp    生成C#代码 (Unity项目)
echo   cpp       生成C++代码
echo   java      生成Java代码
echo   python    生成Python代码
echo   js        生成JavaScript代码
echo   go        生成Go代码
echo   all       生成所有支持的代码
echo.
echo 选项:
echo   -h, --help              显示此帮助信息
echo   -v, --version           显示protoc版本
echo   -o, --output ^<目录^>      指定输出目录 (默认: %DEFAULT_OUTPUT_DIR%)
echo   -p, --proto ^<目录^>       指定proto文件目录 (默认: %DEFAULT_PROTO_DIR%)
echo   -f, --file ^<文件^>        指定单个proto文件
echo   -r, --recursive         递归处理子目录
echo   -c, --clean             清理输出目录
echo   -d, --dry-run           显示将要执行的命令但不执行
echo.
echo 示例:
echo   %~nx0 csharp                    # 生成C#代码
echo   %~nx0 csharp -o ./Scripts       # 生成C#代码到Scripts目录
echo   %~nx0 all -p ./protos -r        # 递归处理protos目录，生成所有代码
echo   %~nx0 cpp -f message.proto      # 处理单个文件
echo   %~nx0 -v                        # 显示版本信息
echo.
goto :eof

:show_version
echo %GREEN%Protoc版本信息:%NC%
%PROTOC_PATH% --version
echo.
echo %GREEN%可用插件:%NC%
%PROTOC_PATH% --help | findstr /C:"Available plugins:"
goto :eof

:clean_output_dir
if exist "%~1" (
    echo %YELLOW%清理输出目录: %~1%NC%
    rmdir /s /q "%~1"
)
goto :eof

:create_output_dir
if not exist "%~1" (
    echo %YELLOW%创建输出目录: %~1%NC%
    mkdir "%~1"
)
goto :eof

:generate_csharp
echo %CYAN%生成C#代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --csharp_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ C#代码生成成功%NC%
    ) else (
        echo %RED%✗ C#代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_cpp
echo %CYAN%生成C++代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --cpp_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ C++代码生成成功%NC%
    ) else (
        echo %RED%✗ C++代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_java
echo %CYAN%生成Java代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --java_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ Java代码生成成功%NC%
    ) else (
        echo %RED%✗ Java代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_python
echo %CYAN%生成Python代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --python_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ Python代码生成成功%NC%
    ) else (
        echo %RED%✗ Python代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_js
echo %CYAN%生成JavaScript代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --js_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ JavaScript代码生成成功%NC%
    ) else (
        echo %RED%✗ JavaScript代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_go
echo %CYAN%生成Go代码...%NC%
set "cmd=%PROTOC_PATH% --proto_path=%~1 --go_out=%~2"
if "%4"=="true" (
    echo 命令: %cmd% *.proto
) else (
    call :create_output_dir "%~2"
    %cmd% *.proto
    if %ERRORLEVEL% EQU 0 (
        echo %GREEN%✓ Go代码生成成功%NC%
    ) else (
        echo %RED%✗ Go代码生成失败%NC%
        exit /b 1
    )
)
goto :eof

:generate_all
echo %CYAN%生成所有支持的代码...%NC%
call :generate_csharp "%~1" "%~2/csharp" "%~3"
call :generate_cpp "%~1" "%~2/cpp" "%~3"
call :generate_java "%~1" "%~2/java" "%~3"
call :generate_python "%~1" "%~2/python" "%~3"
call :generate_js "%~1" "%~2/js" "%~3"
call :generate_go "%~1" "%~2/go" "%~3"
echo %GREEN%✓ 所有代码生成完成%NC%
goto :eof

:process_single_file
echo %CYAN%处理文件: %~1%NC%
if "%4"=="csharp" call :generate_csharp "." "%~2" "%~3"
if "%4"=="cpp" call :generate_cpp "." "%~2" "%~3"
if "%4"=="java" call :generate_java "." "%~2" "%~3"
if "%4"=="python" call :generate_python "." "%~2" "%~3"
if "%4"=="js" call :generate_js "." "%~2" "%~3"
if "%4"=="go" call :generate_go "." "%~2" "%~3"
goto :eof 