using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    public class AutoRotate : MonoBehaviour
    {
        [SerializeField] private Vector3 m_rotateSpeed = new Vector3(0, 0, 60);

        private void Update()
        {
            transform.Rotate(m_rotateSpeed * Time.deltaTime);
        }
    }
}
