using UnityEngine;

namespace NonsensicalKit.Tools
{
    public class EnableChecker : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.Log($"{name} Enabled");
        }

        private void OnDisable()
        {
            Debug.Log($"{name} Disabled");
        }
    }
}
