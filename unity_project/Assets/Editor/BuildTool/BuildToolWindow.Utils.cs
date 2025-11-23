using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// 辅助方法相关功能
    /// </summary>
    public partial class BuildToolWindow
    {
        private BuildTarget GetBuildTarget()
        {
            if (_buildTargetField != null && _buildTargetField.value != null)
            {
                return (BuildTarget)_buildTargetField.value;
            }
            return EditorUserBuildSettings.activeBuildTarget;
        }

        private bool GetDevelopmentBuild()
        {
            if (_developmentBuildToggle != null)
            {
                return _developmentBuildToggle.value;
            }
            return EditorUserBuildSettings.development;
        }

        private string GetPackageName()
        {
            if (_packageNameField != null && !string.IsNullOrEmpty(_packageNameField.value))
            {
                return _packageNameField.value;
            }
            return "HotUpdateTest";
        }

        private string GetPackageVersion()
        {
            if (_packageVersionField != null && !string.IsNullOrEmpty(_packageVersionField.value))
            {
                return _packageVersionField.value;
            }
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        private EBuildPipeline GetBuildPipeline()
        {
            if (_buildPipelineField != null && _buildPipelineField.value != null)
            {
                return (EBuildPipeline)_buildPipelineField.value;
            }
            return EBuildPipeline.ScriptableBuildPipeline;
        }

        private ECompressOption GetCompressOption()
        {
            if (_compressOptionField != null && _compressOptionField.value != null)
            {
                return (ECompressOption)_compressOptionField.value;
            }
            return ECompressOption.LZ4;
        }

        private EFileNameStyle GetFileNameStyle()
        {
            if (_fileNameStyleField != null && _fileNameStyleField.value != null)
            {
                return (EFileNameStyle)_fileNameStyleField.value;
            }
            return EFileNameStyle.HashName;
        }

        private EBuildinFileCopyOption GetBuildinFileCopyOption()
        {
            if (_buildinFileCopyOptionField != null && _buildinFileCopyOptionField.value != null)
            {
                return (EBuildinFileCopyOption)_buildinFileCopyOptionField.value;
            }
            return EBuildinFileCopyOption.None;
        }

        private string GetBuildinFileCopyParams()
        {
            if (_buildinFileCopyParamsField != null)
            {
                return _buildinFileCopyParamsField.value ?? string.Empty;
            }
            return string.Empty;
        }

        private bool GetClearBuildCache()
        {
            if (_clearBuildCacheToggle != null)
            {
                return _clearBuildCacheToggle.value;
            }
            return false;
        }

        private bool GetUseAssetDependencyDB()
        {
            if (_useAssetDependencyDBToggle != null)
            {
                return _useAssetDependencyDBToggle.value;
            }
            return false;
        }

        private bool GetStripUnityVersion()
        {
            if (_stripUnityVersionToggle != null)
            {
                return _stripUnityVersionToggle.value;
            }
            return false;
        }

        private bool GetDisableWriteTypeTree()
        {
            if (_disableWriteTypeTreeToggle != null)
            {
                return _disableWriteTypeTreeToggle.value;
            }
            return false;
        }

        private bool GetIgnoreTypeTreeChanges()
        {
            if (_ignoreTypeTreeChangesToggle != null)
            {
                return _ignoreTypeTreeChangesToggle.value;
            }
            return true;
        }

        private bool GetReplaceAssetPathWithAddress()
        {
            if (_replaceAssetPathWithAddressToggle != null)
            {
                return _replaceAssetPathWithAddressToggle.value;
            }
            return false;
        }

        private bool GetEnableSharePackRule()
        {
            if (_enableSharePackRuleToggle != null)
            {
                return _enableSharePackRuleToggle.value;
            }
            return true;
        }

        private bool GetVerifyBuildingResult()
        {
            if (_verifyBuildingResultToggle != null)
            {
                return _verifyBuildingResultToggle.value;
            }
            return true;
        }

        private bool GetSingleReferencedPackAlone()
        {
            if (_singleReferencedPackAloneToggle != null)
            {
                return _singleReferencedPackAloneToggle.value;
            }
            return true;
        }

        private bool GetTrackSpriteAtlasDependencies()
        {
            if (_trackSpriteAtlasDependenciesToggle != null)
            {
                return _trackSpriteAtlasDependenciesToggle.value;
            }
            return false;
        }

        private bool GetWriteLinkXML()
        {
            if (_writeLinkXMLToggle != null)
            {
                return _writeLinkXMLToggle.value;
            }
            return true;
        }

        private string GetCacheServerHost()
        {
            if (_cacheServerHostField != null && !string.IsNullOrEmpty(_cacheServerHostField.value))
            {
                return _cacheServerHostField.value;
            }
            return string.Empty;
        }

        private int GetCacheServerPort()
        {
            if (_cacheServerPortField != null)
            {
                return _cacheServerPortField.value;
            }
            return 0;
        }

        private void UpdateProgress(float value, string title)
        {
            if (_progressBar != null)
            {
                _progressBar.value = value;
                _progressBar.title = title;
            }
        }

        private void UpdateButtonStates(bool enabled)
        {
            if (_buildHybridCLRBtn != null)
            {
                _buildHybridCLRBtn.SetEnabled(enabled);
            }
            if (_buildYooAssetBtn != null)
            {
                _buildYooAssetBtn.SetEnabled(enabled);
            }
            if (_buildAllBtn != null)
            {
                _buildAllBtn.SetEnabled(enabled);
            }
            if (_copyDllsBtn != null)
            {
                _copyDllsBtn.SetEnabled(enabled);
            }
        }

        private void Log(string message)
        {
            _currentLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            if (_logLabel != null)
            {
                _logLabel.text = _currentLog;
            }

            // 自动滚动到底部
            if (_logScrollView != null)
            {
                EditorApplication.delayCall += () =>
                {
                    _logScrollView.scrollOffset = new Vector2(0, float.MaxValue);
                };
            }

            Debug.Log($"[BuildTool] {message}");
        }

        private void LogError(string message)
        {
            _currentLog += $"[{DateTime.Now:HH:mm:ss}] <color=red>{message}</color>\n";
            if (_logLabel != null)
            {
                _logLabel.text = _currentLog;
            }

            if (_logScrollView != null)
            {
                EditorApplication.delayCall += () =>
                {
                    _logScrollView.scrollOffset = new Vector2(0, float.MaxValue);
                };
            }

            Debug.LogError($"[BuildTool] {message}");
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

