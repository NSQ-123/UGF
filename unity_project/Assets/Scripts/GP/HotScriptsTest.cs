using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

// @Author ：NiShiqiang
// @Created ：2025/11/02 16:42:24
namespace Game.GP
{
    public class HotScriptsTest : MonoBehaviour
    {
        private AssetHandle _musicHandle;
        
        private IEnumerator Start()
        {
            Debug.Log("=============================================================================");
            Debug.Log("Hello World");
            // 加载背景音乐
            _musicHandle = YooAssets.LoadAssetAsync<GameObject>("Cube");
            yield return _musicHandle;
            _musicHandle.InstantiateSync();
            Debug.Log("=============================================================================");

        }

        private void OnDestroy()
        {
            _musicHandle.Dispose();
        }
    }
}