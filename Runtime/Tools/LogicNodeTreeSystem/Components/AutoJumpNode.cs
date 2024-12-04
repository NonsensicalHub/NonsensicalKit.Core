using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 用于某些虚拟节点(无对应的实体对象)的自动跳转
    /// </summary>
    public class AutoJumpNode : MonoBehaviour
    {
        [SerializeField] private string m_jumpNode;     //执行跳转的节点
        [SerializeField] private string m_jumpTarget;   //跳转的目标对象，为空时跳转到上一级

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        protected virtual void OnGetService(LogicNodeManager manager)
        {
            var node = manager.GetNode(m_jumpNode);
            var targetNode = manager.GetNode(m_jumpTarget);
            if (node != null && targetNode != null)
            {
                node.AutoJump = m_jumpTarget;
            }
        }
    }
}
