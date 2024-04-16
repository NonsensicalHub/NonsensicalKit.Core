using NonsensicalKit.Tools.InputTool;
using UnityEngine;

namespace NonsensicalKit.Tools.CameraTool
{
    public class NonsensicalCameraFoucsEverything : MonoBehaviour
    {
        private NonsensicalCamera _camera;
        private RaycastHit _hit;
        private InputHub _input;

        private void Awake()
        {
            _camera = GetComponent<NonsensicalCamera>();
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
                _camera.Foucs(_hit.transform);
            }
        }
    }
}
