using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace NonsensicalKit.Setting
{
    public class DiagnosticsProvider : ISystemSettingProvider
    {
        public string CategoryId { get { return "diagnostics"; } }
        public string CategoryLabel { get { return "诊断信息"; } }
        public int Order { get { return 900; } }
        public SettingTabKind TabKind { get { return SettingTabKind.Diagnostics; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
            yield return Info("diag.unity", "Unity 版本", Application.unityVersion);
            yield return Info("diag.os", "操作系统", SystemInfo.operatingSystem);
            yield return Info("diag.device", "设备型号", SystemInfo.deviceModel);
            yield return Info("diag.cpu", "处理器", SystemInfo.processorType + " ×" + SystemInfo.processorCount);
            yield return Info("diag.ram", "系统内存", SystemInfo.systemMemorySize + " MB");
            yield return Info("diag.gpu", "显卡", SystemInfo.graphicsDeviceName);
            yield return Info("diag.vram", "显存", SystemInfo.graphicsMemorySize + " MB");
            yield return Info("diag.api", "图形 API", SystemInfo.graphicsDeviceType + " " + SystemInfo.graphicsDeviceVersion);
            yield return Info("diag.pipeline", "渲染管线", GetPipelineName());
            yield return Info("diag.screen", "当前分辨率", BuildScreenInfo());

            yield return new SettingItemDef
            {
                Key = "diag.openLog",
                Type = SettingItemType.Button,
                Label = "打开日志目录",
                Description = "打开 Unity 日志所在文件夹。",
                ButtonAction = "openLogFolder"
            };

            yield return new SettingItemDef
            {
                Key = "diag.openData",
                Type = SettingItemType.Button,
                Label = "打开数据目录",
                Description = "打开 persistentDataPath（存档与自定义数据）。",
                ButtonAction = "openDataFolder"
            };
        }

        public bool IsSupported(string key)
        {
            return key != null && key.StartsWith("diag.");
        }

        public SettingValue ReadCurrent(string key)
        {
            return new SettingValue(string.Empty);
        }

        public void Apply(string key, SettingValue value)
        {
            // 诊断项只读 / 按钮
        }

        private static SettingItemDef Info(string key, string label, string value)
        {
            return new SettingItemDef
            {
                Key = key,
                Type = SettingItemType.Info,
                Label = label,
                Description = value,
                DefaultValue = value,
                ReadOnly = true
            };
        }

        private static string GetPipelineName()
        {
            RenderPipelineAsset asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return "内置渲染管线 (Built-in)";
            }

            return asset.GetType().Name;
        }

        private static string BuildScreenInfo()
        {
#if UNITY_2022_2_OR_NEWER
            var rr = Screen.currentResolution.refreshRateRatio;
            double hz = rr.denominator != 0 ? (double)rr.numerator / rr.denominator : 0;
            return Screen.width + "×" + Screen.height + " @" + hz.ToString("0.#") + "Hz";
#else
            return Screen.width + "×" + Screen.height + " @" + Screen.currentResolution.refreshRate + "Hz";
#endif
        }
    }
}
