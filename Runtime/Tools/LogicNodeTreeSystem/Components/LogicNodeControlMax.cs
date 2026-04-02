using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 逻辑节点控制物体激活，挂载在需要控制的 GameObject 上。
    /// 任意一个条件组命中则激活目标物体，全部未命中则隐藏。
    /// </summary>
    public class LogicNodeControlMax : NonsensicalMono
    {
        [Serializable]
        public struct ConditionGroup
        {
            [Tooltip("匹配的节点 ID")]
            [SerializeField] private string m_nodeID;

            [Tooltip("状态检查类型")]
            [SerializeField] private LogicNodeCheckType m_checkType;

            public void SetConfig(string nodeID, string checkType)
            {
                m_nodeID = nodeID;
                m_checkType = Enum.Parse<LogicNodeCheckType>(checkType);
            }

            public bool CheckState(LogicNodeManager manager)
            {
                return manager.CheckState(m_nodeID, m_checkType);
            }
        }

        [Header("条件组（任意一组命中则显示）")]
        [SerializeField] private List<ConditionGroup> m_conditions;

        // ── 运行时状态 ────────────────────────────────────────────

        private GameObject _controlTarget;
        private LogicNodeManager _manager;
        private bool _isRunning;

        // ── 生命周期 ──────────────────────────────────────────────

        private void Awake() =>
            ServiceCore.SafeGet<LogicNodeManager>(OnGetService);

        private void OnEnable()
        {
            if (_isRunning && _manager.CrtSelectNode != null)
                OnSwitchNode(_manager.CrtSelectNode);
        }

        // ── 公开接口 ──────────────────────────────────────────────

        public void Close()
        {
            _isRunning = false;
            Unsubscribe<LogicNode>((int)LogicNodeEnum.SwitchNode, OnSwitchNode);
        }

        public void AddCondition(string nodeId,string checkType)
        {
            var n = new ConditionGroup();
            n.SetConfig(nodeId, checkType);
            m_conditions.Add(n);
        }

        // ── 私有方法 ──────────────────────────────────────────────

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
                OnSwitchNode(_manager.CrtSelectNode);
        }

        private void OnSwitchNode(LogicNode node)
        {
            bool nextActive = m_conditions.Any(c => c.CheckState(_manager));

            // 避免重复 SetActive 调用
            if (_controlTarget.activeSelf != nextActive)
                _controlTarget.SetActive(nextActive);
        }
    }
}
