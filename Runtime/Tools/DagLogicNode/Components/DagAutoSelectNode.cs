using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagAutoSelectNode : MonoBehaviour
    {
        [SerializeField] private string m_node;
        [SerializeField] private bool m_invokeOnEnable;

        private void Awake()
        {
            if (!m_invokeOnEnable)
            {
                ServiceCore.Get<DagLogicManager>().SwitchNode(m_node);
            }
        }

        private void OnEnable()
        {
            if (m_invokeOnEnable)
            {
                ServiceCore.Get<DagLogicManager>().SwitchNode(m_node);
            }
        }
    }
}
