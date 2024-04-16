using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.CameraTool
{
    public class RotateCubePart : MonoBehaviour
    {
        [SerializeField] private Vector3 m_dir;
        [SerializeField] private UnityEvent m_onMouseEnter;
        [SerializeField] private UnityEvent m_onMouseExit;
        private RotateCube _cube;

        private void Awake()
        {
            _cube = GetComponentInParent<RotateCube>();
        }

        public void OnMouseEnter()
        {
            m_onMouseEnter?.Invoke();
        }

        public void OnMouseUpAsButton()
        {
            _cube.PartClick(m_dir);
        }

        public void OnMouseExit()
        {
            m_onMouseExit?.Invoke();
        }
    }
}
