using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    /// <summary>
    /// 逻辑节点控制物体激活。
    /// 通过可嵌套的逻辑表达式自由组合「且 / 或」；表达式为真则激活目标物体，否则隐藏。
    /// 数据以扁平列表存储，避免 Unity 对递归序列化的深度限制告警。
    /// </summary>
    public class DagNodeControlMax : NonsensicalMono
    {
        /// <summary>组节点内子表达式之间的组合方式。</summary>
        public enum LogicCombineOp
        {
            [InspectorName("且（全部满足）")]
            And,
            [InspectorName("或（任一满足）")]
            Or
        }

        public enum LogicExprKind
        {
            [InspectorName("条件")]
            Leaf,
            [InspectorName("逻辑块")]
            Group
        }

        [Serializable]
        public struct ConditionGroup
        {
            [Tooltip("匹配的节点 ID")]
            [SerializeField] private string m_nodeID;

            [Tooltip("状态检查类型")]
            [SerializeField] private DagNodeCheckType m_checkType;

            public void SetConfig(string nodeID, string checkType)
            {
                m_nodeID = nodeID;
                m_checkType = Enum.Parse<DagNodeCheckType>(checkType);
            }

            public bool CheckState(DagLogicManager manager)
            {
                return manager.CheckState(m_nodeID, m_checkType);
            }
        }

        /// <summary>
        /// 单条逻辑节点（扁平表中的一行）。子节点通过 <see cref="ChildIndices"/> 指向同表中的下标。
        /// </summary>
        [Serializable]
        public sealed class LogicExpressionNode
        {
            public LogicExprKind Kind = LogicExprKind.Leaf;

            [Tooltip("仅组：子项之间的逻辑关系")]
            public LogicCombineOp Combine = LogicCombineOp.Or;

            [Tooltip("仅叶子：节点条件")]
            public ConditionGroup Condition;

            [Tooltip("仅组：子节点在表中的下标")]
            public List<int> ChildIndices = new List<int>();

            public void SetLeafConditionConfig(string nodeID, string checkType)
            {
                var c = Condition;
                c.SetConfig(nodeID, checkType);
                Condition = c;
            }
        }

        [Header("逻辑表达式（扁平存储）")]
        [Tooltip("所有节点；根节点下标见 m_rootNodeIndex。")]
        [SerializeField, HideInInspector]
        private List<LogicExpressionNode> m_logicNodes = new List<LogicExpressionNode>();

        [SerializeField, HideInInspector]
        private int m_rootNodeIndex;

        [SerializeField] private List<GameObject> m_controlGameObjects;
        [SerializeField] private List<MonoBehaviour> m_controlComponents;

        private DagLogicManager _manager;
        private bool _isRunning;

        private void Awake()
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            ServiceCore.SafeGet<DagLogicManager>(OnGetService);
        }

        private void OnEnable()
        {
            if (_isRunning && _manager != null && _manager.CrtSelectNode != null)
                OnSwitchNode(_manager.CrtSelectNode);
        }

        private void Reset()
        {
            m_controlGameObjects = new List<GameObject> { gameObject };
            EnsureDefaultEmptyRoot();
        }

        public void Close()
        {
            _isRunning = false;
            Unsubscribe<DagRuntimeNode>(DagLogicNodeEnum.SwitchNode, OnSwitchNode);
        }

        private void OnValidate()
        {
            if (m_logicNodes != null && m_logicNodes.Count > 0)
                ClampRootIndex();
        }

        [ContextMenu("逻辑条件/创建空根（或组，无子项）")]
        private void ContextMenuCreateEmptyRoot()
        {
            CreateEmptyLogicalRoot();
        }

        /// <summary>
        /// 与旧版一致：在根为「或组」时追加一条叶子条件；否则将现有根与新条件用「或」包一层。
        /// </summary>
        public void AddCondition(string nodeId, string checkType)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            if (m_logicNodes.Count == 0)
                EnsureDefaultEmptyRoot();

            var leafIdx = CreateLeafNode(nodeId, checkType);

            ClampRootIndex();
            var root = m_logicNodes[m_rootNodeIndex];
            if (root.Kind == LogicExprKind.Group && root.Combine == LogicCombineOp.Or)
            {
                root.ChildIndices.Add(leafIdx);
                return;
            }

            var wrap = new LogicExpressionNode
            {
                Kind = LogicExprKind.Group,
                Combine = LogicCombineOp.Or,
                ChildIndices = new List<int> { m_rootNodeIndex, leafIdx }
            };
            m_logicNodes.Add(wrap);
            m_rootNodeIndex = m_logicNodes.Count - 1;
        }

        /// <summary>仅保证列表非 null。</summary>
        public void EnsureLogicRootInitialized()
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
        }

        public int CreateLeafNode(string nodeId, string checkType)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            var n = new LogicExpressionNode { Kind = LogicExprKind.Leaf };
            n.SetLeafConditionConfig(nodeId, checkType);
            m_logicNodes.Add(n);
            return m_logicNodes.Count - 1;
        }

        public int CreateEmptyGroupNode(LogicCombineOp combine)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            var n = new LogicExpressionNode
            {
                Kind = LogicExprKind.Group,
                Combine = combine,
                ChildIndices = new List<int>()
            };
            m_logicNodes.Add(n);
            return m_logicNodes.Count - 1;
        }

        public void AppendChildIndexToGroup(int parentNodeIndex, int childNodeIndex)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            if (!IsValidNodeIndex(parentNodeIndex) || !IsValidNodeIndex(childNodeIndex))
                return;
            var p = m_logicNodes[parentNodeIndex];
            if (p.Kind != LogicExprKind.Group)
                return;
            p.ChildIndices.Add(childNodeIndex);
        }

        public void RemoveNodeFromParent(int nodeIndex, int parentNodeIndex, int childSlotInParent)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            if (nodeIndex == m_rootNodeIndex || !IsValidNodeIndex(nodeIndex) || !IsValidNodeIndex(parentNodeIndex))
                return;
            var parent = m_logicNodes[parentNodeIndex];
            if (parent.Kind != LogicExprKind.Group || childSlotInParent < 0 ||
                childSlotInParent >= parent.ChildIndices.Count)
                return;
            if (parent.ChildIndices[childSlotInParent] != nodeIndex)
                return;
            parent.ChildIndices.RemoveAt(childSlotInParent);
            var victims = new HashSet<int>();
            CollectSubtreeIndices(nodeIndex, victims, m_logicNodes);
            RemoveNodesAndCompact(victims);
        }

        public void SwapChildrenInGroup(int parentNodeIndex, int slotA, int slotB)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            if (!IsValidNodeIndex(parentNodeIndex))
                return;
            var p = m_logicNodes[parentNodeIndex];
            if (p.Kind != LogicExprKind.Group)
                return;
            if (slotA < 0 || slotA >= p.ChildIndices.Count || slotB < 0 || slotB >= p.ChildIndices.Count)
                return;
            (p.ChildIndices[slotA], p.ChildIndices[slotB]) = (p.ChildIndices[slotB], p.ChildIndices[slotA]);
        }

        public IReadOnlyList<LogicExpressionNode> LogicNodes => m_logicNodes;

        public int RootNodeIndex => m_rootNodeIndex;

        private void EnsureDefaultEmptyRoot()
        {
            if (m_logicNodes == null)
                m_logicNodes = new List<LogicExpressionNode>();
            m_logicNodes.Clear();
            m_logicNodes.Add(new LogicExpressionNode
            {
                Kind = LogicExprKind.Group,
                Combine = LogicCombineOp.Or,
                ChildIndices = new List<int>()
            });
            m_rootNodeIndex = 0;
        }

        /// <summary>编辑器：创建单个空「或」逻辑根（覆盖当前扁平表）。</summary>
        public void CreateEmptyLogicalRoot()
        {
            EnsureDefaultEmptyRoot();
        }

        private void ClampRootIndex()
        {
            if (m_logicNodes == null || m_logicNodes.Count == 0)
            {
                m_rootNodeIndex = 0;
                return;
            }

            if (m_rootNodeIndex < 0 || m_rootNodeIndex >= m_logicNodes.Count)
                m_rootNodeIndex = 0;
        }

        private bool IsValidNodeIndex(int i) =>
            m_logicNodes != null && i >= 0 && i < m_logicNodes.Count;

        private static void CollectSubtreeIndices(int rootIndex, HashSet<int> into, IReadOnlyList<LogicExpressionNode> nodes)
        {
            into.Add(rootIndex);
            if (nodes[rootIndex].Kind != LogicExprKind.Group || nodes[rootIndex].ChildIndices == null)
                return;
            foreach (var c in nodes[rootIndex].ChildIndices)
            {
                if (!into.Contains(c))
                    CollectSubtreeIndices(c, into, nodes);
            }
        }

        private void RemoveNodesAndCompact(HashSet<int> remove)
        {
            if (remove == null || remove.Count == 0)
                return;

            var oldToNew = new Dictionary<int, int>();
            var newList = new List<LogicExpressionNode>();
            for (var i = 0; i < m_logicNodes.Count; i++)
            {
                if (remove.Contains(i))
                    continue;
                oldToNew[i] = newList.Count;
                newList.Add(m_logicNodes[i]);
            }

            foreach (var n in newList)
            {
                if (n.Kind != LogicExprKind.Group || n.ChildIndices == null)
                    continue;
                var next = new List<int>();
                foreach (var c in n.ChildIndices)
                {
                    if (remove.Contains(c))
                        continue;
                    if (oldToNew.TryGetValue(c, out var mapped))
                        next.Add(mapped);
                }

                n.ChildIndices = next;
            }

            m_logicNodes = newList;
            if (oldToNew.TryGetValue(m_rootNodeIndex, out var newRoot))
                m_rootNodeIndex = newRoot;
            ClampRootIndex();
        }

        private bool EvaluateNode(int nodeIndex, DagLogicManager manager)
        {
            if (!IsValidNodeIndex(nodeIndex) || manager == null)
                return false;
            var n = m_logicNodes[nodeIndex];
            switch (n.Kind)
            {
                case LogicExprKind.Leaf:
                    return n.Condition.CheckState(manager);
                case LogicExprKind.Group:
                    if (n.ChildIndices == null || n.ChildIndices.Count == 0)
                        return false;
                    if (n.Combine == LogicCombineOp.And)
                        return n.ChildIndices.All(i => EvaluateNode(i, manager));
                    return n.ChildIndices.Any(i => EvaluateNode(i, manager));
                default:
                    return false;
            }
        }

        private void OnGetService(DagLogicManager service)
        {
            _manager = service;
            Init();
        }

        private void Init()
        {
            _isRunning = true;
            Subscribe<DagRuntimeNode>(DagLogicNodeEnum.SwitchNode, OnSwitchNode);

            if (_manager.CrtSelectNode != null)
                OnSwitchNode(_manager.CrtSelectNode);
        }

        private void OnSwitchNode(DagRuntimeNode node)
        {
            m_logicNodes ??= new List<LogicExpressionNode>();
            var nextActive = m_logicNodes.Count > 0 &&
                             EvaluateNode(m_rootNodeIndex, _manager);

            if (m_controlGameObjects != null)
            {
                foreach (var go in m_controlGameObjects)
                {
                    if (go != null && go.activeSelf != nextActive)
                        go.SetActive(nextActive);
                }
            }

            if (m_controlComponents != null)
            {
                foreach (var com in m_controlComponents)
                {
                    if (com != null && com.enabled != nextActive)
                        com.enabled = nextActive;
                }
            }
        }
    }
}
