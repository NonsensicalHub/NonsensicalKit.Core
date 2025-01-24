using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.CameraTool
{
    public class RotateCubePart : NonsensicalMono
    {
        [FormerlySerializedAs("useOnWebGL")] [SerializeField] private bool m_useOnWebGL;
        [FormerlySerializedAs("enableRayHit")] [SerializeField] private bool m_enableRayHit = false;
        [SerializeField] private Vector3 m_dir;
        [SerializeField] private UnityEvent m_onMouseEnter;
        [SerializeField] private UnityEvent m_onMouseExit;

        private RotateCube _cube;

        private bool isEntered;

        private void Awake()
        {
            _cube = GetComponentInParent<RotateCube>();

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

        public void OnMouseEnter()
        {
            if (m_enableRayHit) return;
            m_onMouseEnter?.Invoke();
        }

        public void OnMouseUpAsButton()
        {
            if (m_enableRayHit) return;
            _cube.PartClick(m_dir);
        }

        public void OnMouseExit()
        {
            if (m_enableRayHit) return;
            m_onMouseExit?.Invoke();
        }

        private void OnVirtualMouseEnter(string obj)
        {
            if (obj == this.name)
            {
                if (isEntered == false)
                {
                    m_onMouseEnter?.Invoke();
                    isEntered = true;
                }
            }
            else
            {
                if (isEntered == true)
                {
                    m_onMouseExit?.Invoke();
                    isEntered = false;
                }
            }
        }

        private void OnVirtualMouseClick(string obj)
        {
            if (obj == this.name)
            {
                _cube.PartClick(m_dir);
            }
        }
    }
}
