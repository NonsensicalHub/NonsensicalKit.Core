using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    public class LogicNodeMono : NonsensicalMono
    {
        [FormerlySerializedAs("m_nodeName")]
        [SerializeField] private string m_nodeID;

        [SerializeField] private UnityEvent m_NodeEnter = new UnityEvent();
        [SerializeField] private UnityEvent m_NodeExit = new UnityEvent();

        public string NodeID => m_nodeID;

        public UnityEvent OnNodeEnter
        {
            get { return m_NodeEnter; }
            set { m_NodeEnter = value; }
        }
        public UnityEvent OnNodeExit
        {
            get { return m_NodeExit; }
            set { m_NodeExit = value; }
        }

        private LogicNodeManager _manager;

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnGetService(LogicNodeManager service)
        {
            _manager = service;
            if (service.CrtSelectNode.NodeID==NodeID)
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
            m_NodeEnter.Invoke();

        }
        private void OnSwitchExit()
        {
            m_NodeExit.Invoke();
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
