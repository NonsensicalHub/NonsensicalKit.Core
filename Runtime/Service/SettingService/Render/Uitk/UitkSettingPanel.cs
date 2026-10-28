#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// UI Toolkit 设置面板（Unity 2021.2+）。按 JSON Schema 动态生成控件树。
    /// </summary>
    public class UitkSettingPanel : MonoBehaviour, ISettingPanelView
    {
        public string RendererName
        {
            get { return "UIToolkit"; }
        }

        private SettingManager _manager;
        private UIDocument _document;
        private PanelSettings _panelSettings;
        private VisualElement _root;
        private VisualElement _categoryBar;
        private VisualElement _navList;
        private VisualElement _contentHost;
        private Label _breadcrumb;
        private Label _statusLabel;
        private Button _restartButton;
        private TextField _searchField;
        private Toggle _advancedToggle;
        private VisualElement _banner;
        private VisualElement _confirmOverlay;
        private Label _confirmMessage;
        private System.Action _confirmAction;

        private SettingTabKind _category = SettingTabKind.System;
        private string _selectedTabId;
        private string _search = string.Empty;
        private bool _visible;
        private bool _categoryInitialized;
        private bool _showRestartBanner;
        private bool _windowUserMoved;
        private int _dragPointerId = -1;
        private Vector2 _dragPointerStart;
        private Vector2 _dragWindowStart;
        private readonly List<List<SettingItemDef>> _visiblePool = new List<List<SettingItemDef>>();
        private int _visibleDepth;
        private readonly List<SettingTabDef> _tabBuffer = new List<SettingTabDef>();

        public void Bind(SettingManager manager)
        {
            _manager = manager;
            if (manager != null)
            {
                manager.OnRestartRequired += delegate
                {
                    _showRestartBanner = true;
                    if (_banner != null)
                    {
                        _banner.style.display = DisplayStyle.Flex;
                    }

                    RefreshStatus();
                };
            }

            EnsureDocument();
            BuildChrome();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (visible)
            {
                EnsureDocument();
                if (_root == null || _root.parent == null)
                {
                    BuildChrome();
                }
                else if (_document != null && _document.rootVisualElement != null &&
                         _root.parent != _document.rootVisualElement)
                {
                    _document.rootVisualElement.Clear();
                    _document.rootVisualElement.Add(_root);
                }
            }

            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (visible)
            {
                ForceFullScreenLayout();
            }

            if (visible && _manager != null)
            {
                _manager.ClearPending();
                EnsureDefaultSelection();
                RebuildAll();
            }
        }

        public void ToggleVisible()
        {
            SetVisible(!_visible);
        }

        private void Update()
        {
            if (_manager == null || _manager.PanelConfig == null)
            {
                return;
            }

            if (SettingInput.GetKeyDown(_manager.PanelConfig.ToggleKey))
            {
                ToggleVisible();
            }

            if (_visible && _document != null)
            {
                if (_document.panelSettings == null || _document.panelSettings.themeStyleSheet == null)
                {
                    ApplyPanelSettingsWithTheme(_document);
                }
            }
        }

        /// <summary>
        /// ConstantPixelSize 下按实际屏幕像素铺满；窗口用百分比自适应，避免小窗重叠。
        /// </summary>
        private void ForceFullScreenLayout()
        {
            if (_document == null || _root == null)
            {
                return;
            }

            VisualElement panelRoot = _document.rootVisualElement;
            if (panelRoot != null)
            {
                panelRoot.style.flexGrow = 1;
                panelRoot.style.width = Length.Percent(100);
                panelRoot.style.height = Length.Percent(100);
            }

            _root.style.position = Position.Relative;
            _root.style.flexGrow = 1;
            _root.style.width = Length.Percent(100);
            _root.style.height = Length.Percent(100);

            if (_root.childCount > 0)
            {
                VisualElement dim = _root[0];
                dim.style.flexGrow = 1;
                dim.style.width = Length.Percent(100);
                dim.style.height = Length.Percent(100);
                dim.style.justifyContent = Justify.Center;
                dim.style.alignItems = Align.Center;
                dim.style.overflow = Overflow.Hidden;

                VisualElement window = dim.Q("nk-setting-window");
                if (window != null)
                {
                    ApplyWindowStyle(window);
                }
            }

            _root.MarkDirtyRepaint();
        }

        private void OnDestroy()
        {
            if (_panelSettings != null)
            {
                Destroy(_panelSettings);
                _panelSettings = null;
            }
        }

        private void EnsureDocument()
        {
            _document = gameObject.GetComponent<UIDocument>();
            if (_document == null)
            {
                _document = gameObject.AddComponent<UIDocument>();
            }

            // 始终强制挂载带主题的 PanelSettings（无主题时 UITK 整页空白、无文字）
            ApplyPanelSettingsWithTheme(_document);
            _document.sortingOrder = 1000;
        }

        private void ApplyPanelSettingsWithTheme(UIDocument document)
        {
            // 必须使用资源里的 PanelSettings：含 RuntimeShader / Theme。
            // ScriptableObject.CreateInstance<PanelSettings>() 没有着色器，页面会全空白。
            PanelSettings fromResources = Resources.Load<PanelSettings>("UIToolkit/SettingPanelSettings");
#if UNITY_EDITOR
            if (fromResources == null)
            {
                fromResources = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(
                    "Packages/com.nonsensicallab.nonsensicalkit.core/Runtime/Service/SettingService/Resources/UIToolkit/SettingPanelSettings.asset");
            }
#endif
            ThemeStyleSheet theme = LoadThemeStyleSheet();
            if (fromResources != null)
            {
                // 复制一份再改。直接改 Resources 资源会把运行时字体写进资产。
                PanelSettings runtime = UnityEngine.Object.Instantiate(fromResources);
                runtime.hideFlags = HideFlags.DontSave;
                if (runtime.themeStyleSheet == null && theme != null)
                {
                    runtime.themeStyleSheet = theme;
                }

                // 设置面板用 ConstantPixelSize：小 Game 视图下 ScaleWithScreenSize
                // 会把字号缩到不可读，且控件高度相对文字过矮导致裁切。
                runtime.scaleMode = PanelScaleMode.ConstantPixelSize;
                runtime.scale = 1f;
                runtime.sortingOrder = 1000;
                SettingUiFont.Apply(runtime);
                if (_panelSettings != null)
                {
                    Destroy(_panelSettings);
                }

                _panelSettings = runtime;
                document.panelSettings = runtime;
                return;
            }

            Debug.LogError(
                "[Setting] 缺少 Resources/UIToolkit/SettingPanelSettings.asset，UITK 无法渲染。将导致空白页。");
            if (_panelSettings == null)
            {
                _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            }

            _panelSettings.themeStyleSheet = theme;
            _panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            _panelSettings.scale = 1f;
            _panelSettings.sortingOrder = 1000;
            SettingUiFont.Apply(_panelSettings);
            document.panelSettings = _panelSettings;
        }

        private static ThemeStyleSheet LoadThemeStyleSheet()
        {
            ThemeStyleSheet theme = Resources.Load<ThemeStyleSheet>("UIToolkit/UnityDefaultRuntimeTheme");
            if (theme != null)
            {
                return theme;
            }

            PanelSettings bundled = Resources.Load<PanelSettings>("UIToolkit/SettingPanelSettings");
            if (bundled != null && bundled.themeStyleSheet != null)
            {
                return bundled.themeStyleSheet;
            }

#if UNITY_EDITOR
            theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss");
            if (theme != null)
            {
                return theme;
            }

            theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.nonsensicallab.nonsensicalkit.core/Runtime/Service/SettingService/Resources/UIToolkit/UnityDefaultRuntimeTheme.tss");
#endif
            return theme;
        }

        private void OnEnable()
        {
            // UIDocument 启用时可能重建 visual tree，需重新挂载
            if (_manager != null && _document != null)
            {
                EnsureDocument();
                if (_root == null || _root.parent == null)
                {
                    BuildChrome();
                    if (_visible)
                    {
                        SetVisible(true);
                    }
                }
                else if (_document.rootVisualElement != null && _root.parent != _document.rootVisualElement)
                {
                    _document.rootVisualElement.Clear();
                    _document.rootVisualElement.Add(_root);
                }
            }
        }

        private void BuildChrome()
        {
            _root = new VisualElement();
            _root.name = "nk-setting-root";
            ApplyRootStyle(_root);
            ApplyPanelStyleSheet(_root);
            _root.style.display = DisplayStyle.None;

            // 遮罩 + 居中窗口
            VisualElement dim = new VisualElement();
            dim.style.flexGrow = 1;
            dim.style.width = Length.Percent(100);
            dim.style.height = Length.Percent(100);
            dim.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);
            dim.style.justifyContent = Justify.Center;
            dim.style.alignItems = Align.Center;

            VisualElement window = new VisualElement();
            window.name = "nk-setting-window";
            ApplyWindowStyle(window);

            // 标题栏：标题 + 高级开关 + 关闭（单行，省垂直空间）
            VisualElement titleBar = Row();
            titleBar.style.flexShrink = 0;
            titleBar.style.marginBottom = 8;
            titleBar.style.minHeight = 44;
            titleBar.style.alignItems = Align.Center;
            Label title = new Label("设置");
            title.style.fontSize = FontTitle;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = PrimaryText;
            title.style.flexGrow = 1;
            title.style.overflow = Overflow.Visible;
            title.style.unityTextAlign = TextAnchor.MiddleLeft;
            _advancedToggle = new Toggle("高级设置");
            _advancedToggle.name = "nk-setting-advanced-toggle";
            _advancedToggle.style.marginRight = 8;
            _advancedToggle.style.fontSize = FontBody;
            _advancedToggle.style.height = ControlHeight;
            _advancedToggle.style.flexShrink = 0;
            StyleToggleLight(_advancedToggle);
            StyleAdvancedToggle(_advancedToggle);
            _advancedToggle.RegisterValueChangedCallback(evt =>
            {
                if (_manager == null)
                {
                    return;
                }

                _manager.ShowAdvanced = evt.newValue;
                if (!_manager.ShowAdvanced && _category == SettingTabKind.Advanced)
                {
                    SelectCategory(SettingTabKind.System);
                }
                else
                {
                    RebuildCategoryBar();
                    RebuildNav();
                    RebuildContent();
                }
            });
            Button close = MakeButton("关闭", false);
            close.style.flexShrink = 0;
            close.clicked += delegate
            {
                if (_manager != null)
                {
                    _manager.CancelPending();
                }

                SetVisible(false);
            };
            titleBar.Add(title);
            titleBar.Add(_advancedToggle);
            titleBar.Add(close);
            BindWindowDrag(titleBar, window, dim);
            window.Add(titleBar);

            _banner = new VisualElement();
            ApplyBannerStyle(_banner);
            _banner.style.flexShrink = 0;
            Label bannerText = new Label("以下设置将在重启程序后生效。可点击底部「立即重启」。");
            bannerText.style.color = new Color(0.15f, 0.12f, 0.05f);
            bannerText.style.fontSize = FontDesc;
            bannerText.style.whiteSpace = WhiteSpace.Normal;
            _banner.Add(bannerText);
            _banner.style.display = DisplayStyle.None;
            window.Add(_banner);

            // 搜索行：占满宽，避免与分类页叠在一起
            VisualElement top = Row();
            top.style.flexShrink = 0;
            top.style.marginBottom = 8;
            top.style.minHeight = ControlHeight;
            top.style.alignItems = Align.Center;
            Label searchLabel = new Label("搜索");
            searchLabel.style.fontSize = FontBody;
            searchLabel.style.color = PrimaryText;
            searchLabel.style.width = 48;
            searchLabel.style.flexShrink = 0;
            searchLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            _searchField = new TextField();
            StyleInputControl(_searchField, 280);
            _searchField.style.flexGrow = 1;
            _searchField.style.width = StyleKeyword.Auto;
            _searchField.style.maxWidth = StyleKeyword.None;
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _search = evt.newValue ?? string.Empty;
                RebuildContent();
            });
            top.Add(searchLabel);
            top.Add(_searchField);
            window.Add(top);

            _categoryBar = Row();
            _categoryBar.style.flexShrink = 0;
            _categoryBar.style.flexWrap = Wrap.Wrap;
            _categoryBar.style.marginBottom = 12;
            _categoryBar.style.minHeight = 44;
            window.Add(_categoryBar);

            // 左右栏顶部对齐；body 占剩余空间并可滚动
            VisualElement body = new VisualElement();
            body.style.flexDirection = FlexDirection.Row;
            body.style.alignItems = Align.Stretch;
            body.style.flexGrow = 1;
            body.style.flexShrink = 1;
            // UITK 默认 min-height:auto 会按内容撑开；置 0 + height:0 才能被父级压扁，底栏得以保留
            body.style.minHeight = 0;
            body.style.height = 0;

            VisualElement navPanel = new VisualElement();
            ApplyPanelStyle(navPanel);
            navPanel.style.width = 180;
            navPanel.style.minWidth = 120;
            navPanel.style.flexShrink = 1;
            navPanel.style.minHeight = 0;
            navPanel.style.marginRight = 10;
            ScrollView navScroll = new ScrollView(ScrollViewMode.Vertical);
            navScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            navScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _navList = navScroll;
            _navList.style.flexGrow = 1;
            _navList.style.minHeight = 0;
            navPanel.Add(_navList);
            body.Add(navPanel);

            VisualElement contentPanel = new VisualElement();
            ApplyPanelStyle(contentPanel);
            // 右侧不留 padding，避免竖向滚动条与面板右缘之间出现空隙/偏移
            contentPanel.style.paddingRight = 0;
            contentPanel.style.flexGrow = 1;
            contentPanel.style.flexShrink = 1;
            contentPanel.style.minWidth = 200;
            contentPanel.style.minHeight = 0;
            contentPanel.style.overflow = Overflow.Visible;
            _breadcrumb = new Label();
            _breadcrumb.style.fontSize = FontHeader;
            _breadcrumb.style.unityFontStyleAndWeight = FontStyle.Bold;
            _breadcrumb.style.color = AccentText;
            _breadcrumb.style.marginBottom = 8;
            _breadcrumb.style.marginRight = 12;
            _breadcrumb.style.flexShrink = 0;
            contentPanel.Add(_breadcrumb);
            ScrollView contentScroll = new ScrollView(ScrollViewMode.Vertical);
            contentScroll.name = "nk-setting-content-scroll";
            contentScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            // AlwaysVisible：避免 Auto + 自定义样式后滚动条被藏掉
            contentScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            StyleContentScroll(contentScroll);
            _contentHost = contentScroll;
            _contentHost.style.flexGrow = 1;
            _contentHost.style.minHeight = 0;
            _contentHost.style.marginRight = 0;
            contentPanel.Add(_contentHost);
            body.Add(contentPanel);
            window.Add(body);

            // 底栏（永不收缩，避免小窗时被裁掉）
            VisualElement footer = Row();
            ApplyPanelStyle(footer);
            footer.style.flexShrink = 0;
            footer.style.flexGrow = 0;
            footer.style.flexWrap = Wrap.Wrap;
            footer.style.marginTop = 8;
            footer.style.minHeight = 44;
            footer.style.alignItems = Align.Center;
            _statusLabel = new Label("无未保存的修改");
            _statusLabel.style.fontSize = FontBody;
            _statusLabel.style.color = PrimaryText;
            _statusLabel.style.flexGrow = 1;
            _statusLabel.style.minWidth = 160;
            footer.Add(_statusLabel);
            footer.Add(MakeFooterButton("恢复默认", false, delegate { _manager.ResetToDefaults(); RebuildContent(); RefreshStatus(); }));
            _restartButton = MakeFooterButton("立即重启", true, delegate
            {
                _manager.HandleButton("restartApplication");
            });
            _restartButton.style.display = DisplayStyle.None;
            footer.Add(_restartButton);
            footer.Add(MakeFooterButton("应用", false, delegate { RequestApply(false); }));
            footer.Add(MakeFooterButton("确定", true, delegate { RequestApply(true); }));
            footer.Add(MakeFooterButton("取消", false, delegate
            {
                _manager.CancelPending();
                SetVisible(false);
            }));
            window.Add(footer);

            // 确认层
            _confirmOverlay = new VisualElement();
            _confirmOverlay.style.position = Position.Absolute;
            _confirmOverlay.style.left = 0;
            _confirmOverlay.style.right = 0;
            _confirmOverlay.style.top = 0;
            _confirmOverlay.style.bottom = 0;
            _confirmOverlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            _confirmOverlay.style.justifyContent = Justify.Center;
            _confirmOverlay.style.alignItems = Align.Center;
            _confirmOverlay.style.display = DisplayStyle.None;
            VisualElement confirmBox = new VisualElement();
            ApplyWindowStyle(confirmBox);
            confirmBox.style.width = 520;
            confirmBox.style.height = 240;
            Label confirmTitle = new Label("确认");
            confirmTitle.style.fontSize = 18;
            confirmTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            confirmTitle.style.color = Color.white;
            confirmTitle.style.marginBottom = 10;
            _confirmMessage = new Label();
            _confirmMessage.style.fontSize = 14;
            _confirmMessage.style.color = new Color(0.85f, 0.86f, 0.88f);
            _confirmMessage.style.whiteSpace = WhiteSpace.Normal;
            _confirmMessage.style.flexGrow = 1;
            VisualElement confirmBtns = Row();
            confirmBtns.style.justifyContent = Justify.FlexEnd;
            Button cancelBtn = MakeButton("取消", false);
            cancelBtn.clicked += delegate
            {
                _confirmOverlay.style.display = DisplayStyle.None;
                _confirmAction = null;
            };
            Button okBtn = MakeButton("确认", true);
            okBtn.clicked += delegate
            {
                System.Action a = _confirmAction;
                _confirmOverlay.style.display = DisplayStyle.None;
                _confirmAction = null;
                if (a != null)
                {
                    a();
                }
            };
            confirmBtns.Add(cancelBtn);
            confirmBtns.Add(okBtn);
            confirmBox.Add(confirmTitle);
            confirmBox.Add(_confirmMessage);
            confirmBox.Add(confirmBtns);
            _confirmOverlay.Add(confirmBox);
            window.Add(_confirmOverlay);

            dim.Add(window);
            _root.Add(dim);

            _document.rootVisualElement.Clear();
            _document.rootVisualElement.Add(_root);
        }

        private void RebuildAll()
        {
            if (_manager == null)
            {
                return;
            }

            EnsureDefaultSelection();
            if (_advancedToggle != null)
            {
                _advancedToggle.SetValueWithoutNotify(_manager.ShowAdvanced);
            }

            if (_banner != null)
            {
                _banner.style.display = (_showRestartBanner || _manager.HasPendingRestart)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            RebuildCategoryBar();
            RebuildNav();
            RebuildContent();
            RefreshStatus();
        }

        private void RebuildCategoryBar()
        {
            _categoryBar.Clear();
            AddCategoryButton("系统设置", SettingTabKind.System);
            AddCategoryButton("项目设置", SettingTabKind.Project);
            if (_manager != null && _manager.ShowAdvanced)
            {
                AddCategoryButton("高级", SettingTabKind.Advanced);
            }

            AddCategoryButton("诊断信息", SettingTabKind.Diagnostics);
        }

        private void AddCategoryButton(string label, SettingTabKind kind)
        {
            bool has = CategoryHasTabs(kind);
            bool active = _category == kind;
            Button btn = new Button();
            btn.text = label;
            btn.style.height = ControlHeight + 4;
            btn.style.width = 140;
            btn.style.marginRight = 6;
            btn.style.marginBottom = 4;
            btn.style.fontSize = FontBody;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.backgroundColor = active
                ? new Color(0.22f, 0.58f, 0.90f)
                : new Color(0.22f, 0.23f, 0.26f);
            btn.style.color = active ? Color.white : SecondaryText;
            btn.SetEnabled(has);
            SettingTabKind captured = kind;
            btn.clicked += delegate { SelectCategory(captured); };
            _categoryBar.Add(btn);
        }

        private void RebuildNav()
        {
            _navList.Clear();
            Label header = new Label(SettingPanelQuery.CategoryTitle(_category));
            header.style.fontSize = FontDesc;
            header.style.color = SecondaryText;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            _navList.Add(header);

            List<SettingTabDef> tabs = GetTabsInCategory(_category);
            if (tabs.Count == 0)
            {
                Label empty = new Label("此分类暂无页面");
                empty.style.color = SecondaryText;
                empty.style.fontSize = FontBody;
                _navList.Add(empty);
                return;
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                SettingTabDef tab = tabs[i];
                bool active = tab.Id == _selectedTabId;
                Button btn = new Button();
                btn.text = (_category == SettingTabKind.Project ? "◆ " : "• ") + tab.Label;
                btn.style.height = ControlHeight;
                btn.style.marginBottom = 4;
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.paddingLeft = 12;
                btn.style.fontSize = FontBody;
                btn.style.backgroundColor = active
                    ? new Color(0.22f, 0.58f, 0.90f)
                    : new Color(0.19f, 0.20f, 0.23f);
                btn.style.color = active ? Color.white : SecondaryText;
                string id = tab.Id;
                btn.clicked += delegate
                {
                    _selectedTabId = id;
                    RebuildNav();
                    RebuildContent();
                };
                _navList.Add(btn);
            }
        }

        private void RebuildContent()
        {
            _contentHost.Clear();
            SettingTabDef tab = SettingPanelQuery.FindTab(_manager.Schema, _selectedTabId);
            if (tab == null || tab.Kind != _category)
            {
                _breadcrumb.text = "请从左侧选择设置页";
                return;
            }

            _breadcrumb.text = SettingPanelQuery.CategoryTitle(_category) + "  /  " + tab.Label;

            for (int g = 0; g < tab.Groups.Count; g++)
            {
                SettingGroupDef group = tab.Groups[g];
                List<SettingItemDef> visible = BeginVisible(group);
                try
                {
                    if (visible.Count == 0)
                    {
                        continue;
                    }

                    VisualElement box = new VisualElement();
                    ApplyPanelStyle(box);
                    box.style.marginBottom = 12;
                    box.style.marginRight = 4;

                    Label groupTitle = new Label(group.Label);
                    groupTitle.style.fontSize = 17;
                    groupTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                    groupTitle.style.color = AccentText;
                    groupTitle.style.marginBottom = 4;
                    box.Add(groupTitle);

                    if (!string.IsNullOrEmpty(group.Description))
                    {
                        Label gd = new Label(group.Description);
                        gd.style.fontSize = 13;
                        gd.style.color = SecondaryText;
                        gd.style.whiteSpace = WhiteSpace.Normal;
                        gd.style.marginBottom = 8;
                        box.Add(gd);
                    }

                    for (int i = 0; i < visible.Count; i++)
                    {
                        box.Add(BuildItemRow(visible[i]));
                    }

                    _contentHost.Add(box);
                }
                finally
                {
                    EndVisible();
                }
            }
        }

        private VisualElement BuildItemRow(SettingItemDef item)
        {
            VisualElement col = new VisualElement();
            col.style.marginBottom = 14;
            col.style.flexShrink = 0;
            col.style.overflow = Overflow.Visible;

            VisualElement row = Row();
            row.style.alignItems = Align.Center;
            // 禁止换行：换行时徽章会叠到下一设置项上
            row.style.flexWrap = Wrap.NoWrap;
            row.style.overflow = Overflow.Visible;
            row.style.minHeight = ControlHeight;

            if (_manager.IsDirty(item.Key) && item.Type != SettingItemType.Button &&
                item.Type != SettingItemType.Info)
            {
                VisualElement dirty = new VisualElement();
                dirty.style.width = 4;
                dirty.style.height = 22;
                dirty.style.backgroundColor = new Color(0.22f, 0.58f, 0.90f);
                dirty.style.marginRight = 8;
                dirty.style.flexShrink = 0;
                row.Add(dirty);
            }
            else
            {
                VisualElement pad = new VisualElement();
                pad.style.width = 12;
                pad.style.flexShrink = 0;
                row.Add(pad);
            }

            Label name = new Label(item.Label);
            name.style.width = 150;
            name.style.minWidth = 100;
            name.style.flexShrink = 0;
            name.style.fontSize = FontBody;
            name.style.color = PrimaryText;
            name.style.overflow = Overflow.Visible;
            row.Add(name);

            bool enabled = _manager.EvaluateEnabled(item);
            VisualElement control = BuildControl(item);
            control.style.flexShrink = 1;
            control.style.flexGrow = 0;
            control.SetEnabled(enabled);
            row.Add(control);

            AddBadges(row, item);
            col.Add(row);

            if (item.Type != SettingItemType.Info && !string.IsNullOrEmpty(item.Description))
            {
                Label desc = new Label(item.Description);
                desc.style.marginLeft = 12;
                desc.style.marginTop = 4;
                desc.style.fontSize = FontDesc;
                desc.style.color = SecondaryText;
                desc.style.whiteSpace = WhiteSpace.Normal;
                col.Add(desc);
            }

            return col;
        }

        private VisualElement BuildControl(SettingItemDef item)
        {
            switch (item.Type)
            {
                case SettingItemType.Bool:
                {
                    bool v = _manager.GetValue(item.Key).AsBool();
                    Toggle t = new Toggle(v ? "开启" : "关闭");
                    t.value = v;
                    t.style.width = 120;
                    t.style.height = ControlHeight;
                    t.style.fontSize = FontBody;
                    StyleToggleLight(t);
                    string key = item.Key;
                    t.RegisterValueChangedCallback(evt =>
                    {
                        _manager.SetPending(key, SettingValue.FromBool(evt.newValue));
                        t.label = evt.newValue ? "开启" : "关闭";
                        StyleToggleLight(t);
                        RefreshStatus();
                        RebuildContent();
                    });
                    return t;
                }
                case SettingItemType.Int:
                case SettingItemType.Float:
                {
                    VisualElement wrap = Row();
                    wrap.style.alignItems = Align.Center;
                    float v = item.Type == SettingItemType.Int
                        ? _manager.GetValue(item.Key).AsInt()
                        : _manager.GetValue(item.Key).AsFloat();
                    string unit = string.IsNullOrEmpty(item.Unit) ? "" : " " + item.Unit;
                    Label val = new Label(
                        (item.Type == SettingItemType.Int ? ((int)v).ToString() : v.ToString("0.###")) + unit);
                    val.style.width = 96;
                    val.style.fontSize = FontBody;
                    val.style.color = PrimaryText;
                    val.style.unityTextAlign = TextAnchor.MiddleRight;

                    Slider slider = new Slider(item.Min, item.Max);
                    slider.value = v;
                    slider.style.width = 240;
                    slider.style.height = 36;
                    StyleSliderClear(slider);
                    string key = item.Key;
                    SettingItemType type = item.Type;
                    float step = item.Step;
                    slider.RegisterValueChangedCallback(evt =>
                    {
                        float nv = evt.newValue;
                        if (type == SettingItemType.Int)
                        {
                            int iv = Mathf.RoundToInt(nv);
                            if (step > 0.999f)
                            {
                                int s = Mathf.Max(1, Mathf.RoundToInt(step));
                                iv = Mathf.RoundToInt((float)iv / s) * s;
                            }

                            _manager.SetPending(key, SettingValue.FromInt(iv));
                            val.text = iv + unit;
                        }
                        else
                        {
                            if (step > 0.0001f)
                            {
                                nv = Mathf.Round(nv / step) * step;
                            }

                            _manager.SetPending(key, SettingValue.FromFloat(nv));
                            val.text = nv.ToString("0.###") + unit;
                        }

                        RefreshStatus();
                    });
                    wrap.Add(val);
                    wrap.Add(slider);
                    return wrap;
                }
                case SettingItemType.String:
                {
                    TextField field = new TextField();
                    field.value = _manager.GetValue(item.Key).AsString();
                    StyleInputControl(field, 280);
                    string key = item.Key;
                    field.RegisterValueChangedCallback(evt =>
                    {
                        _manager.SetPending(key, SettingValue.FromString(evt.newValue));
                        RefreshStatus();
                    });
                    return field;
                }
                case SettingItemType.Enum:
                {
                    List<string> choices = new List<string>();
                    List<string> values = new List<string>();
                    string current = _manager.GetValue(item.Key).AsString();
                    string currentLabel = current;
                    if (item.Options != null)
                    {
                        for (int i = 0; i < item.Options.Count; i++)
                        {
                            choices.Add(item.Options[i].Label);
                            values.Add(item.Options[i].Value);
                            if (item.Options[i].Value == current)
                            {
                                currentLabel = item.Options[i].Label;
                            }
                        }
                    }

                    DropdownField dropdown = new DropdownField();
                    dropdown.choices = choices;
                    dropdown.SetValueWithoutNotify(currentLabel);
                    StyleInputControl(dropdown, 240);
                    string key = item.Key;
                    dropdown.RegisterValueChangedCallback(evt =>
                    {
                        int idx = choices.IndexOf(evt.newValue);
                        if (idx >= 0 && idx < values.Count)
                        {
                            _manager.SetPending(key, SettingValue.FromString(values[idx]));
                            RefreshStatus();
                            RebuildContent();
                        }
                    });
                    return dropdown;
                }
                case SettingItemType.Button:
                {
                    Button btn = MakeButton(item.Label, item.Risk == SettingRiskLevel.Danger);
                    btn.style.width = 200;
                    SettingItemDef captured = item;
                    btn.clicked += delegate { RequestButton(captured); };
                    return btn;
                }
                case SettingItemType.Info:
                {
                    Label info = new Label(item.Description);
                    info.style.fontSize = FontBody;
                    info.style.color = SecondaryText;
                    info.style.whiteSpace = WhiteSpace.Normal;
                    info.style.flexGrow = 1;
                    return info;
                }
                default:
                    return new VisualElement();
            }
        }

        private void AddBadges(VisualElement row, SettingItemDef item)
        {
            if (item.Type == SettingItemType.Button || item.Type == SettingItemType.Info)
            {
                if (item.Risk == SettingRiskLevel.Danger)
                {
                    row.Add(MakeBadge("⚠ 高危", new Color(0.86f, 0.32f, 0.28f)));
                }

                return;
            }

            if (item.ApplyMode == SettingApplyMode.Restart)
            {
                row.Add(MakeBadge("需重启", new Color(0.92f, 0.68f, 0.22f)));
            }
            else
            {
                row.Add(MakeBadge("立即生效", new Color(0.28f, 0.72f, 0.48f)));
            }

            if (item.Risk == SettingRiskLevel.Danger)
            {
                row.Add(MakeBadge("⚠ 高危", new Color(0.86f, 0.32f, 0.28f)));
            }
            else if (item.Risk == SettingRiskLevel.Advanced)
            {
                row.Add(MakeBadge("高级", new Color(0.92f, 0.68f, 0.22f)));
            }
        }

        private static Label MakeBadge(string text, Color bg)
        {
            Label badge = new Label(text);
            badge.style.backgroundColor = bg;
            badge.style.color = Color.white;
            badge.style.fontSize = FontDesc;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.paddingLeft = 8;
            badge.style.paddingRight = 8;
            badge.style.paddingTop = 4;
            badge.style.paddingBottom = 4;
            badge.style.marginLeft = 8;
            badge.style.height = StyleKeyword.Auto;
            badge.style.minHeight = 26;
            badge.style.flexShrink = 0;
            badge.style.overflow = Overflow.Visible;
            badge.style.unityTextAlign = TextAnchor.MiddleCenter;
            return badge;
        }

        private const float SliderDraggerH = 28f;
        private const float SliderDraggerW = 16f;

        /// <summary>
        /// 去掉 Slider 整块深色底；拖柄加高并用世界坐标垂直对齐到轨道中心。
        /// </summary>
        private static void StyleSliderClear(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.style.backgroundColor = Color.clear;
            slider.style.borderTopWidth = 0;
            slider.style.borderBottomWidth = 0;
            slider.style.borderLeftWidth = 0;
            slider.style.borderRightWidth = 0;
            slider.RegisterCallback<AttachToPanelEvent>(OnSliderAttachedClear);
        }

        private static void OnSliderAttachedClear(AttachToPanelEvent evt)
        {
            Slider slider = evt.target as Slider;
            if (slider == null)
            {
                return;
            }

            slider.UnregisterCallback<AttachToPanelEvent>(OnSliderAttachedClear);
            slider.style.backgroundColor = Color.clear;

            VisualElement input = slider.Q(className: "unity-base-slider__input");
            if (input == null)
            {
                input = slider.Q(className: "unity-base-field__input");
            }

            if (input != null)
            {
                input.style.backgroundColor = Color.clear;
                input.style.borderTopWidth = 0;
                input.style.borderBottomWidth = 0;
                input.style.borderLeftWidth = 0;
                input.style.borderRightWidth = 0;
                input.style.height = Length.Percent(100);
                input.style.justifyContent = Justify.Center;
            }

            VisualElement drag = slider.Q(className: "unity-base-slider__drag-container");
            if (drag != null)
            {
                drag.style.backgroundColor = Color.clear;
                drag.style.height = Length.Percent(100);
                drag.style.justifyContent = Justify.Center;
            }

            VisualElement tracker = slider.Q(className: "unity-base-slider__tracker");
            if (tracker != null)
            {
                tracker.style.height = 4;
                tracker.style.top = StyleKeyword.Auto;
                tracker.style.bottom = StyleKeyword.Auto;
                tracker.style.alignSelf = Align.Center;
            }

            VisualElement dragger = slider.Q(className: "unity-base-slider__dragger");
            if (dragger != null)
            {
                dragger.style.width = SliderDraggerW;
                dragger.style.height = SliderDraggerH;
                dragger.style.backgroundColor = new Color(0.90f, 0.91f, 0.93f, 1f);
                dragger.style.position = Position.Absolute;
            }

            AlignSliderDraggerToTrack(slider);
            // 多帧校正：布局/主题可能晚一拍改写位置
            slider.schedule.Execute(() => AlignSliderDraggerToTrack(slider)).StartingIn(0);
            slider.schedule.Execute(() => AlignSliderDraggerToTrack(slider)).StartingIn(16);
            slider.schedule.Execute(() => AlignSliderDraggerToTrack(slider)).StartingIn(50);
            slider.schedule.Execute(() => AlignSliderDraggerToTrack(slider)).StartingIn(100);
        }

        /// <summary>
        /// 视觉对齐：拖柄中心与轨道中心重合；按实测误差修正 top。
        /// </summary>
        private static void AlignSliderDraggerToTrack(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            VisualElement drag = slider.Q(className: "unity-base-slider__drag-container");
            VisualElement tracker = slider.Q(className: "unity-base-slider__tracker");
            VisualElement dragger = slider.Q(className: "unity-base-slider__dragger");
            if (drag == null || dragger == null)
            {
                return;
            }

            dragger.style.width = SliderDraggerW;
            dragger.style.height = SliderDraggerH;
            dragger.style.position = Position.Absolute;

            Rect parent = drag.worldBound;
            if (parent.height < 1f)
            {
                return;
            }

            // 已有有效 worldBound：直接按误差挪（最准）
            if (tracker != null && tracker.worldBound.height > 0.5f && dragger.worldBound.height > 0.5f)
            {
                float err = tracker.worldBound.center.y - dragger.worldBound.center.y;
                if (Mathf.Abs(err) >= 0.5f)
                {
                    float cur = dragger.resolvedStyle.top;
                    if (float.IsNaN(cur))
                    {
                        cur = 0f;
                    }

                    float next = Mathf.Clamp(cur + err, 0f, Mathf.Max(0f, parent.height - SliderDraggerH));
                    dragger.style.top = next;
                }

                return;
            }

            // 首次估算
            float centerY = tracker != null && tracker.worldBound.height > 0.5f
                ? tracker.worldBound.center.y
                : parent.center.y;
            float top = Mathf.Clamp(centerY - parent.yMin - SliderDraggerH * 0.5f, 0f,
                Mathf.Max(0f, parent.height - SliderDraggerH));
            dragger.style.top = top;
        }

        /// <summary>标题栏「高级设置」：勾选框与文字贴紧。</summary>
        private static void StyleAdvancedToggle(Toggle toggle)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.RegisterCallback<AttachToPanelEvent>(OnAdvancedToggleAttached);
        }

        private static void OnAdvancedToggleAttached(AttachToPanelEvent evt)
        {
            Toggle t = evt.target as Toggle;
            if (t == null)
            {
                return;
            }

            t.UnregisterCallback<AttachToPanelEvent>(OnAdvancedToggleAttached);
            VisualElement check = t.Q(className: "unity-toggle__input");
            if (check != null)
            {
                check.style.marginRight = 0;
                check.style.marginLeft = 0;
            }

            Label label = t.Q<Label>();
            if (label != null)
            {
                label.style.marginLeft = 4;
                label.style.paddingLeft = 0;
                label.style.marginRight = 0;
                label.text = "高级设置";
            }
        }

        private static void ApplyPanelStyleSheet(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Resources.Load<StyleSheet>("UIToolkit/SettingPanel");
#if UNITY_EDITOR
            if (sheet == null)
            {
                sheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Packages/com.nonsensicallab.nonsensicalkit.core/Runtime/Service/SettingService/Resources/UIToolkit/SettingPanel.uss");
            }
#endif
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }
        }

        private void RefreshStatus()
        {
            if (_statusLabel == null || _manager == null)
            {
                return;
            }

            int dirty = _manager.GetDirtyCount();
            int restartPending = _manager.GetPendingRestartCount();
            bool needRestart = _showRestartBanner || _manager.HasPendingRestart || restartPending > 0;
            _statusLabel.text = dirty > 0
                ? dirty + " 项未应用" + (restartPending > 0 || _manager.HasPendingRestart
                    ? "  ·  有设置需重启生效"
                    : string.Empty)
                : (_manager.HasPendingRestart ? "有设置需重启后生效" : "无未保存的修改");

            if (_restartButton != null)
            {
                _restartButton.style.display = needRestart ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RequestApply(bool closeAfter)
        {
            List<SettingItemDef> dangers = _manager.CollectDangerPending();
            if (dangers.Count > 0)
            {
                ShowConfirm(SettingPanelQuery.DangerMessage(dangers), delegate
                {
                    _manager.ApplyPending(true);
                    RebuildAll();
                    RefreshStatus();
                    if (closeAfter)
                    {
                        SetVisible(false);
                    }
                });
                return;
            }

            _manager.ApplyPending(true);
            RebuildAll();
            RefreshStatus();
            if (closeAfter)
            {
                SetVisible(false);
            }
        }

        private void RequestButton(SettingItemDef item)
        {
            if (item.Risk == SettingRiskLevel.Danger || !string.IsNullOrEmpty(item.ConfirmMessage))
            {
                string msg = string.IsNullOrEmpty(item.ConfirmMessage)
                    ? "确认执行「" + item.Label + "」？"
                    : item.ConfirmMessage;
                string action = item.ButtonAction;
                ShowConfirm(msg, delegate
                {
                    _manager.HandleButton(action);
                    RebuildAll();
                });
                return;
            }

            _manager.HandleButton(item.ButtonAction);
            RebuildAll();
        }

        private void ShowConfirm(string message, System.Action action)
        {
            _confirmMessage.text = message;
            _confirmAction = action;
            _confirmOverlay.style.display = DisplayStyle.Flex;
        }

        private List<SettingItemDef> BeginVisible(SettingGroupDef group)
        {
            if (_visibleDepth >= _visiblePool.Count)
            {
                _visiblePool.Add(new List<SettingItemDef>());
            }

            List<SettingItemDef> list = _visiblePool[_visibleDepth];
            _visibleDepth++;
            SettingPanelQuery.CollectVisible(_manager, group, _search, list);
            return list;
        }

        private void EndVisible()
        {
            if (_visibleDepth > 0)
            {
                _visibleDepth--;
            }
        }

        private void EnsureDefaultSelection()
        {
            if (_manager == null || _manager.Schema == null)
            {
                return;
            }

            if (!_categoryInitialized)
            {
                if (CategoryHasTabs(SettingTabKind.Project))
                {
                    _category = SettingTabKind.Project;
                }
                else if (CategoryHasTabs(SettingTabKind.System))
                {
                    _category = SettingTabKind.System;
                }

                _categoryInitialized = true;
            }

            if (!CategoryHasTabs(_category))
            {
                if (CategoryHasTabs(SettingTabKind.System))
                {
                    _category = SettingTabKind.System;
                }
                else if (CategoryHasTabs(SettingTabKind.Project))
                {
                    _category = SettingTabKind.Project;
                }
            }

            List<SettingTabDef> tabs = GetTabsInCategory(_category);
            bool valid = false;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].Id == _selectedTabId)
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
            {
                _selectedTabId = tabs.Count > 0 ? tabs[0].Id : null;
            }
        }

        private void SelectCategory(SettingTabKind kind)
        {
            _category = kind;
            List<SettingTabDef> tabs = GetTabsInCategory(kind);
            _selectedTabId = tabs.Count > 0 ? tabs[0].Id : null;
            RebuildCategoryBar();
            RebuildNav();
            RebuildContent();
        }

        private bool CategoryHasTabs(SettingTabKind kind)
        {
            return SettingPanelQuery.HasTabs(_manager != null ? _manager.Schema : null, kind);
        }

        /// <summary>返回复用列表，调用方用完即弃，不要保存引用。</summary>
        private List<SettingTabDef> GetTabsInCategory(SettingTabKind kind)
        {
            SettingPanelQuery.CopyTabs(_manager != null ? _manager.Schema : null, kind, _tabBuffer);
            return _tabBuffer;
        }

        private Button MakeFooterButton(string text, bool primary, System.Action onClick)
        {
            Button btn = MakeButton(text, primary);
            btn.style.marginLeft = 4;
            btn.style.marginTop = 2;
            btn.style.marginBottom = 2;
            btn.style.minWidth = 72;
            btn.style.height = 36;
            btn.style.minHeight = 36;
            btn.style.fontSize = FontDesc;
            btn.style.width = StyleKeyword.Auto;
            btn.style.flexShrink = 0;
            btn.clicked += delegate
            {
                if (onClick != null)
                {
                    onClick();
                }
            };
            return btn;
        }

        private static Button MakeButton(string text, bool primary)
        {
            Button btn = new Button();
            btn.text = text;
            btn.style.height = ControlHeight;
            btn.style.minHeight = ControlHeight;
            btn.style.fontSize = FontBody;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.paddingTop = 4;
            btn.style.paddingBottom = 4;
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            if (primary)
            {
                btn.style.backgroundColor = new Color(0.22f, 0.58f, 0.90f);
                btn.style.color = Color.white;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else
            {
                btn.style.backgroundColor = new Color(0.30f, 0.32f, 0.35f);
                btn.style.color = Color.white;
            }

            return btn;
        }

        /// <summary>
        /// TextField / DropdownField：固定高度 + 内边距，避免字被上下裁切。
        /// </summary>
        private static void StyleInputControl(VisualElement control, float width)
        {
            if (control == null)
            {
                return;
            }

            control.style.width = width;
            control.style.maxWidth = width;
            control.style.minWidth = 120;
            control.style.flexShrink = 1;
            control.style.height = ControlHeight;
            control.style.minHeight = ControlHeight;
            control.style.fontSize = FontBody;
            // 输入框/下拉为浅色底，用深色字保证对比度
            control.style.color = InputText;
            control.style.unityTextAlign = TextAnchor.MiddleLeft;
            control.style.overflow = Overflow.Visible;
            control.style.paddingTop = 0;
            control.style.paddingBottom = 0;
            control.style.paddingLeft = 0;
            control.style.paddingRight = 0;
            control.RegisterCallback<AttachToPanelEvent>(OnInputControlAttached);
        }

        private static void OnInputControlAttached(AttachToPanelEvent evt)
        {
            VisualElement control = evt.target as VisualElement;
            if (control == null)
            {
                return;
            }

            control.UnregisterCallback<AttachToPanelEvent>(OnInputControlAttached);
            StyleInputInner(control);
        }

        private static void StyleInputInner(VisualElement control)
        {
            VisualElement input = control.Q(className: "unity-base-popup-field__input");
            if (input == null)
            {
                input = control.Q(className: "unity-base-text-field__input");
            }

            if (input == null)
            {
                input = control.Q(className: "unity-base-field__input");
            }

            if (input != null)
            {
                input.style.flexGrow = 1;
                input.style.height = ControlHeight;
                input.style.minHeight = ControlHeight;
                input.style.paddingTop = 10;
                input.style.paddingBottom = 10;
                input.style.paddingLeft = 10;
                input.style.paddingRight = 10;
                input.style.unityTextAlign = TextAnchor.MiddleLeft;
                input.style.overflow = Overflow.Visible;
                input.style.color = InputText;
                input.style.backgroundColor = InputBackground;
                input.style.fontSize = FontBody;
                input.style.justifyContent = Justify.Center;
                input.style.alignItems = Align.Center;
            }

            control.Query<Label>().ForEach(label =>
            {
                label.style.color = InputText;
                label.style.fontSize = FontBody;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.overflow = Overflow.Visible;
                label.style.marginTop = 0;
                label.style.marginBottom = 0;
            });
        }

        /// <summary>
        /// 竖向滚动条贴内容区右缘；保持可见，避免被 overflow/Auto 藏掉。
        /// </summary>
        private static void StyleContentScroll(ScrollView scroll)
        {
            if (scroll == null)
            {
                return;
            }

            scroll.style.marginRight = 0;
            scroll.style.paddingRight = 0;
            scroll.style.overflow = Overflow.Visible;
            Scroller vs = scroll.verticalScroller;
            if (vs != null)
            {
                vs.style.display = DisplayStyle.Flex;
                vs.style.visibility = Visibility.Visible;
                vs.style.width = 14;
                vs.style.minWidth = 14;
                vs.style.flexShrink = 0;
                vs.style.marginLeft = 2;
                vs.style.marginRight = 0;
                vs.style.marginTop = 0;
                vs.style.marginBottom = 0;
                vs.style.paddingLeft = 0;
                vs.style.paddingRight = 0;
                vs.style.backgroundColor = new Color(0.12f, 0.13f, 0.15f, 1f);
                if (vs.slider != null)
                {
                    vs.slider.style.marginLeft = 1;
                    vs.slider.style.marginRight = 1;
                }
            }

            if (scroll.contentViewport != null)
            {
                scroll.contentViewport.style.marginRight = 0;
                scroll.contentViewport.style.paddingRight = 4;
            }
        }

        // ConstantPixelSize：字号即屏幕像素，小窗也保持可读
        private const int FontTitle = 24;
        private const int FontHeader = 18;
        private const int FontBody = 17;
        private const int FontDesc = 14;
        private const int ControlHeight = 44;

        // 深色面板上的高对比文字色
        private static readonly Color PrimaryText = new Color(0.95f, 0.96f, 0.97f, 1f);
        private static readonly Color SecondaryText = new Color(0.82f, 0.84f, 0.87f, 1f);
        private static readonly Color AccentText = new Color(0.45f, 0.78f, 1f, 1f);
        // 浅色输入框/下拉：深字 + 近白底，保证对比度
        private static readonly Color InputText = new Color(0.10f, 0.11f, 0.13f, 1f);
        private static readonly Color InputBackground = new Color(0.92f, 0.93f, 0.95f, 1f);

        private static void StyleToggleLight(Toggle toggle)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.style.color = PrimaryText;
            // Toggle 文案在子 Label 上，主题默认常为深色，需显式覆盖
            Label label = toggle.Q<Label>();
            if (label != null)
            {
                label.style.color = PrimaryText;
            }
            else
            {
                // 尚未挂到树时延后一帧再刷（显式 Action 避免 Execute 重载歧义）
                toggle.RegisterCallback<AttachToPanelEvent>(OnToggleAttachedPaint);
            }
        }

        private static void OnToggleAttachedPaint(AttachToPanelEvent evt)
        {
            Toggle toggle = evt.target as Toggle;
            if (toggle == null)
            {
                return;
            }

            toggle.UnregisterCallback<AttachToPanelEvent>(OnToggleAttachedPaint);
            Label label = toggle.Q<Label>();
            if (label != null)
            {
                label.style.color = PrimaryText;
            }
        }

        private static VisualElement Row()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            return row;
        }

        private static void ApplyRootStyle(VisualElement e)
        {
            // Relative + flexGrow：Absolute+百分比在 UIDocument 下会算出 0x0
            e.style.position = Position.Relative;
            e.style.flexGrow = 1;
            e.style.width = Length.Percent(100);
            e.style.height = Length.Percent(100);
            e.style.minWidth = Length.Percent(100);
            e.style.minHeight = Length.Percent(100);
        }

        private void BindWindowDrag(VisualElement titleBar, VisualElement window, VisualElement parent)
        {
            titleBar.pickingMode = PickingMode.Position;
            titleBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0 || !CanDragTitle(evt.target as VisualElement, titleBar))
                {
                    return;
                }

                Rect layout = window.layout;
                window.style.position = Position.Absolute;
                window.style.left = layout.x;
                window.style.top = layout.y;
                window.style.right = StyleKeyword.Auto;
                window.style.bottom = StyleKeyword.Auto;
                _dragPointerId = evt.pointerId;
                _dragPointerStart = evt.position;
                _dragWindowStart = new Vector2(layout.x, layout.y);
                titleBar.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            titleBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_dragPointerId != evt.pointerId || !titleBar.HasPointerCapture(evt.pointerId))
                {
                    return;
                }

                Vector2 delta = (Vector2)evt.position - _dragPointerStart;
                MoveWindow(window, parent, _dragWindowStart + delta);
                _windowUserMoved = true;
                evt.StopPropagation();
            });
            titleBar.RegisterCallback<PointerUpEvent>(evt => EndWindowDrag(titleBar, evt.pointerId));
            titleBar.RegisterCallback<PointerCancelEvent>(evt => EndWindowDrag(titleBar, evt.pointerId));
            parent.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (!_windowUserMoved || _dragPointerId >= 0)
                {
                    return;
                }

                float left = window.resolvedStyle.left;
                float top = window.resolvedStyle.top;
                float parentW = parent.resolvedStyle.width;
                float parentH = parent.resolvedStyle.height;
                float windowW = window.resolvedStyle.width;
                if (float.IsNaN(left)) left = window.layout.x;
                if (float.IsNaN(top)) top = window.layout.y;
                if (float.IsNaN(parentW) || float.IsNaN(windowW) || parentW <= 1f || windowW <= 1f)
                {
                    return;
                }

                float minLeft = 48f - windowW;
                float maxLeft = parentW - 48f;
                float maxTop = Mathf.Max(0f, parentH - 44f);
                if (left < minLeft || left > maxLeft || top < 0f || top > maxTop)
                {
                    MoveWindow(window, parent, new Vector2(left, top));
                }
            });
        }

        private void EndWindowDrag(VisualElement titleBar, int pointerId)
        {
            if (_dragPointerId != pointerId)
            {
                return;
            }

            if (titleBar.HasPointerCapture(pointerId))
            {
                titleBar.ReleasePointer(pointerId);
            }

            _dragPointerId = -1;
        }

        private static bool CanDragTitle(VisualElement target, VisualElement titleBar)
        {
            VisualElement current = target;
            while (current != null && current != titleBar)
            {
                if (current is Button || current is Toggle)
                {
                    return false;
                }

                current = current.parent;
            }

            return true;
        }

        private static void MoveWindow(VisualElement window, VisualElement parent, Vector2 pos)
        {
            float parentW = parent.resolvedStyle.width;
            float parentH = parent.resolvedStyle.height;
            float windowW = window.resolvedStyle.width;
            float windowH = window.resolvedStyle.height;
            if (float.IsNaN(parentW) || parentW <= 1f)
            {
                parentW = parent.layout.width;
            }

            if (float.IsNaN(parentH) || parentH <= 1f)
            {
                parentH = parent.layout.height;
            }

            if (float.IsNaN(windowW) || windowW <= 1f)
            {
                windowW = window.layout.width;
            }

            if (float.IsNaN(windowH) || windowH <= 1f)
            {
                windowH = window.layout.height;
            }

            if (float.IsNaN(pos.x) || float.IsNaN(pos.y))
            {
                pos = new Vector2(window.layout.x, window.layout.y);
            }

            float left = pos.x;
            float top = pos.y;
            if (parentW > 1f && windowW > 1f)
            {
                left = Mathf.Clamp(left, 48f - windowW, parentW - 48f);
                top = Mathf.Clamp(top, 0f, Mathf.Max(0f, parentH - 44f));
            }

            window.style.left = left;
            window.style.top = top;
        }

        private static void ApplyWindowStyle(VisualElement e)
        {
            // 百分比自适应小窗口；去掉过大的 minWidth/minHeight，避免挤叠
            e.style.flexDirection = FlexDirection.Column;
            e.style.width = Length.Percent(92);
            e.style.height = Length.Percent(90);
            e.style.maxWidth = 1280;
            e.style.maxHeight = 900;
            e.style.minWidth = 320;
            e.style.minHeight = 280;
            e.style.overflow = Overflow.Hidden;
            e.style.backgroundColor = new Color(0.14f, 0.15f, 0.17f, 0.98f);
            e.style.paddingLeft = 18;
            e.style.paddingRight = 18;
            e.style.paddingTop = 18;
            e.style.paddingBottom = 14;
            e.style.borderTopLeftRadius = 6;
            e.style.borderTopRightRadius = 6;
            e.style.borderBottomLeftRadius = 6;
            e.style.borderBottomRightRadius = 6;
        }

        private static void ApplyPanelStyle(VisualElement e)
        {
            e.style.backgroundColor = new Color(0.19f, 0.20f, 0.23f, 1f);
            e.style.paddingLeft = 12;
            e.style.paddingRight = 12;
            e.style.paddingTop = 10;
            e.style.paddingBottom = 10;
            e.style.borderTopLeftRadius = 4;
            e.style.borderTopRightRadius = 4;
            e.style.borderBottomLeftRadius = 4;
            e.style.borderBottomRightRadius = 4;
        }

        private static void ApplyBannerStyle(VisualElement e)
        {
            e.style.backgroundColor = new Color(0.92f, 0.68f, 0.22f, 1f);
            e.style.paddingLeft = 12;
            e.style.paddingRight = 12;
            e.style.paddingTop = 8;
            e.style.paddingBottom = 8;
            e.style.marginBottom = 8;
            e.style.borderTopLeftRadius = 4;
            e.style.borderTopRightRadius = 4;
            e.style.borderBottomLeftRadius = 4;
            e.style.borderBottomRightRadius = 4;
        }
    }
}
#endif
