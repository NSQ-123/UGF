using System;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// HybridCLR构建相关功能（包括DLL复制）
    /// </summary>
    public partial class BuildToolWindow
    {
        private void OnBuildHybridCLR()
        {
            if (_isBuilding)
            {
                Log("正在构建中，请等待...");
                return;
            }

            EditorApplication.delayCall += () =>
            {
                try
                {
                    _isBuilding = true;
                    UpdateButtonStates(false);
                    Log("开始构建 HybridCLR...");
                    UpdateProgress(0.1f, "检查 HybridCLR 安装...");

                    var installer = new HybridCLR.Editor.Installer.InstallerController();
                    if (!installer.HasInstalledHybridCLR())
                    {
                        throw new BuildFailedException(
                            "HybridCLR 未安装，请通过菜单 'HybridCLR/Installer' 安装"
                        );
                    }

                    UpdateProgress(0.2f, "开始编译热更新DLL...");
                    BuildTarget target = GetBuildTarget();
                    bool development = GetDevelopmentBuild();

                    // 编译热更新DLL
                    CompileDllCommand.CompileDll(target, development);
                    Log("✓ 编译热更新DLL完成");
                    UpdateProgress(0.4f, "生成IL2CPP定义...");

                    // 生成IL2CPP版本相关的定义
                    Il2CppDefGeneratorCommand.GenerateIl2CppDef();
                    Log("✓ 生成IL2CPP定义完成");
                    UpdateProgress(0.5f, "生成Link.xml...");

                    // 扫描并生成Link.xml，防止代码被裁剪
                    LinkGeneratorCommand.GenerateLinkXml(target);
                    Log("✓ 生成Link.xml完成");
                    UpdateProgress(0.6f, "生成裁剪后的AOT DLL...");

                    // 生成裁剪后的AOT DLL（补充元数据的基础）
                    StripAOTDllCommand.GenerateStripedAOTDlls(target);
                    Log("✓ 生成裁剪后的AOT DLL完成");
                    UpdateProgress(0.7f, "生成桥接函数...");

                    // 生成解释器与AOT代码交互的桥接函数
                    MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper(
                        target
                    );
                    Log("✓ 生成桥接函数完成");
                    UpdateProgress(0.8f, "生成AOT泛型引用...");

                    // 扫描并生成AOT泛型引用提示
                    AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
                    Log("✓ 生成AOT泛型引用完成");

                    UpdateProgress(1.0f, "HybridCLR 构建完成！");
                    Log("==========================================");
                    Log("HybridCLR 构建完成！");
                    Log("==========================================");
                }
                catch (Exception e)
                {
                    LogError($"HybridCLR 构建失败: {e.Message}");
                    UpdateProgress(0, $"构建失败: {e.Message}");
                }
                finally
                {
                    _isBuilding = false;
                    UpdateButtonStates(true);
                }
            };
        }

        /// <summary>
        /// 复制DLL到目标目录（HybridCLR的附加功能）
        /// </summary>
        private void OnCopyDlls()
        {
            try
            {
                Log("开始复制DLL到目标目录...");
                UpdateProgress(0.1f, "准备复制DLL...");

                BuildTarget target = GetBuildTarget();
                var dstPath = Application.dataPath + "/Res/BundleRes/GameDll";

                // 确保目录存在
                if (!Directory.Exists(dstPath))
                {
                    Directory.CreateDirectory(dstPath);
                    Log($"创建目录: {dstPath}");
                }

                UpdateProgress(0.3f, "复制热更新DLL...");
                var hotUpdateAssemblyFiles = SettingsUtil.HotUpdateAssemblyFilesExcludePreserved;
                foreach (var file in hotUpdateAssemblyFiles)
                {
                    string hotUpdateDllSource =
                        $"{SettingsUtil.HybridCLRDataDir}/HotUpdateDlls/{target}/{file}";
                    if (File.Exists(hotUpdateDllSource))
                    {
                        string hotUpdateDllDest = Path.Combine(dstPath, $"{file}.bytes");
                        File.Copy(hotUpdateDllSource, hotUpdateDllDest, overwrite: true);
                        Log($"✓ 复制热更新DLL: {file}");
                    }
                }

                UpdateProgress(0.6f, "复制AOT元数据DLL...");
                string aotDllsSourceDir =
                    $"{SettingsUtil.HybridCLRDataDir}/StrippedAOTAssembly2/{target}";
                //TODO: 获取所有AOT dll
                var aotDllsToCopy = new[] { "mscorlib.dll", "System.dll", "System.Core.dll" };

                foreach (string dllName in aotDllsToCopy)
                {
                    string srcPath = Path.Combine(aotDllsSourceDir, dllName);
                    if (File.Exists(srcPath))
                    {
                        string destPath = Path.Combine(dstPath, $"{dllName}.bytes");
                        File.Copy(srcPath, destPath, overwrite: true);
                        Log($"✓ 复制AOT DLL: {dllName}");
                    }
                }

                AssetDatabase.Refresh();
                UpdateProgress(1.0f, "DLL复制完成！");
                Log("==========================================");
                Log("DLL复制完成！");
                Log("==========================================");
            }
            catch (Exception e)
            {
                LogError($"复制DLL失败: {e.Message}");
                UpdateProgress(0, $"复制失败: {e.Message}");
            }
        }
    }
}
