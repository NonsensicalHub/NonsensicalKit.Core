using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 逻辑节点控制物体激活,挂载在需要控制的GameObject对象上
    /// </summary>
    public class LogicNodeControlActive : NonsensicalMono
    {
        [SerializeField] private LogicNodeCheckType m_checkType;
        [SerializeField] private string m_nodeID;
        [SerializeField] private List<string> m_spOn;
        [SerializeField] private List<string> m_spOff;

        private GameObject _controlTarget;

        private bool _isRunning;

        private LogicNodeManager _manager;

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnEnable()
        {
            if (_isRunning && _manager.CrtSelectNode != null)
            {
                OnSwitchNode(_manager.CrtSelectNode);
            }
        }

        public void Close()
        {
            _isRunning = false;
            Unsubscribe<LogicNode>((int)LogicNodeEnum.SwitchNode, OnSwitchNode);
        }

        private void OnGetService(LogicNodeManager service)
        {
            _controlTarget = gameObject;
            _manager = service;
            Init();
        }

        private void Init()
        {
            _isRunning = true;

            Subscribe<LogicNode>((int)LogicNodeEnum.SwitchNode, OnSwitchNode);
            if (_manager.CrtSelectNode != null)
            {
                OnSwitchNode(_manager.CrtSelectNode);
            }
        }

        private void OnSwitchNode(LogicNode node)
        {
            if (m_spOn.Contains(node.NodeID))
            {
                _controlTarget.SetActive(true);
                return;
            }

            if (m_spOff.Contains(node.NodeID))
            {
                _controlTarget.SetActive(false);
                return;
            }

            _controlTarget.SetActive(_manager.CheckState(m_nodeID, m_checkType));
        }
    }
}
