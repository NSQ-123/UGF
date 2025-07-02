@echo off
setlocal enabledelayedexpansion

REM Protobuf启动程序使用示例
REM 展示如何使用不同的启动程序

echo === Protobuf启动程序使用示例 ===
echo.

REM 显示版本信息
echo 1. 显示protoc版本信息：
echo    run_protoc.bat --version
echo    protoc_launcher.bat -v
echo    unity_protoc.bat version
echo.

REM 基础用法
echo 2. 基础用法：
echo    REM 生成C#代码
echo    run_protoc.bat --csharp_out=./generated example.proto
echo.

REM 高级启动器用法
echo 3. 高级启动器用法：
echo    REM 生成C#代码
echo    protoc_launcher.bat csharp
echo.
echo    REM 生成所有语言的代码
echo    protoc_launcher.bat all
echo.
echo    REM 指定输出目录
echo    protoc_launcher.bat csharp -o ./Assets/Scripts/Generated
echo.

REM Unity专用启动器用法
echo 4. Unity专用启动器用法：
echo    REM 生成C#代码到Unity Scripts目录
echo    unity_protoc.bat generate
echo.
echo    REM 测试proto文件语法
echo    unity_protoc.bat test
echo.
echo    REM 清理生成的代码
echo    unity_protoc.bat clean
echo.
echo    REM 监视文件变化并自动重新生成
echo    unity_protoc.bat watch
echo.

REM 实际运行示例
echo === 实际运行示例 ===
echo.

echo 显示protoc版本：
call run_protoc.bat --version
echo.

echo 测试proto文件语法：
call unity_protoc.bat test
echo.

echo 生成C#代码到Unity Scripts目录：
call unity_protoc.bat generate
echo.

echo 检查生成的代码：
if exist "..\..\..\Assets\Scripts\Generated\Example.cs" (
    echo ✓ 成功生成C#代码：..\..\..\Assets\Scripts\Generated\Example.cs
    for %%A in ("..\..\..\Assets\Scripts\Generated\Example.cs") do echo    文件大小: %%~zA bytes
) else (
    echo ✗ 未找到生成的代码文件
)
echo.

echo === 使用建议 ===
echo.
echo 1. 对于Unity项目，推荐使用 unity_protoc.bat
echo 2. 对于多语言项目，推荐使用 protoc_launcher.bat
echo 3. 对于简单的一次性任务，可以使用 run_protoc.bat
echo.
echo 4. 开发时需要手动重新生成：
echo    unity_protoc.bat generate
echo.
echo 5. 在提交代码前记得清理和重新生成：
echo    unity_protoc.bat clean
echo    unity_protoc.bat generate
echo.

pause 