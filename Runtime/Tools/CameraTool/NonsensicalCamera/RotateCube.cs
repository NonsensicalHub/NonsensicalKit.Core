using UnityEngine;

namespace NonsensicalKit.Tools.CameraTool
{
    public class RotateCube : MonoBehaviour
    {
        [SerializeField] private NonsensicalCamera m_cameraController;

        public void PartClick(Vector3 dir)
        {
            var dirP = new Vector3(dir.x, 0, dir.z);
            var targetYaw = Vector3.SignedAngle(-Vector3.forward, dirP, Vector3.up);

            var targetPitch = 90 - Vector3.Angle(Vector3.up, dir);

            m_cameraController.SetPitchAndYaw(targetPitch, targetYaw);
        }

        private void Update()
        {
            transform.rotation = Quaternion.Inverse(m_cameraController.CrtRotate);
        }

        public void RotateLeft()
        {
            m_cameraController.ChangeRoll(-22.5f);
        }

        public void RotateRight()
        {
            m_cameraController.ChangeRoll(22.5f);
        }
    }
}
