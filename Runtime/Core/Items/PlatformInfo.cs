using UnityEngine;

namespace NonsensicalKit.Core
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Platform = Application.platform;
            IsEditor = Platform == RuntimePlatform.OSXEditor || Platform == RuntimePlatform.WindowsEditor || Platform == RuntimePlatform.LinuxEditor;
            IsWindow = Platform == RuntimePlatform.WindowsEditor || Platform == RuntimePlatform.WindowsPlayer;
            IsWebGL = Platform == RuntimePlatform.WebGLPlayer;
            IsMobile = Application.isMobilePlatform;
        }
        public static string GetPlaformFolderName()
        {
            switch (Platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXServer:
                    return "OSX";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsServer:
                    return "Windows";
                case RuntimePlatform.IPhonePlayer:
                    return "IOS";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxServer:
                    return "Linux";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerARM:
                    return "WSA";
                case RuntimePlatform.PS4:
                    return "PS4";
                case RuntimePlatform.XboxOne:
                    return "XboxOne";
                case RuntimePlatform.tvOS:
                    return "tvOS";
                case RuntimePlatform.Switch:
                    return "Switch";
                case RuntimePlatform.Lumin:
                    return "Lumin";
                case RuntimePlatform.Stadia:
                    return "Stadia";
                case RuntimePlatform.CloudRendering:
                    return "CloudRendering";
                case RuntimePlatform.GameCoreXboxSeries:
                    return "XboxSeries";
                case RuntimePlatform.GameCoreXboxOne:
                    return "XboxOne";
                case RuntimePlatform.PS5:
                    return "PS5";
                case RuntimePlatform.EmbeddedLinuxArm64:
                case RuntimePlatform.EmbeddedLinuxArm32:
                case RuntimePlatform.EmbeddedLinuxX64:
                case RuntimePlatform.EmbeddedLinuxX86:
                    return "EmbeddedLinux";
                default:
                    return "UnKnow";
            }
        }
    }
}
