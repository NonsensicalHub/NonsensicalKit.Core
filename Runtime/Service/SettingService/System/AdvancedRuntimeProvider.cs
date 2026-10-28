using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace NonsensicalKit.Setting
{
    public class AdvancedRuntimeProvider : ISystemSettingProvider
    {
        public string CategoryId { get { return "runtime"; } }
        public string CategoryLabel { get { return "运行时 ⚠"; } }
        public int Order { get { return 10; } }
        public SettingTabKind TabKind { get { return SettingTabKind.Advanced; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
            yield return new SettingItemDef
            {
                Key = "advanced.fixedDeltaTime",
                Type = SettingItemType.Float,
                Label = "物理固定步长",
                Description =
                    "Time.fixedDeltaTime。影响物理仿真精度与稳定性。过小会增加 CPU 开销，过大可能导致穿透。",
                Min = 0.005f,
                Max = 0.05f,
                Step = 0.001f,
                Unit = "s",
                Risk = SettingRiskLevel.Danger,
                ConfirmMessage = "修改物理步长会改变仿真结果，可能导致碰撞穿透或性能下降。确认继续？",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.maximumDeltaTime",
                Type = SettingItemType.Float,
                Label = "最大允许帧时间",
                Description = "Time.maximumDeltaTime。单帧逻辑时间上限，防止卡顿后物理爆炸式追帧。",
                Min = 0.05f,
                Max = 1f,
                Step = 0.01f,
                Unit = "s",
                Risk = SettingRiskLevel.Danger,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.solverIterations",
                Type = SettingItemType.Int,
                Label = "物理求解迭代次数",
                Description = "Physics.defaultSolverIterations。越高约束越稳，但 CPU 更重。",
                Min = 1,
                Max = 20,
                Step = 1,
                Risk = SettingRiskLevel.Danger,
                ApplyMode = SettingApplyMode.Immediate
            };

#if UNITY_2019_1_OR_NEWER
            yield return new SettingItemDef
            {
                Key = "advanced.gcMode",
                Type = SettingItemType.Enum,
                Label = "GC 模式",
                Description =
                    "关闭 GC 可避免卡顿，但内存会持续上涨直至崩溃。仅建议在极短关键场景或专家指导下使用。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("1", "启用"),
                    new SettingEnumOption("0", "禁用 ⚠")
                },
                Risk = SettingRiskLevel.Danger,
                ConfirmMessage = "禁用 GC 将导致内存持续增长，可能造成程序崩溃。确认继续？",
                ApplyMode = SettingApplyMode.Immediate
            };
#endif

            yield return new SettingItemDef
            {
                Key = "advanced.asyncUploadTimeSlice",
                Type = SettingItemType.Int,
                Label = "异步上传时间片",
                Description = "每帧用于异步纹理/网格上传的毫秒数。",
                Min = 1,
                Max = 16,
                Step = 1,
                Unit = "ms",
                Risk = SettingRiskLevel.Advanced,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.asyncUploadBufferSize",
                Type = SettingItemType.Int,
                Label = "异步上传缓冲",
                Description = "异步上传缓冲区大小（MB）。",
                Min = 2,
                Max = 64,
                Step = 2,
                Unit = "MB",
                Risk = SettingRiskLevel.Advanced,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.shaderMaxLod",
                Type = SettingItemType.Int,
                Label = "Shader 最大 LOD",
                Description = "Shader.globalMaximumLOD。限制着色器复杂度档位。",
                Min = 0,
                Max = 1000,
                Step = 50,
                Risk = SettingRiskLevel.Advanced,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.logFilter",
                Type = SettingItemType.Enum,
                Label = "日志级别",
                Description = "过滤 Unity 日志输出级别。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption(((int)LogType.Error).ToString(), "仅错误"),
                    new SettingEnumOption(((int)LogType.Assert).ToString(), "断言及以上"),
                    new SettingEnumOption(((int)LogType.Warning).ToString(), "警告及以上"),
                    new SettingEnumOption(((int)LogType.Log).ToString(), "全部")
                },
                Risk = SettingRiskLevel.Advanced,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "advanced.restoreSystemDefaults",
                Type = SettingItemType.Button,
                Label = "恢复系统默认设置",
                Description =
                    "将所有系统设置还原为本次启动时记录的引擎原始值，并清除已保存的系统设置。不会修改操作系统设置。",
                ButtonAction = "restoreSystemDefaults",
                Risk = SettingRiskLevel.Danger,
                ConfirmMessage = "将清除已保存的系统设置并还原为启动时的引擎默认值。确认继续？"
            };

            yield return new SettingItemDef
            {
                Key = "advanced.restartApp",
                Type = SettingItemType.Button,
                Label = "立即重启程序",
                Description = "重新启动当前程序（独立包）。编辑器中会退出 Play 模式。",
                ButtonAction = "restartApplication",
                Risk = SettingRiskLevel.Advanced
            };
        }

        public bool IsSupported(string key)
        {
            if (key == "advanced.gcMode")
            {
#if UNITY_2019_1_OR_NEWER
                return true;
#else
                return false;
#endif
            }

            return key != null && key.StartsWith("advanced.");
        }

        public SettingValue ReadCurrent(string key)
        {
            switch (key)
            {
                case "advanced.fixedDeltaTime":
                    return SettingValue.FromFloat(Time.fixedDeltaTime);
                case "advanced.maximumDeltaTime":
                    return SettingValue.FromFloat(Time.maximumDeltaTime);
                case "advanced.solverIterations":
                    return SettingValue.FromInt(Physics.defaultSolverIterations);
#if UNITY_2019_1_OR_NEWER
                case "advanced.gcMode":
                    return SettingValue.FromInt(GarbageCollector.GCMode == GarbageCollector.Mode.Enabled ? 1 : 0);
#endif
                case "advanced.asyncUploadTimeSlice":
                    return SettingValue.FromInt(QualitySettings.asyncUploadTimeSlice);
                case "advanced.asyncUploadBufferSize":
                    return SettingValue.FromInt(QualitySettings.asyncUploadBufferSize);
                case "advanced.shaderMaxLod":
                    return SettingValue.FromInt(Shader.globalMaximumLOD);
                case "advanced.logFilter":
                    return SettingValue.FromInt((int)Debug.unityLogger.filterLogType);
                default:
                    return new SettingValue(string.Empty);
            }
        }

        public void Apply(string key, SettingValue value)
        {
            switch (key)
            {
                case "advanced.fixedDeltaTime":
                    Time.fixedDeltaTime = Mathf.Clamp(value.AsFloat(), 0.001f, 0.1f);
                    break;
                case "advanced.maximumDeltaTime":
                    Time.maximumDeltaTime = Mathf.Clamp(value.AsFloat(), 0.05f, 1f);
                    break;
                case "advanced.solverIterations":
                    Physics.defaultSolverIterations = Mathf.Clamp(value.AsInt(), 1, 50);
                    break;
#if UNITY_2019_1_OR_NEWER
                case "advanced.gcMode":
                    GarbageCollector.GCMode = value.AsInt() == 0
                        ? GarbageCollector.Mode.Disabled
                        : GarbageCollector.Mode.Enabled;
                    break;
#endif
                case "advanced.asyncUploadTimeSlice":
                    QualitySettings.asyncUploadTimeSlice = value.AsInt();
                    break;
                case "advanced.asyncUploadBufferSize":
                    QualitySettings.asyncUploadBufferSize = value.AsInt();
                    break;
                case "advanced.shaderMaxLod":
                    Shader.globalMaximumLOD = value.AsInt();
                    break;
                case "advanced.logFilter":
                    Debug.unityLogger.filterLogType = (LogType)value.AsInt();
                    break;
            }
        }
    }
}
