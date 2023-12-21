using NonsensicalKit.Tools.InputTool;
using UnityEngine;

namespace NonsensicalKit.Tools.CameraTool
{
    /// <summary>
    /// 聚焦摄像机，会围绕目标点旋转
    /// </summary>
    public class FocusCamera : MonoBehaviour
    {
        [SerializeField] private Transform m_target;

        [SerializeField] private float m_zoomSpeed = 10f;
        [SerializeField] private float m_rotateSpeed = 0.8f;
        [SerializeField] private float m_pitchSpeed = 0.5f;

        [SerializeField] private float m_lerpValue = 0.5f;
        [SerializeField, Range(-89, 89)] private float m_minElevation = -89;
        [SerializeField, Range(-89, 89)] private float m_maxElevation = 89;

        private float _oneMinusLerpValue;

        private Vector3 _targetPostion;
        private float _targetRotation;
        private float _targetElevation;
        private float _targetDistance;

        private Vector3 _crtPostion;
        private float _crtRotation;
        private float _crtElevation;
        private float _crtDistance;

        private InputHub _input;

        private void Awake()
        {
            _input = InputHub.Instance;
            _oneMinusLerpValue = 1 - m_lerpValue;
            if (m_minElevation > m_maxElevation)
            {
                m_minElevation = m_maxElevation;
            }

            Init();
        }

        private void Update()
        {
            Input();
            Limit();
            Lerp();
            Move();
        }

        public void SetTarget(Transform newTarget)
        {
            m_target = newTarget;
        }

        private void Init()
        {
            if (m_target != null)
            {
                _targetPostion = m_target.position;
            }
            _crtPostion = _targetPostion;

            Vector3 offset = transform.position - _crtPostion;
            _targetElevation = 90 - Vector3.Angle(offset, Vector3.up);
            _crtElevation = _targetElevation;
            offset.y = 0;
            _targetRotation = Vector3.SignedAngle(-Vector3.forward, offset, Vector3.up);
            _crtRotation = _targetRotation;
            _targetDistance = Vector3.Distance(_crtPostion, transform.position);
            _crtDistance = _targetDistance;
        }

        private void Input()
        {
            var deltaTime = Time.deltaTime;
            if (_input.CrtZoom != 0)
            {
                if (_input.CrtZoom > 0)
                {
                    _targetDistance += m_zoomSpeed * deltaTime;
                }
                else
                {
                    _targetDistance -= m_zoomSpeed * deltaTime;
                }
            }
            if (_input.IsMouseRightButtonHold)
            {
                var mouseMove = _input.CrtMouseMove;
                _targetRotation += mouseMove.x * m_rotateSpeed;
                _targetElevation -= mouseMove.y * m_pitchSpeed;
            }

            if (m_target != null)
            {
                _targetPostion = m_target.position;
            }
        }

        private void Limit()
        {
            _targetElevation = Mathf.Clamp(_targetElevation, m_minElevation, m_maxElevation);
        }

        private void Lerp()
        {
            _crtPostion = Vector3.Lerp(_crtPostion, _targetPostion, m_lerpValue);
            _crtRotation = _crtRotation * _oneMinusLerpValue + _targetRotation * m_lerpValue;
            _crtElevation = _crtElevation * _oneMinusLerpValue + _targetElevation * m_lerpValue;
            _crtDistance = _crtDistance * _oneMinusLerpValue + _targetDistance * m_lerpValue;
        }

        private void Move()
        {
            //通过将旋转负z轴获得新坐标（不使用正z轴因为其正旋时向下）
            transform.position = _crtPostion + Quaternion.Euler(_crtElevation, _crtRotation, 0) * new Vector3(0, 0, -_crtDistance);
            transform.LookAt(_crtPostion);
        }
    }
}
