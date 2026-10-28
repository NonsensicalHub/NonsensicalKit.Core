using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace NonsensicalKit.Setting
{
    public class QualitySettingProvider : ISystemSettingProvider
    {
        public string CategoryId { get { return "quality"; } }
        public string CategoryLabel { get { return "画质"; } }
        public int Order { get { return 20; } }
        public SettingTabKind TabKind { get { return SettingTabKind.System; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
            List<SettingEnumOption> levels = new List<SettingEnumOption>();
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                levels.Add(new SettingEnumOption(i.ToString(), names[i]));
            }

            yield return new SettingItemDef
            {
                Key = "quality.level",
                Type = SettingItemType.Enum,
                Label = "画质等级",
                Description = "切换 Unity Quality Settings 预设。切换后会重置部分子项，随后再应用你单独调整过的阴影/抗锯齿等。",
                Options = levels,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.antiAliasing",
                Type = SettingItemType.Enum,
                Label = "抗锯齿 (MSAA)",
                Description = "多重采样抗锯齿。内置管线写入 QualitySettings；URP 会尝试写入当前管线资源（编辑器退出 Play 时会还原）。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "关闭"),
                    new SettingEnumOption("2", "2x"),
                    new SettingEnumOption("4", "4x"),
                    new SettingEnumOption("8", "8x")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.shadows",
                Type = SettingItemType.Enum,
                Label = "阴影质量",
                Description = "关闭 / 硬阴影 / 软阴影。HDRP 项目中此项可能无效。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "关闭"),
                    new SettingEnumOption("1", "硬阴影"),
                    new SettingEnumOption("2", "软阴影")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.shadowResolution",
                Type = SettingItemType.Enum,
                Label = "阴影分辨率",
                Description = "阴影贴图分辨率，越高越清晰但更耗显存。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "低"),
                    new SettingEnumOption("1", "中"),
                    new SettingEnumOption("2", "高"),
                    new SettingEnumOption("3", "极高")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.shadowDistance",
                Type = SettingItemType.Float,
                Label = "阴影距离",
                Description = "产生阴影的最大摄像机距离。",
                Min = 0f,
                Max = 500f,
                Step = 1f,
                Unit = "m",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.textureLimit",
                Type = SettingItemType.Enum,
                Label = "纹理质量",
                Description = "全局纹理 mipmap 限制。0=全分辨率，数值越大纹理越模糊、越省显存。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "全速"),
                    new SettingEnumOption("1", "1/2"),
                    new SettingEnumOption("2", "1/4"),
                    new SettingEnumOption("3", "1/8")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.anisotropicTextures",
                Type = SettingItemType.Enum,
                Label = "各向异性过滤",
                Description = "改善斜视角度下的纹理清晰度。",
                Options = new List<SettingEnumOption>
                {
                    new SettingEnumOption("0", "禁用"),
                    new SettingEnumOption("1", "按纹理"),
                    new SettingEnumOption("2", "强制开启")
                },
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.lodBias",
                Type = SettingItemType.Float,
                Label = "LOD 偏移",
                Description = "大于 1 更倾向使用高精度模型，小于 1 更早切换到低模。",
                Min = 0.1f,
                Max = 2f,
                Step = 0.05f,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "quality.pixelLightCount",
                Type = SettingItemType.Int,
                Label = "像素光数量",
                Description = "前向渲染中像素光照的最大数量（内置管线）。",
                Min = 0,
                Max = 8,
                Step = 1,
                ApplyMode = SettingApplyMode.Immediate
            };

            if (IsUrpAvailable())
            {
                yield return new SettingItemDef
                {
                    Key = "quality.renderScale",
                    Type = SettingItemType.Float,
                    Label = "渲染缩放 (URP)",
                    Description = "URP 内部渲染分辨率缩放。低于 1 可提升帧率但画面更糊。修改会写入 URP 资源，编辑器退出 Play 时会还原。",
                    Min = 0.5f,
                    Max = 2f,
                    Step = 0.05f,
                    ApplyMode = SettingApplyMode.Immediate,
                    Risk = SettingRiskLevel.Advanced
                };
            }
        }

        public bool IsSupported(string key)
        {
            if (key == "quality.renderScale")
            {
                return IsUrpAvailable();
            }

            return key != null && key.StartsWith("quality.");
        }

        public SettingValue ReadCurrent(string key)
        {
            switch (key)
            {
                case "quality.level":
                    return SettingValue.FromInt(QualitySettings.GetQualityLevel());
                case "quality.antiAliasing":
                    return SettingValue.FromInt(QualitySettings.antiAliasing);
                case "quality.shadows":
                    return SettingValue.FromInt((int)QualitySettings.shadows);
                case "quality.shadowResolution":
                    return SettingValue.FromInt((int)QualitySettings.shadowResolution);
                case "quality.shadowDistance":
                    return SettingValue.FromFloat(QualitySettings.shadowDistance);
                case "quality.textureLimit":
                    return SettingValue.FromInt(GetTextureLimit());
                case "quality.anisotropicTextures":
                    return SettingValue.FromInt((int)QualitySettings.anisotropicFiltering);
                case "quality.lodBias":
                    return SettingValue.FromFloat(QualitySettings.lodBias);
                case "quality.pixelLightCount":
                    return SettingValue.FromInt(QualitySettings.pixelLightCount);
                case "quality.renderScale":
                    return SettingValue.FromFloat(GetUrpRenderScale());
                default:
                    return new SettingValue(string.Empty);
            }
        }

        public void Apply(string key, SettingValue value)
        {
            switch (key)
            {
                case "quality.level":
                    QualitySettings.SetQualityLevel(value.AsInt(), true);
                    break;
                case "quality.antiAliasing":
                    QualitySettings.antiAliasing = value.AsInt();
                    TrySetUrpMsaa(value.AsInt());
                    break;
                case "quality.shadows":
                    QualitySettings.shadows = (ShadowQuality)value.AsInt();
                    break;
                case "quality.shadowResolution":
                    QualitySettings.shadowResolution = (ShadowResolution)value.AsInt();
                    break;
                case "quality.shadowDistance":
                    QualitySettings.shadowDistance = value.AsFloat();
                    TrySetUrpShadowDistance(value.AsFloat());
                    break;
                case "quality.textureLimit":
                    SetTextureLimit(value.AsInt());
                    break;
                case "quality.anisotropicTextures":
                    QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value.AsInt();
                    break;
                case "quality.lodBias":
                    QualitySettings.lodBias = value.AsFloat();
                    break;
                case "quality.pixelLightCount":
                    QualitySettings.pixelLightCount = value.AsInt();
                    break;
                case "quality.renderScale":
                    SetUrpRenderScale(value.AsFloat());
                    break;
            }
        }

        private static int GetTextureLimit()
        {
#if UNITY_2022_2_OR_NEWER
            return QualitySettings.globalTextureMipmapLimit;
#else
            return QualitySettings.masterTextureLimit;
#endif
        }

        private static void SetTextureLimit(int v)
        {
#if UNITY_2022_2_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = v;
#else
            QualitySettings.masterTextureLimit = v;
#endif
        }

        private static bool IsUrpAvailable()
        {
            RenderPipelineAsset asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return false;
            }

            string n = asset.GetType().Name;
            return n.IndexOf("Universal") >= 0 || n.IndexOf("URP") >= 0;
        }

        private static float GetUrpRenderScale()
        {
            object asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return 1f;
            }

            PropertyInfo prop = asset.GetType().GetProperty("renderScale",
                BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.PropertyType == typeof(float))
            {
                return (float)prop.GetValue(asset, null);
            }

            return 1f;
        }

        private static void SetUrpRenderScale(float scale)
        {
            object asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return;
            }

            PropertyInfo prop = asset.GetType().GetProperty("renderScale",
                BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(asset, scale, null);
            }
        }

        private static void TrySetUrpMsaa(int samples)
        {
            object asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return;
            }

            PropertyInfo prop = asset.GetType().GetProperty("msaaSampleCount",
                BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(asset, samples, null);
            }
        }

        private static void TrySetUrpShadowDistance(float distance)
        {
            object asset = GraphicsSettings.renderPipelineAsset;
            if (asset == null)
            {
                return;
            }

            PropertyInfo prop = asset.GetType().GetProperty("shadowDistance",
                BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(asset, distance, null);
            }
        }
    }
}
