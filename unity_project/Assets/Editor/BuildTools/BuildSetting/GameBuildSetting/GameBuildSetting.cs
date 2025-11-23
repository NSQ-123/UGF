using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

// @Author ：NiShiqiang
// @Created ：2025/11/15 15:02:09
namespace Game.BuildTools.BuildSetting.GameBuildSetting
{
    [CreateAssetMenu(fileName = "GameBuildSetting", menuName = "GameBuildSetting")]
    public class GameBuildSetting : ScriptableObject
    {
        BuiltinBuildParameters builtinBuildParameters;
        ScriptableBuildParameters scriptableBuildParameters;
        public BuildParameters buildParameters;

        /// <summary>
        /// 构建输出的根目录
        /// </summary>
        public string BuildOutputRoot;

        /// <summary>
        /// 内置文件的根目录
        /// </summary>
        public string BuildinFileRoot;

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType;

        /// <summary>
        /// 构建的平台
        /// </summary>
        public BuildTarget BuildTarget;

        /// <summary>
        /// 构建的包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 构建的包裹版本
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 构建的包裹备注
        /// </summary>
        public string PackageNote;

        /// <summary>
        /// 清空构建缓存文件
        /// </summary>
        public bool ClearBuildCacheFiles = false;

        /// <summary>
        /// 使用资源依赖缓存数据库
        /// 说明：开启此项可以极大提高资源收集速度！
        /// </summary>
        public bool UseAssetDependencyDB = false;

        /// <summary>
        /// 启用共享资源打包
        /// </summary>
        public bool EnableSharePackRule = false;

        /// <summary>
        /// 对单独引用的共享资源进行独立打包
        /// 说明：关闭该选项单独引用的共享资源将会构建到引用它的资源包内！
        /// </summary>
        public bool SingleReferencedPackAlone = true;

        /// <summary>
        /// 验证构建结果
        /// </summary>
        public bool VerifyBuildingResult = false;

        /// <summary>
        /// 资源包名称样式
        /// </summary>
        public EFileNameStyle FileNameStyle = EFileNameStyle.HashName;

        /// <summary>
        /// 内置文件的拷贝选项
        /// </summary>
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.None;

        /// <summary>
        /// 内置文件的拷贝参数
        /// </summary>
        public string BuildinFileCopyParams;

        /// <summary>
        /// 资源包加密服务类
        /// </summary>
        public IEncryptionServices EncryptionServices;

        /// <summary>
        /// 资源清单加密服务类
        /// </summary>
        public IManifestProcessServices ManifestProcessServices;

        /// <summary>
        /// 资源清单解密服务类
        /// </summary>
        public IManifestRestoreServices ManifestRestoreServices;
    }
}
