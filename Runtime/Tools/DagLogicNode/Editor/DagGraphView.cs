using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core;
using NonsensicalKit.Core.DagLogicNode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonsensicalKit.Core.DagLogicNode.Editor
{
    public class DagGraphView : GraphView
    {
        private DagEditorWindow window;
        private DagGraphConfig graphConfig;
        private DagGraph graph;
        private DagAutoJumpOverlay _autoJumpOverlay;

        public DagGraphView(DagEditorWindow window)
        {
            this.window = window;

            style.flexGrow = 1;

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            serializeGraphElements = OnSerialize;
            unserializeAndPaste = OnPaste;

            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (window.GetGraph() == null) return;

                evt.menu.AppendAction("Create Node", a =>
                {
                    var pos = PanelPointToLocal(contentViewContainer, a.eventInfo.mousePosition);
                    CreateNode("New Node", pos);
                });
            }));
        }

        private static Vector2 PanelPointToLocal(VisualElement element, Vector2 panelPoint)
        {
            var local = element.worldTransform.inverse.MultiplyPoint3x4(
                new Vector3(panelPoint.x, panelPoint.y, 0f));
            return new Vector2(local.x, local.y);
        }

        // ──────────────────────────────── Undo ────────────────────────────────

        private void RecordUndo(string actionName)
        {
            if (graphConfig != null)
                Undo.RecordObject(graphConfig, actionName);
        }

        private void MarkDirty()
        {
            if (graphConfig != null)
                EditorUtility.SetDirty(graphConfig);
        }

        // ──────────────────────────────── 初始化 ────────────────────────────────

        public void PopulateView(DagGraphConfig g)
        {
            graphConfig = g;
            graph = g.DagGraph;
            if (graph == null)
            {
                graph = new DagGraph { GraphName = g.name };
                graphConfig.DagGraph = graph;
                MarkDirty();
            }

            NormalizeGraphNodeIds();
            graph.EnsureAllDefaultParents();

            graphViewChanged -= OnGraphChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphChanged;

            foreach (var node in graph.nodes)
            {
                node.EnsureEdgeData();
                CreateNodeView(node);
            }

            foreach (var node in graph.nodes)
            {
                var fromView = FindNodeView(node.nodeId);
                foreach (var edge in node.GetOutputEdges())
                {
                    var toView = FindNodeView(edge.targetNodeId);
                    if (fromView != null && toView != null)
                        CreateNormalDagEdge(fromView, toView, edge.isDefaultParent);
                }
            }

            foreach (var node in graph.nodes)
            {
                if (!node.autoJumpEnabled
                    || string.IsNullOrWhiteSpace(node.autoJumpTargetNodeId)) continue;
                var fromView = FindNodeView(node.nodeId);
                var toView = FindNodeView(node.autoJumpTargetNodeId.Trim());
                if (fromView != null && toView != null)
                    CreateAutoJumpDagEdge(fromView, toView);
            }

            EnsureAutoJumpOverlay();
            SyncAllEdgeVisualsFromData();
            RefreshAllNodeVisuals();
        }

        private void EnsureAutoJumpOverlay()
        {
            if (_autoJumpOverlay != null)
            {
                return;
            }

            _autoJumpOverlay = new DagAutoJumpOverlay(this);
            contentViewContainer.Insert(0, _autoJumpOverlay);
        }

        private void RefreshAutoJumpOverlay()
        {
            _autoJumpOverlay?.MarkDirtyRepaint();
            ApplyAutoJumpEdgeHiding();
        }

        internal void ApplyAutoJumpEdgeHiding()
        {
            foreach (var edge in edges)
            {
                if (edge?.output == null || !DagNodeView.IsAutoJumpPort(edge.output))
                {
                    continue;
                }

                edge.style.opacity = 0f;
                if (edge.edgeControl != null)
                {
                    edge.edgeControl.style.opacity = 0f;
                }
            }
        }

        public DagGraphConfig GetGraph() => graphConfig;

        // ──────────────────────────────── 端口兼容 ────────────────────────────────

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var startIsAutoJump = DagNodeView.IsAutoJumpPort(startPort);
            return EnumerateAllPorts().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node &&
                DagNodeView.IsAutoJumpPort(p) == startIsAutoJump).ToList();
        }

        private IEnumerable<Port> EnumerateAllPorts()
        {
            foreach (var node in nodes.ToList())
            {
                if (node is DagNodeView dagNode)
                {
                    if (dagNode.Input != null) yield return dagNode.Input;
                    if (dagNode.Output != null) yield return dagNode.Output;
                    if (dagNode.AutoJumpInput != null) yield return dagNode.AutoJumpInput;
                    if (dagNode.AutoJumpOutput != null) yield return dagNode.AutoJumpOutput;
                    continue;
                }

                foreach (var port in node.inputContainer.Children().OfType<Port>())
                {
                    yield return port;
                }

                foreach (var port in node.outputContainer.Children().OfType<Port>())
                {
                    yield return port;
                }
            }
        }

        // ──────────────────────────────── 节点查询 ────────────────────────────────

        public bool IsRootNode(DagNode node) =>
            graph != null && node != null && graph.IsRootNode(node.nodeId);

        public bool IsDefaultNode(DagNode node) =>
            graph != null && node != null && graph.IsDefaultNode(node.nodeId);

        public string GetNodeTitleFallback(DagNode node)
        {
            if (IsDefaultNode(node)) return "默认";
            return IsRootNode(node) ? "Root" : "Node";
        }

        // ──────────────────────────────── 右键菜单 ────────────────────────────────

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            var nodeView = FindNodeViewFromTarget(evt.target as VisualElement);
            if (nodeView != null && graph != null)
            {
                var isDefault = graph.IsDefaultNode(nodeView.Node.nodeId);
                evt.menu.AppendAction(
                    isDefault ? "已是默认节点" : "设为默认节点",
                    _ => SetDefaultNode(nodeView),
                    _ => isDefault
                        ? DropdownMenuAction.Status.Disabled
                        : DropdownMenuAction.Status.Normal);

                var hasAutoJump = nodeView.Node.HasAutoJumpConfigured();
                evt.menu.AppendAction(
                    "清除自动跳转",
                    _ => ClearAutoJumpFromNode(nodeView),
                    _ => hasAutoJump
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
                return;
            }

            if (!(evt.target is Edge edge))
            {
                return;
            }

            if (edge is DagDefaultParentEdge dagEdge && dagEdge.IsAutoJump)
            {
                evt.menu.AppendAction("删除跳转连线", _ =>
                {
                    var from = edge.output?.node as DagNodeView;
                    if (from != null)
                    {
                        ClearAutoJumpFromNode(from);
                    }
                });
                return;
            }

            var fromNode = edge.output?.node as DagNodeView;
            var toNode = edge.input?.node as DagNodeView;
            if (fromNode == null || toNode == null) return;

            var outputEdge = fromNode.Node.FindOutputEdge(toNode.Node.nodeId);
            if (outputEdge == null) return;

            if (outputEdge.isDefaultParent)
            {
                evt.menu.AppendAction("已是默认父边", null, _ => DropdownMenuAction.Status.Disabled);
            }
            else
            {
                evt.menu.AppendAction("设为默认父边", _ =>
                {
                    RecordUndo("设为默认父边");
                    graph.EnsureDefaultParentForChild(toNode.Node.nodeId, fromNode.Node.nodeId);
                    SyncAllEdgeVisualsFromData();
                    MarkDirty();
                });
            }
        }

        // ──────────────────────────────── 节点创建 ────────────────────────────────

        public DagNodeView CreateNode(string nodeName, Vector2 position = default)
        {
            if (graph == null) return null;

            RecordUndo("创建节点");
            var baseNodeId = string.IsNullOrWhiteSpace(nodeName) ? "node" : nodeName.Trim();
            var node = new DagNode
            {
                nodeId = GetUniqueNodeId(baseNodeId),
                describe = nodeName,
                position = (Float2)position
            };

            graph.nodes.Add(node);
            MarkDirty();
            return CreateNodeView(node);
        }

        private DagNodeView CreateNodeView(DagNode node)
        {
            var view = new DagNodeView(node, this);
            view.Init();
            view.SetPosition(new Rect(node.position.ToVector2(), new Vector2(160, 0)));
            AddElement(view);
            return view;
        }

        private DagNodeView FindNodeView(string nodeId)
        {
            return nodes.ToList().OfType<DagNodeView>().FirstOrDefault(v => v.Node.nodeId == nodeId);
        }

        public string ValidateAndApplyNodeId(DagNode node, string expectedNodeId)
        {
            if (graph == null || node == null) return string.Empty;

            var oldNodeId = node.nodeId;
            var normalized = GetUniqueNodeId(expectedNodeId, node);
            if (oldNodeId == normalized) return normalized;

            RecordUndo("重命名节点");
            foreach (var n in graph.nodes)
            {
                n.ReplaceOutputTarget(oldNodeId, normalized);
                if (n.autoJumpEnabled && n.autoJumpTargetNodeId == oldNodeId)
                    n.autoJumpTargetNodeId = normalized;
            }

            graph.RenameDefaultNodeId(oldNodeId, normalized);
            node.nodeId = normalized;
            MarkDirty();
            return normalized;
        }

        // ──────────────────────────────── 复制粘贴 ────────────────────────────────

        [Serializable]
        private class CopyData
        {
            public List<DagNode> nodes = new List<DagNode>();
            public List<CopyEdge> edges = new List<CopyEdge>();
        }

        [Serializable]
        private class CopyEdge
        {
            public string from;
            public string to;
            public bool isDefaultParent;
        }

        private string OnSerialize(IEnumerable<GraphElement> elements)
        {
            var data = new CopyData();
            var nodeViews = elements.OfType<DagNodeView>().ToList();
            var nodeIdSet = new HashSet<string>(nodeViews.Select(v => v.Node.nodeId));

            foreach (var v in nodeViews)
            {
                var keepAutoJump = v.Node.autoJumpEnabled
                                   && nodeIdSet.Contains(v.Node.autoJumpTargetNodeId ?? "");
                data.nodes.Add(new DagNode
                {
                    nodeId = v.Node.nodeId,
                    describe = v.Node.describe,
                    position = v.Node.position,
                    autoJumpEnabled = keepAutoJump,
                    autoJumpTargetNodeId = keepAutoJump ? v.Node.autoJumpTargetNodeId : null,
                    outputEdges = v.Node.GetOutputEdges()
                        .Select(edge => new DagOutputEdge(edge.targetNodeId, edge.isDefaultParent))
                        .ToList()
                });
            }

            foreach (var v in nodeViews)
            foreach (var edge in v.Node.GetOutputEdges())
                if (nodeIdSet.Contains(edge.targetNodeId))
                    data.edges.Add(new CopyEdge
                    {
                        from = v.Node.nodeId,
                        to = edge.targetNodeId,
                        isDefaultParent = edge.isDefaultParent
                    });

            return JsonUtility.ToJson(data);
        }

        private void OnPaste(string _, string json)
        {
            var data = JsonUtility.FromJson<CopyData>(json);
            if (data == null) return;

            RecordUndo("粘贴节点");

            const float pasteOffset = 30f;
            var nodeIdMap = new Dictionary<string, string>();

            ClearSelection();

            foreach (var n in data.nodes)
            {
                var newNodeId = GetUniqueNodeId($"{n.nodeId}_Copy");
                nodeIdMap[n.nodeId] = newNodeId;

                string mappedTarget=string.Empty;
                
                var hasMappedTarget = n.autoJumpEnabled
                                     && !string.IsNullOrWhiteSpace(n.autoJumpTargetNodeId)
                                     && nodeIdMap.TryGetValue(n.autoJumpTargetNodeId.Trim(), out  mappedTarget);
            
                var newNode = new DagNode
                {
                    nodeId = newNodeId,
                    describe = n.describe,
                    position = (Float2)(n.position.ToVector2() + new Vector2(pasteOffset, pasteOffset)),
                    autoJumpEnabled = hasMappedTarget,
                    autoJumpTargetNodeId = hasMappedTarget ? mappedTarget : null
                };

                graph.nodes.Add(newNode);
                var view = CreateNodeView(newNode);
                AddToSelection(view);
            }

            foreach (var e in data.edges)
            {
                if (!nodeIdMap.TryGetValue(e.from, out var newFrom)) continue;
                if (!nodeIdMap.TryGetValue(e.to, out var newTo)) continue;

                var fromNode = graph.FindNode(newFrom);
                if (fromNode == null) continue;
                fromNode.AddOutputEdge(newTo);
                graph.EnsureDefaultParentForChild(newTo, e.isDefaultParent ? newFrom : null);

                var fromView = FindNodeView(newFrom);
                var toView = FindNodeView(newTo);
                if (fromView != null && toView != null)
                {
                    var outputEdgeData = fromNode.FindOutputEdge(newTo);
                    CreateNormalDagEdge(fromView, toView, outputEdgeData != null && outputEdgeData.isDefaultParent);
                }
            }

            foreach (var n in data.nodes)
            {
                if (!n.autoJumpEnabled
                    || string.IsNullOrWhiteSpace(n.autoJumpTargetNodeId)) continue;
                if (!nodeIdMap.TryGetValue(n.nodeId, out var newFromId)) continue;
                var newNode = graph.FindNode(newFromId);
                if (newNode == null || !newNode.autoJumpEnabled
                    || string.IsNullOrWhiteSpace(newNode.autoJumpTargetNodeId)) continue;

                var fromView = FindNodeView(newFromId);
                var toView = FindNodeView(newNode.autoJumpTargetNodeId.Trim());
                if (fromView != null && toView != null)
                    CreateAutoJumpDagEdge(fromView, toView);
            }

            graph.EnsureAllDefaultParents();
            SyncAllEdgeVisualsFromData();
            RefreshAllNodeVisuals();
            MarkDirty();
        }

        // ──────────────────────────────── 变更回调 ────────────────────────────────

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                RecordUndo("连边");
                var blocked = new List<Edge>();
                var edgesChanged = false;

                foreach (var edge in change.edgesToCreate.ToList())
                {
                    var from = edge.output?.node as DagNodeView;
                    var to = edge.input?.node as DagNodeView;
                    if (from == null || to == null)
                    {
                        blocked.Add(edge);
                        continue;
                    }

                    if (IsAutoJumpEditorEdge(edge))
                    {
                        blocked.Add(edge);

                        RemoveAutoJumpEdgeFrom(from);
                        from.Node.SetAutoJumpToNode(to.Node.nodeId);

                        CreateAutoJumpDagEdge(from, to);
                        edgesChanged = true;
                        continue;
                    }

                    if (HasPath(to.Node.nodeId, from.Node.nodeId))
                    {
                        Debug.LogWarning("检测到环，禁止连接");
                        blocked.Add(edge);
                        continue;
                    }

                    if (from.Node.AddOutputEdge(to.Node.nodeId) == false)
                    {
                        blocked.Add(edge);
                    }
                    else
                    {
                        graph.EnsureDefaultParentForChild(to.Node.nodeId);
                        var outputEdgeData = from.Node.FindOutputEdge(to.Node.nodeId);
                        var isDefaultParent = outputEdgeData != null && outputEdgeData.isDefaultParent;
                        change.edgesToCreate.Remove(edge);
                        CreateNormalDagEdge(from, to, isDefaultParent, edge.output, edge.input);
                        edgesChanged = true;
                    }
                }

                foreach (var e in blocked)
                    change.edgesToCreate.Remove(e);

                if (edgesChanged)
                {
                    graph.EnsureAllDefaultParents();
                    SyncAllEdgeVisualsFromData();
                    MarkDirty();
                }
            }

            if (change.movedElements != null)
            {
                RecordUndo("移动节点");
                foreach (var e in change.movedElements)
                    if (e is DagNodeView nodeView)
                        nodeView.Node.position = (Float2)nodeView.GetPosition().position;

                MarkDirty();
            }

            if (change.elementsToRemove != null)
            {
                RecordUndo("删除元素");
                var affectedChildIds = new HashSet<string>();
                foreach (var e in change.elementsToRemove)
                {
                    if (e is DagDefaultParentEdge dagEdge)
                    {
                        var from = dagEdge.output?.node as DagNodeView;
                        var to = dagEdge.input?.node as DagNodeView;
                        if (from == null || to == null) continue;

                        if (dagEdge.IsAutoJump)
                        {
                            from.Node.ClearAutoJump();
                        }
                        else
                        {
                            from.Node.RemoveOutputEdge(to.Node.nodeId);
                            affectedChildIds.Add(to.Node.nodeId);
                        }
                    }
                    else if (e is Edge plainEdge)
                    {
                        var from = plainEdge.output?.node as DagNodeView;
                        var to = plainEdge.input?.node as DagNodeView;
                        if (from == null || to == null) continue;

                        if (DagNodeView.IsAutoJumpPort(plainEdge.output))
                        {
                            from.Node.ClearAutoJump();
                        }
                        else
                        {
                            from.Node.RemoveOutputEdge(to.Node.nodeId);
                            affectedChildIds.Add(to.Node.nodeId);
                        }
                    }
                    else if (e is DagNodeView nodeView)
                    {
                        var deletedNodeId = nodeView.Node.nodeId;
                        foreach (var outgoing in nodeView.Node.GetOutputEdges())
                            affectedChildIds.Add(outgoing.targetNodeId);

                        RemoveEdgesConnectedToNode(deletedNodeId);

                        foreach (var node in graph.nodes)
                        {
                            node.RemoveOutputEdge(deletedNodeId);
                            if (node.autoJumpTargetNodeId == deletedNodeId)
                                node.ClearAutoJump();
                        }

                        graph.ClearDefaultNodeIf(deletedNodeId);
                        graph.nodes.Remove(nodeView.Node);
                    }
                }

                foreach (var childId in affectedChildIds)
                    graph.EnsureDefaultParentForChild(childId);

                if (affectedChildIds.Count > 0 || change.elementsToRemove.Count > 0)
                {
                    graph.EnsureAllDefaultParents();
                    SyncAllEdgeVisualsFromData();
                    RefreshAllNodeVisuals();
                    MarkDirty();
                }
            }

            return change;
        }

        // ──────────────────────────────── 自动跳转管理 ────────────────────────────────

        private void ClearAutoJumpFromNode(DagNodeView nodeView)
        {
            RecordUndo("清除自动跳转");
            nodeView.Node.ClearAutoJump();
            RemoveAutoJumpEdgeFrom(nodeView);
            RefreshAutoJumpOverlay();
            MarkDirty();
        }

        private void RemoveAutoJumpEdgeFrom(DagNodeView fromView)
        {
            foreach (var edge in edges.ToList())
            {
                if (edge is DagDefaultParentEdge dagEdge && dagEdge.IsAutoJump &&
                    edge.output?.node == fromView)
                {
                    RemoveElement(edge);
                }
            }
        }

        private DagDefaultParentEdge CreateAutoJumpDagEdge(DagNodeView fromView, DagNodeView toView)
        {
            var edge = new DagDefaultParentEdge
            {
                output = fromView.AutoJumpOutput,
                input = toView.AutoJumpInput,
                tooltip = "自动跳转"
            };
            edge.SetVisualStyle(false, true);
            AddElement(edge);
            return edge;
        }

        private DagDefaultParentEdge CreateNormalDagEdge(
            DagNodeView fromView,
            DagNodeView toView,
            bool isDefaultParent,
            Port outputPort = null,
            Port inputPort = null)
        {
            var edge = new DagDefaultParentEdge
            {
                output = outputPort ?? fromView.Output,
                input = inputPort ?? toView.Input,
                tooltip = isDefaultParent ? "默认父边" : string.Empty
            };
            edge.SetVisualStyle(isDefaultParent, false);
            AddElement(edge);
            return edge;
        }

        // ──────────────────────────────── DAG 防环 ────────────────────────────────

        private bool HasPath(string from, string target)
        {
            return DFS(from, target, new HashSet<string>());
        }

        private bool DFS(string current, string target, HashSet<string> visited)
        {
            if (current == target) return true;
            visited.Add(current);

            var node = graph.FindNode(current);
            if (node == null) return false;

            foreach (var edge in node.GetOutputEdges())
                if (!visited.Contains(edge.targetNodeId) && DFS(edge.targetNodeId, target, visited))
                    return true;

            return false;
        }

        // ──────────────────────────────── 边样式同步 ────────────────────────────────

        public void SyncGraphEdgeVisuals() => SyncAllEdgeVisualsFromData();

        private void SyncAllEdgeVisualsFromData()
        {
            if (graph == null) return;

            foreach (var edge in edges.ToList())
            {
                if (!(edge is DagDefaultParentEdge dagEdge)) continue;

                var from = edge.output?.node as DagNodeView;
                var to = edge.input?.node as DagNodeView;
                if (from == null || to == null) continue;

                if (dagEdge.IsAutoJump)
                {
                    dagEdge.SetVisualStyle(false, true);
                }
                else
                {
                    var outputEdge = from.Node.FindOutputEdge(to.Node.nodeId);
                    var isDefaultParent = outputEdge != null && outputEdge.isDefaultParent;
                    dagEdge.tooltip = isDefaultParent ? "默认父边" : string.Empty;
                    dagEdge.SetVisualStyle(isDefaultParent, false);
                }
            }

            RefreshAutoJumpOverlay();
        }

        // ──────────────────────────────── 节点样式 ────────────────────────────────

        public void RefreshAllNodeVisualsPublic() => RefreshAllNodeVisuals();

        private void RefreshAllNodeVisuals()
        {
            foreach (var nodeView in nodes.OfType<DagNodeView>())
                nodeView.ApplyNodeVisual();
        }

        private void SetDefaultNode(DagNodeView nodeView)
        {
            if (graph == null || nodeView == null) return;
            RecordUndo("设为默认节点");
            graph.SetDefaultNode(nodeView.Node.nodeId);
            RefreshAllNodeVisuals();
            MarkDirty();
        }

        // ──────────────────────────────── 辅助方法 ────────────────────────────────

        private static DagNodeView FindNodeViewFromTarget(VisualElement element)
        {
            while (element != null)
            {
                if (element is DagNodeView nodeView) return nodeView;
                element = element.parent;
            }

            return null;
        }

        private static bool IsAutoJumpEditorEdge(Edge edge) =>
            edge?.output != null && DagNodeView.IsAutoJumpPort(edge.output);

        private void RemoveEdgesConnectedToNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return;

            foreach (var edge in edges.ToList())
            {
                var from = edge.output?.node as DagNodeView;
                var to = edge.input?.node as DagNodeView;
                if (from?.Node.nodeId == nodeId || to?.Node.nodeId == nodeId)
                    RemoveElement(edge);
            }
        }

        // ──────────────────────────────── NodeId 管理 ────────────────────────────────

        private void NormalizeGraphNodeIds()
        {
            if (graph == null || graph.nodes == null) return;

            var usedNodeIds = new HashSet<string>();
            bool changed = false;

            foreach (var node in graph.nodes)
            {
                var oldNodeId = node.nodeId;
                var normalized = NormalizeId(node.nodeId, "node");
                if (usedNodeIds.Contains(normalized))
                    normalized = BuildUniqueId(normalized, usedNodeIds);

                usedNodeIds.Add(normalized);
                if (oldNodeId != normalized)
                {
                    foreach (var source in graph.nodes)
                        source.ReplaceOutputTarget(oldNodeId, normalized);

                    graph.RenameDefaultNodeId(oldNodeId, normalized);
                    node.nodeId = normalized;
                    changed = true;
                }
            }

            if (changed) MarkDirty();
        }

        private string GetUniqueNodeId(string expectedNodeId, DagNode self = null)
        {
            var normalized = NormalizeId(expectedNodeId, "node");
            if (graph == null) return normalized;

            var usedNodeIds = new HashSet<string>(
                graph.nodes
                    .Where(n => n != null && n != self)
                    .Select(n => n.nodeId)
                    .Where(id => !string.IsNullOrWhiteSpace(id)));

            return usedNodeIds.Contains(normalized)
                ? BuildUniqueId(normalized, usedNodeIds)
                : normalized;
        }

        private static string NormalizeId(string rawNodeId, string fallback)
        {
            return string.IsNullOrWhiteSpace(rawNodeId) ? fallback : rawNodeId.Trim();
        }

        private static string BuildUniqueId(string baseId, HashSet<string> usedNodeIds)
        {
            int index = 1;
            string candidate = baseId;
            while (usedNodeIds.Contains(candidate))
            {
                candidate = $"{baseId}_{index}";
                index++;
            }

            return candidate;
        }
    }
}
