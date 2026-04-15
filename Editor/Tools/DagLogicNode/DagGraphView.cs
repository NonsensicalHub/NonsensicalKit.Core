using System;
using System.Collections.Generic;
using System.Linq;
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

        public DagGraphView(DagEditorWindow window)
        {
            this.window = window;

            style.flexGrow = 1;

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // 复制粘贴
            serializeGraphElements = OnSerialize;
            unserializeAndPaste = OnPaste;

            // 右键菜单
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (window.GetGraph() == null) return;

                evt.menu.AppendAction("Create Node", a =>
                {
                    var pos = contentViewContainer.WorldToLocal(a.eventInfo.mousePosition);
                    CreateNode("New Node", pos);
                });

                evt.menu.AppendAction(
                    "Create Root Node",
                    a =>
                    {
                        var pos = contentViewContainer.WorldToLocal(a.eventInfo.mousePosition);
                        CreateNode("Root", pos, isRoot: true);
                    },
                    _ => graph.HasRootNode()
                        ? DropdownMenuAction.Status.Disabled
                        : DropdownMenuAction.Status.Normal
                );
            }));
        }

        public void PopulateView(DagGraphConfig g)
        {
            graphConfig = g;
            graph = g.DagGraph;
            if (graph == null)
            {
                graph = new DagGraph { GraphName = g.name };
                graphConfig.DagGraph = graph;
                EditorUtility.SetDirty(graphConfig);
            }

            NormalizeGraphNodeIds();
            EnsureRootNodeExists();

            graphViewChanged -= OnGraphChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphChanged;

            foreach (var node in graph.nodes)
                CreateNodeView(node);

            foreach (var node in graph.nodes)
            {
                var fromView = FindNodeView(node.nodeId);
                foreach (var outputNodeId in node.outputs)
                {
                    var toView = FindNodeView(outputNodeId);
                    if (fromView != null && toView != null)
                        AddElement(fromView.Output.ConnectTo(toView.Input));
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node).ToList();
        }

        public DagGraphConfig GetGraph() => graphConfig;

        public DagNodeView CreateNode(string nodeName, Vector2 position = default, bool isRoot = false)
        {
            if (graph == null)
            {
                return null;
            }

            if (isRoot && graph.HasRootNode())
            {
                Debug.LogWarning("图中已存在根节点，禁止新增第二个根节点");
                var existedRoot = graph.nodes.Find(n => n.isRoot);
                return existedRoot == null ? null : FindNodeView(existedRoot.nodeId);
            }

            var baseNodeId = isRoot ? "root" : (string.IsNullOrWhiteSpace(nodeName) ? "node" : nodeName.Trim());
            var node = new DagNode
            {
                nodeId = GetUniqueNodeId(baseNodeId),
                describe = nodeName,
                position = (Float2)position,
                isRoot = isRoot
            };

            graph.nodes.Add(node);
            EditorUtility.SetDirty(graphConfig);

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
            if (graph == null || node == null)
            {
                return string.Empty;
            }

            var oldNodeId = node.nodeId;
            var normalized = GetUniqueNodeId(expectedNodeId, node);
            if (oldNodeId == normalized)
            {
                return normalized;
            }

            foreach (var n in graph.nodes)
            {
                for (int i = 0; i < n.outputs.Count; i++)
                {
                    if (n.outputs[i] == oldNodeId)
                    {
                        n.outputs[i] = normalized;
                    }
                }
            }

            node.nodeId = normalized;
            EditorUtility.SetDirty(graphConfig);
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
        }

        private string OnSerialize(IEnumerable<GraphElement> elements)
        {
            var data = new CopyData();
            var nodeViews = elements.OfType<DagNodeView>().ToList();
            var nodeIdSet = new HashSet<string>(nodeViews.Select(v => v.Node.nodeId));

            foreach (var v in nodeViews)
            {
                data.nodes.Add(new DagNode
                {
                    nodeId = v.Node.nodeId,
                    describe = v.Node.describe,
                    position = v.Node.position,
                    isRoot = v.Node.isRoot,
                    outputs = v.Node.outputs.ToList()
                });
            }

            // 只保留选中范围内部的边
            foreach (var v in nodeViews)
            foreach (var outNodeId in v.Node.outputs)
                if (nodeIdSet.Contains(outNodeId))
                    data.edges.Add(new CopyEdge { from = v.Node.nodeId, to = outNodeId });

            return JsonUtility.ToJson(data);
        }

        private void OnPaste(string _, string json)
        {
            var data = JsonUtility.FromJson<CopyData>(json);
            if (data == null) return;

            const float pasteOffset = 30f;
            // old nodeId → new nodeId
            var nodeIdMap = new Dictionary<string, string>();

            ClearSelection();

            foreach (var n in data.nodes)
            {
                // 已有根节点时跳过粘贴的根节点
                if (n.isRoot && graph.HasRootNode())
                {
                    Debug.LogWarning("图中已存在根节点，跳过粘贴的根节点");
                    continue;
                }

                var newNodeId = GetUniqueNodeId($"{n.nodeId}_Copy");
                nodeIdMap[n.nodeId] = newNodeId;

                var newNode = new DagNode
                {
                    nodeId = newNodeId,
                    describe = n.describe,
                    position = (Float2)(n.position.ToVector2() + new Vector2(pasteOffset, pasteOffset)),
                    isRoot = n.isRoot,
                };

                graph.nodes.Add(newNode);
                var view = CreateNodeView(newNode);
                AddToSelection(view);
            }

            // 重建选中范围内的边
            foreach (var e in data.edges)
            {
                if (!nodeIdMap.TryGetValue(e.from, out var newFrom)) continue;
                if (!nodeIdMap.TryGetValue(e.to, out var newTo)) continue;

                var fromNode = graph.FindNode(newFrom);
                if (fromNode == null) continue;
                fromNode.outputs.Add(newTo);

                var fromView = FindNodeView(newFrom);
                var toView = FindNodeView(newTo);
                if (fromView != null && toView != null)
                    AddElement(fromView.Output.ConnectTo(toView.Input));
            }

            EditorUtility.SetDirty(graphConfig);
        }

        // ──────────────────────────────── 变更回调 ────────────────────────────────

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                var blocked = new List<Edge>();
                foreach (var edge in change.edgesToCreate)
                {
                    var from = edge.output.node as DagNodeView;
                    var to = edge.input.node as DagNodeView;
                    if (from == null || to == null)
                    {
                        blocked.Add(edge);
                        continue;
                    }

                    if (HasPath(to.Node.nodeId, from.Node.nodeId))
                    {
                        Debug.LogWarning("检测到环，禁止连接");
                        blocked.Add(edge);
                        continue;
                    }

                    from.Node.outputs.Add(to.Node.nodeId);
                }

                foreach (var e in blocked)
                    change.edgesToCreate.Remove(e);

                if (change.edgesToCreate.Count > 0)
                    EditorUtility.SetDirty(graphConfig);
            }

            if (change.movedElements != null)
            {
                foreach (var e in change.movedElements)
                    if (e is DagNodeView nodeView)
                        nodeView.Node.position = (Float2)nodeView.GetPosition().position;

                EditorUtility.SetDirty(graphConfig);
            }

            if (change.elementsToRemove != null)
            {
                var blockedRemovals = new List<GraphElement>();
                foreach (var e in change.elementsToRemove)
                {
                    if (e is Edge edge)
                    {
                        var from = edge.output.node as DagNodeView;
                        var to = edge.input.node as DagNodeView;
                        if (from == null || to == null)
                        {
                            continue;
                        }

                        if (from.Node.isRoot || to.Node.isRoot)
                        {
                            if (change.elementsToRemove.OfType<DagNodeView>().Any(n => n.Node.isRoot))
                            {
                                blockedRemovals.Add(edge);
                                continue;
                            }
                        }

                        from.Node.outputs.Remove(to.Node.nodeId);
                    }
                    else if (e is DagNodeView nodeView)
                    {
                        if (nodeView.Node.isRoot)
                        {
                            Debug.LogWarning("根节点不可删除");
                            blockedRemovals.Add(nodeView);
                            continue;
                        }

                        foreach (var node in graph.nodes)
                        {
                            node.outputs.RemoveAll(o => o == nodeView.Node.nodeId);
                        }

                        graph.nodes.Remove(nodeView.Node);
                    }
                }

                foreach (var blocked in blockedRemovals)
                {
                    change.elementsToRemove.Remove(blocked);
                }

                EditorUtility.SetDirty(graphConfig);
            }

            return change;
        }

        // ──────────────────────────────── DAG 防环 ────────────────────────────────

        private bool HasPath(string from, string target)
        {
            return DFS(from, target, new HashSet<string>());
        }

        /// <summary>
        /// 深度优先搜索
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private bool DFS(string current, string target, HashSet<string> visited)
        {
            if (current == target) return true;
            visited.Add(current);

            var node = graph.FindNode(current);
            if (node == null) return false;

            foreach (var next in node.outputs)
                if (!visited.Contains(next) && DFS(next, target, visited))
                    return true;

            return false;
        }

        private void EnsureRootNodeExists()
        {
            if (graph.HasRootNode())
            {
                return;
            }

            CreateNode("Root", Vector2.zero, true);
        }

        private void NormalizeGraphNodeIds()
        {
            if (graph == null || graph.nodes == null)
            {
                return;
            }

            var usedNodeIds = new HashSet<string>();
            bool changed = false;
            bool hasRoot = false;

            foreach (var node in graph.nodes)
            {
                if (node.isRoot)
                {
                    if (hasRoot)
                    {
                        node.isRoot = false;
                        changed = true;
                    }
                    else
                    {
                        hasRoot = true;
                    }
                }

                var oldNodeId = node.nodeId;
                var normalized = NormalizeId(node.nodeId, node.isRoot ? "root" : "node");
                if (usedNodeIds.Contains(normalized))
                {
                    normalized = BuildUniqueId(normalized, usedNodeIds);
                }

                usedNodeIds.Add(normalized);
                if (oldNodeId != normalized)
                {
                    foreach (var source in graph.nodes)
                    {
                        for (int i = 0; i < source.outputs.Count; i++)
                        {
                            if (source.outputs[i] == oldNodeId)
                            {
                                source.outputs[i] = normalized;
                            }
                        }
                    }

                    node.nodeId = normalized;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(graphConfig);
            }
        }

        private string GetUniqueNodeId(string expectedNodeId, DagNode self = null)
        {
            var normalized = NormalizeId(expectedNodeId, "node");
            if (graph == null)
            {
                return normalized;
            }

            var usedNodeIds = new HashSet<string>(
                graph.nodes
                    .Where(n => n != null && n != self)
                    .Select(n => n.nodeId)
                    .Where(id => string.IsNullOrWhiteSpace(id) == false)
            );

            return usedNodeIds.Contains(normalized)
                ? BuildUniqueId(normalized, usedNodeIds)
                : normalized;
        }

        private static string NormalizeId(string rawNodeId, string fallback)
        {
            if (string.IsNullOrWhiteSpace(rawNodeId))
            {
                return fallback;
            }

            return rawNodeId.Trim();
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