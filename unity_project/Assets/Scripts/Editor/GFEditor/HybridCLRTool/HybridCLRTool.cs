using System.Collections;
using System.Collections.Generic;
using System.IO;
using GF.HybridCLR;
using HybridCLR.Editor;
using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// @Author ：NiShiqiang
// @Created ：2025/11/02 21:14:08
namespace Game.Editor.Tools.HybridCLRTool
{
    public class HybridCLRTool
    {
        /// <summary>
        /// 进一步剔除AOT dll中非泛型函数元数据，输出到StrippedAOTAssembly2目录下
        /// </summary>
        [MenuItem("HybridCLR/StripAOTAssembly", priority = 200)]
        public static void StripAOTAssembly()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            // 源目录：GenerateAll后生成的裁剪版AOT DLL所在目录
            string srcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            // 目标目录：存放处理后的精简版AOT DLL
            string dstDir = $"{SettingsUtil.HybridCLRDataDir}/StrippedAOTAssembly2/{target}";
            foreach (var src in Directory.GetFiles(srcDir, "*.dll"))
            {
                string dllName = Path.GetFileName(src);
                string dstFile = $"{dstDir}/{dllName}";
                AOTAssemblyMetadataStripper.Strip(src, dstFile);
            }
        }
        
        
        public static void GenerateAll()
        {
            //PrebuildCommand.GenerateAll();//等同于这个函数
            var installer = new HybridCLR.Editor.Installer.InstallerController();
            if (!installer.HasInstalledHybridCLR())
            {
                throw new BuildFailedException($"You have not initialized HybridCLR, please install it via menu 'HybridCLR/Installer'");
            }
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            // 编译热更新DLL
            CompileDllCommand.CompileDll(target, EditorUserBuildSettings.development);
            // 生成IL2CPP版本相关的定义
            Il2CppDefGeneratorCommand.GenerateIl2CppDef();
            // 扫描并生成Link.xml，防止代码被裁剪
            LinkGeneratorCommand.GenerateLinkXml(target);
            // 生成裁剪后的AOT DLL（补充元数据的基础）
            StripAOTDllCommand.GenerateStripedAOTDlls(target);
            // 生成解释器与AOT代码交互的桥接函数
            MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper(target);
            // 扫描并生成AOT泛型引用提示
            AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
        }
        [MenuItem("HybridCLR/CopyDllsToDstDir", priority = 200)]
        public static void CopyDllsToDstDir()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            var dstPath =Application.dataPath + "/" + "Res/BundleRes/GameDll";
            
            // 确保StreamingAssets目录存在
            if (!Directory.Exists(dstPath))
            {
                Directory.CreateDirectory(dstPath);
            }

            var hotUpdateAssemblyFiles = SettingsUtil.HotUpdateAssemblyFilesExcludePreserved;
            foreach (var file in hotUpdateAssemblyFiles)
            {
                // 拷贝热更新DLL
                string hotUpdateDllSource = $"{SettingsUtil.HybridCLRDataDir}/HotUpdateDlls/{target}/{file}";
                string hotUpdateDllDest = Path.Combine(dstPath, $"{file}.bytes");
                File.Copy(hotUpdateDllSource, hotUpdateDllDest, overwrite: true);
            }
            
            // 拷贝补充元数据DLL（这里以mscorlib.dll为例，实际应拷贝所有需要的DLL）
            // 注意：选择使用精简后的DLL目录还是原始的裁剪DLL目录
            string aotDllsSourceDir = $"{SettingsUtil.HybridCLRDataDir}/StrippedAOTAssembly2/{target}"; // 或使用 GetAssembliesPostIl2CppStripDir
            var aotDllsToCopy = HybridCLRLoadDll.AOTMetaAssemblyFiles; 

            foreach (string dllName in aotDllsToCopy)
            {
                string srcPath = Path.Combine(aotDllsSourceDir, dllName);
                if (File.Exists(srcPath))
                {
                    string destPath = Path.Combine(dstPath, $"{dllName}.bytes");
                    File.Copy(srcPath, destPath, overwrite: true);
                }
            }

            AssetDatabase.Refresh(); // 刷新Unity资源数据库，让新文件生效
        }
        
        
    }
}