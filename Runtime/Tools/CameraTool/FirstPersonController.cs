using NonsensicalKit.Core;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace NonsensicalKit.Tools.CameraTool
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : NonsensicalMono
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] private float m_moveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] private float m_sprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        [SerializeField] private float m_rotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float m_speedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        [SerializeField] private float m_jumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float m_gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] private float m_jumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] private float m_fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool m_grounded = true;
        [Tooltip("Useful for rough ground")]
        [SerializeField] private float m_groundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float m_groundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        [SerializeField] private LayerMask m_groundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject m_cinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float m_topClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float m_bottomClamp = -90.0f;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private CharacterController _controller;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private InputHub _input;

        protected bool _canControl=true;

        private Vector3 _startPos;
        private Quaternion _startRot;

        private bool _jump;

        protected virtual void Awake()
        {
            _input = InputHub.Instance;
            _input.OnSpaceKeyEnter += OnJumpKeyEnter;

            _startPos = transform.localPosition;
            _startRot = transform.localRotation;

            if (_input == null)
            {
                Debug.LogError("未找到输入管理类");
            }

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            // reset our timeouts on start
            _jumpTimeoutDelta = m_jumpTimeout;
            _fallTimeoutDelta = m_fallTimeout;
        }

        private void Update()
        {
            if (_canControl)
            {
                JumpAndGravity();
                GroundedCheck();
                Move();
            }
        }

        private void LateUpdate()
        {
            if (_input.IsMouseRightButtonHold)
            {
                CameraRotation();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (m_grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z), m_groundedRadius);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _input.OnSpaceKeyEnter -= OnJumpKeyEnter;
        }

        protected void ResetState()
        {
            transform.localPosition = _startPos;
            transform.localRotation = _startRot;

        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z);
            m_grounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            // if there is an input
            var crtLook = new Vector2(_input.CrtMousePos.x, -_input.CrtMousePos.x);
            if (crtLook.sqrMagnitude >= _threshold)
            {
                _cinemachineTargetPitch += crtLook.y * m_rotationSpeed * Time.deltaTime;
                _rotationVelocity = crtLook.x * m_rotationSpeed * Time.deltaTime;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, m_bottomClamp, m_topClamp);

                // Update Cinemachine camera target pitch
                m_cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.IsLeftShiftKeyHold ? m_sprintSpeed : m_moveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.CrtMove == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.CrtMove.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * m_speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.CrtMove.x, 0.0f, _input.CrtMove.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.CrtMove != Vector2.zero)
            {
                // move
                inputDirection = transform.right * _input.CrtMove.x + transform.forward * _input.CrtMove.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (m_grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = m_fallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(m_jumpHeight * -2f * m_gravity);
                    _jump = false;
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

                // if we are not grounded, do not jump
                _jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += m_gravity * Time.deltaTime;
            }
        }
        private void OnJumpKeyEnter()
        {
            _jump = true;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            while (lfAngle < -360f)
            {
                lfAngle += 360f;
            }
            while (lfAngle > 360)
            {
                lfAngle -= 360f;
            }
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
