using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.EazyTool
{
    public class MouseEventTrigger : MonoBehaviour
    {
        [SerializeField] UnityEvent m_OnMouseEnter = null;
        [SerializeField] UnityEvent m_OnMouseClick = null;
        [SerializeField] UnityEvent m_OnMouseExit = null;

        private void OnMouseEnter()
        {
            m_OnMouseEnter?.Invoke();
        }

        private void OnMouseUpAsButton()
        {
            m_OnMouseClick?.Invoke();
        }

        private void OnMouseExit()
        {
            m_OnMouseExit?.Invoke();
        }
    }

}
