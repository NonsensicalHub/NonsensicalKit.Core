using UnityEngine;

namespace NonsensicalKit.Core.DagLogicNode
{
    public class DagNodeControlActive : DagSwitchEventBase
    {
        [SerializeField] private GameObject m_controlTarget;

        private void Reset()
        {
            m_controlTarget = gameObject;
        }

        protected override void OnStateChanged(bool newState)
        {
            m_controlTarget.SetActive(newState);
        }
    }
}
