using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 在非编辑器环境下自动销毁
    /// </summary>
    public class JustEditor : MonoBehaviour
    {
        private void Awake()
        {
            if (!PlatformInfo.IsEditor)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
