#if !ENABLE_INPUT_SYSTEM
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class InputHub : MonoSingleton<InputHub>
    {
        private Vector2 _lastMousePos;
        private Vector2 _lastMouseMove;
        private Vector2 _lastMove;
        private float _lastZoom;

        private void Update()
        {
            CrtZoom = Input.GetAxisRaw("Mouse ScrollWheel");
            if (_lastZoom != CrtZoom)
            {
                OnZoomChanged?.Invoke(CrtZoom);
                _lastZoom = CrtZoom;
            }
            CrtMouseMove = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            if (CrtMouseMove != _lastMouseMove)
            {
                OnMouseMoveChanged?.Invoke(CrtMouseMove);
                _lastMouseMove = CrtMouseMove;
            }
            CrtMousePos = Input.mousePosition;
            if (_lastMousePos != CrtMousePos)
            {
                OnMousePosChanged?.Invoke(CrtMousePos);
                _lastMousePos = CrtMousePos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                IsMouseLeftButtonHold = true;
                OnMouseLeftButtonDown?.Invoke();
            }
            if (Input.GetMouseButtonUp(0))
            {
                IsMouseLeftButtonHold = false;
                OnMouseLeftButtonUp?.Invoke();
            }
            if (Input.GetMouseButtonDown(1))
            {
                IsMouseRightButtonHold = true;
                OnMouseRightButtonDown?.Invoke();
            }
            if (Input.GetMouseButtonUp(1))
            {
                IsMouseRightButtonHold = false;
                OnMouseRightButtonUp?.Invoke();
            }
            if (Input.GetMouseButtonDown(2))
            {
                IsMouseMiddleButtonHold = true;
            }
            if (Input.GetMouseButtonUp(2))
            {
                IsMouseMiddleButtonHold = false;
            }

            CrtMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (CrtMove != _lastMove)
            {
                OnMoveChanged?.Invoke(CrtMove);
                _lastMove = CrtMove;
            }
            IsLeftShiftKeyHold = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSpaceKeyEnter?.Invoke();
            }

        }
    }
}
#endif
