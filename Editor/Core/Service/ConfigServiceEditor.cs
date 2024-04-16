using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Service.Config
{
    public class ConfigServiceEditor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                var configService = Resources.Load<ConfigService>("Services/ConfigService");

                if (configService != null)
                {
                    FileTool.EnsureDir(Path.Combine(Application.streamingAssetsPath, "Configs"));
                    configService.OnBeforePlay();
                }
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var configService = Resources.Load<ConfigService>("Services/ConfigService");

            if (configService != null)
            {
                FileTool.EnsureDir(Path.Combine(Application.streamingAssetsPath, "Configs"));
                configService.OnBeforeBuild();
            }
        }
    }
}
