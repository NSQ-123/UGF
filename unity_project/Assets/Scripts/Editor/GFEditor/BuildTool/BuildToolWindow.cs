using System;
using System.Collections;
using System.IO;
using System.Linq;
using Game.Editor.Tools.HybridCLRTool;
using HybridCLR.Editor;
using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;
using YooAsset.Editor;

// @Author ：NiShiqiang
// @Created ：2025/01/XX
namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// 打包编辑器窗口 - 整合HybridCLR和YooAsset
    /// </summary>
    public class BuildToolWindow : EditorWindow
    {
        private VisualElement _root;
        private Button _buildHybridCLRBtn;
        private Button _buildYooAssetBtn;
        private Button _buildAllBtn;
        private Button _copyDllsBtn;
        private Label _logLabel;
        private ScrollView _logScrollView;
        private EnumField _buildTargetField;
        private Toggle _developmentBuildToggle;
        private TextField _packageNameField;
        private ProgressBar _progressBar;
        
        // YooAsset构建参数
        private TextField _packageVersionField;
        private EnumField _compressOptionField;
        private EnumField _fileNameStyleField;
        private EnumField _buildinFileCopyOptionField;
        private TextField _buildinFileCopyParamsField;
        private Toggle _clearBuildCacheToggle;
        private Toggle _useAssetDependencyDBToggle;
        private EnumField _buildPipelineField;
        
        // YooAsset高级构建参数
        private Toggle _stripUnityVersionToggle;
        private Toggle _disableWriteTypeTreeToggle;
        private Toggle _ignoreTypeTreeChangesToggle;
        private Toggle _replaceAssetPathWithAddressToggle;
        private Toggle _enableSharePackRuleToggle;
        private Toggle _verifyBuildingResultToggle;
        private Toggle _singleReferencedPackAloneToggle;
        
        // ScriptableBuildPipeline特有参数
        private Toggle _trackSpriteAtlasDependenciesToggle;
        private Toggle _writeLinkXMLToggle;
        private TextField _cacheServerHostField;
        private IntegerField _cacheServerPortField;
        
        private bool _isBuilding = false;
        private string _currentLog = "";

        [MenuItem("GF/BuildTool/打包编辑器", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildToolWindow>("打包编辑器");
            window.minSize = new Vector2(600, 500);
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;
            
            // 加载UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/Editor/GFEditor/BuildTool/BuildToolWindow.uxml");
            bool hasUXML = visualTree != null;
            
            if (hasUXML)
            {
                visualTree.CloneTree(_root);
                
                // 将configContainer包装在ScrollView中，避免内容超出窗口
                var configContainer = _root.Q<VisualElement>("configContainer");
                if (configContainer != null && configContainer.parent != null)
                {
                    var parent = configContainer.parent;
                    var index = parent.IndexOf(configContainer);
                    parent.Remove(configContainer);
                    
                    var scrollView = new ScrollView();
                    scrollView.style.flexGrow = 1;
                    scrollView.style.maxHeight = 400; // 限制最大高度
                    scrollView.Add(configContainer);
                    parent.Insert(index, scrollView);
                }
            }
            else
            {
                // 如果UXML不存在，使用代码创建UI
                CreateUIFromCode();
            }
            
            // 加载USS样式
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/Editor/GFEditor/BuildTool/BuildToolWindow.uss");
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }
            
            // 如果从UXML加载，需要添加YooAsset参数（UXML中没有这些参数）
            if (hasUXML)
            {
                AddYooAssetParametersToUI();
            }
            
            // 绑定UI元素
            BindUIElements();
            
            // 初始化UI
            InitializeUI();
        }
        
        private void AddYooAssetParametersToUI()
        {
            // 找到Package名称字段的容器或configContainer，在其后添加YooAsset参数
            var packageContainer = _root.Q<VisualElement>("packageContainer");
            var configContainer = _root.Q<VisualElement>("configContainer");
            var buttonContainer = _root.Q<VisualElement>("buttonContainer");
            
            VisualElement insertParent = configContainer ?? _root;
            VisualElement insertAfter = packageContainer;
            
            // 如果找不到packageContainer，尝试在configContainer的最后添加
            if (insertAfter == null && configContainer != null)
            {
                insertAfter = configContainer;
            }
            
            // 如果还是找不到，在buttonContainer之前添加
            if (insertAfter == null && buttonContainer != null && buttonContainer.parent != null)
            {
                insertParent = buttonContainer.parent;
                insertAfter = buttonContainer;
            }
            
            if (insertAfter == null)
            {
                // 如果找不到容器，直接添加到root
                insertParent = _root;
                insertAfter = null;
            }
            
            // Package版本
            var versionContainer = new VisualElement { style = { marginBottom = 10 } };
            versionContainer.Add(new Label("Package版本:") { style = { width = 100 } });
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            _packageVersionField = new TextField { value = DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes };
            _packageVersionField.style.width = 200;
            versionContainer.Add(_packageVersionField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, versionContainer);
                insertAfter = versionContainer;
            }
            else if (insertAfter != null)
            {
                insertParent.Add(versionContainer);
                insertAfter = versionContainer;
            }
            else
            {
                insertParent.Add(versionContainer);
                insertAfter = versionContainer;
            }
            
            // 构建管线
            var pipelineContainer = new VisualElement { style = { marginBottom = 10 } };
            pipelineContainer.Add(new Label("构建管线:") { style = { width = 100 } });
            _buildPipelineField = new EnumField(EBuildPipeline.ScriptableBuildPipeline);
            _buildPipelineField.style.width = 200;
            _buildPipelineField.RegisterValueChangedCallback(evt => OnBuildPipelineChanged());
            pipelineContainer.Add(_buildPipelineField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, pipelineContainer);
                insertAfter = pipelineContainer;
            }
            else if (insertAfter != null)
            {
                insertParent.Add(pipelineContainer);
                insertAfter = pipelineContainer;
            }
            else
            {
                insertParent.Add(pipelineContainer);
                insertAfter = pipelineContainer;
            }
            
            // 压缩选项
            var compressContainer = new VisualElement { style = { marginBottom = 10 } };
            compressContainer.Add(new Label("压缩选项:") { style = { width = 100 } });
            _compressOptionField = new EnumField(ECompressOption.LZ4);
            _compressOptionField.style.width = 200;
            compressContainer.Add(_compressOptionField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, compressContainer);
                insertAfter = compressContainer;
            }
            else
            {
                insertParent.Add(compressContainer);
                insertAfter = compressContainer;
            }
            
            // 文件名样式
            var fileNameStyleContainer = new VisualElement { style = { marginBottom = 10 } };
            fileNameStyleContainer.Add(new Label("文件名样式:") { style = { width = 100 } });
            _fileNameStyleField = new EnumField(EFileNameStyle.HashName);
            _fileNameStyleField.style.width = 200;
            fileNameStyleContainer.Add(_fileNameStyleField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, fileNameStyleContainer);
                insertAfter = fileNameStyleContainer;
            }
            else
            {
                insertParent.Add(fileNameStyleContainer);
                insertAfter = fileNameStyleContainer;
            }
            
            // 首包文件拷贝选项
            var buildinFileCopyOptionContainer = new VisualElement { style = { marginBottom = 10 } };
            buildinFileCopyOptionContainer.Add(new Label("首包文件拷贝:") { style = { width = 100 } });
            _buildinFileCopyOptionField = new EnumField(EBuildinFileCopyOption.None);
            _buildinFileCopyOptionField.style.width = 200;
            buildinFileCopyOptionContainer.Add(_buildinFileCopyOptionField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, buildinFileCopyOptionContainer);
                insertAfter = buildinFileCopyOptionContainer;
            }
            else
            {
                insertParent.Add(buildinFileCopyOptionContainer);
                insertAfter = buildinFileCopyOptionContainer;
            }
            
            // 首包文件拷贝参数（标签）
            var buildinFileCopyParamsContainer = new VisualElement { style = { marginBottom = 10 } };
            buildinFileCopyParamsContainer.Add(new Label("拷贝标签:") { style = { width = 100 } });
            _buildinFileCopyParamsField = new TextField { value = "" };
            _buildinFileCopyParamsField.style.width = 200;
            buildinFileCopyParamsContainer.Add(_buildinFileCopyParamsField);
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, buildinFileCopyParamsContainer);
                insertAfter = buildinFileCopyParamsContainer;
            }
            else
            {
                insertParent.Add(buildinFileCopyParamsContainer);
                insertAfter = buildinFileCopyParamsContainer;
            }
            
            // 清空构建缓存
            _clearBuildCacheToggle = new Toggle("清空构建缓存")
            {
                value = false
            };
            _clearBuildCacheToggle.style.marginBottom = 10;
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, _clearBuildCacheToggle);
                insertAfter = _clearBuildCacheToggle;
            }
            else
            {
                insertParent.Add(_clearBuildCacheToggle);
                insertAfter = _clearBuildCacheToggle;
            }
            
            // 使用资源依赖数据库
            _useAssetDependencyDBToggle = new Toggle("使用资源依赖数据库")
            {
                value = false
            };
            _useAssetDependencyDBToggle.style.marginBottom = 10;
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, _useAssetDependencyDBToggle);
                insertAfter = _useAssetDependencyDBToggle;
            }
            else
            {
                insertParent.Add(_useAssetDependencyDBToggle);
                insertAfter = _useAssetDependencyDBToggle;
            }
            
            // 高级构建参数 - 使用Foldout折叠
            var advancedFoldout = new Foldout
            {
                text = "高级构建参数",
                value = false
            };
            advancedFoldout.style.marginTop = 10;
            advancedFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            advancedFoldout.contentContainer.style.paddingLeft = 15;
            advancedFoldout.contentContainer.style.paddingTop = 5;
            advancedFoldout.contentContainer.style.paddingBottom = 5;
            
            // 从文件头里剥离Unity版本信息
            _stripUnityVersionToggle = new Toggle("剥离Unity版本信息")
            {
                value = false
            };
            _stripUnityVersionToggle.style.marginBottom = 5;
            advancedFoldout.Add(_stripUnityVersionToggle);
            
            // 禁止写入类型树结构
            _disableWriteTypeTreeToggle = new Toggle("禁止写入类型树结构（可降低包体和内存）")
            {
                value = false
            };
            _disableWriteTypeTreeToggle.style.marginBottom = 5;
            advancedFoldout.Add(_disableWriteTypeTreeToggle);
            
            // 忽略类型树变化
            _ignoreTypeTreeChangesToggle = new Toggle("忽略类型树变化")
            {
                value = true
            };
            _ignoreTypeTreeChangesToggle.style.marginBottom = 5;
            advancedFoldout.Add(_ignoreTypeTreeChangesToggle);
            
            // 使用可寻址地址代替资源路径
            _replaceAssetPathWithAddressToggle = new Toggle("使用可寻址地址代替资源路径（节省运行时内存）")
            {
                value = false
            };
            _replaceAssetPathWithAddressToggle.style.marginBottom = 5;
            advancedFoldout.Add(_replaceAssetPathWithAddressToggle);
            
            // 启用共享资源打包
            _enableSharePackRuleToggle = new Toggle("启用共享资源打包")
            {
                value = true
            };
            _enableSharePackRuleToggle.style.marginBottom = 5;
            advancedFoldout.Add(_enableSharePackRuleToggle);
            
            // 验证构建结果
            _verifyBuildingResultToggle = new Toggle("验证构建结果")
            {
                value = true
            };
            _verifyBuildingResultToggle.style.marginBottom = 5;
            advancedFoldout.Add(_verifyBuildingResultToggle);
            
            // 对单独引用的共享资源进行独立打包
            _singleReferencedPackAloneToggle = new Toggle("单独引用的共享资源独立打包")
            {
                value = true
            };
            _singleReferencedPackAloneToggle.style.marginBottom = 5;
            advancedFoldout.Add(_singleReferencedPackAloneToggle);
            
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, advancedFoldout);
                insertAfter = advancedFoldout;
            }
            else
            {
                insertParent.Add(advancedFoldout);
                insertAfter = advancedFoldout;
            }
            
            // ScriptableBuildPipeline特有参数容器 - 使用Foldout折叠
            var sbpFoldout = new Foldout
            {
                text = "ScriptableBuildPipeline 特有参数",
                value = false,
                name = "sbpContainer"
            };
            sbpFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            sbpFoldout.contentContainer.style.paddingLeft = 15;
            sbpFoldout.contentContainer.style.paddingTop = 5;
            sbpFoldout.contentContainer.style.paddingBottom = 5;
            
            // 自动建立资源对象对图集的依赖关系
            _trackSpriteAtlasDependenciesToggle = new Toggle("自动建立图集依赖关系")
            {
                value = false
            };
            _trackSpriteAtlasDependenciesToggle.style.marginBottom = 5;
            sbpFoldout.Add(_trackSpriteAtlasDependenciesToggle);
            
            // 生成代码防裁剪配置
            _writeLinkXMLToggle = new Toggle("生成代码防裁剪配置")
            {
                value = true
            };
            _writeLinkXMLToggle.style.marginBottom = 5;
            sbpFoldout.Add(_writeLinkXMLToggle);
            
            // 缓存服务器地址
            var cacheServerHostContainer = new VisualElement 
            { 
                style = 
                { 
                    marginBottom = 5,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                } 
            };
            cacheServerHostContainer.Add(new Label("缓存服务器地址:") { style = { width = 150 } });
            _cacheServerHostField = new TextField { value = "" };
            _cacheServerHostField.style.width = 200;
            cacheServerHostContainer.Add(_cacheServerHostField);
            sbpFoldout.Add(cacheServerHostContainer);
            
            // 缓存服务器端口
            var cacheServerPortContainer = new VisualElement 
            { 
                style = 
                { 
                    marginBottom = 5,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                } 
            };
            cacheServerPortContainer.Add(new Label("缓存服务器端口:") { style = { width = 150 } });
            _cacheServerPortField = new IntegerField { value = 0 };
            _cacheServerPortField.style.width = 200;
            cacheServerPortContainer.Add(_cacheServerPortField);
            sbpFoldout.Add(cacheServerPortContainer);
            
            if (insertAfter != null && insertAfter.parent != null)
            {
                var index = insertAfter.parent.IndexOf(insertAfter);
                insertAfter.parent.Insert(index + 1, sbpFoldout);
            }
            else
            {
                insertParent.Add(sbpFoldout);
            }
        }

        private void CreateUIFromCode()
        {
            _root.Clear();
            
            // 标题
            var title = new Label("打包编辑器 - HybridCLR & YooAsset")
            {
                style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
            };
            _root.Add(title);
            
            // 参数区域ScrollView
            var paramScrollView = new ScrollView();
            paramScrollView.style.flexGrow = 1;
            paramScrollView.style.maxHeight = 400; // 限制最大高度
            _root.Add(paramScrollView);
            
            // 构建目标选择
            var buildTargetContainer = new VisualElement { style = { marginBottom = 10 } };
            buildTargetContainer.Add(new Label("构建目标:") { style = { width = 100 } });
            _buildTargetField = new EnumField(EditorUserBuildSettings.activeBuildTarget);
            _buildTargetField.style.width = 200;
            buildTargetContainer.Add(_buildTargetField);
            paramScrollView.Add(buildTargetContainer);
            
            // 开发构建选项
            _developmentBuildToggle = new Toggle("开发构建")
            {
                value = EditorUserBuildSettings.development
            };
            _developmentBuildToggle.style.marginBottom = 10;
            paramScrollView.Add(_developmentBuildToggle);
            
            // Package名称
            var packageContainer = new VisualElement { style = { marginBottom = 10 } };
            packageContainer.Add(new Label("Package名称:") { style = { width = 100 } });
            _packageNameField = new TextField { value = "HotUpdateTest" };
            _packageNameField.style.width = 200;
            packageContainer.Add(_packageNameField);
            paramScrollView.Add(packageContainer);
            
            // Package版本
            var versionContainer = new VisualElement { style = { marginBottom = 10 } };
            versionContainer.Add(new Label("Package版本:") { style = { width = 100 } });
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            _packageVersionField = new TextField { value = DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes };
            _packageVersionField.style.width = 200;
            versionContainer.Add(_packageVersionField);
            paramScrollView.Add(versionContainer);
            
            // 构建管线
            var pipelineContainer = new VisualElement { style = { marginBottom = 10 } };
            pipelineContainer.Add(new Label("构建管线:") { style = { width = 100 } });
            _buildPipelineField = new EnumField(EBuildPipeline.ScriptableBuildPipeline);
            _buildPipelineField.style.width = 200;
            _buildPipelineField.RegisterValueChangedCallback(evt => OnBuildPipelineChanged());
            pipelineContainer.Add(_buildPipelineField);
            paramScrollView.Add(pipelineContainer);
            
            // 压缩选项
            var compressContainer = new VisualElement { style = { marginBottom = 10 } };
            compressContainer.Add(new Label("压缩选项:") { style = { width = 100 } });
            _compressOptionField = new EnumField(ECompressOption.LZ4);
            _compressOptionField.style.width = 200;
            compressContainer.Add(_compressOptionField);
            paramScrollView.Add(compressContainer);
            
            // 文件名样式
            var fileNameStyleContainer = new VisualElement { style = { marginBottom = 10 } };
            fileNameStyleContainer.Add(new Label("文件名样式:") { style = { width = 100 } });
            _fileNameStyleField = new EnumField(EFileNameStyle.HashName);
            _fileNameStyleField.style.width = 200;
            fileNameStyleContainer.Add(_fileNameStyleField);
            paramScrollView.Add(fileNameStyleContainer);
            
            // 首包文件拷贝选项
            var buildinFileCopyOptionContainer = new VisualElement { style = { marginBottom = 10 } };
            buildinFileCopyOptionContainer.Add(new Label("首包文件拷贝:") { style = { width = 100 } });
            _buildinFileCopyOptionField = new EnumField(EBuildinFileCopyOption.None);
            _buildinFileCopyOptionField.style.width = 200;
            buildinFileCopyOptionContainer.Add(_buildinFileCopyOptionField);
            paramScrollView.Add(buildinFileCopyOptionContainer);
            
            // 首包文件拷贝参数（标签）
            var buildinFileCopyParamsContainer = new VisualElement { style = { marginBottom = 10 } };
            buildinFileCopyParamsContainer.Add(new Label("拷贝标签:") { style = { width = 100 } });
            _buildinFileCopyParamsField = new TextField { value = "" };
            _buildinFileCopyParamsField.style.width = 200;
            buildinFileCopyParamsContainer.Add(_buildinFileCopyParamsField);
            paramScrollView.Add(buildinFileCopyParamsContainer);
            
            // 清空构建缓存
            _clearBuildCacheToggle = new Toggle("清空构建缓存")
            {
                value = false
            };
            _clearBuildCacheToggle.style.marginBottom = 10;
            paramScrollView.Add(_clearBuildCacheToggle);
            
            // 使用资源依赖数据库
            _useAssetDependencyDBToggle = new Toggle("使用资源依赖数据库")
            {
                value = false
            };
            _useAssetDependencyDBToggle.style.marginBottom = 10;
            paramScrollView.Add(_useAssetDependencyDBToggle);
            
            // 高级构建参数 - 使用Foldout折叠
            var advancedFoldout = new Foldout
            {
                text = "高级构建参数",
                value = false
            };
            advancedFoldout.style.marginTop = 10;
            advancedFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            advancedFoldout.contentContainer.style.paddingLeft = 15;
            advancedFoldout.contentContainer.style.paddingTop = 5;
            advancedFoldout.contentContainer.style.paddingBottom = 5;
            
            // 从文件头里剥离Unity版本信息
            _stripUnityVersionToggle = new Toggle("剥离Unity版本信息")
            {
                value = false
            };
            _stripUnityVersionToggle.style.marginBottom = 5;
            advancedFoldout.Add(_stripUnityVersionToggle);
            
            // 禁止写入类型树结构
            _disableWriteTypeTreeToggle = new Toggle("禁止写入类型树结构（可降低包体和内存）")
            {
                value = false
            };
            _disableWriteTypeTreeToggle.style.marginBottom = 5;
            advancedFoldout.Add(_disableWriteTypeTreeToggle);
            
            // 忽略类型树变化
            _ignoreTypeTreeChangesToggle = new Toggle("忽略类型树变化")
            {
                value = true
            };
            _ignoreTypeTreeChangesToggle.style.marginBottom = 5;
            advancedFoldout.Add(_ignoreTypeTreeChangesToggle);
            
            // 使用可寻址地址代替资源路径
            _replaceAssetPathWithAddressToggle = new Toggle("使用可寻址地址代替资源路径（节省运行时内存）")
            {
                value = false
            };
            _replaceAssetPathWithAddressToggle.style.marginBottom = 5;
            advancedFoldout.Add(_replaceAssetPathWithAddressToggle);
            
            // 启用共享资源打包
            _enableSharePackRuleToggle = new Toggle("启用共享资源打包")
            {
                value = true
            };
            _enableSharePackRuleToggle.style.marginBottom = 5;
            advancedFoldout.Add(_enableSharePackRuleToggle);
            
            // 验证构建结果
            _verifyBuildingResultToggle = new Toggle("验证构建结果")
            {
                value = true
            };
            _verifyBuildingResultToggle.style.marginBottom = 5;
            advancedFoldout.Add(_verifyBuildingResultToggle);
            
            // 对单独引用的共享资源进行独立打包
            _singleReferencedPackAloneToggle = new Toggle("单独引用的共享资源独立打包")
            {
                value = true
            };
            _singleReferencedPackAloneToggle.style.marginBottom = 5;
            advancedFoldout.Add(_singleReferencedPackAloneToggle);
            
            paramScrollView.Add(advancedFoldout);
            
            // ScriptableBuildPipeline特有参数容器 - 使用Foldout折叠
            var sbpFoldout = new Foldout
            {
                text = "ScriptableBuildPipeline 特有参数",
                value = false,
                name = "sbpContainer"
            };
            sbpFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            sbpFoldout.contentContainer.style.paddingLeft = 15;
            sbpFoldout.contentContainer.style.paddingTop = 5;
            sbpFoldout.contentContainer.style.paddingBottom = 5;
            
            // 自动建立资源对象对图集的依赖关系
            _trackSpriteAtlasDependenciesToggle = new Toggle("自动建立图集依赖关系")
            {
                value = false
            };
            _trackSpriteAtlasDependenciesToggle.style.marginBottom = 5;
            sbpFoldout.Add(_trackSpriteAtlasDependenciesToggle);
            
            // 生成代码防裁剪配置
            _writeLinkXMLToggle = new Toggle("生成代码防裁剪配置")
            {
                value = true
            };
            _writeLinkXMLToggle.style.marginBottom = 5;
            sbpFoldout.Add(_writeLinkXMLToggle);
            
            // 缓存服务器地址
            var cacheServerHostContainer = new VisualElement 
            { 
                style = 
                { 
                    marginBottom = 5,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                } 
            };
            cacheServerHostContainer.Add(new Label("缓存服务器地址:") { style = { width = 150 } });
            _cacheServerHostField = new TextField { value = "" };
            _cacheServerHostField.style.width = 200;
            cacheServerHostContainer.Add(_cacheServerHostField);
            sbpFoldout.Add(cacheServerHostContainer);
            
            // 缓存服务器端口
            var cacheServerPortContainer = new VisualElement 
            { 
                style = 
                { 
                    marginBottom = 5,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                } 
            };
            cacheServerPortContainer.Add(new Label("缓存服务器端口:") { style = { width = 150 } });
            _cacheServerPortField = new IntegerField { value = 0 };
            _cacheServerPortField.style.width = 200;
            cacheServerPortContainer.Add(_cacheServerPortField);
            sbpFoldout.Add(cacheServerPortContainer);
            
            paramScrollView.Add(sbpFoldout);
            
            // 按钮容器
            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginBottom = 10
                }
            };
            
            _buildHybridCLRBtn = new Button(OnBuildHybridCLR) { text = "构建 HybridCLR" };
            _buildYooAssetBtn = new Button(OnBuildYooAsset) { text = "构建 YooAsset" };
            _buildAllBtn = new Button(OnBuildAll) { text = "构建全部" };
            _copyDllsBtn = new Button(OnCopyDlls) { text = "复制DLL到目标目录" };
            
            // 设置按钮间距
            _buildHybridCLRBtn.style.marginRight = 10;
            _buildYooAssetBtn.style.marginRight = 10;
            _buildAllBtn.style.marginRight = 10;
            
            buttonContainer.Add(_buildHybridCLRBtn);
            buttonContainer.Add(_buildYooAssetBtn);
            buttonContainer.Add(_buildAllBtn);
            buttonContainer.Add(_copyDllsBtn);
            _root.Add(buttonContainer);
            
            // 进度条
            _progressBar = new ProgressBar { title = "准备就绪" };
            _progressBar.style.marginBottom = 10;
            _root.Add(_progressBar);
            
            // 日志区域
            var logContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    borderTopWidth = 1,
                    borderTopColor = new Color(0.3f, 0.3f, 0.3f),
                    paddingTop = 10
                }
            };
            
            var logTitle = new Label("构建日志:")
            {
                style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 }
            };
            logContainer.Add(logTitle);
            
            _logScrollView = new ScrollView();
            _logLabel = new Label { style = { whiteSpace = WhiteSpace.Normal } };
            _logScrollView.Add(_logLabel);
            logContainer.Add(_logScrollView);
            _root.Add(logContainer);
        }

        private void BindUIElements()
        {
            if (_buildTargetField == null)
            {
                _buildTargetField = _root.Q<EnumField>("buildTargetField");
                // 如果从UXML加载，需要手动初始化类型
                if (_buildTargetField != null)
                {
                    try
                    {
                        // 尝试初始化，如果已经初始化则不会出错
                        if (_buildTargetField.value == null)
                        {
                            _buildTargetField.Init(EditorUserBuildSettings.activeBuildTarget);
                        }
                    }
                    catch
                    {
                        // 如果初始化失败，重新创建EnumField
                        var parent = _buildTargetField.parent;
                        var index = parent.IndexOf(_buildTargetField);
                        parent.Remove(_buildTargetField);
                        _buildTargetField = new EnumField(EditorUserBuildSettings.activeBuildTarget);
                        _buildTargetField.name = "buildTargetField";
                        _buildTargetField.style.width = 200;
                        parent.Insert(index, _buildTargetField);
                    }
                }
            }
            if (_developmentBuildToggle == null)
            {
                _developmentBuildToggle = _root.Q<Toggle>("developmentBuildToggle");
            }
            if (_packageNameField == null)
            {
                _packageNameField = _root.Q<TextField>("packageNameField");
            }
            if (_packageVersionField == null)
            {
                _packageVersionField = _root.Q<TextField>("packageVersionField");
            }
            if (_buildPipelineField == null)
            {
                _buildPipelineField = _root.Q<EnumField>("buildPipelineField");
                if (_buildPipelineField != null && _buildPipelineField.value == null)
                {
                    _buildPipelineField.Init(EBuildPipeline.ScriptableBuildPipeline);
                }
            }
            if (_compressOptionField == null)
            {
                _compressOptionField = _root.Q<EnumField>("compressOptionField");
                if (_compressOptionField != null && _compressOptionField.value == null)
                {
                    _compressOptionField.Init(ECompressOption.LZ4);
                }
            }
            if (_fileNameStyleField == null)
            {
                _fileNameStyleField = _root.Q<EnumField>("fileNameStyleField");
                if (_fileNameStyleField != null && _fileNameStyleField.value == null)
                {
                    _fileNameStyleField.Init(EFileNameStyle.HashName);
                }
            }
            if (_buildinFileCopyOptionField == null)
            {
                _buildinFileCopyOptionField = _root.Q<EnumField>("buildinFileCopyOptionField");
                if (_buildinFileCopyOptionField != null && _buildinFileCopyOptionField.value == null)
                {
                    _buildinFileCopyOptionField.Init(EBuildinFileCopyOption.None);
                }
            }
            if (_buildinFileCopyParamsField == null)
            {
                _buildinFileCopyParamsField = _root.Q<TextField>("buildinFileCopyParamsField");
            }
            if (_clearBuildCacheToggle == null)
            {
                _clearBuildCacheToggle = _root.Q<Toggle>("clearBuildCacheToggle");
            }
            if (_useAssetDependencyDBToggle == null)
            {
                _useAssetDependencyDBToggle = _root.Q<Toggle>("useAssetDependencyDBToggle");
            }
            if (_stripUnityVersionToggle == null)
            {
                _stripUnityVersionToggle = _root.Q<Toggle>("stripUnityVersionToggle");
            }
            if (_disableWriteTypeTreeToggle == null)
            {
                _disableWriteTypeTreeToggle = _root.Q<Toggle>("disableWriteTypeTreeToggle");
            }
            if (_ignoreTypeTreeChangesToggle == null)
            {
                _ignoreTypeTreeChangesToggle = _root.Q<Toggle>("ignoreTypeTreeChangesToggle");
            }
            if (_replaceAssetPathWithAddressToggle == null)
            {
                _replaceAssetPathWithAddressToggle = _root.Q<Toggle>("replaceAssetPathWithAddressToggle");
            }
            if (_enableSharePackRuleToggle == null)
            {
                _enableSharePackRuleToggle = _root.Q<Toggle>("enableSharePackRuleToggle");
            }
            if (_verifyBuildingResultToggle == null)
            {
                _verifyBuildingResultToggle = _root.Q<Toggle>("verifyBuildingResultToggle");
            }
            if (_singleReferencedPackAloneToggle == null)
            {
                _singleReferencedPackAloneToggle = _root.Q<Toggle>("singleReferencedPackAloneToggle");
            }
            if (_trackSpriteAtlasDependenciesToggle == null)
            {
                _trackSpriteAtlasDependenciesToggle = _root.Q<Toggle>("trackSpriteAtlasDependenciesToggle");
            }
            if (_writeLinkXMLToggle == null)
            {
                _writeLinkXMLToggle = _root.Q<Toggle>("writeLinkXMLToggle");
            }
            if (_cacheServerHostField == null)
            {
                _cacheServerHostField = _root.Q<TextField>("cacheServerHostField");
            }
            if (_cacheServerPortField == null)
            {
                _cacheServerPortField = _root.Q<IntegerField>("cacheServerPortField");
            }
            if (_buildHybridCLRBtn == null)
            {
                _buildHybridCLRBtn = _root.Q<Button>("buildHybridCLRBtn");
                if (_buildHybridCLRBtn != null)
                {
                    _buildHybridCLRBtn.clicked += OnBuildHybridCLR;
                }
            }
            if (_buildYooAssetBtn == null)
            {
                _buildYooAssetBtn = _root.Q<Button>("buildYooAssetBtn");
                if (_buildYooAssetBtn != null)
                {
                    _buildYooAssetBtn.clicked += OnBuildYooAsset;
                }
            }
            if (_buildAllBtn == null)
            {
                _buildAllBtn = _root.Q<Button>("buildAllBtn");
                if (_buildAllBtn != null)
                {
                    _buildAllBtn.clicked += OnBuildAll;
                }
            }
            if (_copyDllsBtn == null)
            {
                _copyDllsBtn = _root.Q<Button>("copyDllsBtn");
                if (_copyDllsBtn != null)
                {
                    _copyDllsBtn.clicked += OnCopyDlls;
                }
            }
            if (_logLabel == null)
            {
                _logLabel = _root.Q<Label>("logLabel");
            }
            if (_logScrollView == null)
            {
                _logScrollView = _root.Q<ScrollView>("logScrollView");
            }
            if (_progressBar == null)
            {
                _progressBar = _root.Q<ProgressBar>("progressBar");
            }
        }

        private void InitializeUI()
        {
            if (_buildTargetField != null)
            {
                _buildTargetField.value = EditorUserBuildSettings.activeBuildTarget;
            }
            if (_developmentBuildToggle != null)
            {
                _developmentBuildToggle.value = EditorUserBuildSettings.development;
            }
            if (_packageNameField != null && string.IsNullOrEmpty(_packageNameField.value))
            {
                _packageNameField.value = "HotUpdateTest";
            }
            if (_packageVersionField != null && string.IsNullOrEmpty(_packageVersionField.value))
            {
                int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                _packageVersionField.value = DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
            }
            // 从EditorPrefs加载YooAsset参数
            LoadYooAssetSettings();
            
            // 根据构建管线类型显示/隐藏参数
            UpdateSBPParametersVisibility();
            
            UpdateProgress(0, "准备就绪");
        }
        
        private void OnBuildPipelineChanged()
        {
            // 保存当前设置
            SaveYooAssetSettings();
            
            // 重新加载新构建管线的设置
            LoadYooAssetSettings();
            
            // 更新SBP参数可见性
            UpdateSBPParametersVisibility();
        }
        
        private void UpdateSBPParametersVisibility()
        {
            var sbpContainer = _root.Q<Foldout>("sbpContainer");
            if (sbpContainer != null)
            {
                EBuildPipeline currentPipeline = GetBuildPipeline();
                bool isSBP = currentPipeline == EBuildPipeline.ScriptableBuildPipeline;
                sbpContainer.style.display = isSBP ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

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
                        throw new BuildFailedException("HybridCLR 未安装，请通过菜单 'HybridCLR/Installer' 安装");
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
                    MethodBridgeGeneratorCommand.GenerateMethodBridgeAndReversePInvokeWrapper(target);
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
                    Log($"构建参数: Version={packageVersion}, Target={target}, Pipeline={buildPipeline}");
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
        
        private void BuildYooAssetDirectly(string packageName, string packageVersion, BuildTarget target, EBuildPipeline buildPipeline)
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
                    scriptableParams.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    scriptableParams.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
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
                    scriptableParams.TrackSpriteAtlasDependencies = GetTrackSpriteAtlasDependencies();
                    scriptableParams.WriteLinkXML = GetWriteLinkXML();
                    scriptableParams.CacheServerHost = GetCacheServerHost();
                    scriptableParams.CacheServerPort = GetCacheServerPort();
                    scriptableParams.EncryptionServices = CreateEncryptionServicesInstance(packageName, buildPipeline.ToString());
                    scriptableParams.ManifestProcessServices = CreateManifestProcessServicesInstance(packageName, buildPipeline.ToString());
                    scriptableParams.ManifestRestoreServices = CreateManifestRestoreServicesInstance(packageName, buildPipeline.ToString());
                    
                    // 获取内置着色器资源包名称
                    var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
                    var shadersPackRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
                    scriptableParams.BuiltinShadersBundleName = shadersPackRuleResult.GetBundleName(packageName, uniqueBundleName);
                    
                    // 获取Mono脚本资源包名称
                    var monosPackRuleResult = DefaultPackRule.CreateMonosPackRuleResult();
                    scriptableParams.MonoScriptsBundleName = monosPackRuleResult.GetBundleName(packageName, uniqueBundleName);
                    
                    buildParameters = scriptableParams;
                    pipeline = new ScriptableBuildPipeline();
                }
                else if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
                {
                    BuiltinBuildParameters builtinParams = new BuiltinBuildParameters();
                    builtinParams.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    builtinParams.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
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
                    builtinParams.EncryptionServices = CreateEncryptionServicesInstance(packageName, buildPipeline.ToString());
                    builtinParams.ManifestProcessServices = CreateManifestProcessServicesInstance(packageName, buildPipeline.ToString());
                    builtinParams.ManifestRestoreServices = CreateManifestRestoreServicesInstance(packageName, buildPipeline.ToString());
                    
                    buildParameters = builtinParams;
                    pipeline = new BuiltinBuildPipeline();
                }
                else if (buildPipeline == EBuildPipeline.RawFileBuildPipeline)
                {
                    RawFileBuildParameters rawFileParams = new RawFileBuildParameters();
                    rawFileParams.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    rawFileParams.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
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
                    rawFileParams.EncryptionServices = CreateEncryptionServicesInstance(packageName, buildPipeline.ToString());
                    rawFileParams.ManifestProcessServices = CreateManifestProcessServicesInstance(packageName, buildPipeline.ToString());
                    rawFileParams.ManifestRestoreServices = CreateManifestRestoreServicesInstance(packageName, buildPipeline.ToString());
                    
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
                Log($"构建参数: Package={packageName}, Version={packageVersion}, Target={target}, Pipeline={buildPipeline}");
                Log($"压缩选项: {compressOption}, 文件名样式: {fileNameStyle}");
                Log($"高级参数: 剥离Unity版本={GetStripUnityVersion()}, 禁止写入类型树={GetDisableWriteTypeTree()}");
                Log($"高级参数: 使用可寻址地址={GetReplaceAssetPathWithAddress()}, 启用共享资源打包={GetEnableSharePackRule()}");
                if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
                {
                    Log($"SBP特有参数: 图集依赖={GetTrackSpriteAtlasDependencies()}, 生成LinkXML={GetWriteLinkXML()}");
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
                                foreach (var file in outputDir.GetFiles("*", SearchOption.AllDirectories))
                                {
                                    totalSize += file.Length;
                                    fileCount++;
                                }
                                Log($"构建统计: 文件数量={fileCount}, 总大小={FormatFileSize(totalSize)}");
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
        
        private IEncryptionServices CreateEncryptionServicesInstance(string packageName, string buildPipeline)
        {
            var className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(packageName, buildPipeline);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IEncryptionServices)Activator.CreateInstance(classType);
            else
                return null;
        }
        
        private IManifestProcessServices CreateManifestProcessServicesInstance(string packageName, string buildPipeline)
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(packageName, buildPipeline);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestProcessServices)Activator.CreateInstance(classType);
            else
                return null;
        }
        
        private IManifestRestoreServices CreateManifestRestoreServicesInstance(string packageName, string buildPipeline)
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(packageName, buildPipeline);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestRestoreServices)Activator.CreateInstance(classType);
            else
                return null;
        }

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
                    string hotUpdateDllSource = $"{SettingsUtil.HybridCLRDataDir}/HotUpdateDlls/{target}/{file}";
                    if (File.Exists(hotUpdateDllSource))
                    {
                        string hotUpdateDllDest = Path.Combine(dstPath, $"{file}.bytes");
                        File.Copy(hotUpdateDllSource, hotUpdateDllDest, overwrite: true);
                        Log($"✓ 复制热更新DLL: {file}");
                    }
                }
                
                UpdateProgress(0.6f, "复制AOT元数据DLL...");
                string aotDllsSourceDir = $"{SettingsUtil.HybridCLRDataDir}/StrippedAOTAssembly2/{target}";
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
        
        private void LoadYooAssetSettings()
        {
            string packageName = GetPackageName();
            string buildPipeline = GetBuildPipeline().ToString();
            
            if (_compressOptionField != null)
            {
                var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(packageName, buildPipeline);
                _compressOptionField.value = compressOption;
            }
            
            if (_fileNameStyleField != null)
            {
                var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(packageName, buildPipeline);
                _fileNameStyleField.value = fileNameStyle;
            }
            
            if (_buildinFileCopyOptionField != null)
            {
                var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(packageName, buildPipeline);
                _buildinFileCopyOptionField.value = buildinFileCopyOption;
            }
            
            if (_buildinFileCopyParamsField != null)
            {
                var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(packageName, buildPipeline);
                _buildinFileCopyParamsField.value = buildinFileCopyParams;
            }
            
            if (_clearBuildCacheToggle != null)
            {
                var clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(packageName, buildPipeline);
                _clearBuildCacheToggle.value = clearBuildCache;
            }
            
            if (_useAssetDependencyDBToggle != null)
            {
                var useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(packageName, buildPipeline);
                _useAssetDependencyDBToggle.value = useAssetDependencyDB;
            }
            
            // 加载高级参数（使用EditorPrefs）
            if (_stripUnityVersionToggle != null)
            {
                _stripUnityVersionToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_StripUnityVersion", false);
            }
            
            if (_disableWriteTypeTreeToggle != null)
            {
                _disableWriteTypeTreeToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_DisableWriteTypeTree", false);
            }
            
            if (_ignoreTypeTreeChangesToggle != null)
            {
                _ignoreTypeTreeChangesToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_IgnoreTypeTreeChanges", true);
            }
            
            if (_replaceAssetPathWithAddressToggle != null)
            {
                _replaceAssetPathWithAddressToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_ReplaceAssetPathWithAddress", false);
            }
            
            if (_enableSharePackRuleToggle != null)
            {
                _enableSharePackRuleToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_EnableSharePackRule", true);
            }
            
            if (_verifyBuildingResultToggle != null)
            {
                _verifyBuildingResultToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_VerifyBuildingResult", true);
            }
            
            if (_singleReferencedPackAloneToggle != null)
            {
                _singleReferencedPackAloneToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_SingleReferencedPackAlone", true);
            }
            
            // ScriptableBuildPipeline特有参数
            if (_trackSpriteAtlasDependenciesToggle != null)
            {
                _trackSpriteAtlasDependenciesToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_TrackSpriteAtlasDependencies", false);
            }
            
            if (_writeLinkXMLToggle != null)
            {
                _writeLinkXMLToggle.value = GetEditorPrefsBool($"{packageName}_{buildPipeline}_WriteLinkXML", true);
            }
            
            if (_cacheServerHostField != null)
            {
                _cacheServerHostField.value = EditorPrefs.GetString($"{packageName}_{buildPipeline}_CacheServerHost", "");
            }
            
            if (_cacheServerPortField != null)
            {
                _cacheServerPortField.value = EditorPrefs.GetInt($"{packageName}_{buildPipeline}_CacheServerPort", 0);
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
                AssetBundleBuilderSetting.SetPackageCompressOption(packageName, buildPipeline, (ECompressOption)_compressOptionField.value);
            }
            
            if (_fileNameStyleField != null && _fileNameStyleField.value != null)
            {
                AssetBundleBuilderSetting.SetPackageFileNameStyle(packageName, buildPipeline, (EFileNameStyle)_fileNameStyleField.value);
            }
            
            if (_buildinFileCopyOptionField != null && _buildinFileCopyOptionField.value != null)
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyOption(packageName, buildPipeline, (EBuildinFileCopyOption)_buildinFileCopyOptionField.value);
            }
            
            if (_buildinFileCopyParamsField != null)
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyParams(packageName, buildPipeline, _buildinFileCopyParamsField.value);
            }
            
            if (_clearBuildCacheToggle != null)
            {
                AssetBundleBuilderSetting.SetPackageClearBuildCache(packageName, buildPipeline, _clearBuildCacheToggle.value);
            }
            
            if (_useAssetDependencyDBToggle != null)
            {
                AssetBundleBuilderSetting.SetPackageUseAssetDependencyDB(packageName, buildPipeline, _useAssetDependencyDBToggle.value);
            }
            
            // 保存高级参数到EditorPrefs
            if (_stripUnityVersionToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_StripUnityVersion", _stripUnityVersionToggle.value);
            }
            
            if (_disableWriteTypeTreeToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_DisableWriteTypeTree", _disableWriteTypeTreeToggle.value);
            }
            
            if (_ignoreTypeTreeChangesToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_IgnoreTypeTreeChanges", _ignoreTypeTreeChangesToggle.value);
            }
            
            if (_replaceAssetPathWithAddressToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_ReplaceAssetPathWithAddress", _replaceAssetPathWithAddressToggle.value);
            }
            
            if (_enableSharePackRuleToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_EnableSharePackRule", _enableSharePackRuleToggle.value);
            }
            
            if (_verifyBuildingResultToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_VerifyBuildingResult", _verifyBuildingResultToggle.value);
            }
            
            if (_singleReferencedPackAloneToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_SingleReferencedPackAlone", _singleReferencedPackAloneToggle.value);
            }
            
            // ScriptableBuildPipeline特有参数
            if (_trackSpriteAtlasDependenciesToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_TrackSpriteAtlasDependencies", _trackSpriteAtlasDependenciesToggle.value);
            }
            
            if (_writeLinkXMLToggle != null)
            {
                SetEditorPrefsBool($"{packageName}_{buildPipeline}_WriteLinkXML", _writeLinkXMLToggle.value);
            }
            
            if (_cacheServerHostField != null)
            {
                EditorPrefs.SetString($"{packageName}_{buildPipeline}_CacheServerHost", _cacheServerHostField.value);
            }
            
            if (_cacheServerPortField != null)
            {
                EditorPrefs.SetInt($"{packageName}_{buildPipeline}_CacheServerPort", _cacheServerPortField.value);
            }
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

