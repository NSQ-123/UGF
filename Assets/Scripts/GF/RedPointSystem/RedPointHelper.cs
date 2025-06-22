using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GF.RedPoint
{
    /// <summary>
    /// 红点系统静态辅助类
    /// 提供更直观、更易用的静态API，内部使用RedPointModule实现
    ///
    /// 使用示例：
    /// // 创建红点
    /// RedPointHelper.CreateRedPoint("new_mail", () => GetNewMailCount());
    ///
    /// // 监听红点变化
    /// RedPointHelper.Watch("new_mail", hasRedPoint => mailIcon.SetActive(hasRedPoint));
    /// RedPointHelper.WatchCount("task_count", count => taskText.text = count.ToString());
    /// </summary>
    public static class RedPointHelper
    {
        /// <summary>
        /// 内部使用的RedPointModule静态实例
        /// </summary>
        private static RedPointModule _redPointModule;

        /// <summary>
        /// 获取内部的RedPointModule实例（用于高级操作）
        /// </summary>
        public static RedPointModule Module
        {
            get
            {
                if (_redPointModule == null)
                {
                    _redPointModule = new RedPointModule();
                    _redPointModule.Init();
                }
                return _redPointModule;
            }
        }

        /// <summary>
        /// 初始化红点系统（可选，会在首次使用时自动初始化）
        /// </summary>
        public static void Init()
        {
            var _ = Module; // 触发初始化
        }

        /// <summary>
        /// 销毁红点系统
        /// </summary>
        public static void Dispose()
        {
            _redPointModule?.Dispose();
            _redPointModule = null;
        }

        /// <summary>
        /// 清空红点系统
        /// </summary>
        public static void Clear()
        {
            Module?.Clear();
        }

        #region 简化的API - 让使用更直观

        /// <summary>
        /// 快速创建红点
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="checkFunc">检查函数，返回红点数量</param>
        /// <param name="parentKeys">关联的父级红点</param>
        public static void CreateRedPoint(string key, System.Func<int> checkFunc, params string[] parentKeys)
        {
            RegisterRedPoint(key, checkFunc, parentKeys?.ToList(), null);
        }

        /// <summary>
        /// 快速创建静态红点（由外部手动设置数量）
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="initialCount">初始数量</param>
        /// <param name="parentKeys">关联的父级红点</param>
        public static void CreateStaticRedPoint(string key, int initialCount = 0, params string[] parentKeys)
        {
            RegisterRedPoint(key, null, parentKeys?.ToList(), null);
            SetRedPointCount(key, initialCount);
        }

        /// <summary>
        /// 手动设置红点数量（用于静态红点）
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="count">数量</param>
        public static void SetRedPointCount(string key, int count)
        {
            if (Module == null)
                return;

            var currentNum = Module.GetRedPointNum(key);
            if (count != currentNum)
            {
                // 通过原始API设置，利用其完整的传播机制
                Module.RefreshRedPoint(key);
            }
        }

        /// <summary>
        /// 监听特定红点变化（是否有红点）
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="callback">回调函数，参数为是否有红点</param>
        /// <param name="invokeImmediately">是否立即调用一次</param>
        public static void Watch(string key, System.Action<bool> callback, bool invokeImmediately = true)
        {
            if (callback == null)
                return;

            Module?.BindRefreshAct(
                key,
                (k, count) => callback(count > 0),
                invokeImmediately
            );
        }

        /// <summary>
        /// 监听特定红点变化（数量）
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="callback">回调函数，参数为红点数量</param>
        /// <param name="invokeImmediately">是否立即调用一次</param>
        public static void WatchCount(
            string key,
            System.Action<int> callback,
            bool invokeImmediately = true
        )
        {
            if (callback == null)
                return;

            Module?.BindRefreshAct(key, (k, count) => callback(count), invokeImmediately);
        }

        /// <summary>
        /// 取消监听
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="callback">要取消的回调</param>
        public static void Unwatch(string key, System.Action<bool> callback)
        {
            if (callback == null)
                return;

            Module?.UnbindRefreshAct(key, (k, count) => callback(count > 0));
        }

        /// <summary>
        /// 取消监听（数量）
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="callback">要取消的回调</param>
        public static void UnwatchCount(string key, System.Action<int> callback)
        {
            if (callback == null)
                return;

            Module?.UnbindRefreshAct(key, (k, count) => callback(count));
        }

        /// <summary>
        /// 检查红点是否存在
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <returns>是否有红点</returns>
        public static bool HasRedPoint(string key)
        {
            return Module?.GetRedPointNum(key) > 0;
        }

        /// <summary>
        /// 获取红点数量
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <returns>红点数量</returns>
        public static int GetRedPointCount(string key)
        {
            return Module?.GetRedPointNum(key) ?? 0;
        }

        /// <summary>
        /// 刷新指定红点
        /// </summary>
        /// <param name="key">红点标识</param>
        public static void RefreshRedPoint(string key)
        {
            Module?.RefreshRedPoint(key);
        }

        /// <summary>
        /// 批量刷新红点
        /// </summary>
        /// <param name="keys">红点标识数组</param>
        public static void RefreshRedPoints(params string[] keys)
        {
            if (keys == null || keys.Length == 0) return;
            
            foreach (var key in keys)
            {
                Module?.RefreshRedPoint(key);
            }
        }

        /// <summary>
        /// 销毁红点
        /// </summary>
        /// <param name="key">红点标识</param>
        /// <param name="needRefresh">是否需要刷新相关红点</param>
        public static void DestroyRedPoint(string key, bool needRefresh = true)
        {
            Module?.UnregisterRedPoint(key, needRefresh);
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 内部注册红点方法
        /// </summary>
        private static void RegisterRedPoint(string key, Func<int> checkFunc, List<string> relations = null, List<string> groups = null, bool showErrLog = false)
        {
            if (Module == null)
                return;
            try
            {
                // 先注册红点（不包含关联关系）
                Module.RegisterRedPoint(key, checkFunc, null, groups, showErrLog);
                
                // 然后添加关联关系
                if (relations != null && relations.Count > 0)
                {
                    Module.AddRedPointRelation(key, relations);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RedPointHelper] 注册红点失败: {key}, 错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否存在循环依赖
        /// </summary>
        /// <returns>是否存在循环依赖</returns>
        public static bool HasCircularDependency()
        {
            return Module?.HasCircularDependency() ?? false;
        }

        /// <summary>
        /// 打印红点图（调试用）
        /// </summary>
        public static void PrintGraph()
        {
            Module?.PrintGraph();
        }

        #endregion
    }
}
