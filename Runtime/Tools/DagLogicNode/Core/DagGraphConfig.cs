using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;

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

        public List<DagNode> nodes = new List<DagNode>();

        public DagNode FindNode(string nodeId) => nodes.Find(n => n.nodeId == nodeId);

        public bool HasRootNode() => nodes.Exists(n => n.isRoot);
    }

    [Serializable]
    public class DagNode
    {
        public string nodeId;
        public string describe;
        public Vector2 position;

        public bool isRoot;

        // 出边（nodeId）
        public List<string> outputs = new List<string>();

        public string GetRuntimeId() => nodeId;
    }
}
