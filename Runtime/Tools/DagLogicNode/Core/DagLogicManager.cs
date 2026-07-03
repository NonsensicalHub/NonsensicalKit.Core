using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public enum DagNodeCheckType
    {
        [InspectorName("自身 · 选中")]
        SelfSelect,
        [InspectorName("自身 · 未选中")]
        SelfUnselect,
        [InspectorName("父节点 · 选中")]
        ParentSelect,
        [InspectorName("父节点 · 未选中")]
        ParentUnselect,
        [InspectorName("子节点 · 选中")]
        ChildSelect,
        [InspectorName("子节点 · 未选中")]
        ChildUnselect,
        [InspectorName("父或子 · 选中")]
        ParentOrChildSelect,
        [InspectorName("父或子 · 未选中")]
        ParentOrChildUnselect
    }

    public sealed class DagRuntimeNode
    {
        public string NodeID;
        public string AutoJumpNode;
        public DagNode SourceNode;
        public DagRuntimeNode DefaultParentNode;
        public DagRuntimeNode LastFromNode;
        public string LastFromNodeID;
        public readonly List<DagRuntimeNode> ParentNodes = new List<DagRuntimeNode>();
        public readonly List<DagRuntimeNode> ChildNodes = new List<DagRuntimeNode>();

        public DagRuntimeNode(string nodeID, DagNode sourceNode)
        {
            NodeID = nodeID;
            SourceNode = sourceNode;
        }

        public void MarkActivatedFrom(DagRuntimeNode fromNode)
        {
            LastFromNode = fromNode;
            LastFromNodeID = fromNode?.NodeID;
        }
    }

    public readonly struct DagSwitchContext
    {
        public readonly DagRuntimeNode FromNode;
        public readonly DagRuntimeNode ToNode;
        public readonly string FromNodeId;
        public readonly string ToNodeId;

        public DagSwitchContext(DagRuntimeNode fromNode, DagRuntimeNode toNode)
        {
            FromNode = fromNode;
            ToNode = toNode;
            FromNodeId = fromNode?.NodeID;
            ToNodeId = toNode?.NodeID;
        }
    }

    public class DagLogicManager : NonsensicalMono, IMonoService
    {
        public DagRuntimeNode CrtSelectNode { get; private set; }
        public DagSwitchContext LastSwitchContext { get; private set; }
        public bool IsReady { get; private set; }

        /// <summary>
        /// IService.InitCompleted — ServiceCore.SafeGet 依赖此属性。
        /// </summary>
        Action IService.InitCompleted { get; set; }

        /// <summary>当前激活链路（从最早激活节点到当前节点）。</summary>
        public IReadOnlyList<string> ActivationChain => _activationChain;

        /// <summary>激活链路的可读摘要，供 Inspector 显示。</summary>
        public string ActivationChainSummary => m_activationChainSummary;

        [SerializeField, HideInInspector]
        private List<string> m_activationChainInspector = new List<string>();

        [SerializeField, HideInInspector]
        private string m_activationChainSummary;

        private readonly Dictionary<string, DagRuntimeNode> _nodesById = new Dictionary<string, DagRuntimeNode>();
        private readonly List<string> _activationChain = new List<string>();
        private readonly Stack<string> _history = new Stack<string>();
        private string _switchBuffer;
        private string _defaultNodeId;

        public void InitGraph(DagGraph targetGraph)
        {
            if (targetGraph == null)
            {
                LogCore.Error("DagLogicManager 初始化失败：DagGraph 为空");
                return;
            }

            _defaultNodeId = targetGraph.defaultNodeId;
            BuildRuntimeNodes(targetGraph);
            SelectInitialNode();

            if (string.IsNullOrWhiteSpace(_switchBuffer) == false)
            {
                var buffered = _switchBuffer;
                _switchBuffer = null;
                SwitchNode(buffered);
            }

            if (IsReady == false)
            {
                IsReady = true;
                ((IService)this).InitCompleted?.Invoke();
                ((IService)this).InitCompleted = null;
            }
            else
            {
                LastSwitchContext = new DagSwitchContext(null, CrtSelectNode);
                SetActivationChain(BuildActivationChainTo(CrtSelectNode));
                Publish(DagLogicNodeEnum.SwitchNode, LastSwitchContext);
            }
        }

        public DagRuntimeNode GetNode(string nodeID)
        {
            if (string.IsNullOrWhiteSpace(nodeID))
            {
                return null;
            }

            _nodesById.TryGetValue(nodeID, out var node);
            return node;
        }

        public bool SwitchNode(string nodeID)
        {
            if (string.IsNullOrWhiteSpace(nodeID))
            {
                return false;
            }

            if (IsReady == false)
            {
                _switchBuffer = nodeID;
                return false;
            }

            if (_nodesById.TryGetValue(nodeID, out var targetNode) == false)
            {
                LogCore.Warning($"未找到节点: {nodeID}");
                return false;
            }

            DoSwitchNode(targetNode, true, null);
            return true;
        }

        public bool ReturnLast()
        {
            while (_history.Count > 0)
            {
                var previousId = _history.Pop();
                if (_nodesById.TryGetValue(previousId, out var node))
                {
                    DoSwitchNode(node, false, null);
                    return true;
                }
            }

            return false;
        }

        public void ReturnPreviousLevel()
        {
            if (CrtSelectNode == null)
            {
                return;
            }

            var fallbackParent = CrtSelectNode.DefaultParentNode;
            if (fallbackParent != null)
            {
                DoSwitchNode(fallbackParent, true, null);
                return;
            }

            if (CrtSelectNode.ParentNodes.Count > 0)
            {
                DoSwitchNode(CrtSelectNode.ParentNodes[0], true, null);
            }
        }

        public bool CheckState(string nodeID, DagNodeCheckType checkType)
        {
            switch (checkType)
            {
                case DagNodeCheckType.SelfSelect:
                    return CheckState(nodeID);
                case DagNodeCheckType.SelfUnselect:
                    return !CheckState(nodeID);
                case DagNodeCheckType.ParentSelect:
                    return CheckStateWithParent(nodeID);
                case DagNodeCheckType.ParentUnselect:
                    return !CheckStateWithParent(nodeID);
                case DagNodeCheckType.ChildSelect:
                    return CheckStateWithChild(nodeID);
                case DagNodeCheckType.ChildUnselect:
                    return !CheckStateWithChild(nodeID);
                case DagNodeCheckType.ParentOrChildSelect:
                    return CheckStateWithParent(nodeID) || CheckStateWithChild(nodeID, false);
                case DagNodeCheckType.ParentOrChildUnselect:
                    return !CheckStateWithParent(nodeID) && !CheckStateWithChild(nodeID, false);
                default:
                    return false;
            }
        }

        public bool CheckState(string nodeID)
        {
            return CrtSelectNode != null && CrtSelectNode.NodeID == nodeID;
        }

        /// <summary>
        /// 检测节点是否在当前激活链路中（含当前选中节点）。
        /// </summary>
        public bool IsInActivationChain(string nodeID)
        {
            if (string.IsNullOrWhiteSpace(nodeID) || _activationChain.Count == 0)
            {
                return false;
            }

            var normalized = nodeID.Trim();
            for (int i = 0; i < _activationChain.Count; i++)
            {
                if (_activationChain[i] == normalized)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInActivationChain(DagRuntimeNode node) =>
            node != null && IsInActivationChain(node.NodeID);

        public bool CheckStateWithParent(string nodeID, bool includeSelf = true)
        {
            if (CrtSelectNode == null || _nodesById.TryGetValue(nodeID, out var checkNode) == false)
            {
                return false;
            }

            if (includeSelf && checkNode == CrtSelectNode)
            {
                return true;
            }

            var queue = new Queue<DagRuntimeNode>(checkNode.ParentNodes);
            var visited = new HashSet<string>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Add(node.NodeID) == false)
                {
                    continue;
                }

                if (node == CrtSelectNode)
                {
                    return true;
                }

                foreach (var parent in node.ParentNodes)
                {
                    queue.Enqueue(parent);
                }
            }

            return false;
        }

        public bool CheckStateWithChild(string nodeID, bool includeSelf = true)
        {
            if (CrtSelectNode == null || _nodesById.TryGetValue(nodeID, out var checkNode) == false)
            {
                return false;
            }

            var queue = new Queue<DagRuntimeNode>();
            if (includeSelf)
            {
                queue.Enqueue(checkNode);
            }
            else
            {
                foreach (var child in checkNode.ChildNodes)
                {
                    queue.Enqueue(child);
                }
            }

            var visited = new HashSet<string>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Add(node.NodeID) == false)
                {
                    continue;
                }

                if (node == CrtSelectNode)
                {
                    return true;
                }

                foreach (var child in node.ChildNodes)
                {
                    queue.Enqueue(child);
                }
            }

            return false;
        }

        private List<DagRuntimeNode> GetActivationChainNodes()
        {
            var nodes = new List<DagRuntimeNode>(_activationChain.Count);
            for (int i = 0; i < _activationChain.Count; i++)
            {
                if (_nodesById.TryGetValue(_activationChain[i], out var node) && node != null)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private void SetActivationChain(IReadOnlyList<DagRuntimeNode> path)
        {
            _activationChain.Clear();
            if (path == null || path.Count == 0)
            {
                SyncActivationChainInspector();
                return;
            }

            for (int i = 0; i < path.Count; i++)
            {
                var node = path[i];
                if (node == null)
                {
                    continue;
                }

                _activationChain.Add(node.NodeID);
                node.MarkActivatedFrom(i > 0 ? path[i - 1] : null);
            }

            SyncActivationChainInspector();
        }

        private static DagRuntimeNode GetDefaultParent(DagRuntimeNode node)
        {
            if (node == null)
            {
                return null;
            }

            if (node.DefaultParentNode != null)
            {
                return node.DefaultParentNode;
            }

            return node.ParentNodes.Count > 0 ? node.ParentNodes[0] : null;
        }

        /// <summary>
        /// 基于当前激活链路，构建到目标节点的连续路径（含中间节点）。
        /// </summary>
        private List<DagRuntimeNode> BuildActivationChainTo(DagRuntimeNode target)
        {
            if (target == null)
            {
                return new List<DagRuntimeNode>();
            }

            var chain = GetActivationChainNodes();

            int targetIndex = chain.FindIndex(n => n.NodeID == target.NodeID);
            if (targetIndex >= 0)
            {
                return chain.GetRange(0, targetIndex + 1);
            }

            var anchor = chain.Count > 0 ? chain[chain.Count - 1] : CrtSelectNode;
            if (anchor == null)
            {
                return new List<DagRuntimeNode> { target };
            }

            if (anchor == target)
            {
                return new List<DagRuntimeNode>(chain);
            }

            return MergeChainSegment(chain, anchor, target);
        }

        private List<DagRuntimeNode> MergeChainSegment(
            List<DagRuntimeNode> chainPrefix,
            DagRuntimeNode from,
            DagRuntimeNode to)
        {
            chainPrefix ??= new List<DagRuntimeNode>();
            if (to == null)
            {
                return chainPrefix;
            }

            int existingIndex = chainPrefix.FindIndex(n => n.NodeID == to.NodeID);
            if (existingIndex >= 0)
            {
                return chainPrefix.GetRange(0, existingIndex + 1);
            }

            if (from == null)
            {
                return new List<DagRuntimeNode> { to };
            }

            if (from == to)
            {
                if (chainPrefix.Count == 0 || chainPrefix[chainPrefix.Count - 1] != from)
                {
                    var copy = new List<DagRuntimeNode>(chainPrefix) { from };
                    return copy;
                }

                return new List<DagRuntimeNode>(chainPrefix);
            }

            if (TryFindForwardPath(from, to, out var forwardSegment))
            {
                var merged = new List<DagRuntimeNode>(chainPrefix);
                for (int i = 1; i < forwardSegment.Count; i++)
                {
                    merged.Add(forwardSegment[i]);
                }

                return merged;
            }

            if (TryBuildSegmentViaDefaultParents(chainPrefix, from, to, out var defaultSegment))
            {
                return defaultSegment;
            }

            var fallback = new List<DagRuntimeNode>(chainPrefix);
            if (fallback.Count == 0 || fallback[fallback.Count - 1] != from)
            {
                fallback.Add(from);
            }

            if (to != from)
            {
                fallback.Add(to);
            }

            return fallback;
        }

        private bool TryFindForwardPath(DagRuntimeNode from, DagRuntimeNode to, out List<DagRuntimeNode> path)
        {
            path = null;
            if (from == null || to == null)
            {
                return false;
            }

            if (from == to)
            {
                path = new List<DagRuntimeNode> { from };
                return true;
            }

            var parentMap = new Dictionary<string, DagRuntimeNode>();
            var queue = new Queue<DagRuntimeNode>();
            parentMap[from.NodeID] = null;
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in current.ChildNodes)
                {
                    if (child == null || parentMap.ContainsKey(child.NodeID))
                    {
                        continue;
                    }

                    parentMap[child.NodeID] = current;
                    if (child == to)
                    {
                        path = ReconstructPath(parentMap, from, to);
                        return true;
                    }

                    queue.Enqueue(child);
                }
            }

            return false;
        }

        private static List<DagRuntimeNode> ReconstructPath(
            Dictionary<string, DagRuntimeNode> parentMap,
            DagRuntimeNode from,
            DagRuntimeNode to)
        {
            var reversed = new List<DagRuntimeNode>();
            var node = to;
            while (node != null)
            {
                reversed.Add(node);
                if (node == from)
                {
                    break;
                }

                parentMap.TryGetValue(node.NodeID, out var parent);
                node = parent;
            }

            reversed.Reverse();
            return reversed;
        }

        /// <summary>
        /// 从目标沿默认父边向上，接到已有链路中的锚点，再与前缀合并。
        /// </summary>
        private bool TryBuildSegmentViaDefaultParents(
            List<DagRuntimeNode> chainPrefix,
            DagRuntimeNode from,
            DagRuntimeNode to,
            out List<DagRuntimeNode> merged)
        {
            merged = null;
            var upward = new List<DagRuntimeNode>();
            var visited = new HashSet<string>();
            DagRuntimeNode anchorInChain = null;
            int anchorIndex = -1;
            var node = to;

            while (node != null && visited.Add(node.NodeID))
            {
                upward.Add(node);

                if (node == from)
                {
                    anchorInChain = from;
                    anchorIndex = chainPrefix.FindIndex(n => n == from);
                    if (anchorIndex < 0)
                    {
                        anchorIndex = chainPrefix.Count - 1;
                    }

                    break;
                }

                anchorIndex = chainPrefix.FindIndex(n => n.NodeID == node.NodeID);
                if (anchorIndex >= 0)
                {
                    anchorInChain = chainPrefix[anchorIndex];
                    break;
                }

                node = GetDefaultParent(node);
            }

            if (anchorInChain == null)
            {
                return false;
            }

            upward.Reverse();

            merged = chainPrefix.GetRange(0, anchorIndex + 1);
            for (int i = 1; i < upward.Count; i++)
            {
                var segmentNode = upward[i];
                if (merged.Count == 0 || merged[merged.Count - 1] != segmentNode)
                {
                    merged.Add(segmentNode);
                }
            }

            return merged.Count > 0 && merged[merged.Count - 1] == to;
        }

        private void SyncActivationChainInspector()
        {
            m_activationChainInspector ??= new List<string>();
            m_activationChainInspector.Clear();
            m_activationChainInspector.AddRange(_activationChain);
            m_activationChainSummary = _activationChain.Count > 0
                ? string.Join(" → ", _activationChain)
                : "(空)";
        }

        private void BuildRuntimeNodes(DagGraph targetGraph)
        {
            _nodesById.Clear();
            _history.Clear();
            _activationChain.Clear();
            SyncActivationChainInspector();
            CrtSelectNode = null;

            foreach (var source in targetGraph.nodes)
            {
                if (source == null)
                {
                    continue;
                }

                var runtimeId = source.GetRuntimeId();
                if (string.IsNullOrWhiteSpace(runtimeId))
                {
                    LogCore.Warning("存在空 nodeId 的节点，已忽略");
                    continue;
                }

                if (_nodesById.ContainsKey(runtimeId))
                {
                    LogCore.Warning($"节点ID重复，后续同名节点会被忽略: {runtimeId}");
                    continue;
                }

                var runtimeNode = new DagRuntimeNode(runtimeId, source);
                runtimeNode.AutoJumpNode = ResolveAutoJumpTarget(source);
                _nodesById.Add(runtimeId, runtimeNode);
            }

            foreach (var pair in _nodesById)
            {
                var node = pair.Value;
                foreach (var outputEdge in node.SourceNode.GetOutputEdges())
                {
                    var outputNodeId = outputEdge.targetNodeId;
                    if (_nodesById.TryGetValue(outputNodeId, out var childNode))
                    {
                        node.ChildNodes.Add(childNode);
                        childNode.ParentNodes.Add(node);

                        if (outputEdge.isDefaultParent)
                        {
                            if (childNode.DefaultParentNode == null)
                            {
                                childNode.DefaultParentNode = node;
                            }
                            else if (childNode.DefaultParentNode != node)
                            {
                                LogCore.Warning(
                                    $"节点 {childNode.NodeID} 存在多条默认父边，保留 {childNode.DefaultParentNode.NodeID}，忽略 {node.NodeID}");
                            }
                        }
                    }
                }
            }

            foreach (var pair in _nodesById)
            {
                var child = pair.Value;
                if (child.ParentNodes.Count > 0 && child.DefaultParentNode == null)
                {
                    child.DefaultParentNode = child.ParentNodes[0];
                    LogCore.Warning($"节点 {child.NodeID} 未配置默认父边，已回落到 {child.DefaultParentNode.NodeID}");
                }
            }
        }

        private void SelectInitialNode()
        {
            DagRuntimeNode initialNode = null;

            if (string.IsNullOrWhiteSpace(_defaultNodeId) == false)
            {
                _nodesById.TryGetValue(_defaultNodeId.Trim(), out initialNode);
            }

            if (initialNode == null)
            {
                foreach (var pair in _nodesById)
                {
                    if (pair.Value.ParentNodes.Count == 0)
                    {
                        initialNode = pair.Value;
                        break;
                    }
                }
            }

            if (initialNode == null)
            {
                foreach (var pair in _nodesById)
                {
                    initialNode = pair.Value;
                    break;
                }
            }

            CrtSelectNode = initialNode;
            if (CrtSelectNode != null)
            {
                SetActivationChain(new List<DagRuntimeNode> { CrtSelectNode });
                LastSwitchContext = new DagSwitchContext(null, CrtSelectNode);
                Publish(DagLogicNodeEnum.NodeEnter, CrtSelectNode.NodeID);
                Publish(DagLogicNodeEnum.SwitchNode, LastSwitchContext);
            }
        }

        /// <param name="autoJumpVisited">
        /// 自动跳转环检测集合。首次外部调用传 null，内部递归时传递已访问节点集。
        /// </param>
        private void DoSwitchNode(DagRuntimeNode targetNode, bool recordHistory, HashSet<string> autoJumpVisited)
        {
            if (targetNode == null)
            {
                return;
            }

            if (TryResolveAutoJumpLanding(targetNode, autoJumpVisited, out var landingNode, out var jumpOrigins) == false)
            {
                return;
            }

            if (landingNode == CrtSelectNode)
            {
                return;
            }

            LogCore.Debug("切换到节点:" + landingNode.NodeID);
            var previousNode = CrtSelectNode;
            if (recordHistory && CrtSelectNode != null)
            {
                _history.Push(CrtSelectNode.NodeID);
            }

            if (previousNode != null)
            {
                Publish(DagLogicNodeEnum.NodeExit, previousNode.NodeID);
                PublishWithID(DagLogicNodeEnum.NodeExit, previousNode.NodeID);
            }

            var newChain = BuildActivationChainWithStops(landingNode, jumpOrigins);
            SetActivationChain(newChain);
            CrtSelectNode = landingNode;
            LastSwitchContext = new DagSwitchContext(previousNode, landingNode);
            Publish(DagLogicNodeEnum.NodeEnter, landingNode.NodeID);
            PublishWithID(DagLogicNodeEnum.NodeEnter, landingNode.NodeID);
            Publish(DagLogicNodeEnum.SwitchNode, LastSwitchContext);
        }

        /// <summary>
        /// 解析自动跳转：jumpOrigins 为途经的跳转起点（含自动跳转节点），landingNode 为最终落点。
        /// </summary>
        private bool TryResolveAutoJumpLanding(
            DagRuntimeNode entryNode,
            HashSet<string> autoJumpVisited,
            out DagRuntimeNode landingNode,
            out List<DagRuntimeNode> jumpOrigins)
        {
            jumpOrigins = new List<DagRuntimeNode>();
            landingNode = entryNode;
            if (entryNode == null)
            {
                return false;
            }

            var current = entryNode;
            while (string.IsNullOrEmpty(current.AutoJumpNode) == false)
            {
                autoJumpVisited ??= new HashSet<string>();
                if (autoJumpVisited.Add(current.NodeID) == false)
                {
                    LogCore.Error($"自动跳转检测到环：{string.Join(" → ", autoJumpVisited)} → {current.NodeID}，已中止");
                    return false;
                }

                jumpOrigins.Add(current);

                if (_nodesById.TryGetValue(current.AutoJumpNode, out var next) == false)
                {
                    LogCore.Warning($"自动跳转目标节点不存在: {current.AutoJumpNode}");
                    return false;
                }

                LogCore.Debug($"自动跳转节点:{current.NodeID} => {current.AutoJumpNode}");

                current = next;
            }

            landingNode = current;
            return true;
        }

        /// <summary>
        /// 构建到目标节点的连续链路；jumpOrigins 为自动跳转途经节点（按顺序插入）。
        /// </summary>
        private List<DagRuntimeNode> BuildActivationChainWithStops(
            DagRuntimeNode target,
            IReadOnlyList<DagRuntimeNode> jumpOrigins)
        {
            if (target == null)
            {
                return new List<DagRuntimeNode>();
            }

            if (jumpOrigins == null || jumpOrigins.Count == 0)
            {
                return BuildActivationChainTo(target);
            }

            var chain = GetActivationChainNodes();
            var anchor = chain.Count > 0 ? chain[chain.Count - 1] : CrtSelectNode;

            foreach (var origin in jumpOrigins)
            {
                if (origin == null)
                {
                    continue;
                }

                chain = MergeChainSegment(chain, anchor, origin);
                anchor = chain.Count > 0 ? chain[chain.Count - 1] : origin;
            }

            return MergeChainSegment(chain, anchor, target);
        }

        private static string ResolveAutoJumpTarget(DagNode source)
        {
            return source?.GetResolvedAutoJumpTargetId();
        }
    }
}
