using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Tools.CameraTool
{
    public enum ControlMouseKey
    {
        Uncontrol,
        LeftKeyControl,
        RightKeyControl,
        MiddleKeyControl,
        AlwaysControl,
    }

    public class NonsensicalCamera : NonsensicalMono
    {
        [SerializeField] private Transform m_viewPoint;     //视点
        [SerializeField] private Transform m_camera;        //相机

        [Range(-90, 90)][SerializeField] protected float m_minPitch = -90;  //最小俯仰角
        [Range(-90, 90)][SerializeField] protected float m_maxPitch = 90;   //最大俯仰角

        [SerializeField] protected float m_minDistance = 1;        //最近距离
        [SerializeField] protected float m_maxDistance = 10;       //最远距离

        [SerializeField] protected float m_moveSpeedMinZoom = 1;    //最小移动速度，速度和距离正相关
        [SerializeField] protected float m_moveSpeedMaxZoom = 10;   //最大移动速度
        [SerializeField] protected float m_rotationSpeed = 1;      //旋转速率
        [SerializeField] protected float m_rollSpeed = 1;      //翻滚速率
        [SerializeField] protected float m_zoomSpeed = 3;     //缩放速率

        [SerializeField] protected bool m_checkUI = true;       //是否在鼠标悬浮在UI上时禁止控制相机

        [SerializeField] protected ControlMouseKey m_moveControl = ControlMouseKey.LeftKeyControl;
        [SerializeField] protected ControlMouseKey m_rotateControl = ControlMouseKey.RightKeyControl;
        [SerializeField] protected ControlMouseKey m_rollControl = ControlMouseKey.MiddleKeyControl;
        [SerializeField] protected bool m_UseKeyBoardControlMove = true;

        [SerializeField] protected bool m_autoInit = true;      //自动初始化
        [SerializeField] protected bool m_resetOnEnable = true; //重新激活时回到初始位置

        protected Bounds bounds;
        public bool IsOn { get; set; } = true;
        public Quaternion CrtRotate { get; set; }

        /// <summary>
        /// 目标缩放
        /// </summary>
        protected float _TargetZoom
        {
            get
            {
                return _targetZoom;
            }
            set
            {
                _targetZoom = Mathf.Clamp01(value);
            }
        }
        /// <summary>
        /// 目标距离
        /// </summary>
        protected float _TargetDistance
        {
            get
            {
                return _targetZoom * (m_maxDistance - m_minDistance) + m_minDistance;
            }
            set
            {
                _targetZoom = Mathf.Clamp01((value - m_minDistance) / (m_maxDistance - m_minDistance));
            }
        }

        /// <summary>
        /// 鼠标是否没有放置在UI上
        /// </summary>
        protected bool _MouseNotInUI
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

                return !_crtEventSystem.IsPointerOverGameObject();
            }
        }

        protected Vector3 _targetPos;   //视点目标位置，运行时会不断接近
        protected float _targetZoom;    //目标缩放，范围在0到1之间
        protected float _targetYaw;     //目标旋转角
        protected float _targetPitch;   //目标俯仰角
        protected float _targetRoll;    //目标翻滚角

        private float _crtZoom;         //当前缩放
        private float _crtPitch;        //当前俯仰角
        private float _crtYaw;          //当前旋转角
        private float _crtRoll;         //当前翻滚角

        protected EventSystem _crtEventSystem;
        protected InputHub _input;

        private Vector3 _startPos;      //初始视点位置
        private Quaternion _startRot;   //初始视点旋转
        private Vector3 _startPos2;     //初始相机位置
        private Quaternion _startRot2;  //初始相机旋转

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
        }

        protected virtual void Start()
        {
            _crtEventSystem = EventSystem.current;
            _input = InputHub.Instance;
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
                if (_MouseNotInUI)
                {
                    var v = -_input.CrtZoom;
                    if (v > 0)
                    {
                        v = 1.2f;
                    }
                    else if (v < 0)
                    {
                        v = -1.2f;
                    }
                    if (v != 0)
                    {
                        AdjustZoom(v);
                    }

                    bool needMove = false;
                    switch (m_moveControl)
                    {
                        case ControlMouseKey.LeftKeyControl:
                            needMove = _input.IsMouseLeftButtonHold;
                            break;
                        case ControlMouseKey.RightKeyControl:
                            needMove = _input.IsMouseRightButtonHold;
                            break;
                        case ControlMouseKey.MiddleKeyControl:
                            needMove = _input.IsMouseMiddleButtonHold;
                            break;
                        case ControlMouseKey.AlwaysControl:
                            needMove = true;
                            break;
                        default:
                            break;
                    }
                    if (needMove)
                    {
                        AdjustPosition(_input.CrtMouseMove);
                    }
                }

                bool needRotate = false;
                switch (m_rotateControl)
                {
                    case ControlMouseKey.LeftKeyControl:
                        needRotate = _input.IsMouseLeftButtonHold;
                        break;
                    case ControlMouseKey.RightKeyControl:
                        needRotate = _input.IsMouseRightButtonHold;
                        break;
                    case ControlMouseKey.MiddleKeyControl:
                        needRotate = _input.IsMouseMiddleButtonHold;
                        break;
                    case ControlMouseKey.AlwaysControl:
                        needRotate = true;
                        break;
                    default:
                        break;
                }
                if (needRotate)
                {
                    AdjustRotation(new Vector2(_input.CrtMouseMove.x, -_input.CrtMouseMove.y));
                }

                bool needRoll = false;
                switch (m_rollControl)
                {
                    case ControlMouseKey.LeftKeyControl:
                        needRoll = _input.IsMouseLeftButtonHold;
                        break;
                    case ControlMouseKey.RightKeyControl:
                        needRoll = _input.IsMouseRightButtonHold;
                        break;
                    case ControlMouseKey.MiddleKeyControl:
                        needRoll = _input.IsMouseMiddleButtonHold;
                        break;
                    case ControlMouseKey.AlwaysControl:
                        needRoll = true;
                        break;
                    default:
                        break;
                }
                if (needRoll)
                {
                    AdjustRoll(_input.CrtMouseMove.x);
                }

                if (m_UseKeyBoardControlMove)
                {
                    AdjustPosition(-_input.CrtMove);
                }
            }
        }

        protected void LateUpdate()
        {
            if (m_camera == null || m_viewPoint == null)
            {
                return;
            }

            m_viewPoint.position = Vector3.Lerp(m_viewPoint.position, _targetPos, 0.05f);
            _crtZoom = _crtZoom * 0.95f + _TargetZoom * 0.05f;
            float distance = Mathf.Lerp(m_minDistance, m_maxDistance, _crtZoom);

            _crtPitch = _crtPitch * 0.95f + _targetPitch * 0.05f;
            _crtYaw = _crtYaw * 0.95f + _targetYaw * 0.05f;
            _crtRoll = _crtRoll * 0.95f + _targetRoll * 0.05f;

            CrtRotate = Quaternion.Euler(_crtPitch, _crtYaw, _crtRoll);
            m_camera.position = m_viewPoint.position + CrtRotate * Vector3.forward * -distance;
            m_camera.rotation = CrtRotate;
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
            if (_targetPitch.NatureAngle() == pitch.NatureAngle() && _targetYaw.NatureAngle() == yaw.NatureAngle())
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
        public virtual void Foucs(Vector3 point)
        {
            m_viewPoint.position = point;
            _targetPos = point;
            _targetRoll = 0;
        }

        /// <summary>
        /// 聚焦到某个对象
        /// </summary>
        /// <param name="tsf"></param>
        public virtual void Foucs(Transform tsf)
        {
            Foucs(tsf.position);
        }

        /// <summary>
        /// 根据改变量进行缩放
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustZoom(float delta)
        {
            _TargetZoom += delta * m_zoomSpeed * 0.01f;
        }

        /// <summary>
        /// 根据改变量进行旋转
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRotation(Vector2 delta)
        {
            _targetYaw += delta.x * m_rotationSpeed * 0.3f;
            _targetPitch += delta.y * m_rotationSpeed * 0.3f;
            if (_targetPitch < m_minPitch)
            {
                _targetPitch = m_minPitch;
            }
            else if (_targetPitch >= m_maxPitch)
            {
                _targetPitch = m_maxPitch;
            }
        }

        /// <summary>
        /// 根据改变量进行翻滚
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRoll(float delta)
        {
            _targetRoll += delta * m_rollSpeed * 0.3f;
        }

        /// <summary>
        /// 根据改变量进行位移
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustPosition(Vector2 delta)
        {
            Vector3 direction = m_camera.rotation * new Vector3(-delta.x, -delta.y, 0f).normalized;
            float damping = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, _crtZoom) * damping * Time.deltaTime;
            _targetPos += direction * distance;
        }

        [ContextMenu("LookAt")]
        private void LookAt()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(m_camera, m_camera.gameObject.name);
#endif
            m_camera.LookAt(m_viewPoint);
        }
    }
}
