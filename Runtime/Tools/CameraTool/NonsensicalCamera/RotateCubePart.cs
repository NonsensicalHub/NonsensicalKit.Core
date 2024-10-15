using NonsensicalKit.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.CameraTool
{
    public class RotateCubePart : NonsensicalMono
    {
        [SerializeField] private bool useOnWebGL;
        [SerializeField] private bool enableRayHit = false;
        [SerializeField] private Vector3 m_dir;
        [SerializeField] private UnityEvent m_onMouseEnter;
        [SerializeField] private UnityEvent m_onMouseExit;

        private RotateCube _cube;

        private bool isEntered;
        private void Awake()
        {
            _cube = GetComponentInParent<RotateCube>();

            if (useOnWebGL)
            {
                if (PlatformInfo.IsWebGL)
                {
                    enableRayHit = true;
                }
            }
            if (enableRayHit)
            {
                Subscribe<string>("onVirtualMouseEnter", OnVirtualMouseEnter);
                Subscribe<string>("onVirtualMouseClick", OnVirtualMouseClick);
            }
        }

        public void OnMouseEnter()
        {
            if (enableRayHit) return;
            m_onMouseEnter?.Invoke();
        }

        public void OnMouseUpAsButton()
        {
            if (enableRayHit) return;
            _cube.PartClick(m_dir);
        }

        public void OnMouseExit()
        {
            if (enableRayHit) return;
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
