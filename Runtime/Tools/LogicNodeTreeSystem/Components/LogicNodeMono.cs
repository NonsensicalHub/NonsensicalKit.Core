using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    public class LogicNodeMono : NonsensicalMono
    {
        [FormerlySerializedAs("m_nodeName")]
        [SerializeField] private string m_nodeID;

        [FormerlySerializedAs("m_NodeEnter")] [SerializeField]
        private UnityEvent m_nodeEnter = new UnityEvent();

        [FormerlySerializedAs("m_NodeExit")] [SerializeField]
        private UnityEvent m_nodeExit = new UnityEvent();

        public string NodeID => m_nodeID;

        public UnityEvent OnNodeEnter
        {
            get => m_nodeEnter;
            set => m_nodeEnter = value;
        }

        public UnityEvent OnNodeExit
        {
            get => m_nodeExit;
            set => m_nodeExit = value;
        }

        private LogicNodeManager _manager;

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnGetService(LogicNodeManager service)
        {
            _manager = service;
            if (service.CrtSelectNode.NodeID == NodeID)
            {
                OnSwitchEnter();
            }
            else
            {
                OnSwitchExit();
            }

            Subscribe((int)LogicNodeEnum.NodeEnter, m_nodeID, OnSwitchEnter);
            Subscribe((int)LogicNodeEnum.NodeExit, m_nodeID, OnSwitchExit);
        }

        private void OnSwitchEnter()
        {
            m_nodeEnter.Invoke();
        }

        private void OnSwitchExit()
        {
            m_nodeExit.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("设置节点ID为物体名称")]
        private void SetGameObjectNameToNodeName()
        {
            m_nodeID = gameObject.name;
        }
#endif
    }
}
