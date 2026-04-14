using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagAutoJumpNode : MonoBehaviour
    {
        [SerializeField] private string m_jumpNode; //执行跳转的节点
        [SerializeField] private string m_jumpTarget; //跳转的目标对象，为空时跳转到上一级

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<DagLogicManager>(OnGetService);
        }

        protected virtual void OnGetService(DagLogicManager manager)
        {
            var node = manager.GetNode(m_jumpNode);
            var targetNode = manager.GetNode(m_jumpTarget);
            if (node != null && targetNode != null)
            {
                node.AutoJumpNode = m_jumpTarget;
            }
        }
    }
}