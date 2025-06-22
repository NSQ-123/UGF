using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
        ///     key: 红点key值
        ///     value: 刷新回调
        ///        param string: 红点key
        ///        param int: 对应红点数量
        /// </summary>
        private Dictionary<string, Action<string, int>> _allRefreshCallback;

        #region const

        /// <summary>
        /// 初始化时默认注册的红点
        /// 注意：此处注册的红点没有获取红点信息Func
        /// </summary>
        private readonly HashSet<string> _defaultRedPoint = new HashSet<string>
        {

        };


        /// <summary>
        /// 初始化时默认添加的红点关系，key:子级, value:父级
        /// </summary>
        private readonly Dictionary<string, List<string>> _defaultRelation = new Dictionary<string, List<string>>
        {

        };

        /// <summary>
        /// 初始化时默认添加的反向红点关系, key:父级, value: 子级
        /// </summary>
        private readonly Dictionary<string, List<string>> _defaultReverseRelation = new Dictionary<string, List<string>>
        {
           
        };
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
            if (_allRefreshCallback == null)
                _allRefreshCallback = new Dictionary<string, Action<string, int>>();
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
            _tempRedPointData = null;
            _allRedPointDict = null;
            _allRelationSetDict = null;
            _allRefreshCallback = null;
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
                _allRedPointDict?.Clear();
            }

            if (_allRelationSetDict != null)
            {
                foreach (var pair in _allRelationSetDict)
                    _allRelationSetDict[pair.Key]?.Clear();
                _allRelationSetDict.Clear();
            }

            if (_allRefreshCallback != null && _allRefreshCallback.Count > 0)
            {
                foreach (var key in _allRefreshCallback.Keys.ToList())
                    _allRefreshCallback[key] = null;
            }
            _allRefreshCallback?.Clear();

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
        /// <param name="groups">所属group，暂时没用</param>
        /// <param name="relations">由该红点指向的其他红点的keys</param>
        public void RegisterRedPoint(string key, Func<int> checkFunc, string relation = null, List<string> groups = null, bool showErrLog = false)
        {
            if (_allRedPointDict.TryGetValue(key, out _tempRedPointData))
            {
                if (showErrLog)
                    Debug.LogError("RedPointModule::Register Error, Register Existed Key:" + key + "!");
                return;
            }

            _allRedPointDict[key] = new RedPointData()
            {
                RedPointKey = key,
                CheckFunc = checkFunc,
                Num = 0
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
            AddRedPointRelation(key, relation);
        }


        /// <summary>
        /// 移除注册的红点，同时会移除对应刷新事件
        /// 移除红点前默认会刷新相关红点数量，不需刷新可传入false
        /// todo: 未清除relation中相应key
        /// </summary>
        /// <param name="key">红点key</param>
        public void UnregisterRedPoint(string key, bool needRefresh = true)
        {
            if (_allRedPointDict.TryGetValue(key, out RedPointData data))
            {
                // 移除红点，则计算数量为0
                data.CheckFunc = RemoveCallback;

                // 相应回调置为空
                if (_allRefreshCallback.ContainsKey(key))
                    _allRefreshCallback[key] = null;

                if (needRefresh)
                    RefreshRedPoint(key);

                data.CheckFunc = null;

                _allRedPointDict.Remove(key);
            }

            // 外部也清空一次，相应回调置为空
            if (_allRefreshCallback.ContainsKey(key))
                _allRefreshCallback[key] = null;
            _allRefreshCallback.Remove(key);

            if (_allGroups != null)
            {
                foreach (string group in _allGroups.Keys)
                    _allGroups[group].Remove(key);
            }
        }

        /// <summary>
        /// 移除红点时所用的callback，主要用于移除红点时，使对应data红点数量为0
        /// </summary>
        private int RemoveCallback()
        {
            return 0;
        }

        /// <summary>
        /// 添加红点关联信息
        /// </summary>
        /// <param name="key">红点key</param>
        /// <param name="relation">指向的其他红点的key</param>
        public void AddRedPointRelation(string key, string relation)
        {
            if (string.IsNullOrEmpty(relation))
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
            if (relations == null)
                return;

            if (!_allRelationSetDict.ContainsKey(key))
                _allRelationSetDict[key] = new HashSet<string>();

            HashSet<string> relationSet = _allRelationSetDict[key];
            foreach (string v in relations)
                relationSet.Add(v);
        }

        /// <summary>
        /// 绑定红点刷新回调
        /// </summary>
        /// <param name="key"></param>
        /// <param name="refreshCallBack">刷新回调</param>
        public void BindRefreshAct(string key, Action<string, int> refreshCallBack, bool invokeRefresh = true, bool showErrLog = true)
        {
            if (refreshCallBack == null)
                return;

            if (!_allRefreshCallback.TryGetValue(key, out var callBacks))
                callBacks = null;

            // 回调列表为空，或者不为空但不包含该回调时，可添加
            if (callBacks == null || !callBacks.GetInvocationList().Contains(refreshCallBack))
                callBacks += refreshCallBack;
            _allRefreshCallback[key] = callBacks;

            if (invokeRefresh && _allRedPointDict.TryGetValue(key, out _tempRedPointData))
                refreshCallBack(key, _tempRedPointData.Num);
        }

        /// <summary>
        /// 删除红点刷新回调
        /// </summary>
        public void UnbindRefreshAct(string key, Action<string, int> refreshCallBack)
        {
            if (_allRefreshCallback == null)
                return;
            if (_allRefreshCallback.TryGetValue(key, out var callBacks))
            {
                callBacks -= refreshCallBack;
                _allRefreshCallback[key] = callBacks;
            }
        }

        /// <summary>
        /// 刷新指定红点
        /// 刷新会使红点数量变化信息，沿有向图传播
        /// </summary>
        /// <param name="key"></param>
        public void RefreshRedPoint(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (!_allRedPointDict.TryGetValue(key, out RedPointData redPoint))
            {
                return;
            }

            int num = redPoint.CheckFunc == null ? redPoint.Num : redPoint.CheckFunc();
            num = num > 0 ? num : 0;

            // 红点数量未发生变化时不须广播
            if (redPoint.Num == num)
                return;

            int numDelta = num - redPoint.Num;
            RefreshRedPointState(key, num);
            CheckRelationRedPoint(new HashSet<string>(), key, numDelta);
        }


        /// <summary>
        /// 递归刷新有向图上的红点信息
        /// </summary>
        /// <param name="passedMap"></param>
        /// <param name="key">红点key</param>
        /// <param name="numDelta">红点变化数量</param>
        private void CheckRelationRedPoint(HashSet<string> passedMap, string key, int numDelta)
        {
            if (passedMap.Contains(key))
            {
                Debug.LogError("Config Error, Circle appeared in relation:" + key + "!");
                return;
            }
            passedMap.Add(key);

            if (!_allRelationSetDict.ContainsKey(key))
                return;

            HashSet<string> relations = _allRelationSetDict[key];
            foreach (string relationKey in relations)
            {
                if (!_allRedPointDict.TryGetValue(relationKey, out var redPoint))
                {
                    // Debug.LogError($"Relation Error, Relation RedPoint Not Register In: {relationKey} !");
                    continue;
                }
                RefreshRedPointState(relationKey, redPoint.Num + numDelta);
                CheckRelationRedPoint(passedMap, relationKey, numDelta);
            }
        }

        private void RefreshRedPointState(string key, int num)
        {
            // 更新数量
            if (_allRedPointDict.TryGetValue(key, out _tempRedPointData))
            {
                _tempRedPointData.Num = num;
            }

            // 刷新回调
            if (_allRefreshCallback.TryGetValue(key, out var callbacks))
            {
                // Debug.LogError($"RefreshRedPointState: {key}, {num}");
                callbacks?.Invoke(key, num);
            }
        }

        /// <summary>
        /// 获取红点数量
        /// </summary>
        public int GetRedPointNum(string key)
        {
            if (_allRedPointDict.ContainsKey(key))
                return _allRedPointDict[key].Num;
            return 0;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 打印红点图
        /// </summary>
        public void PrintGraph()
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"=============== RedPoint Ralations =============");
            // ralations
            foreach (var key in _allRelationSetDict.Keys)
            {
                stringBuilder.AppendLine($"--- {key} ---");
                PrintNode(new HashSet<string>(), key, stringBuilder);
            }
            Debug.Log($"[RedPoint]=>{stringBuilder.ToString()}");

            // node
            stringBuilder.Clear();
            stringBuilder.AppendLine($"=============== RedPoint Nodes =============");
            foreach (var redPointPair in _allRedPointDict)
            {
                stringBuilder.AppendLine($"key: {redPointPair.Key}, num: {redPointPair.Value.Num}");
            }

            stringBuilder.Clear();
            stringBuilder.AppendLine($"=============== RedPoint CallBacks =============");
            int callbackCount = 0;
            foreach (var redPointPair in _allRefreshCallback)
            {
                callbackCount = (_allRefreshCallback.ContainsKey(redPointPair.Key) && _allRefreshCallback[redPointPair.Key] != null) ? _allRefreshCallback[redPointPair.Key].GetInvocationList().Length : 0;
                stringBuilder.AppendLine($"key: {redPointPair.Key}, callbacks count: {callbackCount}");
            }

            Debug.Log(stringBuilder.ToString());
        }

        private void PrintNode(HashSet<string> passedMap, string key, System.Text.StringBuilder stringBuilder)
        {
            if (passedMap.Contains(key))
            {
                Debug.LogError("ConfigError,Circle appeared in relation:" + key + "!");
                return;
            }

            if (!_allRelationSetDict.ContainsKey(key))
                return;

            HashSet<string> relations = _allRelationSetDict[key];
            foreach (string relationKey in relations)
            {
                RedPointData redPoint = _allRedPointDict[relationKey];
                stringBuilder.AppendLine($"{redPoint.RedPointKey}: {redPoint.Num}");
                PrintNode(passedMap, relationKey, stringBuilder);
            }
        }


#endif

    }
}
