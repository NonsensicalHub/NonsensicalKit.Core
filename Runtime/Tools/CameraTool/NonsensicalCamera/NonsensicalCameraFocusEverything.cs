using System;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;

namespace NonsensicalKit.Tools.CameraTool
{
    public class NonsensicalCameraFocusEverything : MonoBehaviour
    {
        [SerializeField] private bool m_setDistance;
        [SerializeField] private bool m_immediate;
        
        private NonsensicalCamera _camera;
        private RaycastHit _hit;
        private InputHub _input;

        private void Start()
        {
            if (_camera == null)
            {
                _camera = IOCC.Get<NonsensicalCamera>();
            }
            if (_camera != null)
            {
                _input = InputHub.Instance;
                _input.OnMouseLeftButtonDown += OnLeftMouseButtonDown;
            }
        }

        private void OnLeftMouseButtonDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(_input.CrtMousePos);

            Physics.Raycast(ray, out _hit, 100);
            if (_hit.transform != null)
            {
                _camera.Focus(_hit.transform,m_immediate,m_setDistance);
            }
        }
    }
}
