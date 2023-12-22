using NonsensicalKit.Core;
using System;
using UnityEngine;

namespace NonsensicalKit.Tools.InputTool
{
    /// <summary>
    /// 聚合InputManager和InputSystem的常用键鼠输入
    /// </summary>
    public partial class InputHub : MonoSingleton<InputHub>
    {
        public Action<Vector2> OnMousePosChanged { get; set; }
        public Action<Vector2> OnMouseMoveChanged { get; set; }
        public Action<float> OnZoomChanged { get; set; }

        public Action OnMouseLeftButtonDown { get; set; }
        public Action OnMouseLeftButtonUp { get; set; }
        public Action OnMouseRightButtonDown { get; set; }
        public Action OnMouseRightButtonUp { get; set; }

        public Action<Vector2> OnMoveChanged { get; set; }

        public Action OnSpaceKeyEnter { get; set; }

        public float CrtZoom { get; private set; }

        public bool IsMouseLeftButtonHold { get; private set; }
        public bool IsMouseRightButtonHold { get; private set; }
        public bool IsMouseMiddleButtonHold { get; private set; }
        public bool IsLeftShiftKeyHold { get; private set; }

        public Vector2 CrtMove { get; private set; }
        public Vector2 CrtMousePos { get; private set; }
        public Vector2 CrtMouseMove { get; private set; }
    }
}
