using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 工业软件风格主题：全部自绘纹理，不依赖默认 GUISkin 外观。
    /// 说明：为兼容 Unity 2018.4，渲染层使用 IMGUI 而非 UI Toolkit。
    /// </summary>
    public class ImguiTheme
    {
        public const int FontLabel = 16;
        public const int FontDesc = 13;
        public const int FontHeader = 18;
        public const int FontTitle = 20;
        public const int FontButton = 15;
        public const int FontBadge = 12;
        public const int FontTab = 15;
        public const int FontCategory = 16;

        public GUIStyle Window;
        public GUIStyle Title;
        public GUIStyle Label;
        public GUIStyle Desc;
        public GUIStyle SectionHeader;
        public GUIStyle CategoryNormal;
        public GUIStyle CategoryActive;
        public GUIStyle NavHeader;
        public GUIStyle TabNormal;
        public GUIStyle TabActive;
        public GUIStyle GroupBox;
        public GUIStyle PopupBox;
        public GUIStyle BadgeImmediate;
        public GUIStyle BadgeRestart;
        public GUIStyle BadgeDanger;
        public GUIStyle DirtyBar;
        public GUIStyle Button;
        public GUIStyle ButtonPrimary;
        public GUIStyle ToggleOn;
        public GUIStyle ToggleOff;
        public GUIStyle TextField;
        public GUIStyle Slider;
        public GUIStyle SliderThumb;
        public GUIStyle StatusBar;
        public GUIStyle SearchField;
        public GUIStyle Banner;
        public GUIStyle ValueLabel;

        public Color Accent = new Color(0.22f, 0.58f, 0.90f, 1f);
        public Color AccentDim = new Color(0.16f, 0.38f, 0.60f, 1f);
        public Color Bg = new Color(0.14f, 0.15f, 0.17f, 0.98f);
        public Color Panel = new Color(0.19f, 0.20f, 0.23f, 1f);
        public Color PanelAlt = new Color(0.22f, 0.23f, 0.26f, 1f);
        public Color Border = new Color(0.35f, 0.37f, 0.40f, 1f);
        public Color Text = new Color(0.93f, 0.94f, 0.95f, 1f);
        public Color TextDim = new Color(0.70f, 0.72f, 0.75f, 1f);
        public Color Danger = new Color(0.86f, 0.32f, 0.28f, 1f);
        public Color Warning = new Color(0.92f, 0.68f, 0.22f, 1f);
        public Color Ok = new Color(0.28f, 0.72f, 0.48f, 1f);
        public Color Track = new Color(0.30f, 0.32f, 0.35f, 1f);
        public Color Thumb = new Color(0.85f, 0.87f, 0.90f, 1f);

        private Texture2D _texBg;
        private Texture2D _texPanel;
        private Texture2D _texPanelAlt;
        private Texture2D _texAccent;
        private Texture2D _texAccentDim;
        private Texture2D _texDanger;
        private Texture2D _texWarning;
        private Texture2D _texOk;
        private Texture2D _texButton;
        private Texture2D _texButtonHover;
        private Texture2D _texTrack;
        private Texture2D _texThumb;
        private Texture2D _texToggleOn;
        private Texture2D _texToggleOff;
        private bool _built;

        public void Ensure()
        {
            if (_built && Window != null)
            {
                return;
            }

            _texBg = MakeTex(Bg);
            _texPanel = MakeTex(Panel);
            _texPanelAlt = MakeTex(PanelAlt);
            _texAccent = MakeTex(Accent);
            _texAccentDim = MakeTex(AccentDim);
            _texDanger = MakeTex(Danger);
            _texWarning = MakeTex(Warning);
            _texOk = MakeTex(Ok);
            _texButton = MakeTex(new Color(0.30f, 0.32f, 0.35f, 1f));
            _texButtonHover = MakeTex(new Color(0.38f, 0.40f, 0.44f, 1f));
            _texTrack = MakeTex(Track);
            _texThumb = MakeTex(Thumb);
            _texToggleOn = MakeTex(Ok);
            _texToggleOff = MakeTex(new Color(0.40f, 0.42f, 0.45f, 1f));

            Window = new GUIStyle(GUI.skin.window);
            ApplyBg(Window, _texBg);
            // 标题「设置」：覆盖皮肤各态，避免深色底上仍是默认黑字
            Window.normal.textColor = Text;
            Window.onNormal.textColor = Text;
            Window.hover.textColor = Text;
            Window.onHover.textColor = Text;
            Window.active.textColor = Text;
            Window.onActive.textColor = Text;
            Window.focused.textColor = Text;
            Window.onFocused.textColor = Text;
            Window.fontSize = FontTitle;
            Window.fontStyle = FontStyle.Bold;
            Window.alignment = TextAnchor.UpperCenter;
            Window.padding = new RectOffset(12, 12, 28, 10);
            Window.border = new RectOffset(4, 4, 4, 4);

            Title = TextStyle(FontTitle, Text, true);
            Label = TextStyle(FontLabel, Text, false);
            Label.alignment = TextAnchor.MiddleLeft;
            Desc = TextStyle(FontDesc, TextDim, false);
            Desc.wordWrap = true;
            SectionHeader = TextStyle(FontHeader, Accent, true);
            ValueLabel = TextStyle(FontLabel, Text, false);
            ValueLabel.alignment = TextAnchor.MiddleRight;

            NavHeader = TextStyle(FontDesc, TextDim, true);
            NavHeader.alignment = TextAnchor.MiddleLeft;
            NavHeader.padding = new RectOffset(8, 4, 8, 4);

            CategoryNormal = MakeButtonStyle(_texPanelAlt, TextDim, FontCategory);
            CategoryNormal.fontStyle = FontStyle.Bold;
            CategoryNormal.fixedHeight = 36f;
            CategoryNormal.alignment = TextAnchor.MiddleCenter;
            CategoryNormal.margin = new RectOffset(2, 2, 0, 0);

            CategoryActive = MakeButtonStyle(_texAccent, Color.white, FontCategory);
            CategoryActive.fontStyle = FontStyle.Bold;
            CategoryActive.fixedHeight = 36f;
            CategoryActive.alignment = TextAnchor.MiddleCenter;
            CategoryActive.margin = new RectOffset(2, 2, 0, 0);

            TabNormal = MakeButtonStyle(_texPanel, TextDim, FontTab);
            TabNormal.alignment = TextAnchor.MiddleLeft;
            TabNormal.fixedHeight = 34f;
            TabNormal.padding = new RectOffset(14, 10, 8, 8);
            TabNormal.margin = new RectOffset(2, 2, 2, 2);

            TabActive = MakeButtonStyle(_texAccent, Color.white, FontTab);
            TabActive.fontStyle = FontStyle.Bold;
            TabActive.alignment = TextAnchor.MiddleLeft;
            TabActive.fixedHeight = 34f;
            TabActive.padding = new RectOffset(14, 10, 8, 8);
            TabActive.margin = new RectOffset(2, 2, 2, 2);

            GroupBox = new GUIStyle(GUI.skin.box);
            ApplyBg(GroupBox, _texPanel);
            GroupBox.border = new RectOffset(2, 2, 2, 2);
            GroupBox.margin = new RectOffset(0, 0, 8, 12);
            GroupBox.padding = new RectOffset(14, 14, 12, 14);

            PopupBox = new GUIStyle(GroupBox);
            PopupBox.margin = new RectOffset(4, 4, 2, 4);
            PopupBox.padding = new RectOffset(1, 1, 2, 2);

            BadgeImmediate = BadgeStyle(_texOk);
            BadgeRestart = BadgeStyle(_texWarning);
            BadgeDanger = BadgeStyle(_texDanger);

            DirtyBar = new GUIStyle();
            DirtyBar.normal.background = _texAccent;
            DirtyBar.fixedWidth = 4f;
            DirtyBar.fixedHeight = 22f;

            Button = MakeButtonStyle(_texButton, Text, FontButton);
            Button.fixedHeight = 32f;
            Button.margin = new RectOffset(4, 4, 0, 0);
            Button.padding = new RectOffset(14, 14, 6, 6);
            Button.hover.background = _texButtonHover;
            Button.active.background = _texAccentDim;

            ButtonPrimary = MakeButtonStyle(_texAccent, Color.white, FontButton);
            ButtonPrimary.fontStyle = FontStyle.Bold;
            ButtonPrimary.fixedHeight = 32f;
            ButtonPrimary.padding = new RectOffset(14, 14, 6, 6);
            ButtonPrimary.hover.background = _texAccentDim;

            ToggleOn = MakeButtonStyle(_texToggleOn, Color.white, FontButton);
            ToggleOn.fontStyle = FontStyle.Bold;
            ToggleOn.fixedHeight = 32f;
            ToggleOn.margin = new RectOffset(4, 4, 0, 0);
            ToggleOn.fixedWidth = 88f;
            ToggleOn.alignment = TextAnchor.MiddleCenter;

            ToggleOff = MakeButtonStyle(_texToggleOff, Text, FontButton);
            ToggleOff.fixedHeight = 32f;
            ToggleOff.margin = new RectOffset(4, 4, 0, 0);
            ToggleOff.fixedWidth = 88f;
            ToggleOff.alignment = TextAnchor.MiddleCenter;

            TextField = new GUIStyle(GUI.skin.textField);
            ApplyBg(TextField, _texButton);
            TextField.normal.textColor = Text;
            TextField.focused.textColor = Text;
            TextField.fontSize = FontLabel;
            TextField.padding = new RectOffset(10, 10, 8, 8);
            TextField.fixedHeight = 32f;
            TextField.border = new RectOffset(2, 2, 2, 2);

            SearchField = new GUIStyle(TextField);
            SearchField.fixedHeight = 30f;

            // 与 UITK 一致：细轨道 + 加高拖柄，垂直居中「骑」在轨道上
            Slider = new GUIStyle();
            Slider.normal.background = _texTrack;
            Slider.border = new RectOffset(0, 0, 0, 0);
            Slider.margin = new RectOffset(0, 0, 0, 0);
            Slider.padding = new RectOffset(0, 0, 0, 0);
            Slider.overflow = new RectOffset(0, 0, 0, 0);
            Slider.stretchWidth = true;
            Slider.stretchHeight = true;

            SliderThumb = new GUIStyle();
            SliderThumb.normal.background = _texThumb;
            SliderThumb.border = new RectOffset(0, 0, 0, 0);
            SliderThumb.margin = new RectOffset(0, 0, 0, 0);
            SliderThumb.padding = new RectOffset(0, 0, 0, 0);
            SliderThumb.overflow = new RectOffset(0, 0, 0, 0);
            SliderThumb.stretchWidth = true;
            SliderThumb.stretchHeight = true;

            StatusBar = new GUIStyle(GUI.skin.box);
            ApplyBg(StatusBar, _texPanelAlt);
            StatusBar.normal.textColor = TextDim;
            StatusBar.fontSize = FontDesc;
            StatusBar.alignment = TextAnchor.MiddleLeft;
            StatusBar.padding = new RectOffset(12, 12, 8, 8);
            StatusBar.fixedHeight = 48f;

            Banner = new GUIStyle(GUI.skin.box);
            ApplyBg(Banner, _texWarning);
            Banner.normal.textColor = new Color(0.15f, 0.12f, 0.05f, 1f);
            Banner.fontSize = FontDesc;
            Banner.alignment = TextAnchor.MiddleLeft;
            Banner.padding = new RectOffset(12, 12, 10, 10);
            Banner.wordWrap = true;
            ApplyCjkFont();

            _built = true;
        }

        void ApplyCjkFont()
        {
            Font font = SettingUiFont.Font;
            if (font == null)
            {
                return;
            }

            Window.font = font;
            Title.font = font;
            Label.font = font;
            Desc.font = font;
            SectionHeader.font = font;
            CategoryNormal.font = font;
            CategoryActive.font = font;
            NavHeader.font = font;
            TabNormal.font = font;
            TabActive.font = font;
            BadgeImmediate.font = font;
            BadgeRestart.font = font;
            BadgeDanger.font = font;
            Button.font = font;
            ButtonPrimary.font = font;
            ToggleOn.font = font;
            ToggleOff.font = font;
            TextField.font = font;
            StatusBar.font = font;
            SearchField.font = font;
            Banner.font = font;
            ValueLabel.font = font;
        }

        /// <summary>强制重建主题（分辨率/DPI 变化时可调用）。</summary>
        public void Rebuild()
        {
            _built = false;
            Ensure();
        }

        private GUIStyle TextStyle(int size, Color color, bool bold)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.normal.textColor = color;
            s.fontSize = size;
            s.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            s.richText = false;
            s.wordWrap = false;
            return s;
        }

        private GUIStyle MakeButtonStyle(Texture2D bg, Color text, int fontSize)
        {
            GUIStyle s = new GUIStyle(GUI.skin.button);
            ApplyBg(s, bg);
            s.normal.textColor = text;
            s.hover.textColor = Color.white;
            s.active.textColor = Color.white;
            s.focused.textColor = text;
            s.onNormal.textColor = text;
            s.fontSize = fontSize;
            s.alignment = TextAnchor.MiddleCenter;
            s.border = new RectOffset(4, 4, 4, 4);
            return s;
        }

        private GUIStyle BadgeStyle(Texture2D bg)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box);
            ApplyBg(s, bg);
            s.normal.textColor = Color.white;
            s.fontSize = FontBadge;
            s.fontStyle = FontStyle.Bold;
            s.alignment = TextAnchor.MiddleCenter;
            s.padding = new RectOffset(8, 8, 4, 4);
            s.margin = new RectOffset(6, 0, 0, 0);
            s.fixedHeight = 32f;
            return s;
        }

        private static void ApplyBg(GUIStyle style, Texture2D tex)
        {
            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;
            style.onNormal.background = tex;
            style.onHover.background = tex;
            style.onActive.background = tex;
            style.onFocused.background = tex;
        }

        private static Texture2D MakeTex(Color c)
        {
            Texture2D t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                pixels[i] = c;
            }

            t.SetPixels(pixels);
            t.Apply(false, true);
            t.hideFlags = HideFlags.HideAndDontSave;
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            return t;
        }
    }
}
