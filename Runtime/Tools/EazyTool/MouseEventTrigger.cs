using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.EazyTool
{
    public class MouseEventTrigger : NonsensicalMono
    {

        [SerializeField] private bool useOnWebGL;
        [SerializeField] private bool enableRayHit = false;
        [SerializeField] UnityEvent m_OnMouseEnter = null;
        [SerializeField] UnityEvent m_OnMouseClick = null;
        [SerializeField] UnityEvent m_OnMouseExit = null;

        private bool isEntered;
        private void Awake()
        {
            if (useOnWebGL)
            {
                if (PlatformInfo.IsWebGL)
                {
                    enabled = true;
                }
            }

            if (enableRayHit)
            {
                Subscribe<string>("onVirtualMouseEnter", OnVirtualMouseEnter);
                Subscribe<string>("onVirtualMouseClick", OnVirtualMouseClick);
            }
        }

        private void OnMouseEnter()
        {
            if (enableRayHit) return;
            m_OnMouseEnter?.Invoke();
        }

        private void OnMouseUpAsButton()
        {
            if (enableRayHit) return;
            m_OnMouseClick?.Invoke();
        }

        private void OnMouseExit()
        {
            if (enableRayHit) return;
            m_OnMouseExit?.Invoke();
        }

        private void OnVirtualMouseEnter(string obj)
        {
            if (obj == this.name)
            {
                if (isEntered == false)
                {
                    m_OnMouseEnter?.Invoke();
                    isEntered = true;
                }
            }
            else
            {
                if (isEntered == true)
                {
                    m_OnMouseExit?.Invoke();
                    isEntered = false;
                }
            }
        }

        private void OnVirtualMouseClick(string obj)
        {
            if (obj == this.name)
            {
                m_OnMouseClick?.Invoke();
            }
        }


    }

}
