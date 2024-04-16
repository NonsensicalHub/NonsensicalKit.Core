using NonsensicalKit.Tools.EditorTool;
using UnityEngine;

namespace NonsensicalKit.Tools.EazyTool
{
    public enum FixedMode
    {
        FixedRadio,
        FixedLeftDistance,
        FixedRightDistance,
    }

    public class OrthographicFixer : MonoBehaviour
    {
        [SerializeField] private Camera m_orthographicCamera;


        [SerializeField] private FixedMode m_fixedMode;
        [SerializeField][ShowIF("m_fixedMode", FixedMode.FixedRadio)][Range(0, 1)] private float m_radio = 0.9f;
        [SerializeField][ShowIF("m_fixedMode", FixedMode.FixedLeftDistance)][CustomLabel("Distance")] private float m_leftDistance = 200f;
        [SerializeField][ShowIF("m_fixedMode", FixedMode.FixedRightDistance)][CustomLabel("Distance")] private float m_rightDistance = 200f;

        private Transform _cameraTrans;

        private Vector3 _localPos;

        private float _orthographicSize;

        private void Awake()
        {
            _cameraTrans = m_orthographicCamera.transform;
            _localPos = _cameraTrans.InverseTransformPoint(transform.position);
            _orthographicSize = m_orthographicCamera.orthographicSize;
        }

        private void Update()
        {
            float width = Screen.width;
            float height = Screen.height;
            float ratio = width / height;
            float halfSize = ratio * _orthographicSize;
            float horizontalRadio;
            switch (m_fixedMode)
            {
                case FixedMode.FixedRadio:
                    {
                        horizontalRadio = m_radio;
                    }
                    break;
                case FixedMode.FixedLeftDistance:
                case FixedMode.FixedRightDistance:
                    {
                        bool left = m_fixedMode == FixedMode.FixedLeftDistance;
                        float targetPos = left ? m_leftDistance : (width - m_rightDistance);
                        horizontalRadio = targetPos / width;

                    }
                    break;
                default:
                    return;
            }
            var xOffset = halfSize * (horizontalRadio - 0.5f) * 2;
            _localPos.x = xOffset;

            transform.position = _cameraTrans.TransformPoint(_localPos);
        }
    }
}
