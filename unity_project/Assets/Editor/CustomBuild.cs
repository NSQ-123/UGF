using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

// @Author ：NiShiqiang
// @Created ：2025/11/17 22:36:55
namespace Game.BuildTools
{
    public class CustomBuild
    {
        // 从构建命令里获取参数示例
        private static string GetBuildPackageName()
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("buildPackage"))
                    return arg.Split("="[0])[1];
            }
            return string.Empty;
        }

        [MenuItem("BuildTools/TestBuild")]
        public static void TestBuild()
        {
            Build(
                "HotUpdateTest",
                "1.0.0.test",
                BuildTarget.StandaloneWindows64,
                "ScriptableBuildPipeline",
                true,
                true
            );
        }

        public static void Build(
            string packageName,
            string buildVersion,
            BuildTarget buildTarget,
            string pipelineName,
            bool clearBuildCache = false,
            bool useAssetDependencyDB = true
        )
        {
            // 构建资源包

            ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = pipelineName;
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = buildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = buildVersion;
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.BundleName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
            buildParameters.BuildinFileCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = clearBuildCache;
            buildParameters.UseAssetDependencyDB = useAssetDependencyDB;
            buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
            buildParameters.ManifestProcessServices = CreateManifestProcessServicesInstance();
            buildParameters.ManifestRestoreServices = CreateManifestRestoreServicesInstance();
            buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName);
            buildParameters.MonoScriptsBundleName = GetMonoScriptsBundleName(packageName);
            buildParameters.EnableSharePackRule = false;
            buildParameters.StripUnityVersion = true;
            buildParameters.ReplaceAssetPathWithAddress = true;

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }

        /// <summary>
        /// 创建资源包加密服务类实例
        /// </summary>
        protected static IEncryptionServices CreateEncryptionServicesInstance()
        {
            return null;
        }

        /// <summary>
        /// 创建资源清单加密服务类实例
        /// </summary>
        protected static IManifestProcessServices CreateManifestProcessServicesInstance()
        {
            return null;
        }

        /// <summary>
        /// 创建资源清单解密服务类实例
        /// </summary>
        protected static IManifestRestoreServices CreateManifestRestoreServicesInstance()
        {
            return null;
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        protected static string GetBuiltinShaderBundleName(
            string packageName,
            bool uniqueBundleName = true
        )
        {
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        /// <summary>
        /// Mono脚本的资源包名称
        /// </summary>
        protected static string GetMonoScriptsBundleName(
            string packageName,
            bool uniqueBundleName = true
        )
        {
            var packRuleResult = DefaultPackRule.CreateMonosPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }
    }
}
