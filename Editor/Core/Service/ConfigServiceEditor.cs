using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NonsensicalKit.Core.Editor.Service.Config
{
    public class ConfigServiceEditor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var configService = Resources.Load<ConfigService>("Services/ConfigService");

            if (configService != null)
            {
                FileTool.EnsureDir(Path.Combine(Application.streamingAssetsPath, "Configs"));
                configService.WriteConfigs();
            }
        }
    }
}
