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

        [Range(-90, 90)] [SerializeField] protected float m_minX = -90;
        [Range(-90, 90)] [SerializeField] protected float m_maxX = 90;

        [SerializeField] protected bool m_checkUI = true;
        [SerializeField] protected bool m_UIReverse = false;

        [SerializeField] protected bool m_canMove = true;

        [SerializeField] protected bool m_mouseReverse = false; //默认左键旋转右键平移，反转后左键旋转右键平移

        [SerializeField] protected bool m_autoInit = true;
        [SerializeField] protected bool m_resetOnEnable = true;

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

                if (m_UIReverse)
                {
                    return CrtEventSystem.IsPointerOverGameObject();
                }
                else
                {
                    return !CrtEventSystem.IsPointerOverGameObject();
                }
            }
        }

        protected Vector3 TarPos;
        protected float Zoom;
        protected float YAngle;
        protected float XAngle;
        protected float TargetZoom;
        protected EventSystem CrtEventSystem;
        protected InputHub Input;
        protected bool IsOn = true;

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

        private void OnEnable()
        {
            if (m_resetOnEnable)
            {
                ResetState();
            }
        }


        protected virtual void Update()
        {
            if (CrtEventSystem == null && EventSystem.current != null)
            {
                CrtEventSystem = EventSystem.current;
            }

            if (IsOn)
            {
                if (MouseNotInUI)
                {
                    var v = -Input.CrtZoom;
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
                            if (Input.IsMouseRightButtonHold)
                            {
                                AdjustPosition(Input.CrtMouseMove);
                            }
                        }
                    }
                    else
                    {
                        if (m_canMove)
                        {
                            if (Input.IsMouseLeftButtonHold)
                            {
                                AdjustPosition(Input.CrtMouseMove);
                            }
                        }
                    }
                }

                if ((m_mouseReverse && Input.IsMouseLeftButtonHold) || (!m_mouseReverse && Input.IsMouseRightButtonHold))
                {
                    AdjustRotation(new Vector2(Input.CrtMouseMove.x, -Input.CrtMouseMove.y));
                }
            }
        }

        protected void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, TarPos, 0.05f);
            Zoom = Zoom * 0.95f + TargetZoom * 0.05f;
            float distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, Zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);

            _trueX = _trueX * 0.95f + XAngle * 0.05f;
            _trueY = _trueY * 0.95f + YAngle * 0.05f;

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
            TarPos = transform.position;
            TargetZoom = (m_stick.localPosition.z / (m_stickMinZoom + m_stickMaxZoom));
            Zoom = TargetZoom;
            YAngle = m_swivel.transform.localEulerAngles.y;
            _trueY = YAngle;
            XAngle = m_swivel.transform.localEulerAngles.x;
            while (XAngle > 90)
            {
                XAngle -= 180;
            }

            if (XAngle < m_minX)
            {
                XAngle = m_minX;
            }
            else if (XAngle >= m_maxX)
            {
                XAngle = m_maxX;
            }

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
            TargetZoom = Mathf.Clamp01(TargetZoom + delta * m_zoomSpeed);
        }

        /// <summary>
        /// 根据改变量进行旋转
        /// </summary>
        /// <param name="delta"></param>
        protected void AdjustRotation(Vector2 delta)
        {
            YAngle += delta.x * m_rotationSpeed * Time.deltaTime;

            XAngle += delta.y * m_rotationSpeed * Time.deltaTime;
            if (XAngle < m_minX)
            {
                XAngle = m_minX;
            }
            else if (XAngle >= m_maxX)
            {
                XAngle = m_maxX;
            }
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
