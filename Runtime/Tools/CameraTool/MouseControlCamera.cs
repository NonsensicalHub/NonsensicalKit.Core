using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Tools.CameraTool
{
    /// <summary>
    /// 完全鼠标控制摄像机，用于键盘不方便操控的场景，如网页
    /// 有一个虚拟的视点，右键围绕试点旋转，左键以当前正前方为基准向上下左右移动试点
    /// 分为四级
    /// 第一级挂载脚本，负责平移
    /// 第二级swivel负责旋转
    /// 第三级stick负责远近
    /// 第四级为摄像机
    /// </summary>
    public class MouseControlCamera : NonsensicalMono
    {
        /// <summary>
        /// 旋轴（与视点旋转）
        /// </summary>
        [SerializeField] private Transform m_swivel;
        /// <summary>
        /// 旋杆（与视点距离）
        /// </summary>
        [SerializeField] private Transform m_stick;

        [SerializeField] protected float m_stickMinZoom = -1;
        [SerializeField] protected float m_stickMaxZoom = -10;
        [SerializeField] protected float m_moveSpeedMinZoom = 1;
        [SerializeField] protected float m_moveSpeedMaxZoom = 10;
        [SerializeField] protected float m_rotationSpeed = 30;
        [SerializeField] protected float m_zoomSpeed = 0.0001f;

        [Range(-90, 90)][SerializeField] protected float m_minX = -90;
        [Range(-90, 90)][SerializeField] protected float m_maxX = 90;

        [SerializeField] protected bool m_checkUI = true;
        [SerializeField] protected bool m_UIReverse = false;

        [SerializeField] protected bool m_canMove = true;

        [SerializeField] protected bool m_mouseReverse = false; //默认左键旋转右键平移，反转后左键旋转右键平移

        [SerializeField] protected bool m_autoInit = true;
        [SerializeField] protected bool m_resetOnEnable = true;

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

                if (m_UIReverse)
                {
                    return _crtEventSystem.IsPointerOverGameObject();

                }
                else
                {
                    return !_crtEventSystem.IsPointerOverGameObject();
                }

            }
        }

        protected Vector3 _tarPos;
        protected float _zoom;
        protected float _yAngle;
        protected float _xAngle;
        protected float _targetZoom;
        protected EventSystem _crtEventSystem;
        protected InputHub _input;
        protected bool _isOn = true;

        private Vector3 _startPos;
        private Vector3 _startRot;
        private Vector3 _startZoom;
        private float _trueX;
        private float _trueY;

        protected virtual void Awake()
        {
            _startPos = transform.localPosition;
            _startRot = m_swivel.transform.localEulerAngles;
            _startZoom = m_stick.localPosition;

            if (m_autoInit)
            {
                Init();
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
            if (_crtEventSystem == null && EventSystem.current != null)
            {
                _crtEventSystem = EventSystem.current;
            }

            if (_isOn)
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
                    if (m_mouseReverse)
                    {
                        if (m_canMove)
                        {
                            if (_input.IsMouseRightButtonHold)
                            {
                                AdjustPosition(_input.CrtMouseMove);
                            }
                        }
                    }
                    else
                    {
                        if (m_canMove)
                        {
                            if (_input.IsMouseLeftButtonHold)
                            {
                                AdjustPosition(_input.CrtMouseMove);
                            }
                        }
                    }
                }
                if ((m_mouseReverse && _input.IsMouseLeftButtonHold) || (!m_mouseReverse && _input.IsMouseRightButtonHold))
                {
                    AdjustRotation(new Vector2(_input.CrtMouseMove.x, -_input.CrtMouseMove.y));
                }
            }
        }

        protected void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, _tarPos, 0.05f);
            _zoom = _zoom * 0.95f + _TargetZoom * 0.05f;
            float distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, _zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);

            _trueX = _trueX * 0.95f + _xAngle * 0.05f;
            _trueY = _trueY * 0.95f + _yAngle * 0.05f;

            m_swivel.transform.localRotation = Quaternion.Euler(_trueX, _trueY, 0f);
        }

        public void ResetState()
        {
            transform.localPosition = _startPos;
            m_swivel.transform.localEulerAngles = _startRot;
            m_stick.localPosition = _startZoom;

            Init();
        }

        public void Init()
        {
            _tarPos = transform.position;
            _targetZoom = (m_stick.localPosition.z / (m_stickMinZoom + m_stickMaxZoom));
            _zoom = _targetZoom;
            _yAngle = m_swivel.transform.localEulerAngles.y;
            _trueY = _yAngle;
            _xAngle = m_swivel.transform.localEulerAngles.x;
            while (_xAngle > 90)
            {
                _xAngle -= 180;
            }
            if (_xAngle < m_minX)
            {
                _xAngle = m_minX;
            }
            else if (_xAngle >= m_maxX)
            {
                _xAngle = m_maxX;
            }
            _trueX = _xAngle;
        }

        public void Foucs(Transform tsf)
        {
            transform.position = tsf.position;
            _tarPos = tsf.position;
        }

        /// <summary>
        /// 根据改变量进行缩放
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustZoom(float delta)
        {
            _TargetZoom += delta * m_zoomSpeed;
        }

        /// <summary>
        /// 根据改变量进行旋转
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRotation(Vector2 delta)
        {
            _yAngle += delta.x * m_rotationSpeed * Time.deltaTime;

            _xAngle += delta.y * m_rotationSpeed * Time.deltaTime;
            if (_xAngle < m_minX)
            {
                _xAngle = m_minX;
            }
            else if (_xAngle >= m_maxX)
            {
                _xAngle = m_maxX;
            }
        }

        protected void AdjustPosition(Vector2 delta)
        {
            Vector3 direction = m_swivel.transform.localRotation * new Vector3(-delta.x, -delta.y, 0f).normalized;
            float damping = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, _zoom) * damping * Time.deltaTime;
            _tarPos += direction * distance;
        }
    }
}
