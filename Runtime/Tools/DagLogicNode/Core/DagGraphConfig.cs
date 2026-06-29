using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Core.DagLogicNode
{
    [CreateAssetMenu(fileName = "New DAG", menuName = "ScriptableObjects/DAG")]
    public class DagGraphConfig : ConfigObject
    {
        public DagGraph DagGraph;

        public override ConfigData GetData()
        {
            return DagGraph;
        }

        public override void SetData(ConfigData cd)
        {
            if (cd is DagGraph graph)
            {
                DagGraph = graph;
            }
        }
    }

    [Serializable]
    public class DagGraph : ConfigData
    {
        public string GraphName;

        [Tooltip("图初始化时进入的默认节点 ID")]
        public string defaultNodeId;

        public List<DagNode> nodes = new List<DagNode>();

        public DagNode FindNode(string nodeId) => nodes.Find(n => n.nodeId == nodeId);

        public bool IsRootNode(string nodeId) => GetParentCount(nodeId) == 0;

        public bool IsDefaultNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(defaultNodeId) || string.IsNullOrWhiteSpace(nodeId))
            {
                return false;
            }

            return defaultNodeId.Trim() == nodeId.Trim();
        }

        public void SetDefaultNode(string nodeId)
        {
            defaultNodeId = string.IsNullOrWhiteSpace(nodeId) ? null : nodeId.Trim();
        }

        public void ClearDefaultNodeIf(string nodeId)
        {
            if (IsDefaultNode(nodeId))
            {
                defaultNodeId = null;
            }
        }

        public void RenameDefaultNodeId(string oldNodeId, string newNodeId)
        {
            if (IsDefaultNode(oldNodeId))
            {
                SetDefaultNode(newNodeId);
            }
        }

        /// <summary>
        /// 确保每个「有父节点」的子节点，恰有一条入边被标记为默认父边。
        /// </summary>
        public void EnsureAllDefaultParents()
        {
            if (nodes == null)
            {
                return;
            }

            var childIds = new HashSet<string>();
            foreach (var node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                foreach (var edge in node.GetOutputEdges())
                {
                    childIds.Add(edge.targetNodeId);
                }
            }

            foreach (var childId in childIds)
            {
                EnsureDefaultParentForChild(childId);
            }
        }

        /// <summary>
        /// 为指定子节点保证唯一默认父边；可指定优先父节点（切换默认父时使用）。
        /// </summary>
        public void EnsureDefaultParentForChild(string childNodeId, string preferredParentNodeId = null)
        {
            if (string.IsNullOrWhiteSpace(childNodeId) || nodes == null)
            {
                return;
            }

            var childId = childNodeId.Trim();
            var incoming = new List<(DagNode parent, DagOutputEdge edge)>();
            foreach (var parent in nodes)
            {
                if (parent == null)
                {
                    continue;
                }

                foreach (var edge in parent.GetOutputEdges())
                {
                    if (edge.targetNodeId == childId)
                    {
                        incoming.Add((parent, edge));
                    }
                }
            }

            if (incoming.Count == 0)
            {
                return;
            }

            var preferredId = string.IsNullOrWhiteSpace(preferredParentNodeId)
                ? null
                : preferredParentNodeId.Trim();
            var previouslyMarked = incoming.Where(pair => pair.edge.isDefaultParent).ToList();

            DagOutputEdge selectedEdge = null;
            if (string.IsNullOrEmpty(preferredId) == false)
            {
                var preferredIndex = incoming.FindIndex(pair => pair.parent.nodeId == preferredId);
                if (preferredIndex >= 0)
                {
                    selectedEdge = incoming[preferredIndex].edge;
                }
            }
            else if (previouslyMarked.Count == 1)
            {
                selectedEdge = previouslyMarked[0].edge;
            }
            else
            {
                selectedEdge = incoming[0].edge;
            }

            foreach (var pair in incoming)
            {
                pair.edge.isDefaultParent = pair.edge == selectedEdge;
            }
        }

        public int GetParentCount(string childNodeId)
        {
            if (string.IsNullOrWhiteSpace(childNodeId) || nodes == null)
            {
                return 0;
            }

            var childId = childNodeId.Trim();
            var count = 0;
            foreach (var parent in nodes)
            {
                if (parent?.FindOutputEdge(childId) != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    [Serializable]
    public class DagOutputEdge
    {
        public string targetNodeId;
        public bool isDefaultParent;

        public DagOutputEdge()
        {
        }

        public DagOutputEdge(string targetNodeId, bool isDefaultParent = false)
        {
            this.targetNodeId = targetNodeId;
            this.isDefaultParent = isDefaultParent;
        }
    }

    [Serializable]
    public class DagNode
    {
        public string nodeId;
        public string describe;
        public Float2 position;

        [Tooltip("进入本节点后是否自动跳转到目标节点")]
        [FormerlySerializedAs("autoJumpType")]
        public bool autoJumpEnabled;

        [Tooltip("autoJumpEnabled 为 true 时的目标节点 ID")]
        public string autoJumpTargetNodeId;

        public List<DagOutputEdge> outputEdges = new List<DagOutputEdge>();

        public IReadOnlyList<DagOutputEdge> GetOutputEdges()
        {
            EnsureEdgeData();
            return outputEdges;
        }

        public void EnsureEdgeData()
        {
            outputEdges ??= new List<DagOutputEdge>();

            var targets = new HashSet<string>();
            for (int i = outputEdges.Count - 1; i >= 0; i--)
            {
                var edge = outputEdges[i];
                if (edge == null || string.IsNullOrWhiteSpace(edge.targetNodeId))
                {
                    outputEdges.RemoveAt(i);
                    continue;
                }

                edge.targetNodeId = edge.targetNodeId.Trim();
                if (targets.Add(edge.targetNodeId) == false)
                {
                    outputEdges.RemoveAt(i);
                }
            }
        }

        public bool AddOutputEdge(string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                return false;
            }

            EnsureEdgeData();
            var normalized = targetNodeId.Trim();
            foreach (var edge in outputEdges)
            {
                if (edge.targetNodeId == normalized)
                {
                    return false;
                }
            }

            outputEdges.Add(new DagOutputEdge(normalized));
            return true;
        }

        public bool RemoveOutputEdge(string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                return false;
            }

            EnsureEdgeData();
            var normalized = targetNodeId.Trim();
            var removed = false;
            for (int i = outputEdges.Count - 1; i >= 0; i--)
            {
                if (outputEdges[i].targetNodeId == normalized)
                {
                    outputEdges.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        public void ReplaceOutputTarget(string oldTargetNodeId, string newTargetNodeId)
        {
            EnsureEdgeData();
            if (string.IsNullOrWhiteSpace(oldTargetNodeId) || string.IsNullOrWhiteSpace(newTargetNodeId))
            {
                return;
            }

            var oldId = oldTargetNodeId.Trim();
            var newId = newTargetNodeId.Trim();
            if (oldId == newId)
            {
                return;
            }

            DagOutputEdge migratedEdge = null;
            for (int i = outputEdges.Count - 1; i >= 0; i--)
            {
                var edge = outputEdges[i];
                if (edge.targetNodeId != oldId)
                {
                    continue;
                }

                migratedEdge ??= new DagOutputEdge(newId, edge.isDefaultParent);
                outputEdges.RemoveAt(i);
            }

            if (migratedEdge != null)
            {
                var existing = outputEdges.Find(e => e.targetNodeId == newId);
                if (existing == null)
                {
                    outputEdges.Add(migratedEdge);
                }
                else if (migratedEdge.isDefaultParent)
                {
                    existing.isDefaultParent = true;
                }
            }
        }

        public DagOutputEdge FindOutputEdge(string targetNodeId)
        {
            EnsureEdgeData();
            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                return null;
            }

            var normalized = targetNodeId.Trim();
            return outputEdges.Find(edge => edge.targetNodeId == normalized);
        }

        public bool SetOutputEdgeDefaultParent(string targetNodeId, bool isDefaultParent)
        {
            var edge = FindOutputEdge(targetNodeId);
            if (edge == null)
            {
                return false;
            }

            edge.isDefaultParent = isDefaultParent;
            return true;
        }

        public string GetRuntimeId() => nodeId;

        public bool HasAutoJumpConfigured() => autoJumpEnabled;

        public string GetResolvedAutoJumpTargetId()
        {
            if (!autoJumpEnabled)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(autoJumpTargetNodeId) ? null : autoJumpTargetNodeId.Trim();
        }

        public void SetAutoJumpToNode(string targetNodeId)
        {
            autoJumpEnabled = true;
            autoJumpTargetNodeId = targetNodeId;
        }

        public void ClearAutoJump()
        {
            autoJumpEnabled = false;
            autoJumpTargetNodeId = null;
        }
    }
}
