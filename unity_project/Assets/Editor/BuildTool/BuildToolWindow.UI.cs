using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;
using YooAsset.Editor;

namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// UI创建和初始化相关功能
    /// </summary>
    public partial class BuildToolWindow
    {
        private void CreateUIFromCode()
        {
            _root.Clear();

            // 标题
            var title = new Label("打包编辑器 - HybridCLR & YooAsset")
            {
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10,
                },
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
                value = EditorUserBuildSettings.development,
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
            _packageVersionField = new TextField
            {
                value = DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes,
            };
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
            var buildinFileCopyOptionContainer = new VisualElement
            {
                style = { marginBottom = 10 },
            };
            buildinFileCopyOptionContainer.Add(
                new Label("首包文件拷贝:") { style = { width = 100 } }
            );
            _buildinFileCopyOptionField = new EnumField(EBuildinFileCopyOption.None);
            _buildinFileCopyOptionField.style.width = 200;
            buildinFileCopyOptionContainer.Add(_buildinFileCopyOptionField);
            paramScrollView.Add(buildinFileCopyOptionContainer);

            // 首包文件拷贝参数（标签）
            var buildinFileCopyParamsContainer = new VisualElement
            {
                style = { marginBottom = 10 },
            };
            buildinFileCopyParamsContainer.Add(new Label("拷贝标签:") { style = { width = 100 } });
            _buildinFileCopyParamsField = new TextField { value = "" };
            _buildinFileCopyParamsField.style.width = 200;
            buildinFileCopyParamsContainer.Add(_buildinFileCopyParamsField);
            paramScrollView.Add(buildinFileCopyParamsContainer);

            // 清空构建缓存
            _clearBuildCacheToggle = new Toggle("清空构建缓存") { value = false };
            _clearBuildCacheToggle.style.marginBottom = 10;
            paramScrollView.Add(_clearBuildCacheToggle);

            // 使用资源依赖数据库
            _useAssetDependencyDBToggle = new Toggle("使用资源依赖数据库") { value = false };
            _useAssetDependencyDBToggle.style.marginBottom = 10;
            paramScrollView.Add(_useAssetDependencyDBToggle);

            // 高级构建参数 - 使用Foldout折叠
            var advancedFoldout = new Foldout { text = "高级构建参数", value = false };
            advancedFoldout.style.marginTop = 10;
            advancedFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            advancedFoldout.contentContainer.style.paddingLeft = 15;
            advancedFoldout.contentContainer.style.paddingTop = 5;
            advancedFoldout.contentContainer.style.paddingBottom = 5;

            // 从文件头里剥离Unity版本信息
            _stripUnityVersionToggle = new Toggle("剥离Unity版本信息") { value = false };
            _stripUnityVersionToggle.style.marginBottom = 5;
            advancedFoldout.Add(_stripUnityVersionToggle);

            // 禁止写入类型树结构
            _disableWriteTypeTreeToggle = new Toggle("禁止写入类型树结构（可降低包体和内存）")
            {
                value = false,
            };
            _disableWriteTypeTreeToggle.style.marginBottom = 5;
            advancedFoldout.Add(_disableWriteTypeTreeToggle);

            // 忽略类型树变化
            _ignoreTypeTreeChangesToggle = new Toggle("忽略类型树变化") { value = true };
            _ignoreTypeTreeChangesToggle.style.marginBottom = 5;
            advancedFoldout.Add(_ignoreTypeTreeChangesToggle);

            // 使用可寻址地址代替资源路径
            _replaceAssetPathWithAddressToggle = new Toggle(
                "使用可寻址地址代替资源路径（节省运行时内存）"
            )
            {
                value = false,
            };
            _replaceAssetPathWithAddressToggle.style.marginBottom = 5;
            advancedFoldout.Add(_replaceAssetPathWithAddressToggle);

            // 启用共享资源打包
            _enableSharePackRuleToggle = new Toggle("启用共享资源打包") { value = true };
            _enableSharePackRuleToggle.style.marginBottom = 5;
            advancedFoldout.Add(_enableSharePackRuleToggle);

            // 验证构建结果
            _verifyBuildingResultToggle = new Toggle("验证构建结果") { value = true };
            _verifyBuildingResultToggle.style.marginBottom = 5;
            advancedFoldout.Add(_verifyBuildingResultToggle);

            // 对单独引用的共享资源进行独立打包
            _singleReferencedPackAloneToggle = new Toggle("单独引用的共享资源独立打包")
            {
                value = true,
            };
            _singleReferencedPackAloneToggle.style.marginBottom = 5;
            advancedFoldout.Add(_singleReferencedPackAloneToggle);

            paramScrollView.Add(advancedFoldout);

            // ScriptableBuildPipeline特有参数容器 - 使用Foldout折叠
            var sbpFoldout = new Foldout
            {
                text = "ScriptableBuildPipeline 特有参数",
                value = false,
                name = "sbpContainer",
            };
            sbpFoldout.style.marginBottom = 10;
            // 给Foldout内容区域添加padding，避免文字重叠
            sbpFoldout.contentContainer.style.paddingLeft = 15;
            sbpFoldout.contentContainer.style.paddingTop = 5;
            sbpFoldout.contentContainer.style.paddingBottom = 5;

            // 自动建立资源对象对图集的依赖关系
            _trackSpriteAtlasDependenciesToggle = new Toggle("自动建立图集依赖关系")
            {
                value = false,
            };
            _trackSpriteAtlasDependenciesToggle.style.marginBottom = 5;
            sbpFoldout.Add(_trackSpriteAtlasDependenciesToggle);

            // 生成代码防裁剪配置
            _writeLinkXMLToggle = new Toggle("生成代码防裁剪配置") { value = true };
            _writeLinkXMLToggle.style.marginBottom = 5;
            sbpFoldout.Add(_writeLinkXMLToggle);

            // 缓存服务器地址
            var cacheServerHostContainer = new VisualElement
            {
                style =
                {
                    marginBottom = 5,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
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
                    alignItems = Align.Center,
                },
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
                    marginBottom = 10,
                },
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
                    paddingTop = 10,
                },
            };

            var logTitle = new Label("构建日志:")
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 5,
                },
            };
            logContainer.Add(logTitle);

            _logScrollView = new ScrollView();
            _logLabel = new Label { style = { whiteSpace = WhiteSpace.Normal } };
            _logScrollView.Add(_logLabel);
            logContainer.Add(_logScrollView);
            _root.Add(logContainer);
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
                _packageVersionField.value =
                    DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
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
    }
}

