using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editor
{
    /// <summary>
    ///  使用脚本创建窗口
    /// </summary>
    public partial class BuildToolWindow
    {
        private void CreateGUIWithCode()
        {
            GetRoot();
            // 创建一个标签
            Label titleLabel = new Label("Hello, UI Toolkit!");
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _root.Add(titleLabel);

            // 创建一个按钮
            Button actionButton = new Button { text = "Click Me" };
            actionButton.clicked += () => Debug.Log("Button clicked!");
            _root.Add(actionButton);

            CreateParamScrollView();
            CreateBuildTargetField();
            CreateYooAssetBuildParams();
        }

        private VisualElement _root;

        private void GetRoot()
        {
            _root = this.rootVisualElement; // 获取窗口的根元素
        }

        private ScrollView _paramScrollView; // 参数区域ScrollView
        private EnumField _buildTargetField; //构建目标
        private TextField _yooAssetBuildOutputField; //YooAsset构建输出路径

        private void CreateParamScrollView()
        {
            _paramScrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    maxHeight = 400, // 限制最大高度
                },
            };
            _root.Add(_paramScrollView);
        }

        private void CreateBuildTargetField()
        {
            var buildTargetContainer = new VisualElement
            {
                style =
                {
                    marginBottom = 10,
                    flexDirection = FlexDirection.Row, // 设置为水平布局
                    alignItems = Align.Center, // 子元素在交叉轴上居中对齐
                },
            };
            var label = new Label("构建目标:") { style = { width = 100 } };
            buildTargetContainer.Add(label);
            _buildTargetField = new EnumField(EditorUserBuildSettings.activeBuildTarget)
            {
                style =
                {
                    maxWidth = 200,
                    //unityTextAlign = TextAnchor.MiddleCenter,
                    flexGrow = 1, // 字段占据剩余宽度
                },
            };
            buildTargetContainer.Add(_buildTargetField);
            _paramScrollView.Add(buildTargetContainer);
        }

        private void CreateYooAssetBuildParams()
        {

            var yooAssetBuildParamsContainer = new VisualElement
            {
                style = { marginBottom = 10, paddingTop = 10 },
            };
            yooAssetBuildParamsContainer.Add(
                new Label("YooAsset构建参数:") { style = { width = 100 } }
            );
            _paramScrollView.Add(yooAssetBuildParamsContainer);

             _yooAssetContainer ??= new VisualElement();
            _yooAssetContainer.Clear();
            ShowYooAssetBuildView(_yooAssetContainer);
            _paramScrollView.Add(_yooAssetContainer);
            return;
        }
    }
}
