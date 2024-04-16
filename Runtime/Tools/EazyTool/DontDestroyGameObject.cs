using UnityEngine;

namespace NonsensicalKit.Tools.EazyTool
{
    public class DontDestroyGameObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
