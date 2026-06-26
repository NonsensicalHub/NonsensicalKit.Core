using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Tools.CameraTool
{
    /// <summary>
    /// 自由飞行摄像机
    /// 使用asdw移动，鼠标右键旋转
    /// </summary>
    public class FreedomCamera : MonoBehaviour
    {
        [SerializeField] private Texture2D m_handTexture;

        [SerializeField] private float m_mouseWheelRollSpeed = 1f; //鼠标滚轮滚动速度
        [SerializeField] private float m_mouseWheelDownSpeed = 5f; //鼠标滚轮按下速度
        [SerializeField] private float m_rotateSpeed = 5f; //自转速度
        [SerializeField] private float m_moveSpeed = 0.1f; //移动速度
        [SerializeField] private float m_shiftMagnification = 3f; //shift加速倍率

        protected bool CanOperation = true; //是否可以操作

        private void Update()
        {
            if (CanOperation)
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null) { return; }

                Mouse mouse = Mouse.current;
                if (mouse == null) { return; }

                var move = new Vector2(keyboard.dKey.isPressed ? 1 : (keyboard.aKey.isPressed ? -1 : 0),
                    keyboard.wKey.isPressed ? 1 : (keyboard.sKey.isPressed ? -1 : 0));
                var shiftKey = keyboard.leftShiftKey.isPressed;
                var altKey = keyboard.leftAltKey.isPressed;
                var mouseMove = mouse.delta.ReadValue() * 0.1f;
                var mousePos = mouse.position.ReadValue();
                var mouseScroll = mouse.scroll.ReadValue().y * 0.01f;
                var mouse0 = mouse.leftButton.isPressed;
                var mouse1 = mouse.rightButton.isPressed;
                var mouse2 = mouse.middleButton.isPressed;

#else
                var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                var shiftKey = Input.GetKey(KeyCode.LeftShift);
                var altKey = Input.GetKey(KeyCode.LeftAlt);
                var mouseMove = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                var mousePos = Input.mousePosition;
                var mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
                var mouse0 = Input.GetMouseButton(0);
                var mouse1 = Input.GetMouseButton(1);
                var mouse2 = Input.GetMouseButton(2);
#endif

                CameraMove(move, shiftKey);

                if (mouse1)
                {
                    RotateSelf(mouseMove);
                }

                PressWheelMove(mouseMove, mouse2, shiftKey);

                WheelChangeDistance(mousePos, mouseScroll, shiftKey);
            }
        }

        private void CameraMove(Vector2 axis, bool leftShift)
        {
            Vector3 offset = Vector3.zero;

            offset += transform.right * axis.x;
            offset += transform.forward * axis.y;

            offset *= m_moveSpeed;

            if (leftShift)
            {
                offset *= m_shiftMagnification;
            }

            transform.position += offset;
        }

        private void RotateSelf(Vector2 axis)
        {
            transform.RotateAround(transform.position, Vector3.up, axis.x * m_rotateSpeed);

            transform.RotateAround(transform.position, -transform.right, axis.y * m_rotateSpeed);
        }

        private void PressWheelMove(Vector2 axis, bool mouseMiddle, bool leftShift)
        {
            if (mouseMiddle)
            {
                Cursor.SetCursor(m_handTexture, Vector2.zero, CursorMode.ForceSoftware);

                var offset = (transform.right * -axis.x + transform.up * -axis.y) * (m_mouseWheelDownSpeed * Time.deltaTime);
                if (leftShift)
                {
                    offset *= m_shiftMagnification;
                }

                transform.position += offset;
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
        }

        private void WheelChangeDistance(Vector2 axis, float mouseScrollWheel, bool leftShift)
        {
            if (CheckMousePosition(axis))
            {
                var offset = transform.forward * (mouseScrollWheel * m_mouseWheelRollSpeed);
                if (leftShift)
                {
                    offset *= m_shiftMagnification;
                }

                transform.position += offset;
            }
        }

        private bool CheckMousePosition(Vector2 axis)
        {
            if (axis.x > 0 && axis.x < Screen.width && axis.y > 0 && axis.y < Screen.height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
