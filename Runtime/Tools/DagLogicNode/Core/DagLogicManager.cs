using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;

namespace NonsensicalKit.Core.DagLogicNode
{
    public enum DagNodeCheckType
    {
        SelfSelect,
        SelfUnselect,
        ParentSelect,
        ParentUnselect,
        ChildSelect,
        ChildUnselect,
        ParentOrChildSelect,
        ParentOrChildUnselect
    }

    public sealed class DagRuntimeNode
    {
        public string NodeID;
        public string AutoJumpNode;
        public DagNode SourceNode;
        public readonly List<DagRuntimeNode> ParentNodes = new List<DagRuntimeNode>();
        public readonly List<DagRuntimeNode> ChildNodes = new List<DagRuntimeNode>();

        public DagRuntimeNode(string nodeID, DagNode sourceNode)
        {
            NodeID = nodeID;
            SourceNode = sourceNode;
        }
    }

    public class DagLogicManager : NonsensicalMono, IMonoService
    {
        public DagRuntimeNode CrtSelectNode { get; private set; }
        public bool IsReady { get; private set; }
        Action IService.InitCompleted { get; set; }

        public event Action InitCompleted;

        private readonly Dictionary<string, DagRuntimeNode> _nodesById = new Dictionary<string, DagRuntimeNode>();
        private readonly Stack<string> _history = new Stack<string>();
        private string _switchBuffer;

        public void InitGraph(DagGraph targetGraph)
        {
            if (targetGraph == null)
            {
                LogCore.Error("DagLogicManager 初始化失败：DagGraph 为空");
                return;
            }

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
                InitCompleted?.Invoke();
                InitCompleted = null;
            }
            else
            {
                Publish(DagLogicNodeEnum.SwitchNode, CrtSelectNode);
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

            DoSwitchNode(targetNode, true);
            return true;
        }

        public bool ReturnPreviousLevel()
        {
            while (_history.Count > 0)
            {
                var previousId = _history.Pop();
                if (_nodesById.TryGetValue(previousId, out var node))
                {
                    DoSwitchNode(node, false);
                    return true;
                }
            }

            return false;
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

        private void BuildRuntimeNodes(DagGraph targetGraph)
        {
            _nodesById.Clear();
            _history.Clear();
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

                _nodesById.Add(runtimeId, new DagRuntimeNode(runtimeId, source));
            }

            foreach (var pair in _nodesById)
            {
                var node = pair.Value;
                foreach (var outputNodeId in node.SourceNode.outputs)
                {
                    if (_nodesById.TryGetValue(outputNodeId, out var childNode))
                    {
                        node.ChildNodes.Add(childNode);
                        childNode.ParentNodes.Add(node);
                    }
                }
            }
        }

        private void SelectInitialNode()
        {
            DagRuntimeNode rootNode = null;

            foreach (var pair in _nodesById)
            {
                if (pair.Value.SourceNode.isRoot)
                {
                    rootNode = pair.Value;
                    break;
                }
            }

            if (rootNode == null)
            {
                foreach (var pair in _nodesById)
                {
                    if (pair.Value.ParentNodes.Count == 0)
                    {
                        rootNode = pair.Value;
                        break;
                    }
                }
            }

            if (rootNode == null)
            {
                foreach (var pair in _nodesById)
                {
                    rootNode = pair.Value;
                    break;
                }
            }

            CrtSelectNode = rootNode;
            if (CrtSelectNode != null)
            {
                Publish(DagLogicNodeEnum.NodeEnter, CrtSelectNode.NodeID);
                Publish(DagLogicNodeEnum.SwitchNode, CrtSelectNode);
            }
        }

        private void DoSwitchNode(DagRuntimeNode targetNode, bool recordHistory)
        {
            if (targetNode == null)
            {
                return;
            }
            if (string.IsNullOrEmpty( targetNode.AutoJumpNode)==false)
            {
                LogCore.Debug($"自动跳转节点:{targetNode.NodeID} => {targetNode.AutoJumpNode}");
                SwitchNode(targetNode.AutoJumpNode);
                return;
            }

            if (targetNode == CrtSelectNode)
            {
                return;
            }

            LogCore.Debug("切换到节点:" + targetNode.NodeID);
            if (recordHistory && CrtSelectNode != null)
            {
                _history.Push(CrtSelectNode.NodeID);
            }

            if (CrtSelectNode != null)
            {
                Publish(DagLogicNodeEnum.NodeExit, CrtSelectNode.NodeID);
            }

            CrtSelectNode = targetNode;
            Publish(DagLogicNodeEnum.NodeEnter, targetNode.NodeID);
            Publish(DagLogicNodeEnum.SwitchNode, targetNode);
        }
    }
}
