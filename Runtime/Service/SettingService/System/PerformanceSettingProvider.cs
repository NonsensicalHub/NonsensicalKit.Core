using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace NonsensicalKit.Setting
{
    public class PerformanceSettingProvider : ISystemSettingProvider
    {
        public string CategoryId { get { return "performance"; } }
        public string CategoryLabel { get { return "性能"; } }
        public int Order { get { return 40; } }
        public SettingTabKind TabKind { get { return SettingTabKind.System; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
#if UNITY_2019_3_OR_NEWER
            yield return new SettingItemDef
            {
                Key = "performance.onDemandInterval",
                Type = SettingItemType.Int,
                Label = "降频渲染间隔",
                Description =
                    "OnDemandRendering：每隔 N 帧渲染一次。1=正常，大于 1 可在空闲时省电/降耗。需要引擎支持（2019.3+）。",
                Min = 1,
                Max = 10,
                Step = 1,
                ApplyMode = SettingApplyMode.Immediate
            };
#endif

            yield return new SettingItemDef
            {
                Key = "performance.backgroundLoadingPriority",
                Type = SettingItemType.Enum,
                Label = "后台加载优先级",
                Description = "异步加载资源时的线程优先级，影响加载速度与主线程流畅度的平衡。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption(((int)ThreadPriority.Low).ToString(), "低"),
                    new SettingEnumOption(((int)ThreadPriority.BelowNormal).ToString(), "较低"),
                    new SettingEnumOption(((int)ThreadPriority.Normal).ToString(), "正常"),
                    new SettingEnumOption(((int)ThreadPriority.High).ToString(), "高")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "performance.streamingMipmaps",
                Type = SettingItemType.Bool,
                Label = "纹理流送",
                Description = "启用 Mipmap 流送，按需加载纹理级别以节省显存。",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "performance.streamingMemory",
                Type = SettingItemType.Int,
                Label = "纹理流送内存预算",
                Description = "纹理流送可用的内存预算（MB）。",
                Min = 64,
                Max = 2048,
                Step = 32,
                Unit = "MB",
                VisibleIfKey = "performance.streamingMipmaps",
                VisibleIfValue = "true",
                ApplyMode = SettingApplyMode.Immediate
            };
        }

        public bool IsSupported(string key)
        {
            if (key == "performance.onDemandInterval")
            {
#if UNITY_2019_3_OR_NEWER
                return true;
#else
                return false;
#endif
            }

            return key != null && key.StartsWith("performance.");
        }

        public SettingValue ReadCurrent(string key)
        {
            switch (key)
            {
#if UNITY_2019_3_OR_NEWER
                case "performance.onDemandInterval":
                    return SettingValue.FromInt(OnDemandRendering.renderFrameInterval);
#endif
                case "performance.backgroundLoadingPriority":
                    return SettingValue.FromInt((int)Application.backgroundLoadingPriority);
                case "performance.streamingMipmaps":
                    return SettingValue.FromBool(QualitySettings.streamingMipmapsActive);
                case "performance.streamingMemory":
                    return SettingValue.FromInt(Mathf.RoundToInt(QualitySettings.streamingMipmapsMemoryBudget));
                default:
                    return new SettingValue(string.Empty);
            }
        }

        public void Apply(string key, SettingValue value)
        {
            switch (key)
            {
#if UNITY_2019_3_OR_NEWER
                case "performance.onDemandInterval":
                    OnDemandRendering.renderFrameInterval = Mathf.Clamp(value.AsInt(), 1, 10);
                    break;
#endif
                case "performance.backgroundLoadingPriority":
                    Application.backgroundLoadingPriority = (ThreadPriority)value.AsInt();
                    break;
                case "performance.streamingMipmaps":
                    QualitySettings.streamingMipmapsActive = value.AsBool();
                    break;
                case "performance.streamingMemory":
                    QualitySettings.streamingMipmapsMemoryBudget = value.AsInt();
                    break;
            }
        }
    }
}
