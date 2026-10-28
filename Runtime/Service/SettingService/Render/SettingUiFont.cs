using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
#endif

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 打包后的默认字体没有汉字。面板使用 Resources 里的中文字体，缺字再找项目里的思源黑体。
    /// </summary>
    static class SettingUiFont
    {
        const string ResourcePath = "UIToolkit/SettingPanelFont";
        const string SourceHanResource = "Fonts & Materials/SourceHanSansSC-Normal-2-Dynamic";

        static Font _font;
        static bool _tried;

        public static Font Font
        {
            get
            {
                if (!_tried)
                {
                    _tried = true;
                    _font = Resources.Load<Font>(ResourcePath);
                }

                return _font;
            }
        }

#if UNITY_2021_2_OR_NEWER
        static PanelTextSettings _textSettings;

        public static void Apply(PanelSettings panel)
        {
            if (panel == null)
            {
                return;
            }

            PanelTextSettings settings = TextSettings();
            if (settings != null)
            {
                panel.textSettings = settings;
            }
        }

        static PanelTextSettings TextSettings()
        {
            if (_textSettings != null)
            {
                return _textSettings;
            }

            FontAsset main = CreateDynamic(Font);
            if (main == null)
            {
                return null;
            }

            FontAsset sourceHan = CreateDynamic(LoadSourceHan());
            if (sourceHan != null && main.fallbackFontAssetTable != null)
            {
                main.fallbackFontAssetTable.Add(sourceHan);
            }

            _textSettings = ScriptableObject.CreateInstance<PanelTextSettings>();
            _textSettings.hideFlags = HideFlags.HideAndDontSave;
            _textSettings.defaultFontAsset = main;
            List<FontAsset> fallbacks = new List<FontAsset>();
            fallbacks.Add(main);
            if (sourceHan != null)
            {
                fallbacks.Add(sourceHan);
            }

            _textSettings.fallbackFontAssets = fallbacks;
            return _textSettings;
        }

        static FontAsset CreateDynamic(Font font)
        {
            if (font == null)
            {
                return null;
            }

            FontAsset asset = FontAsset.CreateFontAsset(
                font,
                48,
                5,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);
            if (asset != null)
            {
                asset.hideFlags = HideFlags.HideAndDontSave;
            }

            return asset;
        }

        static Font LoadSourceHan()
        {
            Object tmp = Resources.Load(SourceHanResource);
            if (tmp == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = tmp.GetType().GetField("m_SourceFontFile", flags);
            if (field != null)
            {
                return field.GetValue(tmp) as Font;
            }

            PropertyInfo property = tmp.GetType().GetProperty("sourceFontFile", flags);
            if (property != null)
            {
                return property.GetValue(tmp, null) as Font;
            }

            return null;
        }
#endif
    }
}
