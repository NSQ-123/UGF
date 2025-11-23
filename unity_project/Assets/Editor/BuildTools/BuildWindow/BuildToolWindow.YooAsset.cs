using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset.Editor;

// @Author ：NiShiqiang
// @Created ：2025/11/15 10:33:01
namespace Game.Editor
{
    public partial class BuildToolWindow
    {
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }

        private string _buildPackage;
        private string _buildPipeline;
        private VisualElement _yooAssetContainer;
        private Dictionary<string, Type> _viewClassDic = new Dictionary<string, Type>(10);

        private void ShowYooAssetBuildView(VisualElement parentContainer)
        {
            // 检测构建包裹
            var packageNames = GetBuildPackageNames();
            if (packageNames.Count == 0)
            {
                var notFoundLabel = new Label { text = "Not found any package" };
                notFoundLabel.style.width = 100;
                _paramScrollView.Add(notFoundLabel);
                return;
            }
            YooAssetBuildPipelineInitialize();

            var packageNameContainer = new VisualElement
            {
                style =
                {
                    marginBottom = 10,
                    flexDirection = FlexDirection.Row, // 设置为水平布局
                    alignItems = Align.Center, // 子元素在交叉轴上居中对齐
                },
            };

            var packageLabel = new Label("构建包裹:") { style = { width = 100 } };
            var packageDropdownField = new DropdownField()
            {
                style =
                {
                    maxWidth = 200,
                    //flexGrow = 1, // 字段占据剩余宽度
                    marginBottom = 10,
                },
            };
            packageDropdownField.choices = packageNames;
            packageDropdownField.index = 0;
            packageDropdownField.RegisterValueChangedCallback(evt =>
            {
                _buildPackage = evt.newValue;
                Debug.Log("构建包裹: " + evt.newValue);
            });

            packageNameContainer.Add(packageLabel);
            packageNameContainer.Add(packageDropdownField);
            parentContainer.Add(packageNameContainer);

            _buildPackage = packageNames[0];

            _buildPipeline = AssetBundleBuilderSetting.GetPackageBuildPipeline(_buildPackage);

            var buildPipelineNameContainer = new VisualElement
            {
                style =
                {
                    marginBottom = 10,
                    flexDirection = FlexDirection.Row, // 设置为水平布局
                    alignItems = Align.Center, // 子元素在交叉轴上居中对齐
                },
            };
            var buildPipelineLabel = new Label("构建管线:") { style = { width = 100 } };
            var buildPipelineDropdownField = new DropdownField()
            {
                style =
                {
                    maxWidth = 200,
                    //flexGrow = 1, // 字段占据剩余宽度
                    marginBottom = 10,
                },
            };
            buildPipelineDropdownField.choices = new List<string>(_viewClassDic.Keys);
            buildPipelineDropdownField.value = _buildPipeline;
            buildPipelineDropdownField.RegisterValueChangedCallback(evt =>
            {
                _buildPipeline = evt.newValue;
                Debug.Log("构建管线: " + evt.newValue);
            });

            buildPipelineNameContainer.Add(buildPipelineLabel);
            buildPipelineNameContainer.Add(buildPipelineDropdownField);
            parentContainer.Add(buildPipelineNameContainer);

            CreateYooAssetBuildView(parentContainer);
        }

        private void YooAssetBuildPipelineInitialize()
        {
            var viewerClassTypes = EditorTools.GetAssignableTypes(typeof(BuildPipelineViewerBase));
            foreach (var classType in viewerClassTypes)
            {
                var buildPipelineAttribute = EditorTools.GetAttribute<BuildPipelineAttribute>(
                    classType
                );
                if (buildPipelineAttribute == null)
                {
                    Debug.LogWarning(
                        $"The class {classType.FullName} need attribute {nameof(BuildPipelineAttribute)}"
                    );
                    continue;
                }

                string pipelineName = buildPipelineAttribute.PipelineName;
                if (_viewClassDic.ContainsKey(pipelineName))
                {
                    Debug.LogWarning($"The pipeline has already exist : {pipelineName}");
                }
                else
                {
                    _viewClassDic.Add(pipelineName, classType);
                }
            }
        }

        private void CreateYooAssetBuildView(VisualElement parentContainer)
        {
            if (_viewClassDic.TryGetValue(_buildPipeline, out Type value))
            {
                var buildTarget = _buildTargetField.value;
                var viewer = Activator.CreateInstance(value) as BuildPipelineViewerBase;
                viewer.InitView(_buildPackage, _buildPipeline, (BuildTarget)buildTarget);
                viewer.CreateView(parentContainer);
            }
            else
            {
                Debug.LogError($"Not found build pipeline : {_buildPipeline}");
            }
        }
    }
}
