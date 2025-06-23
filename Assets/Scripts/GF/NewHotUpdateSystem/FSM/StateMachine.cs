using System;
using System.Collections.Generic;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 状态节点接口
    /// </summary>
    public interface IStateNode
    {
        void OnCreate(StateMachine machine);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    /// <summary>
    /// 状态机
    /// </summary>
    public class StateMachine
    {
        private readonly Dictionary<string, object> _blackboard = new Dictionary<string, object>(100);
        private readonly Dictionary<string, IStateNode> _nodes = new Dictionary<string, IStateNode>(100);
        private IStateNode _curNode;
        private IStateNode _preNode;

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// 当前运行的节点名称
        /// </summary>
        public string CurrentNode
        {
            get { return _curNode != null ? _curNode.GetType().FullName : string.Empty; }
        }

        /// <summary>
        /// 之前运行的节点名称
        /// </summary>
        public string PreviousNode
        {
            get { return _preNode != null ? _preNode.GetType().FullName : string.Empty; }
        }

        public StateMachine(object owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            if (_curNode != null)
                _curNode.OnUpdate();
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public void Run<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            Run(nodeName);
        }

        public void Run(Type entryNode)
        {
            var nodeName = entryNode.FullName;
            Run(nodeName);
        }

        public void Run(string entryNode)
        {
            _curNode = TryGetNode(entryNode);
            _preNode = _curNode;

            if (_curNode == null)
                throw new Exception($"Not found entry node: {entryNode}");

            _curNode.OnEnter();
        }

        /// <summary>
        /// 加入一个节点
        /// </summary>
        public void AddNode<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
            AddNode(stateNode);
        }

        public void AddNode(IStateNode stateNode)
        {
            var nodeName = stateNode.GetType().FullName;
            if (_nodes.ContainsKey(nodeName))
                throw new Exception($"Node already exists: {nodeName}");

            _nodes.Add(nodeName, stateNode);
            stateNode.OnCreate(this);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }

        public void ChangeState(Type nodeType)
        {
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }

        public void ChangeState(string nodeName)
        {
            if (_curNode != null)
                _curNode.OnExit();

            _preNode = _curNode;
            _curNode = TryGetNode(nodeName);

            if (_curNode == null)
                throw new Exception($"Not found node: {nodeName}");

            _curNode.OnEnter();
        }

        /// <summary>
        /// 尝试获取节点
        /// </summary>
        private IStateNode TryGetNode(string nodeName)
        {
            if (_nodes.TryGetValue(nodeName, out IStateNode node))
                return node;
            return null;
        }

        /// <summary>
        /// 设置黑板值
        /// </summary>
        public void SetBlackboardValue(string key, object value)
        {
            _blackboard[key] = value;
        }

        /// <summary>
        /// 获取黑板值
        /// </summary>
        public object GetBlackboardValue(string key)
        {
            if (_blackboard.TryGetValue(key, out object value))
                return value;
            return null;
        }

        /// <summary>
        /// 获取黑板值（泛型）
        /// </summary>
        public T GetBlackboardValue<T>(string key)
        {
            var value = GetBlackboardValue(key);
            if (value is T result)
                return result;
            return default(T);
        }

        /// <summary>
        /// 移除黑板值
        /// </summary>
        public void RemoveBlackboardValue(string key)
        {
            _blackboard.Remove(key);
        }

        /// <summary>
        /// 清空黑板
        /// </summary>
        public void ClearBlackboard()
        {
            _blackboard.Clear();
        }
    }
} 