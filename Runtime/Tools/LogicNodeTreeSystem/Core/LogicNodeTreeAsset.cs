using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 自定义序列化存储参考： https://docs.unity3d.com/cn/current/Manual/script-Serialization-Custom.html
    /// </summary>
    [CreateAssetMenu(fileName = "LogicNodeTree", menuName = "ScriptableObjects/LogicNodeTreeConfigData")]
    public class LogicNodeTreeAsset : ConfigObject, ISerializationCallbackReceiver
    {
        public LogicNodeTreeConfigData ConfigData;

        public void OnBeforeSerialize()
        {
            if (ConfigData != null)
            {
                ConfigData.OnBeforeSerialize();
            }
        }

        public void OnAfterDeserialize()
        {
            ConfigData.OnAfterDeserialize();
        }

        public override ConfigData GetData()
        {
            return ConfigData;
        }

        public override void AfterSetData()
        {
            base.AfterSetData();
            ConfigData.OnAfterDeserialize();
        }

        public override void SetData(ConfigData cd)
        {
            if (CheckType<LogicNodeTreeConfigData>(cd))
            {
                ConfigData = cd as LogicNodeTreeConfigData;
            }
        }
    }

    [Serializable]
    public class LogicNodeTreeConfigData : ConfigData
    {
        [NonSerialized]
        public LogicNodeData Root;

        public List<SerializableNode> SerializedNodes = new List<SerializableNode>();

        public void OnBeforeSerialize()
        {
            if (Root == null)
            {
                Debug.Log("根节点为空");
                Root = new LogicNodeData("root");
            }

            SerializedNodes.Clear();
            AddNodeToSerializedNodes(Root);
        }

        public void OnAfterDeserialize()
        {
            if (SerializedNodes.Count > 0)
            {
                ReadNodeFromSerializedNodes(0, out Root);
            }
            else
            {
                Debug.Log("序列化链表为空");
                Root = new LogicNodeData("root");
            }
        }

        private void AddNodeToSerializedNodes(LogicNodeData n)
        {
            var serializedNode = new SerializableNode()
            {
                NodeID = n.NodeID,
                ChildCount = n.Children.Count,
            };

            SerializedNodes.Add(serializedNode);
            foreach (var child in n.Children)
                AddNodeToSerializedNodes(child);
        }

        private int ReadNodeFromSerializedNodes(int index, out LogicNodeData node)
        {
            var serializedNode = SerializedNodes[index];
            LogicNodeData newNode = new LogicNodeData()
            {
                NodeID = serializedNode.NodeID,
                Children = new List<LogicNodeData>()
            };

            for (int i = 0; i < serializedNode.ChildCount; i++)
            {
                LogicNodeData childNode;
                index = ReadNodeFromSerializedNodes(++index, out childNode);
                childNode.Parent = newNode;
                newNode.Children.Add(childNode);
            }

            node = newNode;
            return index;
        }
    }

    public class LogicNodeData : TreeData<LogicNodeData>
    {
        [FormerlySerializedAs("NodeName")]
        public string NodeID; //节点名，ID

        public LogicNodeData Parent;
        public List<LogicNodeData> Children = new List<LogicNodeData>();

        public LogicNodeData()
        {
        }

        public LogicNodeData(string nodeID)
        {
            this.NodeID = nodeID;
        }

        public override List<LogicNodeData> GetChildren()
        {
            return Children;
        }
    }

    // 用于序列化的 Node 类。
    [Serializable]
    public struct SerializableNode
    {
        [FormerlySerializedAs("NodeName")]
        public string NodeID; //节点名，ID

        public int ChildCount;
    }

    public abstract class TreeData<T>
    {
        public bool IsFoldout = true;
        public abstract List<T> GetChildren();
    }
}
