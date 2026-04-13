using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public abstract class DagSwitchEventBase : NonsensicalMono
    {
        [SerializeField] private string m_nodeId;
        [SerializeField] private DagNodeCheckType m_checkType;

        public string NodeId { get => m_nodeId; set => m_nodeId = value; }

        public DagNodeCheckType CheckType { get => m_checkType; set => m_checkType = value; }

        private DagLogicManager _manager;

        private bool _state;

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<DagLogicManager>(GetService);
        }

        private void GetService(DagLogicManager manager)
        {
            _manager = manager;
            Init();
        }

        private void Init()
        {
            Subscribe<string>(DagLogicNodeEnum.SwitchNode, OnSwitchNode);
            if (_manager.CrtSelectNode != null)
            {
                OnSwitchNode(_manager.CrtSelectNode.NodeID);
            }
        }

        private void OnSwitchNode(string nodeId)
        {
            var newState = _manager.CheckState(m_nodeId, m_checkType);

            if (_state != newState)
            {
                _state = newState;
                OnStateChanged(_state);
            }
        }

        protected abstract void OnStateChanged(bool newState);
    }
}
