#if !ENABLE_INPUT_SYSTEM
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    public partial class InputHub
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
                OnMouseMiddleButtonDown?.Invoke();
            }

            if (Input.GetMouseButtonUp(2))
            {
                IsMouseMiddleButtonHold = false;
                OnMouseMiddleButtonUp?.Invoke();
            }

            CrtMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (CrtMove != _lastMove)
            {
                OnMoveChanged?.Invoke(CrtMove);
                _lastMove = CrtMove;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                IsLeftShiftKeyHold = true;
                OnLeftShiftKeyChanged?.Invoke(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                IsLeftShiftKeyHold = false;
                OnLeftShiftKeyChanged?.Invoke(false);
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                IsLeftAltKeyHold = true;
                OnLeftAltKeyChanged?.Invoke(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                IsLeftAltKeyHold = false;
                OnLeftAltKeyChanged?.Invoke(false);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSpaceKeyEnter?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                OnFKeyEnter?.Invoke();
            }
        }
    }
}
#endif
