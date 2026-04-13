using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagNodeSwitcher : MonoBehaviour
    {
        [SerializeField] private string m_targetNodeId;

        public string TargetNodeID
        {
            get => m_targetNodeId;
            set => m_targetNodeId = value;
        }
        
        private DagLogicManager _manager;

        public void Switch()
        {
            if (_manager == null)
            {
                _manager = ServiceCore.Get<DagLogicManager>();
            }

            _manager?.SwitchNode(m_targetNodeId);
        }
    }
}
