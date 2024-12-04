using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 逻辑节点控制
    /// </summary>
    public class LogicNodeControlPlus : NonsensicalMono
    {
        [SerializeField] private LogicNodeCheckType m_checkType;
        [SerializeField] private string m_nodeID;
        [SerializeField] private List<string> m_spOn;
        [SerializeField] private List<string> m_spOff;

        [SerializeField] private GameObject[] _controlGameobjectss;
        [SerializeField] private MonoBehaviour[] _controlComponents;

        private bool _isRunning;

        private LogicNodeManager _manager;

        private void Awake()
        {
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);
        }

        private void OnGetService(LogicNodeManager service)
        {
            _manager = service;
            Init();
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
                SetState(true);
                return;
            }
            if (m_spOff.Contains(node.NodeID))
            {
                SetState(false);
                return;
            }
            SetState(_manager.CheckState(m_nodeID, m_checkType));
        }

        private void SetState(bool newState)
        {
            foreach (var item in _controlGameobjectss)
            {
                item.SetActive(newState);
            }

            foreach (var item in _controlComponents)
            {
                item.enabled = newState;
            }
        }
    }
}
