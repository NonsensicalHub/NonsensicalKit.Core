using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    public class LogicNodeSwitcher : MonoBehaviour
    {
        [SerializeField] private string m_targetNodeID;

        private string _buffer;

        private LogicNodeManager _manager;

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetManager);
            if (string.IsNullOrEmpty(m_targetNodeID))
            {
                LogCore.Debug($"{nameof(LogicNodeSwitcher)}未设置ID", this);
            }
        }

        public void Switch()
        {
            if (_manager != null)
            {
                _manager.SwitchNode(m_targetNodeID);
            }
            else
            {
                _buffer = m_targetNodeID;
            }
        }

        private void OnGetManager(LogicNodeManager manager)
        {
            _manager = manager;
            if (string.IsNullOrEmpty(_buffer) == false)
            {
                _manager.SwitchNode(_buffer);
            }
        }
    }
}
