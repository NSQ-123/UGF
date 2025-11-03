using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;

// @Author ：NiShiqiang
// @Created ：2025/11/02 22:22:21
namespace GF.HybridCLR
{
    public class HybridCLRLoadDll
    {
        // 根据AOTGenericReferences.cs的提示确定列表
        public static readonly IReadOnlyList<string> AOTMetaAssemblyFiles = new List<string>
        {
            "mscorlib.dll", "System.dll", "System.Core.dll",
        };
        
        public static readonly IReadOnlyList<string> HotUpdateAssemblyFiles = new List<string>
        {
            "GamePlay.dll",
        };
        
        /// <summary>
        /// 热更新脚本初始化
        /// </summary>
        public static void HotUpdateScriptInit()
        {
            // 加载AOT dll的元数据
            LoadMetadataForAOTAssemblies();
            // 加载热更dll
#if !UNITY_EDITOR
            foreach (var assemblyFile in HotUpdateAssemblyFiles)
            {
                Assembly.Load(ReadBytesFromStreamingAssets(assemblyFile));
            }
#endif
            
        }
        
        #region 补充元数据

        //补充元数据dll的列表
        //通过RuntimeApi.LoadMetadataForAOTAssembly()函数来补充AOT泛型的原始元数据
        private static Dictionary<string, TextAsset> s_assetDatas = new Dictionary<string, TextAsset>();
        private static Assembly _hotUpdateAss;

        private static byte[] ReadBytesFromStreamingAssets(string dllName)
        {
            //TODO: 加载文件
            if (s_assetDatas.ContainsKey(dllName))
            {
                return s_assetDatas[dllName].bytes;
            }

            return Array.Empty<byte>();
        }
        
        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用 LoadMetadataForAOTAssembly 会返回错误
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AOTMetaAssemblyFiles)
            {
                byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }

        #endregion
        
        
    }
}