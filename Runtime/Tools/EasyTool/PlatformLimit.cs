using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.EasyTool
{
    public enum PlatformLimitMode
    {
        Include,
        Exclude,
    }
    public class PlatformLimit : MonoBehaviour
    {
        [SerializeField] private PlatformLimitMode m_limitMode;
        [SerializeField] private List<RuntimePlatform> m_platforms;

        private void Awake()
        {
            switch (m_limitMode)
            {
                case PlatformLimitMode.Include:
                    if (m_platforms.Contains(Application.platform) == false)
                    {
                        gameObject.SetActive(false);
                    }
                    break;
                case PlatformLimitMode.Exclude:
                    if (m_platforms.Contains(Application.platform))
                    {
                        gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
}
