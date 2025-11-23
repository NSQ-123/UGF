using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.Editor
{
    /// <summary>
    ///  使用UIBuilder创建窗口
    /// </summary>
    public partial class BuildToolWindow
    {
        private void CreateGUIWithBuilder()
        {
            VisualElement root = rootVisualElement;
            _root = root;
            // 加载UXML模板
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/BuildToolWindow.uxml");
            VisualElement tree = visualTree.Instantiate();
            root.Add(tree);

            // （可选）加载USS样式
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BuildToolWindow.uss");
            root.styleSheets.Add(styleSheet);
            
            
        }

    }
}