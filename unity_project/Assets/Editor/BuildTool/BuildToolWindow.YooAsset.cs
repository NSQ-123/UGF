using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using YooAsset;
using YooAsset.Editor;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// YooAsset构建相关功能
    /// </summary>
    public partial class BuildToolWindow
    {
        private void OnBuildYooAsset()
        {
            if (_isBuilding)
            {
                Log("正在构建中，请等待...");
                return;
            }

            // 验证参数
            string packageName = GetPackageName();
            if (string.IsNullOrEmpty(packageName))
            {
                LogError("Package名称不能为空！");
                return;
            }

            string packageVersion = GetPackageVersion();
            if (string.IsNullOrEmpty(packageVersion))
            {
                LogError("Package版本不能为空！");
                return;
            }

            EditorApplication.delayCall += () =>
            {
                try
                {
                    _isBuilding = true;
                    UpdateButtonStates(false);

                    // 保存YooAsset设置
                    SaveYooAssetSettings();

                    BuildTarget target = GetBuildTarget();
                    EBuildPipeline buildPipeline = GetBuildPipeline();

                    Log($"开始构建 YooAsset Package: {packageName}...");
                    Log(
                        $"构建参数: Version={packageVersion}, Target={target}, Pipeline={buildPipeline}"
                    );
                    UpdateProgress(0.1f, "准备构建参数...");

                    // 构建YooAsset资源包
                    BuildYooAssetDirectly(packageName, packageVersion, target, buildPipeline);
                }
                catch (Exception e)
                {
                    LogError($"YooAsset 构建失败: {e.Message}");
                    LogError($"详细错误: {e.StackTrace}");
                    UpdateProgress(0, $"构建失败: {e.Message}");
                }
                finally
                {
                    _isBuilding = false;
                    UpdateButtonStates(true);
                }
            };
        }

        private void BuildYooAssetDirectly(
            string packageName,
            string packageVersion,
            BuildTarget target,
            EBuildPipeline buildPipeline
        )
        {
            try
            {
                UpdateProgress(0.2f, "创建构建参数...");

                // 获取构建参数
                ECompressOption compressOption = GetCompressOption();
                EFileNameStyle fileNameStyle = GetFileNameStyle();
                EBuildinFileCopyOption buildinFileCopyOption = GetBuildinFileCopyOption();
                string buildinFileCopyParams = GetBuildinFileCopyParams();
                bool clearBuildCache = GetClearBuildCache();
                bool useAssetDependencyDB = GetUseAssetDependencyDB();

                // 根据构建管线类型创建对应的构建参数
                BuildParameters buildParameters = null;
                IBuildPipeline pipeline = null;

                if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
                {
                    ScriptableBuildParameters scriptableParams = new ScriptableBuildParameters();
                    scriptableParams.BuildOutputRoot =
                        AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    scriptableParams.BuildinFileRoot =
                        AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                    scriptableParams.BuildPipeline = buildPipeline.ToString();
                    scriptableParams.BuildBundleType = (int)EBuildBundleType.AssetBundle;
                    scriptableParams.BuildTarget = target;
                    scriptableParams.PackageName = packageName;
                    scriptableParams.PackageVersion = packageVersion;
                    scriptableParams.EnableSharePackRule = GetEnableSharePackRule();
                    scriptableParams.VerifyBuildingResult = GetVerifyBuildingResult();
                    scriptableParams.SingleReferencedPackAlone = GetSingleReferencedPackAlone();
                    scriptableParams.FileNameStyle = fileNameStyle;
                    scriptableParams.BuildinFileCopyOption = buildinFileCopyOption;
                    scriptableParams.BuildinFileCopyParams = buildinFileCopyParams;
                    scriptableParams.CompressOption = compressOption;
                    scriptableParams.ClearBuildCacheFiles = clearBuildCache;
                    scriptableParams.UseAssetDependencyDB = useAssetDependencyDB;
                    scriptableParams.StripUnityVersion = GetStripUnityVersion();
                    scriptableParams.DisableWriteTypeTree = GetDisableWriteTypeTree();
                    scriptableParams.IgnoreTypeTreeChanges = GetIgnoreTypeTreeChanges();
                    scriptableParams.ReplaceAssetPathWithAddress = GetReplaceAssetPathWithAddress();
                    scriptableParams.TrackSpriteAtlasDependencies =
                        GetTrackSpriteAtlasDependencies();
                    scriptableParams.WriteLinkXML = GetWriteLinkXML();
                    scriptableParams.CacheServerHost = GetCacheServerHost();
                    scriptableParams.CacheServerPort = GetCacheServerPort();
                    scriptableParams.EncryptionServices = CreateEncryptionServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );
                    scriptableParams.ManifestProcessServices =
                        CreateManifestProcessServicesInstance(
                            packageName,
                            buildPipeline.ToString()
                        );
                    scriptableParams.ManifestRestoreServices =
                        CreateManifestRestoreServicesInstance(
                            packageName,
                            buildPipeline.ToString()
                        );

                    // 获取内置着色器资源包名称
                    var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
                    var shadersPackRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
                    scriptableParams.BuiltinShadersBundleName = shadersPackRuleResult.GetBundleName(
                        packageName,
                        uniqueBundleName
                    );

                    // 获取Mono脚本资源包名称
                    var monosPackRuleResult = DefaultPackRule.CreateMonosPackRuleResult();
                    scriptableParams.MonoScriptsBundleName = monosPackRuleResult.GetBundleName(
                        packageName,
                        uniqueBundleName
                    );

                    buildParameters = scriptableParams;
                    pipeline = new ScriptableBuildPipeline();
                }
                else if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
                {
                    BuiltinBuildParameters builtinParams = new BuiltinBuildParameters();
                    builtinParams.BuildOutputRoot =
                        AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    builtinParams.BuildinFileRoot =
                        AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                    builtinParams.BuildPipeline = buildPipeline.ToString();
                    builtinParams.BuildBundleType = (int)EBuildBundleType.AssetBundle;
                    builtinParams.BuildTarget = target;
                    builtinParams.PackageName = packageName;
                    builtinParams.PackageVersion = packageVersion;
                    builtinParams.EnableSharePackRule = GetEnableSharePackRule();
                    builtinParams.VerifyBuildingResult = GetVerifyBuildingResult();
                    builtinParams.SingleReferencedPackAlone = GetSingleReferencedPackAlone();
                    builtinParams.FileNameStyle = fileNameStyle;
                    builtinParams.BuildinFileCopyOption = buildinFileCopyOption;
                    builtinParams.BuildinFileCopyParams = buildinFileCopyParams;
                    builtinParams.CompressOption = compressOption;
                    builtinParams.ClearBuildCacheFiles = clearBuildCache;
                    builtinParams.UseAssetDependencyDB = useAssetDependencyDB;
                    builtinParams.StripUnityVersion = GetStripUnityVersion();
                    builtinParams.DisableWriteTypeTree = GetDisableWriteTypeTree();
                    builtinParams.IgnoreTypeTreeChanges = GetIgnoreTypeTreeChanges();
                    builtinParams.ReplaceAssetPathWithAddress = GetReplaceAssetPathWithAddress();
                    builtinParams.EncryptionServices = CreateEncryptionServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );
                    builtinParams.ManifestProcessServices = CreateManifestProcessServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );
                    builtinParams.ManifestRestoreServices = CreateManifestRestoreServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );

                    buildParameters = builtinParams;
                    pipeline = new BuiltinBuildPipeline();
                }
                else if (buildPipeline == EBuildPipeline.RawFileBuildPipeline)
                {
                    RawFileBuildParameters rawFileParams = new RawFileBuildParameters();
                    rawFileParams.BuildOutputRoot =
                        AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    rawFileParams.BuildinFileRoot =
                        AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                    rawFileParams.BuildPipeline = buildPipeline.ToString();
                    rawFileParams.BuildBundleType = (int)EBuildBundleType.RawBundle;
                    rawFileParams.BuildTarget = target;
                    rawFileParams.PackageName = packageName;
                    rawFileParams.PackageVersion = packageVersion;
                    rawFileParams.EnableSharePackRule = GetEnableSharePackRule();
                    rawFileParams.VerifyBuildingResult = GetVerifyBuildingResult();
                    rawFileParams.SingleReferencedPackAlone = GetSingleReferencedPackAlone();
                    rawFileParams.FileNameStyle = fileNameStyle;
                    rawFileParams.BuildinFileCopyOption = buildinFileCopyOption;
                    rawFileParams.BuildinFileCopyParams = buildinFileCopyParams;
                    rawFileParams.ClearBuildCacheFiles = clearBuildCache;
                    rawFileParams.UseAssetDependencyDB = useAssetDependencyDB;
                    rawFileParams.EncryptionServices = CreateEncryptionServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );
                    rawFileParams.ManifestProcessServices = CreateManifestProcessServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );
                    rawFileParams.ManifestRestoreServices = CreateManifestRestoreServicesInstance(
                        packageName,
                        buildPipeline.ToString()
                    );

                    buildParameters = rawFileParams;
                    pipeline = new RawFileBuildPipeline();
                }
                else
                {
                    throw new Exception($"不支持的构建管线类型: {buildPipeline}");
                }

                // 检查构建参数
                try
                {
                    buildParameters.CheckBuildParameters();
                }
                catch (Exception e)
                {
                    throw new Exception($"构建参数验证失败: {e.Message}");
                }

                UpdateProgress(0.3f, "开始构建资源包...");
                Log(
                    $"构建参数: Package={packageName}, Version={packageVersion}, Target={target}, Pipeline={buildPipeline}"
                );
                Log($"压缩选项: {compressOption}, 文件名样式: {fileNameStyle}");
                Log(
                    $"高级参数: 剥离Unity版本={GetStripUnityVersion()}, 禁止写入类型树={GetDisableWriteTypeTree()}"
                );
                Log(
                    $"高级参数: 使用可寻址地址={GetReplaceAssetPathWithAddress()}, 启用共享资源打包={GetEnableSharePackRule()}"
                );
                if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
                {
                    Log(
                        $"SBP特有参数: 图集依赖={GetTrackSpriteAtlasDependencies()}, 生成LinkXML={GetWriteLinkXML()}"
                    );
                    string cacheHost = GetCacheServerHost();
                    int cachePort = GetCacheServerPort();
                    if (!string.IsNullOrEmpty(cacheHost) && cachePort > 0)
                    {
                        Log($"缓存服务器: {cacheHost}:{cachePort}");
                    }
                }

                // 执行构建
                UpdateProgress(0.4f, "执行构建流程...");
                BuildResult buildResult = pipeline.Run(buildParameters, true);

                if (buildResult.Success)
                {
                    UpdateProgress(1.0f, "YooAsset 构建完成！");
                    Log("==========================================");
                    Log("YooAsset 构建完成！");
                    Log($"输出目录: {buildResult.OutputPackageDirectory}");

                    // 输出构建统计信息
                    if (!string.IsNullOrEmpty(buildResult.OutputPackageDirectory))
                    {
                        try
                        {
                            var outputDir = new DirectoryInfo(buildResult.OutputPackageDirectory);
                            if (outputDir.Exists)
                            {
                                long totalSize = 0;
                                int fileCount = 0;
                                foreach (
                                    var file in outputDir.GetFiles("*", SearchOption.AllDirectories)
                                )
                                {
                                    totalSize += file.Length;
                                    fileCount++;
                                }
                                Log(
                                    $"构建统计: 文件数量={fileCount}, 总大小={FormatFileSize(totalSize)}"
                                );
                            }
                        }
                        catch
                        {
                            // 忽略统计信息获取失败
                        }
                    }

                    Log("==========================================");

                    // 打开输出目录
                    if (!string.IsNullOrEmpty(buildResult.OutputPackageDirectory))
                    {
                        EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
                    }
                }
                else
                {
                    throw new Exception($"构建失败: {buildResult.ErrorInfo}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"YooAsset构建失败: {e.Message}");
            }
        }

        private IEncryptionServices CreateEncryptionServicesInstance(
            string packageName,
            string buildPipeline
        )
        {
            var className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(
                packageName,
                buildPipeline
            );
            var classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IEncryptionServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        private IManifestProcessServices CreateManifestProcessServicesInstance(
            string packageName,
            string buildPipeline
        )
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(
                packageName,
                buildPipeline
            );
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestProcessServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        private IManifestRestoreServices CreateManifestRestoreServicesInstance(
            string packageName,
            string buildPipeline
        )
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(
                packageName,
                buildPipeline
            );
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestRestoreServices)Activator.CreateInstance(classType);
            else
                return null;
        }
    }
}

