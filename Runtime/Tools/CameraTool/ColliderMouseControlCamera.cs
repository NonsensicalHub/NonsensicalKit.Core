using NonsensicalKit.Core;
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

        protected bool MouseNotInUI
        {
            get
            {
                if (!m_checkUI)
                {
                    return true;
                }

                if (CrtEventSystem == null)
                {
                    return true;
                }

                return !CrtEventSystem.IsPointerOverGameObject();
            }
        }

        protected Vector3 TarPos;
        protected float Zoom;
        protected float YAngle;
        protected float XAngle;
        protected float TargetZoom;
        protected EventSystem CrtEventSystem;
        protected InputHub Input;
        protected  bool IsOn = true;

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
            CrtEventSystem = EventSystem.current;
            Input = InputHub.Instance;
        }

        protected virtual void Update()
        {
            if (CrtEventSystem == null && EventSystem.current != null)
            {
                CrtEventSystem = EventSystem.current;
            }

            if (IsOn && MouseNotInUI)
            {
                var v = -Input.CrtZoom;
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

                if (Input.IsMouseRightButtonHold)
                {
                    AdjustRotation(new Vector2(Input.CrtMousePos.x, -Input.CrtMousePos.x));
                }

                if (m_canMove)
                {
                    if (Input.IsMouseLeftButtonHold)
                    {
                        AdjustPosition(Input.CrtMouseMove);
                    }
                }
            }
        }

        protected void LateUpdate()
        {
            var targetPos = TarPos + Quaternion.Euler(XAngle, YAngle, 0) *
                new Vector3(0, 0, Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, TargetZoom));
            if (Physics.CheckSphere(targetPos, 0.1f, m_checkLayers, QueryTriggerInteraction.Ignore))
            {
                TarPos = transform.position;
                TargetZoom = Mathf.Clamp01(Zoom);
                XAngle = _trueX;
                YAngle = _trueY;
            }

            transform.position = Vector3.Lerp(transform.position, TarPos, 0.05f);
            Zoom = Zoom * 0.95f + TargetZoom * 0.05f;
            float distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, Zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);


            _trueX = _trueX * 0.95f + XAngle * 0.05f;
            _trueY = _trueY * 0.95f + YAngle * 0.05f;

            m_swivel.transform.localRotation = Quaternion.Euler(_trueX, _trueY, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);

            Gizmos.color = transparentGreen;

            var targetPos = TarPos + Quaternion.Euler(XAngle, YAngle, 0) *
                new Vector3(0, 0, Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, TargetZoom));
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
            TarPos = transform.position;
            TargetZoom = (m_stick.localPosition.z / (m_stickMinZoom + m_stickMaxZoom));
            Zoom = TargetZoom;
            YAngle = m_swivel.transform.localEulerAngles.y;
            _trueY = YAngle;
            XAngle = m_swivel.transform.localEulerAngles.x;
            _trueX = XAngle;
        }

        public void Foucs(Transform tsf)
        {
            transform.position = tsf.position;
            TarPos = tsf.position;
        }

        /// <summary>
        /// 根据改变量进行缩放
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustZoom(float delta)
        {
            TargetZoom =Mathf.Clamp01(TargetZoom+delta * m_zoomSpeed) ;
        }

        /// <summary>
        /// 根据改变量进行旋转
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRotation(Vector2 delta)
        {
            YAngle += delta.x * m_rotationSpeed * Time.deltaTime;
            //if (yAngle < 0f)
            //{
            //    yAngle += 360f;
            //}
            //else if (yAngle >= 360f)
            //{
            //    yAngle -= 360f;
            //}

            XAngle += delta.y * m_rotationSpeed * Time.deltaTime;
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
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, Zoom) * damping * Time.deltaTime;

            TarPos += direction * distance;
        }
    }
}
