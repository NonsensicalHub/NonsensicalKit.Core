using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Setting
{
    public class DisplaySettingProvider : ISystemSettingProvider
    {
        public string CategoryId { get { return "display"; } }
        public string CategoryLabel { get { return "显示"; } }
        public int Order { get { return 10; } }
        public SettingTabKind TabKind { get { return SettingTabKind.System; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
            yield return EnumItem("display.resolution", "分辨率",
                "游戏窗口分辨率。修改后立即切换。", BuildResolutionOptions(), SettingRiskLevel.Normal);

            yield return EnumItem("display.mode", "显示模式",
                "窗口 / 全屏窗口 / 独占全屏。独占全屏切换分辨率时会影响桌面显示，退出程序后系统会还原。",
                new List<SettingEnumOption>
                {
                    new SettingEnumOption(((int)FullScreenMode.ExclusiveFullScreen).ToString(), "独占全屏"),
                    new SettingEnumOption(((int)FullScreenMode.FullScreenWindow).ToString(), "全屏窗口"),
                    new SettingEnumOption(((int)FullScreenMode.MaximizedWindow).ToString(), "最大化窗口"),
                    new SettingEnumOption(((int)FullScreenMode.Windowed).ToString(), "窗口")
                }, SettingRiskLevel.Normal);

            yield return new SettingItemDef
            {
                Key = "display.vsync",
                Type = SettingItemType.Enum,
                Label = "垂直同步",
                Description = "开启后帧率锁定到显示器刷新率，可减少画面撕裂。开启时「目标帧率」无效。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "关闭"),
                    new SettingEnumOption("1", "每个垂直空白同步"),
                    new SettingEnumOption("2", "每隔一个垂直空白同步")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "display.targetFrameRate",
                Type = SettingItemType.Int,
                Label = "目标帧率",
                Description = "限制最大帧率。0 或 -1 表示不限制。垂直同步开启时此项无效。",
                Min = -1,
                Max = 240,
                Step = 1,
                Unit = "FPS",
                EnabledIfKey = "display.vsync",
                EnabledIfValue = "0",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "display.runInBackground",
                Type = SettingItemType.Bool,
                Label = "后台运行",
                Description = "窗口失去焦点时是否继续运行。关闭可降低后台资源占用。",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "display.showFPS",
                Type = SettingItemType.Bool,
                Label = "显示帧率",
                Description = "在画面左上角显示实时 FPS 与帧耗时。",
                DefaultValue = "false",
                ApplyMode = SettingApplyMode.Immediate
            };

#if !UNITY_STANDALONE && !UNITY_EDITOR
            yield return new SettingItemDef
            {
                Key = "display.neverSleep",
                Type = SettingItemType.Bool,
                Label = "屏幕常亮",
                Description = "移动设备上阻止自动锁屏。",
                ApplyMode = SettingApplyMode.Immediate
            };
#endif
        }

        public bool IsSupported(string key)
        {
            if (key == "display.neverSleep")
            {
#if UNITY_STANDALONE || UNITY_EDITOR
                return false;
#else
                return true;
#endif
            }

            return key != null && key.StartsWith("display.");
        }

        public SettingValue ReadCurrent(string key)
        {
            switch (key)
            {
                case "display.resolution":
                    return SettingValue.FromString(Screen.width + "x" + Screen.height);
                case "display.mode":
                    return SettingValue.FromInt((int)Screen.fullScreenMode);
                case "display.vsync":
                    return SettingValue.FromInt(QualitySettings.vSyncCount);
                case "display.targetFrameRate":
                    return SettingValue.FromInt(Application.targetFrameRate);
                case "display.runInBackground":
                    return SettingValue.FromBool(Application.runInBackground);
                case "display.showFPS":
                    return SettingValue.FromBool(SettingFpsOverlay.Visible);
                case "display.neverSleep":
                    return SettingValue.FromBool(Screen.sleepTimeout == SleepTimeout.NeverSleep);
                default:
                    return new SettingValue(string.Empty);
            }
        }

        public void Apply(string key, SettingValue value)
        {
            switch (key)
            {
                case "display.resolution":
                    ApplyResolution(value.AsString(), Screen.fullScreenMode);
                    break;
                case "display.mode":
                    FullScreenMode mode = (FullScreenMode)value.AsInt();
                    ApplyResolution(Screen.width + "x" + Screen.height, mode);
                    break;
                case "display.vsync":
                    QualitySettings.vSyncCount = Mathf.Clamp(value.AsInt(), 0, 2);
                    break;
                case "display.targetFrameRate":
                    Application.targetFrameRate = value.AsInt();
                    break;
                case "display.runInBackground":
                    Application.runInBackground = value.AsBool();
                    break;
                case "display.showFPS":
                    SettingFpsOverlay.Visible = value.AsBool();
                    break;
                case "display.neverSleep":
                    Screen.sleepTimeout = value.AsBool() ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
                    break;
            }
        }

        private static void ApplyResolution(string res, FullScreenMode mode)
        {
            int w = Screen.width;
            int h = Screen.height;
            if (!string.IsNullOrEmpty(res))
            {
                string[] parts = res.Split('x');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0], out w);
                    int.TryParse(parts[1], out h);
                }
            }

            if (w <= 0 || h <= 0)
            {
                return;
            }

            Screen.SetResolution(w, h, mode);
        }

        private static SettingItemDef EnumItem(string key, string label, string desc, List<SettingEnumOption> options,
            SettingRiskLevel risk)
        {
            return new SettingItemDef
            {
                Key = key,
                Type = SettingItemType.Enum,
                Label = label,
                Description = desc,
                Options = options,
                Risk = risk,
                ApplyMode = SettingApplyMode.Immediate
            };
        }

        private static List<SettingEnumOption> BuildResolutionOptions()
        {
            List<SettingEnumOption> list = new List<SettingEnumOption>();
            HashSet<string> seen = new HashSet<string>();
            Resolution[] resolutions = Screen.resolutions;
            if (resolutions != null)
            {
                for (int i = 0; i < resolutions.Length; i++)
                {
                    string s = resolutions[i].width + "x" + resolutions[i].height;
                    if (seen.Add(s))
                    {
                        list.Add(new SettingEnumOption(s, s));
                    }
                }
            }

            string[] fallback =
            {
                "1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160"
            };
            for (int i = 0; i < fallback.Length; i++)
            {
                if (seen.Add(fallback[i]))
                {
                    list.Add(new SettingEnumOption(fallback[i], fallback[i]));
                }
            }

            return list;
        }
    }
}
