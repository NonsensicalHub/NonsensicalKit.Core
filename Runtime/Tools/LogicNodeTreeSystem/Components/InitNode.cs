using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    public class InitNode : MonoBehaviour
    {
        [SerializeField] private string m_node; //执行跳转的节点

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnGetService(LogicNodeManager manager)
        {
            manager.SwitchNode(m_node);
        }
    }
}
