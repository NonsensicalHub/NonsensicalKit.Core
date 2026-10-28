using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 工业风格 IMGUI 设置面板（Unity 2018.4 ~ 2021.1，或强制回退时使用）。
    /// 顶部分类页：系统设置 / 项目设置 / 高级 / 诊断；左侧为该分类下的子页。
    /// </summary>
    public class ImguiSettingPanel : MonoBehaviour, ISettingPanelView
    {
        public string RendererName
        {
            get { return "IMGUI"; }
        }

        private SettingManager _manager;
        private readonly ImguiTheme _theme = new ImguiTheme();
        private bool _visible;
        private Rect _windowRect = new Rect(60, 40, 1080, 720);
        private Vector2 _navScroll;
        private Vector2 _contentScroll;
        private SettingTabKind _category = SettingTabKind.System;
        private string _selectedTabId;
        private string _search = string.Empty;
        private int _windowId;
        private bool _confirmOpen;
        private List<SettingItemDef> _confirmItems;
        private string _confirmMessage;
        private System.Action _confirmAction;
        private bool _showRestartBanner;
        private string _enumPopupKey;
        private float _enumWidth;
        private float _enumWidthMeasured;
        private GUIStyle _popupOpt;
        private GUIStyle _popupOptOn;
        private bool _categoryInitialized;
        private readonly List<List<SettingItemDef>> _visiblePool = new List<List<SettingItemDef>>();
        private int _visibleDepth;
        private readonly List<SettingTabDef> _tabBuffer = new List<SettingTabDef>();
        private readonly Dictionary<string, CachedLabel> _intLabels = new Dictionary<string, CachedLabel>();
        private readonly Dictionary<string, CachedLabel> _floatLabels = new Dictionary<string, CachedLabel>();
        private readonly Dictionary<string, CachedLabel> _enumLabels = new Dictionary<string, CachedLabel>();
        private readonly Dictionary<string, CachedLabel> _navLabels = new Dictionary<string, CachedLabel>();
        private int _statusDirty = int.MinValue;
        private int _statusRestart = int.MinValue;
        private string _statusText;
        private SettingTabKind _crumbKind;
        private string _crumbTabId;
        private string _crumbLabel;
        private string _crumbText;

        public void Bind(SettingManager manager)
        {
            _manager = manager;
            if (manager != null && manager.PanelConfig != null)
            {
                SettingPanelConfig c = manager.PanelConfig;
                _windowRect = new Rect(c.WindowX, c.WindowY, Mathf.Max(c.WindowWidth, 1000f),
                    Mathf.Max(c.WindowHeight, 680f));
                if (c.UseHeightPercentage)
                {
                    _windowRect.height = Screen.height * c.HeightPercentage;
                }
            }

            _windowId = GetInstanceID();
            manager.OnRestartRequired += delegate { _showRestartBanner = true; };
        }

        public void SetVisible(bool v)
        {
            _visible = v;
            if (v && _manager != null)
            {
                _manager.ClearPending();
                EnsureDefaultSelection();
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
        }

        private void OnGUI()
        {
            if (!_visible || _manager == null || _manager.Schema == null)
            {
                return;
            }

            _theme.Ensure();
            if (Event.current.type == EventType.Layout && _enumWidthMeasured > 8f)
            {
                _enumWidth = _enumWidthMeasured;
            }
            EnsureDefaultSelection();

            if (_manager.PanelConfig.UseHeightPercentage)
            {
                _windowRect.height = Mathf.Max(560f, Screen.height * _manager.PanelConfig.HeightPercentage);
            }

            _windowRect.width = Mathf.Clamp(_windowRect.width, 960f, Screen.width);
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0, Screen.width - _windowRect.width));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0, Screen.height - 40));

            _windowRect = GUI.Window(_windowId, _windowRect, DrawWindow, "设置", _theme.Window);

            if (_confirmOpen)
            {
                DrawConfirmModal();
            }
        }

        private void DrawWindow(int id)
        {
            HoldSliderDrag();
            GUILayout.Space(6);

            if (_showRestartBanner || _manager.HasPendingRestart)
            {
                GUILayout.Box("以下设置将在重启程序后生效。可点击底部「立即重启」。", _theme.Banner,
                    GUILayout.ExpandWidth(true));
                GUILayout.Space(6);
            }

            DrawTopBar();
            GUILayout.Space(8);
            DrawCategoryTabs();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawNav();
            GUILayout.Space(12);
            DrawContent();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            DrawFooter();

            if (!string.IsNullOrEmpty(_activeSliderKey) && Event.current.type == EventType.MouseDrag)
            {
                Event.current.Use();
                GUIUtility.hotControl = SliderHoldId;
            }

            if (Event.current.rawType == EventType.MouseUp)
            {
                _activeSliderKey = null;
                if (GUIUtility.hotControl == SliderHoldId)
                {
                    GUIUtility.hotControl = 0;
                }
            }

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 28f));
        }

        /// <summary>
        /// 滑块按设置项 key 占住拖拽。IMGUI 的控件编号会在「未保存」色条出现时整体后移，
        /// 下一条滑块或窗口拖拽会接到同一次鼠标。
        /// </summary>
        private void HoldSliderDrag()
        {
            Event e = Event.current;
            if (string.IsNullOrEmpty(_activeSliderKey))
            {
                return;
            }

            if (e.type == EventType.MouseDrag || e.rawType == EventType.MouseUp)
            {
                GUIUtility.hotControl = SliderHoldId;
            }
        }

        private void DrawTopBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索", _theme.Label, GUILayout.Width(48));
            _search = GUILayout.TextField(_search ?? string.Empty, _theme.SearchField, GUILayout.Width(280));
            GUILayout.FlexibleSpace();

            bool adv = _manager.ShowAdvanced;
            if (DrawSwitch(adv, adv ? "高级：开" : "高级：关"))
            {
                _manager.ShowAdvanced = !adv;
                if (!_manager.ShowAdvanced && _category == SettingTabKind.Advanced)
                {
                    SelectCategory(SettingTabKind.System);
                }
            }

            GUILayout.Space(8);
            if (GUILayout.Button("关闭", _theme.Button, GUILayout.Width(72)))
            {
                _visible = false;
                _manager.CancelPending();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawCategoryTabs()
        {
            GUILayout.BeginHorizontal();
            DrawCategoryButton("系统设置", SettingTabKind.System);
            DrawCategoryButton("项目设置", SettingTabKind.Project);
            if (_manager.ShowAdvanced)
            {
                DrawCategoryButton("高级", SettingTabKind.Advanced);
            }

            DrawCategoryButton("诊断信息", SettingTabKind.Diagnostics);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawCategoryButton(string label, SettingTabKind kind)
        {
            if (!CategoryHasTabs(kind))
            {
                GUI.enabled = false;
            }

            GUIStyle style = _category == kind ? _theme.CategoryActive : _theme.CategoryNormal;
            if (GUILayout.Button(label, style, GUILayout.Width(128)))
            {
                SelectCategory(kind);
            }

            GUI.enabled = true;
        }

        private void DrawNav()
        {
            GUILayout.BeginVertical(_theme.GroupBox, GUILayout.Width(200), GUILayout.ExpandHeight(true));
            GUILayout.Label(SettingPanelQuery.CategoryTitle(_category), _theme.NavHeader);
            GUILayout.Space(4);
            _navScroll = GUILayout.BeginScrollView(_navScroll);

            List<SettingTabDef> tabs = GetTabsInCategory(_category);
            if (tabs.Count == 0)
            {
                GUILayout.Label("此分类暂无页面", _theme.Desc);
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                SettingTabDef tab = tabs[i];
                GUIStyle style = tab.Id == _selectedTabId ? _theme.TabActive : _theme.TabNormal;
                if (GUILayout.Button(NavLabel(tab), style, GUILayout.ExpandWidth(true)))
                {
                    _selectedTabId = tab.Id;
                    _contentScroll = Vector2.zero;
                    _enumPopupKey = null;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawContent()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            SettingTabDef tab = SettingPanelQuery.FindTab(_manager.Schema, _selectedTabId);
            if (tab == null || tab.Kind != _category)
            {
                GUILayout.Label("请从左侧选择设置页", _theme.Desc);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label(Breadcrumb(tab), _theme.SectionHeader);
            GUILayout.Space(8);
            _contentScroll = GUILayout.BeginScrollView(_contentScroll);

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

                    GUILayout.BeginVertical(_theme.GroupBox);
                    GUILayout.Label(group.Label, _theme.SectionHeader);
                    if (!string.IsNullOrEmpty(group.Description))
                    {
                        GUILayout.Space(2);
                        GUILayout.Label(group.Description, _theme.Desc);
                    }

                    GUILayout.Space(8);
                    for (int i = 0; i < visible.Count; i++)
                    {
                        DrawItem(visible[i]);
                        GUILayout.Space(10);
                    }

                    GUILayout.EndVertical();
                }
                finally
                {
                    EndVisible();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawItem(SettingItemDef item)
        {
            bool enabled = _manager.EvaluateEnabled(item);
            bool dirty = item.Type != SettingItemType.Button && item.Type != SettingItemType.Info &&
                         _manager.IsDirty(item.Key);

            GUILayout.BeginHorizontal();
            Rect gutter = GUILayoutUtility.GetRect(8f, 32f, GUILayout.Width(8f), GUILayout.Height(32f));
            if (dirty && Event.current.type == EventType.Repaint && _theme.DirtyBar.normal.background != null)
            {
                GUI.DrawTexture(new Rect(gutter.x, gutter.y + 6f, 4f, 20f), _theme.DirtyBar.normal.background);
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(item.Label, _theme.Label, GUILayout.Width(200), GUILayout.Height(32));

            GUI.enabled = enabled;
            switch (item.Type)
            {
                case SettingItemType.Bool:
                    DrawBool(item);
                    break;
                case SettingItemType.Int:
                    DrawInt(item);
                    break;
                case SettingItemType.Float:
                    DrawFloat(item);
                    break;
                case SettingItemType.String:
                    DrawString(item);
                    break;
                case SettingItemType.Enum:
                    DrawEnum(item);
                    break;
                case SettingItemType.Button:
                    if (GUILayout.Button(item.Label,
                            item.Risk == SettingRiskLevel.Danger ? _theme.ButtonPrimary : _theme.Button,
                            GUILayout.Width(200), GUILayout.Height(32)))
                    {
                        RequestButton(item);
                    }

                    break;
                case SettingItemType.Info:
                    GUILayout.Label(item.Description, _theme.Desc);
                    break;
            }

            GUI.enabled = true;
            DrawBadges(item);
            GUILayout.EndHorizontal();

            if (item.Type == SettingItemType.Enum)
            {
                DrawEnumPopup(item);
            }

            if (item.Type != SettingItemType.Info && !string.IsNullOrEmpty(item.Description))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.Label(item.Description, _theme.Desc);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawBadges(SettingItemDef item)
        {
            if (item.Type == SettingItemType.Button || item.Type == SettingItemType.Info)
            {
                if (item.Risk == SettingRiskLevel.Danger)
                {
                    GUILayout.Box("⚠ 高危", _theme.BadgeDanger, GUILayout.Width(64));
                }

                return;
            }

            if (item.ApplyMode == SettingApplyMode.Restart)
            {
                GUILayout.Box("需重启", _theme.BadgeRestart, GUILayout.Width(64));
            }
            else
            {
                GUILayout.Box("立即生效", _theme.BadgeImmediate, GUILayout.Width(76));
            }

            if (item.Risk == SettingRiskLevel.Danger)
            {
                GUILayout.Box("⚠ 高危", _theme.BadgeDanger, GUILayout.Width(64));
            }
            else if (item.Risk == SettingRiskLevel.Advanced)
            {
                GUILayout.Box("高级", _theme.BadgeRestart, GUILayout.Width(52));
            }
        }

        private void DrawBool(SettingItemDef item)
        {
            bool v = _manager.GetValue(item.Key).AsBool();
            if (DrawSwitch(v, v ? "开启" : "关闭"))
            {
                _manager.SetPending(item.Key, SettingValue.FromBool(!v));
            }
        }

        private bool DrawSwitch(bool on, string label)
        {
            GUIStyle style = on ? _theme.ToggleOn : _theme.ToggleOff;
            return GUILayout.Button(label, style);
        }

        private const float SliderRowH = 32f;
        private const float SliderThumbW = 16f;
        private const float SliderThumbH = 28f;
        private const float SliderTrackH = 4f;
        private const int SliderHoldId = 0x4E6B0001;
        private string _activeSliderKey;

        /// <summary>
        /// 只认设置项 key，不认 IMGUI 顺序编号。按下之后整次拖拽都留给这一条。
        /// </summary>
        private float DrawAlignedSlider(string key, float value, float min, float max)
        {
            Rect row = GUILayoutUtility.GetRect(220f, SliderRowH, GUILayout.MinWidth(220f),
                GUILayout.Height(SliderRowH), GUILayout.ExpandWidth(true));
            Rect track = new Rect(row.x, row.y + (row.height - SliderTrackH) * 0.5f, row.width, SliderTrackH);
            float span = Mathf.Max(0.0001f, max - min);
            float usable = Mathf.Max(1f, track.width - SliderThumbW);
            float shown = value;
            Event e = Event.current;
            bool mine = _activeSliderKey == key;
            if (mine && e.type == EventType.MouseDrag)
            {
                shown = SliderValueAt(e.mousePosition.x, track, min, max);
            }

            float t = Mathf.Clamp01((shown - min) / span);
            Rect thumb = new Rect(track.x + t * usable, row.y + (row.height - SliderThumbH) * 0.5f,
                SliderThumbW, SliderThumbH);

            if (GUI.enabled && e.type == EventType.MouseDown && e.button == 0 &&
                string.IsNullOrEmpty(_activeSliderKey) && row.Contains(e.mousePosition))
            {
                _activeSliderKey = key;
                GUIUtility.hotControl = SliderHoldId;
                shown = SliderValueAt(e.mousePosition.x, track, min, max);
                e.Use();
            }
            else if (mine && e.type == EventType.MouseDrag)
            {
                GUIUtility.hotControl = SliderHoldId;
                e.Use();
            }
            else if (mine && e.rawType == EventType.MouseUp)
            {
                _activeSliderKey = null;
                if (GUIUtility.hotControl == SliderHoldId)
                {
                    GUIUtility.hotControl = 0;
                }

                e.Use();
            }

            if (e.type == EventType.Repaint)
            {
                if (_theme.Slider.normal.background != null)
                {
                    GUI.DrawTexture(track, _theme.Slider.normal.background);
                }

                if (_theme.SliderThumb.normal.background != null)
                {
                    GUI.DrawTexture(thumb, _theme.SliderThumb.normal.background);
                }
            }

            return shown;
        }

        private static float SliderValueAt(float mouseX, Rect track, float min, float max)
        {
            float usable = Mathf.Max(1f, track.width - SliderThumbW);
            float t = Mathf.Clamp01((mouseX - track.x - SliderThumbW * 0.5f) / usable);
            return Mathf.Lerp(min, max, t);
        }

        private void DrawInt(SettingItemDef item)
        {
            int v = _manager.GetValue(item.Key).AsInt();
            GUILayout.Label(CachedIntText(item.Key, v, item.Unit), _theme.ValueLabel, GUILayout.Width(88),
                GUILayout.Height(SliderRowH));
            float fv = DrawAlignedSlider(item.Key, v, item.Min, item.Max);
            int nv = Mathf.RoundToInt(fv);
            if (item.Step > 0.999f)
            {
                int step = Mathf.Max(1, Mathf.RoundToInt(item.Step));
                nv = Mathf.RoundToInt((float)nv / step) * step;
            }

            nv = Mathf.Clamp(nv, Mathf.RoundToInt(item.Min), Mathf.RoundToInt(item.Max));
            if (nv != v)
            {
                _manager.SetPending(item.Key, SettingValue.FromInt(nv));
            }
        }

        private void DrawFloat(SettingItemDef item)
        {
            float v = _manager.GetValue(item.Key).AsFloat();
            GUILayout.Label(CachedFloatText(item.Key, v, item.Unit), _theme.ValueLabel, GUILayout.Width(88),
                GUILayout.Height(SliderRowH));
            float nv = DrawAlignedSlider(item.Key, v, item.Min, item.Max);
            if (item.Step > 0.0001f)
            {
                nv = Mathf.Round(nv / item.Step) * item.Step;
            }

            nv = Mathf.Clamp(nv, item.Min, item.Max);
            if (Mathf.Abs(nv - v) > 0.0001f)
            {
                _manager.SetPending(item.Key, SettingValue.FromFloat(nv));
            }
        }

        private void DrawString(SettingItemDef item)
        {
            string v = _manager.GetValue(item.Key).AsString();
            string nv = GUILayout.TextField(v, _theme.TextField, GUILayout.MinWidth(240));
            if (nv != v)
            {
                _manager.SetPending(item.Key, SettingValue.FromString(nv));
            }
        }

        private void DrawEnum(SettingItemDef item)
        {
            string current = _manager.GetValue(item.Key).AsString();
            string label = current;
            int idx = FindOptionIndex(item, current);
            if (idx >= 0 && idx < item.Options.Count)
            {
                label = item.Options[idx].Label;
            }

            Rect slot = GUILayoutUtility.GetRect(220f, 32f, _theme.Button, GUILayout.MinWidth(220f),
                GUILayout.Height(32f), GUILayout.ExpandWidth(true));
            if (GUI.Button(slot, CachedEnumText(item.Key, label), _theme.Button))
            {
                _enumPopupKey = _enumPopupKey == item.Key ? null : item.Key;
            }

            if (_enumPopupKey == item.Key && Event.current.type == EventType.Repaint)
            {
                _enumWidthMeasured = slot.width;
            }
        }

        /// <summary>
        /// 列表跟在完整的一行后面。宽度用上一帧按钮的宽度，Layout 和 Repaint 一致，避免列表缩成文字宽。
        /// </summary>
        private void DrawEnumPopup(SettingItemDef item)
        {
            if (_enumPopupKey != item.Key || item.Options == null)
            {
                return;
            }

            EnsurePopupStyles();
            float w = _enumWidth > 8f ? _enumWidth : 280f;
            string current = _manager.GetValue(item.Key).AsString();
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            GUILayout.BeginVertical(_theme.PopupBox, GUILayout.Width(w));
            for (int i = 0; i < item.Options.Count; i++)
            {
                SettingEnumOption opt = item.Options[i];
                bool selected = opt.Value == current;
                if (GUILayout.Button(opt.Label, selected ? _popupOptOn : _popupOpt, GUILayout.ExpandWidth(true)))
                {
                    _manager.SetPending(item.Key, SettingValue.FromString(opt.Value));
                    _enumPopupKey = null;
                }
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void EnsurePopupStyles()
        {
            if (_popupOpt != null)
            {
                return;
            }

            _popupOpt = new GUIStyle(_theme.TabNormal);
            _popupOpt.stretchWidth = true;
            _popupOpt.alignment = TextAnchor.MiddleCenter;
            _popupOpt.margin = new RectOffset(0, 0, 1, 1);
            _popupOptOn = new GUIStyle(_theme.TabActive);
            _popupOptOn.stretchWidth = true;
            _popupOptOn.alignment = TextAnchor.MiddleCenter;
            _popupOptOn.margin = new RectOffset(0, 0, 1, 1);
        }

        private static int FindOptionIndex(SettingItemDef item, string value)
        {
            if (item.Options == null)
            {
                return -1;
            }

            for (int i = 0; i < item.Options.Count; i++)
            {
                if (item.Options[i].Value == value)
                {
                    return i;
                }
            }

            return -1;
        }

        private void DrawFooter()
        {
            int dirty = _manager.GetDirtyCount();
            int restart = _manager.GetPendingRestartCount();
            string status = FooterStatus(dirty, restart);

            GUILayout.BeginHorizontal(_theme.StatusBar);
            GUILayout.Label(status, _theme.Desc, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("恢复默认", _theme.Button, GUILayout.Width(100)))
            {
                _manager.ResetToDefaults();
            }

            if (_showRestartBanner || _manager.HasPendingRestart)
            {
                if (GUILayout.Button("立即重启", _theme.ButtonPrimary, GUILayout.Width(100)))
                {
                    _manager.HandleButton("restartApplication");
                }
            }

            GUI.enabled = dirty > 0;
            if (GUILayout.Button("应用", _theme.Button, GUILayout.Width(88)))
            {
                RequestApply(false);
            }

            if (GUILayout.Button("确定", _theme.ButtonPrimary, GUILayout.Width(88)))
            {
                RequestApply(true);
            }

            GUI.enabled = true;
            if (GUILayout.Button("取消", _theme.Button, GUILayout.Width(88)))
            {
                _manager.CancelPending();
                _visible = false;
            }

            GUILayout.EndHorizontal();
        }

        private void RequestApply(bool closeAfter)
        {
            List<SettingItemDef> dangers = _manager.CollectDangerPending();
            if (dangers.Count > 0)
            {
                _confirmItems = dangers;
                _confirmMessage = SettingPanelQuery.DangerMessage(dangers);
                _confirmAction = delegate
                {
                    _manager.ApplyPending(true);
                    if (closeAfter)
                    {
                        _visible = false;
                    }
                };
                _confirmOpen = true;
                return;
            }

            _manager.ApplyPending(true);
            if (closeAfter)
            {
                _visible = false;
            }
        }

        private void RequestButton(SettingItemDef item)
        {
            if (item.Risk == SettingRiskLevel.Danger || !string.IsNullOrEmpty(item.ConfirmMessage))
            {
                _confirmItems = new List<SettingItemDef> { item };
                _confirmMessage = string.IsNullOrEmpty(item.ConfirmMessage)
                    ? "确认执行「" + item.Label + "」？"
                    : item.ConfirmMessage;
                string action = item.ButtonAction;
                _confirmAction = delegate { _manager.HandleButton(action); };
                _confirmOpen = true;
                return;
            }

            _manager.HandleButton(item.ButtonAction);
        }

        private void DrawConfirmModal()
        {
            float w = 520f;
            float h = 260f;
            Rect r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.ModalWindow(_windowId + 99, r, DrawConfirmWindow, "确认", _theme.Window);
        }

        private void DrawConfirmWindow(int id)
        {
            GUILayout.Space(8);
            GUILayout.Label(_confirmMessage ?? "确认执行此操作？", _theme.Desc);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("取消", _theme.Button, GUILayout.Width(100)))
            {
                _confirmOpen = false;
                _confirmAction = null;
            }

            GUILayout.Space(8);
            if (GUILayout.Button("确认", _theme.ButtonPrimary, GUILayout.Width(100)))
            {
                System.Action a = _confirmAction;
                _confirmOpen = false;
                _confirmAction = null;
                if (a != null)
                {
                    a();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void EnsureDefaultSelection()
        {
            if (_manager == null || _manager.Schema == null)
            {
                return;
            }

            if (!_categoryInitialized)
            {
                // 有项目页时默认进「项目设置」，突出项目配置
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
            _contentScroll = Vector2.zero;
            _navScroll = Vector2.zero;
            _enumPopupKey = null;
            List<SettingTabDef> tabs = GetTabsInCategory(kind);
            _selectedTabId = tabs.Count > 0 ? tabs[0].Id : null;
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

        private string NavLabel(SettingTabDef tab)
        {
            CachedLabel slot = Slot(_navLabels, tab.Id);
            if (slot.Text != null && slot.IntValue == (int)_category && slot.Source == tab.Label)
            {
                return slot.Text;
            }

            slot.IntValue = (int)_category;
            slot.Source = tab.Label;
            slot.Text = (_category == SettingTabKind.Project ? "◆ " : "• ") + tab.Label;
            return slot.Text;
        }

        private string Breadcrumb(SettingTabDef tab)
        {
            if (_crumbText != null && _crumbKind == _category && _crumbTabId == tab.Id && _crumbLabel == tab.Label)
            {
                return _crumbText;
            }

            _crumbKind = _category;
            _crumbTabId = tab.Id;
            _crumbLabel = tab.Label;
            _crumbText = SettingPanelQuery.CategoryTitle(_category) + "  /  " + tab.Label;
            return _crumbText;
        }

        private string FooterStatus(int dirty, int restart)
        {
            if (_statusText != null && _statusDirty == dirty && _statusRestart == restart)
            {
                return _statusText;
            }

            _statusDirty = dirty;
            _statusRestart = restart;
            if (dirty <= 0)
            {
                _statusText = "无未保存的修改";
            }
            else if (restart > 0)
            {
                _statusText = dirty + " 项未应用  ·  " + restart + " 项需重启生效";
            }
            else
            {
                _statusText = dirty + " 项未应用";
            }

            return _statusText;
        }

        private string CachedIntText(string key, int value, string unit)
        {
            CachedLabel slot = Slot(_intLabels, key);
            if (slot.Text != null && slot.IntValue == value && slot.Source == unit)
            {
                return slot.Text;
            }

            slot.IntValue = value;
            slot.Source = unit;
            slot.Text = string.IsNullOrEmpty(unit) ? value.ToString() : value.ToString() + " " + unit;
            return slot.Text;
        }

        private string CachedFloatText(string key, float value, string unit)
        {
            CachedLabel slot = Slot(_floatLabels, key);
            if (slot.Text != null && slot.FloatValue == value && slot.Source == unit)
            {
                return slot.Text;
            }

            slot.FloatValue = value;
            slot.Source = unit;
            string number = value.ToString("0.###");
            slot.Text = string.IsNullOrEmpty(unit) ? number : number + " " + unit;
            return slot.Text;
        }

        private string CachedEnumText(string key, string label)
        {
            CachedLabel slot = Slot(_enumLabels, key);
            if (slot.Text != null && slot.Source == label)
            {
                return slot.Text;
            }

            slot.Source = label;
            slot.Text = label + "   ▾";
            return slot.Text;
        }

        private static CachedLabel Slot(Dictionary<string, CachedLabel> cache, string key)
        {
            if (key == null)
            {
                key = string.Empty;
            }

            CachedLabel slot;
            if (!cache.TryGetValue(key, out slot))
            {
                slot = new CachedLabel();
                cache[key] = slot;
            }

            return slot;
        }

        sealed class CachedLabel
        {
            public int IntValue;
            public float FloatValue;
            public string Source;
            public string Text;
        }
    }
}
