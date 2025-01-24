using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.EasyTool
{
    public class MouseEventTrigger : NonsensicalMono
    {
        [FormerlySerializedAs("useOnWebGL")] [SerializeField]
        private bool m_useOnWebGL;

        [FormerlySerializedAs("enableRayHit")] [SerializeField]
        private bool m_enableRayHit;

        [FormerlySerializedAs("m_OnMouseEnter")] [SerializeField]
        private UnityEvent m_onMouseEnter;

        [FormerlySerializedAs("m_OnMouseClick")] [SerializeField]
        private UnityEvent m_onMouseClick;

        [FormerlySerializedAs("m_OnMouseExit")] [SerializeField]
        private UnityEvent m_onMouseExit;

        private bool _isEntered;

        private void Awake()
        {
            if (m_useOnWebGL)
            {
                if (PlatformInfo.IsWebGL)
                {
                    m_enableRayHit = true;
                }
            }

            if (m_enableRayHit)
            {
                Subscribe<string>("onVirtualMouseEnter", OnVirtualMouseEnter);
                Subscribe<string>("onVirtualMouseClick", OnVirtualMouseClick);
            }
        }

        private void OnMouseEnter()
        {
            if (m_enableRayHit) return;
            m_onMouseEnter?.Invoke();
        }

        private void OnMouseUpAsButton()
        {
            if (m_enableRayHit) return;
            m_onMouseClick?.Invoke();
        }

        private void OnMouseExit()
        {
            if (m_enableRayHit) return;
            m_onMouseExit?.Invoke();
        }

        private void OnVirtualMouseEnter(string obj)
        {
            if (obj == name)
            {
                if (_isEntered == false)
                {
                    m_onMouseEnter?.Invoke();
                    _isEntered = true;
                }
            }
            else
            {
                if (_isEntered)
                {
                    m_onMouseExit?.Invoke();
                    _isEntered = false;
                }
            }
        }

        private void OnVirtualMouseClick(string obj)
        {
            if (obj == name)
            {
                m_onMouseClick?.Invoke();
            }
        }
    }
}
