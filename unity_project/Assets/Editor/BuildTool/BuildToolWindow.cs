using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// @Author ：NiShiqiang
// @Created ：2025/01/XX
namespace Game.Editor.Tools.BuildTool
{
    /// <summary>
    /// 打包编辑器窗口 - 整合HybridCLR和YooAsset
    /// </summary>
    public partial class BuildToolWindow : EditorWindow
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

            // 使用代码创建UI
            CreateUIFromCode();

            // 加载USS样式（可选）
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/Editor/GFEditor/BuildTool/BuildToolWindow.uss"
            );
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }

            // 初始化UI
            InitializeUI();
        }
    }
}
