using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// @Author ：NiShiqiang
// @Created ：2025/11/09 14:19:59
namespace Game.Editor
{
    public partial class BuildToolWindow : EditorWindow
    {
        [MenuItem("Build/Show Build Window")]
        public static void ShowWindow()
        {
            BuildToolWindow wnd = GetWindow<BuildToolWindow>();
            wnd.titleContent = new GUIContent("Build Window");
        }
        
       
        
        public void CreateGUI()
        {
            CreateGUIWithCode();
            //CreateGUIWithBuilder();
        }
    }
}