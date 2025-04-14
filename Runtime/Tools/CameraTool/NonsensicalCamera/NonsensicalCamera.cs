using NaughtyAttributes;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.EasyTool;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace NonsensicalKit.Tools.CameraTool
{
    public enum ControlMouseKey
    {
        Uncontrolled,
        LeftKeyControl,
        RightKeyControl,
        MiddleKeyControl,
        AlwaysControl,
    }

    public enum ControlFinger
    {
        Uncontrolled,
        OneFinger,
        TwoFinger,
    }

    public class NonsensicalCamera : NonsensicalMono
    {
        [SerializeField] private Transform m_viewPoint; //视点
        [SerializeField] private Transform m_camera; //相机

        [SerializeField] private bool m_isLimitPith = true;
        [Range(-90, 90)] [SerializeField] protected float m_minPitch = -90; //最小俯仰角
        [Range(-90, 90)] [SerializeField] protected float m_maxPitch = 90; //最大俯仰角

        [SerializeField] protected float m_minDistance = 1; //最近距离
        [SerializeField] protected float m_maxDistance = 100; //最远距离

        [SerializeField] protected float m_moveSpeedMinZoom = 1; //最小移动速度，速度和距离正相关
        [SerializeField] protected float m_moveSpeedMaxZoom = 10; //最大移动速度
        [SerializeField] protected float m_rotationSpeed = 1; //旋转速率
        [SerializeField] protected float m_rollSpeed = 1; //翻滚速率
        [SerializeField] protected float m_zoomSpeed = 1; //缩放速率
        [SerializeField] protected float m_dragZoomSpeed = 1; //拖缩缩放速率

        [SerializeField] protected bool m_checkUI = true; //是否在鼠标悬浮在UI上时禁止控制相机

        [SerializeField] protected ControlMouseKey m_moveControl = ControlMouseKey.LeftKeyControl;
        [SerializeField] protected ControlMouseKey m_rotateControl = ControlMouseKey.RightKeyControl;
        [SerializeField] protected ControlMouseKey m_rollControl = ControlMouseKey.MiddleKeyControl;

        [FormerlySerializedAs("m_DragZoomControl")] [SerializeField]
        protected ControlMouseKey m_dragZoomControl = ControlMouseKey.Uncontrolled;

        [SerializeField] protected bool m_enableMobileInput;

        [ShowIf("m_enableMobileInput"), SerializeField]
        protected ControlFinger m_fingerMoveControl = ControlFinger.Uncontrolled;

        [ShowIf("m_enableMobileInput"), SerializeField]
        protected ControlFinger m_fingerRotateControl = ControlFinger.OneFinger;

        [ShowIf("m_enableMobileInput"), SerializeField]
        protected ControlFinger m_fingerDragZoomControl = ControlFinger.TwoFinger;

        [FormerlySerializedAs("m_UseKeyBoardControlMove")] [SerializeField]
        protected bool m_useKeyBoardControlMove;

        [SerializeField] protected bool m_lerpMove = true; //插值移动
        [SerializeField] protected bool m_autoInit = true; //自动初始化
        [SerializeField] protected bool m_resetOnEnable = true; //重新激活时回到初始位置

        [SerializeField] [FormerlySerializedAs("m_renderBox")]
        protected RenderBox m_limitBox;

        public bool IsOn { get; set; } = true;
        public Quaternion CrtRotate { get; set; }

        /// <summary>
        /// 目标缩放
        /// </summary>
        public float TargetZoom
        {
            get => _targetZoom;
            set => _targetZoom = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 目标距离
        /// </summary>
        public float TargetDistance
        {
            get => _targetZoom * (m_maxDistance - m_minDistance) + m_minDistance;
            set => _targetZoom = Mathf.Clamp01((value - m_minDistance) / (m_maxDistance - m_minDistance));
        }

        public Vector3 TargetPosition
        {
            set => _targetPos = value;
        }

        /// <summary>
        /// 鼠标是否没有放置在UI上
        /// </summary>
        protected bool MouseNotInUI
        {
            get
            {
                if (!m_checkUI)
                {
                    return true;
                }

                if (_crtEventSystem == null)
                {
                    return true;
                }

                if (PlatformInfo.IsMobile)
                {
                    if (Input.touchCount > 0)
                    {
                        return !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }
                }

#if ENABLE_INPUT_SYSTEM
                var inputModule = _crtEventSystem.currentInputModule;
                if (inputModule != null && inputModule is InputSystemUIInputModule)
                {
                    return !((InputSystemUIInputModule)inputModule).GetLastRaycastResult(Pointer.current.deviceId).isValid;
                }
                else
                {
                    return !_crtEventSystem.IsPointerOverGameObject();
                }
#else
                return !_crtEventSystem.IsPointerOverGameObject();
#endif
            }
        }

        protected Vector3 _targetPos; //视点目标位置，运行时会不断接近
        protected float _targetZoom; //目标缩放，范围在0到1之间
        protected float _targetYaw; //目标旋转角
        protected float _targetPitch; //目标俯仰角
        protected float _targetRoll; //目标翻滚角

        private float _crtZoom; //当前缩放
        private float _crtPitch; //当前俯仰角
        private float _crtYaw; //当前旋转角
        private float _crtRoll; //当前翻滚角

        protected EventSystem _crtEventSystem;
        protected InputHub _input;

        protected MobileInputHub _mobileInput;

        private Vector3 _startPos; //初始视点位置
        private Quaternion _startRot; //初始视点旋转
        private Vector3 _startPos2; //初始相机位置
        private Quaternion _startRot2; //初始相机旋转

        private bool _needMove;
        private bool _needRotate;
        private bool _needRoll;
        private bool _needDragZoom;
        private bool _mouseEventInitFlag;
        private bool _fingerEventInitFlag;

        protected virtual void Awake()
        {
            if (m_camera != null && m_viewPoint != null)
            {
                m_camera.LookAt(m_viewPoint);

                _startPos = m_viewPoint.localPosition;
                _startRot = m_viewPoint.localRotation;
                _startPos2 = m_camera.localPosition;
                _startRot2 = m_camera.localRotation;

                if (m_autoInit)
                {
                    Init();
                }
            }

            IOCC.Set(this);
        }

        protected virtual void Start()
        {
            _crtEventSystem = EventSystem.current;

            if (PlatformInfo.IsMobile)
            {
                _mobileInput = MobileInputHub.Instance;
                AddMobileInputEvent();
            }
            else
            {
                _input = InputHub.Instance;
                AddMouseEvent();
            }
        }

        private void OnEnable()
        {
            if (m_resetOnEnable)
            {
                ResetState();
            }
        }

        protected virtual void Update()
        {
            if (m_camera == null || m_viewPoint == null)
            {
                return;
            }

            if (_crtEventSystem == null && EventSystem.current != null)
            {
                _crtEventSystem = EventSystem.current;
            }

            if (IsOn)
            {
                if (PlatformInfo.IsMobile == false)
                {
                    if (MouseNotInUI)
                    {
                        var v = -_input.CrtZoom;
                        if (v > 0) //统一在不同平台中差异较大的滚动值
                        {
                            v = 1f;
                        }
                        else if (v < 0)
                        {
                            v = -1f;
                        }

                        if (v != 0)
                        {
                            AdjustZoom(v);
                        }
                    }

                    if (_needMove)
                    {
                        AdjustPositionWithFixedInterval(_input.CrtMouseMove);
                    }

                    if (_needRotate)
                    {
                        AdjustRotation(new Vector2(_input.CrtMouseMove.x, -_input.CrtMouseMove.y));
                    }

                    if (_needRoll)
                    {
                        AdjustRoll(_input.CrtMouseMove.x);
                    }

                    if (_needDragZoom)
                    {
                        AdjustDragZoom(_input.CrtMouseMove);
                    }

                    if (m_useKeyBoardControlMove)
                    {
                        AdjustPosition(-_input.CrtMove);
                    }
                }
                else
                {
                    //IsMobile
                    if (MouseNotInUI == false)
                    {
                        return;
                    }

                    if (_needMove)
                    {
                        AdjustPositionWithFixedInterval(_mobileInput.TheOneFingerMove);
                    }

                    if (_needRotate)
                    {
                        AdjustRotation(new Vector2(_mobileInput.TheOneFingerMove.x, -_mobileInput.TheOneFingerMove.y));
                    }

                    if (_needDragZoom)
                    {
                        var v = -_mobileInput.TwoFingerDistance;
                        if (v > 0) //统一在不同平台中差异较大的滚动值
                        {
                            v = 1f;
                        }
                        else if (v < 0)
                        {
                            v = -1f;
                        }

                        if (v != 0)
                        {
                            AdjustZoom(v);
                        }
                    }
                }
            }
        }

        protected void LateUpdate()
        {
            if (m_camera == null || m_viewPoint == null)
            {
                return;
            }

            if (m_lerpMove)
            {
                m_viewPoint.position = Vector3.Lerp(m_viewPoint.position, _targetPos, 0.05f);
                _crtZoom = _crtZoom * 0.95f + TargetZoom * 0.05f;
                float distance = Mathf.Lerp(m_minDistance, m_maxDistance, _crtZoom);

                _crtPitch = _crtPitch * 0.95f + _targetPitch * 0.05f;
                _crtYaw = _crtYaw * 0.95f + _targetYaw * 0.05f;
                _crtRoll = _crtRoll * 0.95f + _targetRoll * 0.05f;

                CrtRotate = Quaternion.Euler(_crtPitch, _crtYaw, _crtRoll);
                m_camera.position = m_viewPoint.position + CrtRotate * Vector3.forward * -distance;
                m_camera.rotation = CrtRotate;
            }
            else
            {
                m_viewPoint.position = _targetPos;
                _crtZoom = TargetZoom;
                float distance = Mathf.Lerp(m_minDistance, m_maxDistance, _crtZoom);

                _crtPitch = _targetPitch;
                _crtYaw = _targetYaw;
                _crtRoll = _targetRoll;

                CrtRotate = Quaternion.Euler(_crtPitch, _crtYaw, _crtRoll);
                m_camera.position = m_viewPoint.position + CrtRotate * Vector3.forward * -distance;
                m_camera.rotation = CrtRotate;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_mouseEventInitFlag)
            {
                RemoveMouseEvent();
            }

            if (_fingerEventInitFlag)
            {
                RemoveMobileInputEvent();
            }
        }

        public void Init()
        {
            var dir = m_camera.position - m_viewPoint.position;
            var distance = dir.magnitude;

            _targetPos = m_viewPoint.position;
            _targetZoom = Mathf.Clamp01((distance - m_minDistance) / (m_maxDistance - m_minDistance));
            _crtZoom = _targetZoom;

            var dirP = new Vector3(dir.x, 0, dir.z);
            _targetYaw = Vector3.SignedAngle(-Vector3.forward, dirP, Vector3.up);
            _crtYaw = _targetYaw;

            _targetPitch = 90 - Vector3.Angle(Vector3.up, dir);
            _crtPitch = _targetPitch;
            if (m_isLimitPith)
            {
                while (_targetPitch > 90)
                {
                    _targetPitch -= 180;
                }

                if (_targetPitch < m_minPitch)
                {
                    _targetPitch = m_minPitch;
                }
                else if (_targetPitch >= m_maxPitch)
                {
                    _targetPitch = m_maxPitch;
                }
            }
        }

        /// <summary>
        /// 重置到初始状态
        /// </summary>
        public void ResetState()
        {
            if (m_camera == null || m_viewPoint == null)
            {
                return;
            }

            m_camera.LookAt(m_viewPoint);

            m_viewPoint.localPosition = _startPos;
            m_viewPoint.localRotation = _startRot;
            m_camera.localPosition = _startPos2;
            m_camera.localRotation = _startRot2;

            Init();
        }

        /// <summary>
        /// 设置俯仰角和旋转角
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        public void SetPitchAndYaw(float pitch, float yaw)
        {
            //Debug.Log(pitch + " " + yaw);

            if (Mathf.Approximately(_targetPitch.NatureAngle(), pitch.NatureAngle()) &&
                Mathf.Approximately(_targetYaw.NatureAngle(), yaw.NatureAngle()))
            {
                _targetRoll = 0;
            }
            else
            {
                _targetPitch = pitch;
                _targetYaw = yaw.AngleNear(_targetYaw);
            }
        }

        public void ChangeRoll(float delta)
        {
            _targetRoll += delta;
        }

        /// <summary>
        /// 聚焦到某个点
        /// </summary>
        /// <param name="point"></param>
        /// <param name="immediate"></param>
        public virtual void Focus(Vector3 point, bool immediate = false)
        {
            _targetPos = point;
            _targetRoll = 0;
            if (immediate)
            {
                m_viewPoint.position = point;
                _crtRoll = 0;
            }
        }

        /// <summary>
        /// 聚焦到某个对象
        /// </summary>
        /// <param name="tsf"></param>
        /// <param name="immediate"></param>
        /// <param name="setDistance"></param>
        public virtual void Focus(Transform tsf, bool immediate = false, bool setDistance = false)
        {
            if (setDistance)
            {
                var result = tsf.GetFocus();
                Focus(result.Item1, immediate);
                TargetDistance = result.Item2;
                if (immediate)
                {
                    _crtZoom = _targetZoom;
                }
            }
            else
            {
                var b = tsf.BoundingBoxGlobal();
                Focus(tsf.position + b.center, immediate);
            }
        }

        /// <summary>
        /// 根据改变量进行缩放
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustZoom(float delta)
        {
            TargetZoom += delta * m_zoomSpeed * 0.01f;
        }

        /// <summary>
        /// 根据改变量进行旋转
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRotation(Vector2 delta)
        {
            _targetYaw += delta.x * m_rotationSpeed * 0.3f;
            _targetPitch += delta.y * m_rotationSpeed * 0.3f;
            if (m_isLimitPith)
            {
                _targetPitch = Mathf.Clamp(_targetPitch, m_minPitch, m_maxPitch);
            }
        }

        /// <summary>
        /// 根据改变量进行翻滚
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRoll(float delta)
        {
            _targetRoll += delta * m_rollSpeed;
        }

        protected void AdjustDragZoom(Vector2 delta)
        {
            TargetZoom += (delta.x + delta.y) * m_dragZoomSpeed * 0.001f;
        }

        /// <summary>
        /// 根据改变量进行位移
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustPosition(Vector2 delta)
        {
            Vector3 direction = m_camera.rotation * new Vector3(-delta.x, -delta.y, 0f).normalized;
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, _crtZoom) * delta.magnitude * Time.deltaTime;

            if (m_limitBox == null)
            {
                _targetPos += direction * distance;
            }
            else
            {
                _targetPos = m_limitBox.NearestPoint(_targetPos + direction * distance);
            }
        }

        /// <summary>
        /// 根据改变量进行位移(使用固定间隔时间)
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="interval"></param>
        protected void AdjustPositionWithFixedInterval(Vector2 delta, float interval = 0.02f)
        {
            Vector3 direction = m_camera.rotation * new Vector3(-delta.x, -delta.y, 0f);
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, _crtZoom) * delta.magnitude * interval;

            if (m_limitBox == null)
            {
                _targetPos += direction * distance;
            }
            else
            {
                _targetPos = m_limitBox.NearestPoint(_targetPos + direction * distance);
            }
        }

        private void AddMouseEvent()
        {
            if (_mouseEventInitFlag)
            {
                return;
            }

            _mouseEventInitFlag = true;
            switch (m_moveControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown += StartMove;
                    _input.OnMouseLeftButtonUp += StopMove;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown += StartMove;
                    _input.OnMouseRightButtonUp += StopMove;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown += StartMove;
                    _input.OnMouseMiddleButtonUp += StopMove;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needMove = true;
                    break;
            }

            switch (m_rotateControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown += StartRotate;
                    _input.OnMouseLeftButtonUp += StopRotate;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown += StartRotate;
                    _input.OnMouseRightButtonUp += StopRotate;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown += StartRotate;
                    _input.OnMouseMiddleButtonUp += StopRotate;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needRotate = true;
                    break;
            }

            switch (m_rollControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown += StartRoll;
                    _input.OnMouseLeftButtonUp += StopRoll;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown += StartRoll;
                    _input.OnMouseRightButtonUp += StopRoll;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown += StartRoll;
                    _input.OnMouseMiddleButtonUp += StopRoll;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needRoll = true;
                    break;
            }

            switch (m_dragZoomControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown += StartZoom;
                    _input.OnMouseLeftButtonUp += StopZoom;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown += StartZoom;
                    _input.OnMouseRightButtonUp += StopZoom;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown += StartZoom;
                    _input.OnMouseMiddleButtonUp += StopZoom;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needDragZoom = true;
                    break;
            }
        }

        private void AddMobileInputEvent()
        {
            if (_fingerEventInitFlag) return;

            _fingerEventInitFlag = true;

            switch (m_fingerMoveControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown += StartMove;
                    _mobileInput.OnOneFingerUp += StopMove;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnTwoFingerDown += StartMove;
                    _mobileInput.OnTwoFingerUp += StopMove;
                    break;
            }

            switch (m_fingerRotateControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown += StartRotate;
                    _mobileInput.OnOneFingerUp += StopRotate;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnTwoFingerDown += StartRotate;
                    _mobileInput.OnTwoFingerUp += StopRotate;
                    break;
            }

            switch (m_fingerDragZoomControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown += StartZoom;
                    _mobileInput.OnOneFingerUp += StopZoom;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnTwoFingerDown += StartZoom;
                    _mobileInput.OnTwoFingerUp += StopZoom;
                    break;
            }
        }

        private void RemoveMouseEvent()
        {
            if (!_mouseEventInitFlag)
            {
                return;
            }

            _mouseEventInitFlag = false;
            switch (m_moveControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown -= StartMove;
                    _input.OnMouseLeftButtonUp -= StopMove;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown -= StartMove;
                    _input.OnMouseRightButtonUp -= StopMove;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown -= StartMove;
                    _input.OnMouseMiddleButtonUp -= StopMove;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needMove = false;
                    break;
            }

            switch (m_rotateControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown -= StartRotate;
                    _input.OnMouseLeftButtonUp -= StopRotate;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown -= StartRotate;
                    _input.OnMouseRightButtonUp -= StopRotate;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown -= StartRotate;
                    _input.OnMouseMiddleButtonUp -= StopRotate;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needRotate = false;
                    break;
            }

            switch (m_rollControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown -= StartRoll;
                    _input.OnMouseLeftButtonUp -= StopRoll;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown -= StartRoll;
                    _input.OnMouseRightButtonUp -= StopRoll;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown -= StartRoll;
                    _input.OnMouseMiddleButtonUp -= StopRoll;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needRoll = false;
                    break;
            }

            switch (m_dragZoomControl)
            {
                case ControlMouseKey.LeftKeyControl:
                    _input.OnMouseLeftButtonDown -= StartZoom;
                    _input.OnMouseLeftButtonUp -= StopZoom;
                    break;
                case ControlMouseKey.RightKeyControl:
                    _input.OnMouseRightButtonDown -= StartZoom;
                    _input.OnMouseRightButtonUp -= StopZoom;
                    break;
                case ControlMouseKey.MiddleKeyControl:
                    _input.OnMouseMiddleButtonDown -= StartZoom;
                    _input.OnMouseMiddleButtonUp -= StopZoom;
                    break;
                case ControlMouseKey.AlwaysControl:
                    _needDragZoom = false;
                    break;
            }
        }

        private void RemoveMobileInputEvent()
        {
            if (!_fingerEventInitFlag)
            {
                return;
            }

            _fingerEventInitFlag = false;

            switch (m_fingerMoveControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown -= StartMove;
                    _mobileInput.OnOneFingerUp -= StopMove;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnTwoFingerDown -= StartMove;
                    _mobileInput.OnTwoFingerUp -= StopMove;
                    break;
            }

            switch (m_fingerRotateControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown -= StartRotate;
                    _mobileInput.OnOneFingerUp -= StopRotate;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnOneFingerDown -= StartRotate;
                    _mobileInput.OnOneFingerUp -= StopRotate;
                    break;
            }

            switch (m_fingerDragZoomControl)
            {
                case ControlFinger.OneFinger:
                    _mobileInput.OnOneFingerDown -= StartZoom;
                    _mobileInput.OnOneFingerUp -= StopZoom;
                    break;
                case ControlFinger.TwoFinger:
                    _mobileInput.OnTwoFingerDown -= StartZoom;
                    _mobileInput.OnTwoFingerUp -= StopZoom;
                    break;
            }
        }

        private void StartMove()
        {
            if (MouseNotInUI)
            {
                _needMove = true;
            }
        }

        private void StopMove()
        {
            _needMove = false;
        }

        private void StartRotate()
        {
            if (MouseNotInUI)
            {
                _needRotate = true;
            }
        }

        private void StopRotate()
        {
            _needRotate = false;
        }

        private void StartRoll()
        {
            if (MouseNotInUI)
            {
                _needRoll = true;
            }
        }

        private void StopRoll()
        {
            _needRoll = false;
        }

        private void StartZoom()
        {
            if (MouseNotInUI)
            {
                _needDragZoom = true;
            }
        }

        private void StopZoom()
        {
            _needDragZoom = false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (m_camera != null && m_viewPoint != null)
            {
                m_camera.LookAt(m_viewPoint);
            }
        }
#endif
    }
}
