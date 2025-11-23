using UnityEditor;
using YooAsset;
using YooAsset.Editor;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// 设置加载和保存相关功能
    /// </summary>
    public partial class BuildToolWindow
    {
        private void LoadYooAssetSettings()
        {
            string packageName = GetPackageName();
            string buildPipeline = GetBuildPipeline().ToString();

            if (_compressOptionField != null)
            {
                var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(
                    packageName,
                    buildPipeline
                );
                _compressOptionField.value = compressOption;
            }

            if (_fileNameStyleField != null)
            {
                var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(
                    packageName,
                    buildPipeline
                );
                _fileNameStyleField.value = fileNameStyle;
            }

            if (_buildinFileCopyOptionField != null)
            {
                var buildinFileCopyOption =
                    AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(
                        packageName,
                        buildPipeline
                    );
                _buildinFileCopyOptionField.value = buildinFileCopyOption;
            }

            if (_buildinFileCopyParamsField != null)
            {
                var buildinFileCopyParams =
                    AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(
                        packageName,
                        buildPipeline
                    );
                _buildinFileCopyParamsField.value = buildinFileCopyParams;
            }

            if (_clearBuildCacheToggle != null)
            {
                var clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(
                    packageName,
                    buildPipeline
                );
                _clearBuildCacheToggle.value = clearBuildCache;
            }

            if (_useAssetDependencyDBToggle != null)
            {
                var useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(
                    packageName,
                    buildPipeline
                );
                _useAssetDependencyDBToggle.value = useAssetDependencyDB;
            }

            // 加载高级参数（使用EditorPrefs）
            if (_stripUnityVersionToggle != null)
            {
                _stripUnityVersionToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_StripUnityVersion",
                    false
                );
            }

            if (_disableWriteTypeTreeToggle != null)
            {
                _disableWriteTypeTreeToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_DisableWriteTypeTree",
                    false
                );
            }

            if (_ignoreTypeTreeChangesToggle != null)
            {
                _ignoreTypeTreeChangesToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_IgnoreTypeTreeChanges",
                    true
                );
            }

            if (_replaceAssetPathWithAddressToggle != null)
            {
                _replaceAssetPathWithAddressToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_ReplaceAssetPathWithAddress",
                    false
                );
            }

            if (_enableSharePackRuleToggle != null)
            {
                _enableSharePackRuleToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_EnableSharePackRule",
                    true
                );
            }

            if (_verifyBuildingResultToggle != null)
            {
                _verifyBuildingResultToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_VerifyBuildingResult",
                    true
                );
            }

            if (_singleReferencedPackAloneToggle != null)
            {
                _singleReferencedPackAloneToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_SingleReferencedPackAlone",
                    true
                );
            }

            // ScriptableBuildPipeline特有参数
            if (_trackSpriteAtlasDependenciesToggle != null)
            {
                _trackSpriteAtlasDependenciesToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_TrackSpriteAtlasDependencies",
                    false
                );
            }

            if (_writeLinkXMLToggle != null)
            {
                _writeLinkXMLToggle.value = GetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_WriteLinkXML",
                    true
                );
            }

            if (_cacheServerHostField != null)
            {
                _cacheServerHostField.value = EditorPrefs.GetString(
                    $"{packageName}_{buildPipeline}_CacheServerHost",
                    ""
                );
            }

            if (_cacheServerPortField != null)
            {
                _cacheServerPortField.value = EditorPrefs.GetInt(
                    $"{packageName}_{buildPipeline}_CacheServerPort",
                    0
                );
            }
        }

        private bool GetEditorPrefsBool(string key, bool defaultValue)
        {
            return EditorPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
        }

        private void SetEditorPrefsBool(string key, bool value)
        {
            EditorPrefs.SetInt(key, value ? 1 : 0);
        }

        private void SaveYooAssetSettings()
        {
            string packageName = GetPackageName();
            string buildPipeline = GetBuildPipeline().ToString();

            if (_compressOptionField != null && _compressOptionField.value != null)
            {
                AssetBundleBuilderSetting.SetPackageCompressOption(
                    packageName,
                    buildPipeline,
                    (ECompressOption)_compressOptionField.value
                );
            }

            if (_fileNameStyleField != null && _fileNameStyleField.value != null)
            {
                AssetBundleBuilderSetting.SetPackageFileNameStyle(
                    packageName,
                    buildPipeline,
                    (EFileNameStyle)_fileNameStyleField.value
                );
            }

            if (_buildinFileCopyOptionField != null && _buildinFileCopyOptionField.value != null)
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyOption(
                    packageName,
                    buildPipeline,
                    (EBuildinFileCopyOption)_buildinFileCopyOptionField.value
                );
            }

            if (_buildinFileCopyParamsField != null)
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyParams(
                    packageName,
                    buildPipeline,
                    _buildinFileCopyParamsField.value
                );
            }

            if (_clearBuildCacheToggle != null)
            {
                AssetBundleBuilderSetting.SetPackageClearBuildCache(
                    packageName,
                    buildPipeline,
                    _clearBuildCacheToggle.value
                );
            }

            if (_useAssetDependencyDBToggle != null)
            {
                AssetBundleBuilderSetting.SetPackageUseAssetDependencyDB(
                    packageName,
                    buildPipeline,
                    _useAssetDependencyDBToggle.value
                );
            }

            // 保存高级参数到EditorPrefs
            if (_stripUnityVersionToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_StripUnityVersion",
                    _stripUnityVersionToggle.value
                );
            }

            if (_disableWriteTypeTreeToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_DisableWriteTypeTree",
                    _disableWriteTypeTreeToggle.value
                );
            }

            if (_ignoreTypeTreeChangesToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_IgnoreTypeTreeChanges",
                    _ignoreTypeTreeChangesToggle.value
                );
            }

            if (_replaceAssetPathWithAddressToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_ReplaceAssetPathWithAddress",
                    _replaceAssetPathWithAddressToggle.value
                );
            }

            if (_enableSharePackRuleToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_EnableSharePackRule",
                    _enableSharePackRuleToggle.value
                );
            }

            if (_verifyBuildingResultToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_VerifyBuildingResult",
                    _verifyBuildingResultToggle.value
                );
            }

            if (_singleReferencedPackAloneToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_SingleReferencedPackAlone",
                    _singleReferencedPackAloneToggle.value
                );
            }

            // ScriptableBuildPipeline特有参数
            if (_trackSpriteAtlasDependenciesToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_TrackSpriteAtlasDependencies",
                    _trackSpriteAtlasDependenciesToggle.value
                );
            }

            if (_writeLinkXMLToggle != null)
            {
                SetEditorPrefsBool(
                    $"{packageName}_{buildPipeline}_WriteLinkXML",
                    _writeLinkXMLToggle.value
                );
            }

            if (_cacheServerHostField != null)
            {
                EditorPrefs.SetString(
                    $"{packageName}_{buildPipeline}_CacheServerHost",
                    _cacheServerHostField.value
                );
            }

            if (_cacheServerPortField != null)
            {
                EditorPrefs.SetInt(
                    $"{packageName}_{buildPipeline}_CacheServerPort",
                    _cacheServerPortField.value
                );
            }
        }
    }
}

