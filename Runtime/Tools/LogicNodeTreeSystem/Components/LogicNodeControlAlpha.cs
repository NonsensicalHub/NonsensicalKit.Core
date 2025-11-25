using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LogicNodeControlAlpha : NonsensicalMono
    {
        [SerializeField] private LogicNodeCheckType m_checkType;
        [SerializeField] private string m_nodeID;
        [SerializeField] private List<string> m_spOn;
        [SerializeField] private List<string> m_spOff;

        private CanvasGroup _controlTarget;

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
            _controlTarget = GetComponent<CanvasGroup>();
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
                ChangeAlpha(true);
                return;
            }

            if (m_spOff.Contains(node.NodeID))
            {
                ChangeAlpha(false);
                return;
            }

            ChangeAlpha(_manager.CheckState(m_nodeID, m_checkType));
        }

        private void ChangeAlpha(bool show)
        {
            if (show)
            {
                _controlTarget.alpha=1;
                _controlTarget.blocksRaycasts=true;
            }
            else
            {
                _controlTarget.alpha=0;
                _controlTarget.blocksRaycasts=false;
            }
        }
    }
}
