using System;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    /// <summary>
    /// 逻辑节点控制物体激活
    /// 任意一个条件组命中则激活目标物体，全部未命中则隐藏。
    /// </summary>
    public class DagNodeControlMax : NonsensicalMono
    {
        [Serializable]
        public struct ConditionGroup
        {
            [Tooltip("匹配的节点 ID")]
            [SerializeField] private string m_nodeID;

            [Tooltip("状态检查类型")]
            [SerializeField] private DagNodeCheckType m_checkType;

            public void SetConfig(string nodeID, string checkType)
            {
                m_nodeID = nodeID;
                m_checkType = Enum.Parse<DagNodeCheckType>(checkType);
            }

            public bool CheckState(DagLogicManager manager)
            {
                return manager.CheckState(m_nodeID, m_checkType);
            }
        }

        [Header("条件组（任意一组命中则显示）")]
        [SerializeField] private List<ConditionGroup> m_conditions;

        [SerializeField] private List<GameObject> m_controlGameObjects;
        [SerializeField] private List<MonoBehaviour> m_controlComponents;

        private DagLogicManager _manager;
        private bool _isRunning;

        private void Awake() =>
            ServiceCore.SafeGet<DagLogicManager>(OnGetService);

        private void OnEnable()
        {
            if (_isRunning && _manager.CrtSelectNode != null)
                OnSwitchNode(_manager.CrtSelectNode);
        }

        private void Reset()
        {
            m_controlGameObjects = new List<GameObject>() { gameObject };
        }

        public void Close()
        {
            _isRunning = false;
            Unsubscribe<DagRuntimeNode>(DagLogicNodeEnum.SwitchNode, OnSwitchNode);
        }

        public void AddCondition(string nodeId, string checkType)
        {
            var n = new ConditionGroup();
            n.SetConfig(nodeId, checkType);
            m_conditions.Add(n);
        }

        private void OnGetService(DagLogicManager service)
        {
            _manager = service;
            Init();
        }

        private void Init()
        {
            _isRunning = true;
            Subscribe<DagRuntimeNode>(DagLogicNodeEnum.SwitchNode, OnSwitchNode);

            if (_manager.CrtSelectNode != null)
                OnSwitchNode(_manager.CrtSelectNode);
        }

        private void OnSwitchNode(DagRuntimeNode node)
        {
            bool nextActive = m_conditions.Any(c => c.CheckState(_manager));

            foreach (var go in m_controlGameObjects)
            {
                if (go.activeSelf != nextActive)
                    go.SetActive(nextActive);
            }

            foreach (var com in m_controlComponents)
            {
                if (com.enabled != nextActive)
                    com.enabled = nextActive;
            }
        }
    }
}
