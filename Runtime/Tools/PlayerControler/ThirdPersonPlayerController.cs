using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.PlayerController
{
    public enum RotateControlType
    {
        Always,
        OnMouseLeftButtonHold,
        OnMouseRightButtonHold,
    }

    /// <summary>
    /// 第三人称控制器
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonPlayerController : NonsensicalMono
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s||水平移动速度，单位米/秒")]
        [SerializeField] private float m_moveSpeed = 4;

        [Tooltip("Sprint speed of the character in m/s||冲刺速度，单位米/秒")]
        [SerializeField] private float m_sprintSpeed = 8;

        [Tooltip("Acceleration and deceleration||移动加速度")]
        [SerializeField] private float m_speedChangeRate = 10.0f;

        [Tooltip("Rotation speed of the camera||摄像机旋转速度")]
        [SerializeField] private float m_rotationSpeed = 0.05f;

        [Tooltip("How fast the character turns to face movement direction||角色旋转平滑时间")]
        [Range(0.0f, 0.3f)]
        [SerializeField] private float m_rotationSmoothTime = 0.12f;

        [Space(10)]
        [Tooltip("The height the player can jump||跳跃高度")]
        [SerializeField] private float m_jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f||重力加速度")]
        [SerializeField] private float m_gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again||跳跃最低间隔")]
        [SerializeField] private float m_jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs||坠落延迟")]
        [SerializeField] private float m_fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("Useful for rough ground||接地碰撞球偏移量")]
        [SerializeField] private float m_groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController||接地碰撞球尺寸")]
        [SerializeField] private float m_groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground||接地检测层")]
        [SerializeField] private LayerMask m_groundLayers;

        [Tooltip("旋转控制模式")]
        [SerializeField] private RotateControlType m_rotateControlType;

        [FormerlySerializedAs("_mainCamera")]
        [Header("Cinemachine")]
        [Tooltip("Main Camera||主摄像机")]
        [SerializeField] private GameObject m_mainCamera;

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow||角色的Cinemachine虚拟相机跟随对象")]
        [SerializeField] private GameObject m_cinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up||旋转上限角度")]
        [SerializeField] private float m_topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down||旋转下限角度")]
        [SerializeField] private float m_bottomClamp = -30.0f;

        /// <summary>
        /// 临界垂直速度，当下落时垂直速度大于此速度时不再加速，以模拟空气阻力
        /// </summary>
        private const float TerminalVelocity = 53.0f;

        /// <summary>
        /// 摄像机旋转最低临界值，当旋转变量的长度小于此值时不执行摄像机旋转，放置因为鼠标轻微抖动导致的画面抖动
        /// </summary>
        private const float Threshold = 0.01f;

        /// <summary>
        /// 控制是否能够控制移动和旋转
        /// </summary>
        public bool CanControl
        {
            set
            {
                _canMove = value;
                _canRotate = value;
            }
        }

        public bool CanMove { get => _canMove; set => _canMove = value; }
        public bool CanRotate { get => _canRotate; set => _canRotate = value; }
        private bool _canMove = true;
        private bool _canRotate = true;


        /// <summary>
        /// 当前水平速度
        /// </summary>
        protected float HorizontalVelocity;

        /// <summary>
        /// 当前垂直速度
        /// </summary>
        protected float VerticalVelocity;

        /// <summary>
        /// 是否正在下落
        /// </summary>
        protected bool Falling;

        /// <summary>
        /// 水平位移输入移动幅度
        /// </summary>
        protected float InputMagnitude;

        /// <summary>
        /// 角色是否接地
        /// </summary>
        protected bool Grounded = true;

        /// <summary>
        /// 输入
        /// </summary>
        protected InputHub Input;

        /// <summary>
        /// 相机俯仰
        /// </summary>
        private float _cinemachineTargetYaw;

        /// <summary>
        /// 相机旋转
        /// </summary>
        private float _cinemachineTargetPitch;

        /// <summary>
        /// 当前移动方向所代表的目标人物旋转方向
        /// </summary>
        private float _targetRotation;

        /// <summary>
        /// 当前人物旋转速度
        /// </summary>
        private float _rotationVelocity;

        /// <summary>
        /// 再次跳跃计时
        /// </summary>
        private float _jumpTimeoutDelta;

        /// <summary>
        /// 下落延迟计时
        /// </summary>
        private float _fallTimeoutDelta;

        /// <summary>
        /// 角色控制组件
        /// </summary>
        private CharacterController _controller;

        /// <summary>
        /// 用户是否按下跳跃键尝试进行跳跃
        /// </summary>
        private bool _jump;

        protected virtual void Awake()
        {
            Input = InputHub.Instance;
            Input.OnSpaceKeyEnter += OnJumpKeyEnter;

            if (m_mainCamera == null)
            {
                m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        protected virtual void Start()
        {
            _cinemachineTargetYaw = m_cinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _controller = GetComponent<CharacterController>();

            // reset our timeouts on start
            _jumpTimeoutDelta = m_jumpTimeout;
            _fallTimeoutDelta = m_fallTimeout;
        }

        protected virtual void Update()
        {
            JumpAndGravity();
            GroundedCheck();

            if (_canMove)
            {
                Move();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_groundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            if (_canRotate)
            {
                bool controlCameraRotate;
                switch (m_rotateControlType)
                {
                    case RotateControlType.OnMouseLeftButtonHold:
                        controlCameraRotate = Input.IsMouseLeftButtonHold;
                        break;
                    case RotateControlType.OnMouseRightButtonHold:
                        controlCameraRotate = Input.IsMouseRightButtonHold;
                        break;
                    default:
                        controlCameraRotate = true;
                        break;
                }

                if (controlCameraRotate)
                {
                    // if there is an input
                    var crtLook = new Vector2(Input.CrtMouseMove.x, -Input.CrtMouseMove.y);
                    // if there is an input and camera position is not fixed
                    if (crtLook.sqrMagnitude >= Threshold)
                    {
                        _cinemachineTargetYaw += crtLook.x * m_rotationSpeed;
                        _cinemachineTargetPitch += crtLook.y * m_rotationSpeed;
                    }
                }
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, m_bottomClamp, m_topClamp);

            // Cinemachine will follow this target
            m_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = Input.IsLeftShiftKeyHold ? m_sprintSpeed : m_moveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (Input.CrtMove == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            InputMagnitude = Input.CrtMove.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                HorizontalVelocity = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * InputMagnitude,
                    Time.deltaTime * m_speedChangeRate);

                // round speed to 3 decimal places
                HorizontalVelocity = Mathf.Round(HorizontalVelocity * 1000f) / 1000f;
            }
            else
            {
                HorizontalVelocity = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(Input.CrtMove.x, 0.0f, Input.CrtMove.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (Input.CrtMove != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  m_mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    m_rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (HorizontalVelocity * Time.deltaTime) +
                             new Vector3(0.0f, VerticalVelocity, 0.0f) * Time.deltaTime);
        }

        protected virtual void AfterJump()
        {
        }

        private void JumpAndGravity()
        {
            Falling = false;
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = m_fallTimeout;


                // stop our velocity dropping infinitely when grounded
                if (VerticalVelocity < 0.0f)
                {
                    VerticalVelocity = -2f;
                }

                // Jump
                if (_jump && _canMove && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    VerticalVelocity = Mathf.Sqrt(m_jumpHeight * -2f * m_gravity);
                    AfterJump();
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = m_jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    Falling = true;
                }

                // if we are not grounded, do not jump
                _jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (VerticalVelocity < TerminalVelocity)
            {
                VerticalVelocity += m_gravity * Time.deltaTime;
            }
        }

        private void OnJumpKeyEnter()
        {
            _jump = true;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z),
                m_groundedRadius);
        }
    }
}
