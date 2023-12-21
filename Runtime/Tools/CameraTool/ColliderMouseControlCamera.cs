using NonsensicalKit.Editor;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Tools.CameraTool
{
    /// <summary>
    /// MouseControlCamera为基础的碰撞体检测版
    /// </summary>
    public class ColliderMouseControlCamera : NonsensicalMono
    {
        /// <summary>
        /// 旋轴（与视点旋转）
        /// </summary>
        [SerializeField] protected Transform m_swivel;
        /// <summary>
        /// 旋杆（与视点距离）
        /// </summary>
        [SerializeField] protected Transform m_stick;

        [SerializeField] protected float m_stickMinZoom = -1;
        [SerializeField] protected float m_stickMaxZoom = -10;
        [SerializeField] protected float m_moveSpeedMinZoom = 1;
        [SerializeField] protected float m_moveSpeedMaxZoom = 10;
        [SerializeField] protected float m_rotationSpeed = 30;
        [SerializeField] protected float m_zoomSpeed = 0.0001f;
        [SerializeField] protected bool m_canMove = true;
        [SerializeField] protected LayerMask m_checkLayers;
        [SerializeField] protected bool m_checkUI = true;
        [SerializeField] protected bool m_autoInit = true;

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

                return !_crtEventSystem.IsPointerOverGameObject();
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

        protected virtual void Update()
        {
            if (_crtEventSystem == null && EventSystem.current != null)
            {
                _crtEventSystem = EventSystem.current;
            }

            if (_isOn && _MouseNotInUI)
            {
                var v = -_input.CrtZoom;
                if (v > 0)
                {
                    v = 120;
                }
                else if (v < 0)
                {
                    v = -120;
                }
                if (v != 0)
                {
                    AdjustZoom(v);
                }
                if (_input.IsMouseRightButtonHold)
                {
                    AdjustRotation(new Vector2( _input.CrtMousePos.x, -_input.CrtMousePos.x));
                }
                if (m_canMove)
                {
                    if (_input.IsMouseLeftButtonHold)
                    {
                        AdjustPosition(_input.CrtMouseMove);
                    }
                }
            }
        }

        protected void LateUpdate()
        {
            var targetPos = _tarPos + Quaternion.Euler(_xAngle, _yAngle, 0) * new Vector3(0, 0, Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, _TargetZoom));
            if (Physics.CheckSphere(targetPos, 0.1f, m_checkLayers, QueryTriggerInteraction.Ignore))
            {
                _tarPos = transform.position;
                _TargetZoom = _zoom;
                _xAngle = _trueX;
                _yAngle = _trueY;
            }

            transform.position = Vector3.Lerp(transform.position, _tarPos, 0.05f);
            _zoom = _zoom * 0.95f + _TargetZoom * 0.05f;
            float distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, _zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);



            _trueX = _trueX * 0.95f + _xAngle * 0.05f;
            _trueY = _trueY * 0.95f + _yAngle * 0.05f;

            m_swivel.transform.localRotation = Quaternion.Euler(_trueX, _trueY, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);

            Gizmos.color = transparentGreen;

            var targetPos = _tarPos + Quaternion.Euler(_xAngle, _yAngle, 0) * new Vector3(0, 0, Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, _TargetZoom));
            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(targetPos, 0.1f);
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
            //if (yAngle < 0f)
            //{
            //    yAngle += 360f;
            //}
            //else if (yAngle >= 360f)
            //{
            //    yAngle -= 360f;
            //}

            _xAngle += delta.y * m_rotationSpeed * Time.deltaTime;
            //if (xAngle < 0f)
            //{
            //    xAngle += 360f;
            //}
            //else if (xAngle >= 360f)
            //{
            //    xAngle -= 360f;
            //}
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
