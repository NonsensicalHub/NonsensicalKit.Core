using UnityEngine;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 运行平台相关信息
    /// </summary>
    public static class PlatformInfo
    {
        public static RuntimePlatform Platform { get; private set; }
        public static bool IsEditor { get; private set; }
        public static bool IsWindow { get; private set; }
        public static bool IsWebGL { get; private set; }
        public static bool IsMobile { get; private set; }

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Platform = Application.platform;
            IsEditor = Platform == RuntimePlatform.OSXEditor || Platform == RuntimePlatform.WindowsEditor || Platform == RuntimePlatform.LinuxEditor;
            IsWindow = Platform == RuntimePlatform.WindowsEditor || Platform == RuntimePlatform.WindowsPlayer;
            IsWebGL = Platform == RuntimePlatform.WebGLPlayer;
            IsMobile = Application.isMobilePlatform;
        }
    }
}
