using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GF.RedPoint
{
    /// <summary>
    /// 红点模块
    /// 红点之间的关联影响：本质上是构建了一个有向图，当某一个节点的红点发生变更时，会同时更新它所关联的红点，并按照图的有向边扩散这个更新
    /// 注意：构建出来的图必须是没有闭环的，否则会导致更新关联红点的时候 出现重复计算红点的情况
    /// 若配置上导致出现了 "环"，则更新时遇到为环的节点将只计算一次红点，
    /// 所以红点状态并不会出错，但这样的配置也是不正确的，将会抛出一个Error提示
    /// 一个红点可以对多个红点产生关联
    /// todo: group相关处理
    /// </summary>
    public class RedPointModule
    {
        //public static RedPointModule Inst { get { return GameModuleMgr.Get<RedPointModule>(); } }

        /// <summary>
        /// 所有红点数据
        /// </summary>
        /// <typeparam name="string">应全局唯一</typeparam>
        /// <typeparam name="RedPointData"></typeparam>
        private Dictionary<string, RedPointData> _allRedPointDict;

        /// <summary>
        /// 所有红点关联红点数据集
        /// from: string -> to: HashSet<string>
        /// </summary>
        private Dictionary<string, HashSet<string>> _allRelationSetDict;

        /// <summary>
        /// 按组分类红点
        /// </summary>
        private Dictionary<string, HashSet<string>> _allGroups;

        /// <summary>
        /// 红点刷新回调
        /// 使用HashSet存储回调方法，避免重复回调和提高查找效率
        /// </summary>
        private Dictionary<string, HashSet<Action<string, int>>> _allRefreshCallbacks;

        /// <summary>
        /// 最大递归深度限制
        /// </summary>
        private const int MAX_RECURSION_DEPTH = 50;

        #region const

        /// <summary>
        /// 初始化时默认注册的红点
        /// 注意：此处注册的红点没有获取红点信息Func
        /// </summary>
        private readonly HashSet<string> _defaultRedPoint = new HashSet<string> { };

        /// <summary>
        /// 初始化时默认添加的红点关系，key:子级, value:父级
        /// </summary>
        private readonly Dictionary<string, List<string>> _defaultRelation = new Dictionary<
            string,
            List<string>
        >
        { };

        /// <summary>
        /// 初始化时默认添加的反向红点关系, key:父级, value: 子级
        /// </summary>
        private readonly Dictionary<string, List<string>> _defaultReverseRelation = new Dictionary<
            string,
            List<string>
        >
        { };
        #endregion const

        /// <summary>
        /// 零时红点
        /// </summary>
        private RedPointData _tempRedPointData;

        public void Init()
        {
            if (_allRedPointDict == null)
                _allRedPointDict = new Dictionary<string, RedPointData>();
            if (_allRelationSetDict == null)
                _allRelationSetDict = new Dictionary<string, HashSet<string>>();
            if (_allRefreshCallbacks == null)
                _allRefreshCallbacks = new Dictionary<string, HashSet<Action<string, int>>>();
            if (_allGroups == null)
                _allGroups = new Dictionary<string, HashSet<string>>();

            foreach (var redPointKey in _defaultRedPoint)
            {
                RegisterRedPoint(redPointKey, null);
            }

            foreach (var relationPair in _defaultRelation)
            {
                AddRedPointRelation(relationPair.Key, relationPair.Value);
            }

            // 根据子级列表添加relation
            foreach (var reverseRelationPair in _defaultReverseRelation)
            {
                if (reverseRelationPair.Value != null)
                {
                    var list = reverseRelationPair.Value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        AddRedPointRelation(list[i], reverseRelationPair.Key);
                    }
                }
            }
        }

        public void Dispose()
        {
            ClearRedPointData();
            _allRedPointDict = null;
            _allRelationSetDict = null;
            _allRefreshCallbacks = null;
            _allGroups = null;
        }

        public void Clear()
        {
            ClearRedPointData();
        }

        private void ClearRedPointData()
        {
            if (_allRedPointDict != null)
            {
                foreach (var pair in _allRedPointDict)
                    pair.Value?.Dispose();
                _allRedPointDict.Clear();
            }

            if (_allRelationSetDict != null)
            {
                foreach (var pair in _allRelationSetDict)
                    pair.Value?.Clear();
                _allRelationSetDict.Clear();
            }

            if (_allRefreshCallbacks != null)
            {
                foreach (var pair in _allRefreshCallbacks)
                    pair.Value?.Clear();
                _allRefreshCallbacks.Clear();
            }

            if (_allGroups != null)
            {
                foreach (var pair in _allGroups)
                    pair.Value?.Clear();
                _allGroups.Clear();
            }
        }

        /// <summary>
        /// 动态注册红点信息
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="checkFunc">检查函数</param>
        /// <param name="relation">关联的红点key</param>
        /// <param name="groups">所属group，暂时没用</param>
        /// <param name="showErrLog">是否显示错误日志</param>
        public void RegisterRedPoint(string key, Func<int> checkFunc, string relation = null, List<string> groups = null, bool showErrLog = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (showErrLog)
                    Debug.LogError("RedPointModule::Register Error, Key is null or empty!");
                return;
            }

            if (_allRedPointDict.ContainsKey(key))
            {
                if (showErrLog)
                    Debug.LogError(
                        "RedPointModule::Register Error, Register Existed Key:" + key + "!"
                    );
                return;
            }

            _allRedPointDict[key] = new RedPointData()
            {
                RedPointKey = key,
                CheckFunc = checkFunc,
                Num = 0,
            };

            if (groups != null)
            {
                foreach (string group in groups)
                {
                    if (!_allGroups.ContainsKey(group))
                        _allGroups[group] = new HashSet<string>();
                    _allGroups[group].Add(key);
                }
            }

            if (!string.IsNullOrEmpty(relation))
            {
                AddRedPointRelation(key, relation);
            }
        }

        /// <summary>
        /// 移除注册的红点，同时会移除对应刷新事件
        /// 移除红点前默认会刷新相关红点数量，不需刷新可传入false
        /// todo: 未清除relation中相应key
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="needRefresh">是否需要刷新</param>
        public void UnregisterRedPoint(string key, bool needRefresh = true)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_allRedPointDict.TryGetValue(key, out RedPointData data))
            {
                // 移除红点，则计算数量为0
                var originalCheckFunc = data.CheckFunc;
                data.CheckFunc = () => 0;

                if (needRefresh)
                    RefreshRedPoint(key);

                data.Dispose();
                _allRedPointDict.Remove(key);
            }

            // 清除回调
            if (_allRefreshCallbacks.ContainsKey(key))
            {
                _allRefreshCallbacks[key].Clear();
                _allRefreshCallbacks.Remove(key);
            }

            // 从组中移除
            if (_allGroups != null)
            {
                foreach (var group in _allGroups.Keys.ToArray())
                {
                    _allGroups[group].Remove(key);
                    if (_allGroups[group].Count == 0)
                        _allGroups.Remove(group);
                }
            }
        }

        /// <summary>
        /// 添加红点关联信息
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="relation">指向的其他红点的key</param>
        public void AddRedPointRelation(string key, string relation)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(relation))
                return;

            if (!_allRelationSetDict.ContainsKey(key))
                _allRelationSetDict[key] = new HashSet<string>();
            _allRelationSetDict[key].Add(relation);
        }

        /// <summary>
        /// 添加红点关联信息
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="relations">由该红点指向的其他红点的keys</param>
        public void AddRedPointRelation(string key, List<string> relations)
        {
            if (string.IsNullOrEmpty(key) || relations == null || relations.Count == 0)
                return;

            if (!_allRelationSetDict.ContainsKey(key))
                _allRelationSetDict[key] = new HashSet<string>();

            HashSet<string> relationSet = _allRelationSetDict[key];
            foreach (string v in relations)
            {
                if (!string.IsNullOrEmpty(v))
                    relationSet.Add(v);
            }
        }

        /// <summary>
        /// 绑定红点刷新回调
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="refreshCallBack">刷新回调</param>
        /// <param name="invokeRefresh">是否立即刷新</param>
        /// <param name="showErrLog">是否显示错误日志</param>
        public void BindRefreshAct(
            string key,
            Action<string, int> refreshCallBack,
            bool invokeRefresh = true,
            bool showErrLog = true
        )
        {
            if (string.IsNullOrEmpty(key) || refreshCallBack == null)
            {
                if (showErrLog)
                    Debug.LogError(
                        "RedPointModule::BindRefreshAct Error, key or callback is null!"
                    );
                return;
            }

            if (!_allRefreshCallbacks.ContainsKey(key))
                _allRefreshCallbacks[key] = new HashSet<Action<string, int>>();

            // 使用HashSet自动处理重复
            _allRefreshCallbacks[key].Add(refreshCallBack);

            if (invokeRefresh && _allRedPointDict.TryGetValue(key, out var redPointData))
                refreshCallBack(key, redPointData.Num);
        }

        /// <summary>
        /// 删除红点刷新回调
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="refreshCallBack">要删除的回调</param>
        public void UnbindRefreshAct(string key, Action<string, int> refreshCallBack)
        {
            if (string.IsNullOrEmpty(key) || refreshCallBack == null)
                return;

            if (_allRefreshCallbacks?.TryGetValue(key, out var callbacks) == true)
            {
                callbacks.Remove(refreshCallBack);
                if (callbacks.Count == 0)
                    _allRefreshCallbacks.Remove(key);
            }
        }

        /// <summary>
        /// 刷新指定红点
        /// 刷新会使红点数量变化信息，沿有向图传播
        /// </summary>
        /// <param name="key">红点key</param>
        public void RefreshRedPoint(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (!_allRedPointDict.TryGetValue(key, out RedPointData redPoint))
            {
                return;
            }

            if (redPoint.IsDisposed)
                return;

            int num = redPoint.GetCurrentNum();

            // 红点数量未发生变化时不须广播
            if (redPoint.Num == num)
                return;

            int numDelta = num - redPoint.Num;
            RefreshRedPointState(key, num);
            CheckRelationRedPoint(new HashSet<string>(), key, numDelta, 0);
        }

        /// <summary>
        /// 递归刷新有向图上的红点信息
        /// </summary>
        /// <param name="passedMap">已访问的节点集合</param>
        /// <param name="key">红点key</param>
        /// <param name="numDelta">红点变化数量</param>
        /// <param name="depth">当前递归深度</param>
        private void CheckRelationRedPoint(
            HashSet<string> passedMap,
            string key,
            int numDelta,
            int depth
        )
        {
            if (depth > MAX_RECURSION_DEPTH)
            {
                Debug.LogError($"RedPoint recursion depth exceeded for key: {key}");
                return;
            }

            if (passedMap.Contains(key))
            {
                Debug.LogError("Config Error, Circle appeared in relation:" + key + "!");
                return;
            }
            passedMap.Add(key);

            if (!_allRelationSetDict.TryGetValue(key, out var relations) || relations.Count == 0)
            {
                passedMap.Remove(key);
                return;
            }

            foreach (string relationKey in relations)
            {
                if (!_allRedPointDict.TryGetValue(relationKey, out var redPoint))
                {
                    // Debug.LogError($"Relation Error, Relation RedPoint Not Register In: {relationKey} !");
                    continue;
                }

                if (redPoint.IsDisposed)
                    continue;

                RefreshRedPointState(relationKey, redPoint.Num + numDelta);
                CheckRelationRedPoint(passedMap, relationKey, numDelta, depth + 1);
            }

            passedMap.Remove(key);
        }

        /// <summary>
        /// 刷新红点状态
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="num">新的数量</param>
        private void RefreshRedPointState(string key, int num)
        {
            // 更新数量
            if (_allRedPointDict.TryGetValue(key, out var redPointData))
            {
                redPointData.Num = num;
            }

            // 刷新回调
            if (_allRefreshCallbacks.TryGetValue(key, out var callbacks) && callbacks.Count > 0)
            {
                // 复制回调集合以避免在回调中修改原集合导致的问题
                var callbacksCopy = new HashSet<Action<string, int>>(callbacks);
                foreach (var callback in callbacksCopy)
                {
                    try
                    {
                        callback?.Invoke(key, num);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"RedPoint callback error for key '{key}': {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 获取红点数量
        /// </summary>
        /// <param name="key">红点key</param>
        /// <returns>红点数量</returns>
        public int GetRedPointNum(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            if (_allRedPointDict.TryGetValue(key, out var redPointData))
                return redPointData.IsDisposed ? 0 : redPointData.Num;

            return 0;
        }

        /// <summary>
        /// 批量刷新红点
        /// </summary>
        /// <param name="keys">要刷新的红点key列表</param>
        public void RefreshRedPoints(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return;

            var processedKeys = new HashSet<string>();

            foreach (string key in keys)
            {
                if (!processedKeys.Contains(key))
                {
                    RefreshRedPoint(key);
                    processedKeys.Add(key);
                }
            }
        }

        /// <summary>
        /// 检查红点系统是否存在循环依赖
        /// </summary>
        /// <returns>是否存在循环依赖</returns>
        public bool HasCircularDependency()
        {
            var visitedNodes = new HashSet<string>();
            var pathNodes = new HashSet<string>();

            foreach (var key in _allRelationSetDict.Keys)
            {
                if (HasCircularDependencyHelper(key, visitedNodes, pathNodes))
                    return true;
            }

            return false;
        }

        private bool HasCircularDependencyHelper(
            string key,
            HashSet<string> visitedNodes,
            HashSet<string> pathNodes
        )
        {
            if (pathNodes.Contains(key))
                return true;

            if (visitedNodes.Contains(key))
                return false;

            visitedNodes.Add(key);
            pathNodes.Add(key);

            if (_allRelationSetDict.TryGetValue(key, out var relations))
            {
                foreach (var relation in relations)
                {
                    if (HasCircularDependencyHelper(relation, visitedNodes, pathNodes))
                        return true;
                }
            }

            pathNodes.Remove(key);
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 打印红点图
        /// </summary>
        public void PrintGraph()
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"=============== RedPoint Relations =============");

            // relations
            foreach (var key in _allRelationSetDict.Keys)
            {
                stringBuilder.AppendLine($"--- {key} ---");
                PrintNode(new HashSet<string>(), key, stringBuilder, 0);
            }
            Debug.Log($"[RedPoint]=>{stringBuilder.ToString()}");

            // nodes
            stringBuilder.Clear();
            stringBuilder.AppendLine($"=============== RedPoint Nodes =============");
            foreach (var redPointPair in _allRedPointDict)
            {
                stringBuilder.AppendLine(
                    $"key: {redPointPair.Key}, num: {redPointPair.Value.Num}, disposed: {redPointPair.Value.IsDisposed}"
                );
            }

            // callbacks
            stringBuilder.Clear();
            stringBuilder.AppendLine($"=============== RedPoint CallBacks =============");
            foreach (var redPointPair in _allRefreshCallbacks)
            {
                int callbackCount = redPointPair.Value?.Count ?? 0;
                stringBuilder.AppendLine(
                    $"key: {redPointPair.Key}, callbacks count: {callbackCount}"
                );
            }

            Debug.Log(stringBuilder.ToString());

            // 检查循环依赖
            if (HasCircularDependency())
            {
                Debug.LogError("RedPoint system has circular dependency!");
            }
        }

        private void PrintNode(
            HashSet<string> passedMap,
            string key,
            System.Text.StringBuilder stringBuilder,
            int depth
        )
        {
            if (depth > MAX_RECURSION_DEPTH)
            {
                stringBuilder.AppendLine($"Max recursion depth reached for: {key}");
                return;
            }

            if (passedMap.Contains(key))
            {
                Debug.LogError("ConfigError, Circle appeared in relation:" + key + "!");
                return;
            }

            if (!_allRelationSetDict.TryGetValue(key, out var relations))
                return;

            passedMap.Add(key);

            foreach (string relationKey in relations)
            {
                if (_allRedPointDict.TryGetValue(relationKey, out var redPoint))
                {
                    stringBuilder.AppendLine(
                        $"{"  ".PadLeft(depth * 2)}{redPoint.RedPointKey}: {redPoint.Num}"
                    );
                    PrintNode(passedMap, relationKey, stringBuilder, depth + 1);
                }
            }

            passedMap.Remove(key);
        }
#endif
    }
}
