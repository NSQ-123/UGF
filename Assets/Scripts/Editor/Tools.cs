using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

// @Author ：#Name#
// @Created ：#CreateTime#
namespace GF.Editor
{
    public class Tools : MonoBehaviour
    {
        // 打开 Application.persistentDataPath 文件夹
        [MenuItem("GF/打开持久化数据文件夹")]
        public static void OpenPersistentDataPath()
        {
#if UNITY_EDITOR_WIN
            Process.Start("explorer.exe", Application.persistentDataPath.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
            Process.Start("open", Application.persistentDataPath);
#else
            UnityEngine.Debug.LogWarning("当前平台不支持此操作。");
#endif
        }
        
    }
}