using NonsensicalKit.Core.Service;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagNodeMono : NonsensicalMono
    {
        [SerializeField] private string m_nodeId;
        [SerializeField] private UnityEvent m_onNodeEnter;
        [SerializeField] private UnityEvent m_onNodeExit;

        private DagLogicManager _manager;
        public string NodeID => m_nodeId;
        public UnityEvent OnNodeEnter => m_onNodeEnter;
        public UnityEvent OnNodeExit => m_onNodeExit;

        private void Awake()
        {
            Subscribe<string>(DagLogicNodeEnum.NodeEnter, HandleNodeEnter);
            Subscribe<string>(DagLogicNodeEnum.NodeExit, HandleNodeExit);

            ServiceCore.SafeGet<DagLogicManager>(GetService);
        }
        
        private void GetService(DagLogicManager manager)
        {
            _manager = manager;
            if (_manager.CheckState(m_nodeId))
            {
                m_onNodeEnter.Invoke();
            }
            else
            {
                m_onNodeExit.Invoke();
            }
        }

        private void HandleNodeEnter(string enterNodeId)
        {
            if (enterNodeId == m_nodeId)
            {
                m_onNodeEnter.Invoke();
            }
        }

        private void HandleNodeExit(string exitNodeId)
        {
            if (exitNodeId == m_nodeId)
            {
                m_onNodeExit.Invoke();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("设置节点ID为物体名称")]
        private void SetGameObjectNameToNodeId()
        {
            m_nodeId = gameObject.name;
        }
#endif
    }
}
