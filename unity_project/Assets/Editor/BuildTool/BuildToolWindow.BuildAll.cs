using System;
using UnityEditor;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// 完整构建流程相关功能
    /// </summary>
    public partial class BuildToolWindow
    {
        private void OnBuildAll()
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
                    Log("==========================================");
                    Log("开始完整构建流程...");
                    Log("==========================================");

                    // 步骤1: 构建 HybridCLR
                    Log("\n[步骤 1/3] 构建 HybridCLR");
                    OnBuildHybridCLR();

                    // 等待一小段时间后执行下一步
                    EditorApplication.delayCall += () =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            // 步骤2: 复制DLL
                            Log("\n[步骤 2/3] 复制DLL到目标目录");
                            OnCopyDlls();

                            EditorApplication.delayCall += () =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    // 步骤3: 构建 YooAsset
                                    Log("\n[步骤 3/3] 构建 YooAsset");
                                    OnBuildYooAsset();

                                    EditorApplication.delayCall += () =>
                                    {
                                        Log("\n==========================================");
                                        Log("完整构建流程完成！");
                                        Log("==========================================");
                                        _isBuilding = false;
                                        UpdateButtonStates(true);
                                    };
                                };
                            };
                        };
                    };
                }
                catch (Exception e)
                {
                    LogError($"完整构建失败: {e.Message}");
                    _isBuilding = false;
                    UpdateButtonStates(true);
                }
            };
        }
    }
}

