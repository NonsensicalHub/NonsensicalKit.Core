using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NonsensicalKit.Core.Service.Setting;
using NonsensicalKit.Setting;
using NonsensicalKit.Setting.Json;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Setting.Editor
{
    /// <summary>
    /// 可视化编辑 SettingObject 上的 v2 设置 JSON：系统开关、页签、分组、设置项。
    /// </summary>
    public class SettingJsonEditorWindow : EditorWindow
    {
        const string SessionKey = "NK.SettingJsonEditor.Asset";

        static readonly string[] TypeLabels = { "布尔", "整数", "小数", "文本", "枚举", "按钮", "说明" };
        static readonly string[] ApplyLabels = { "立即生效", "需重启" };
        static readonly string[] RiskLabels = { "普通", "高级", "高危" };
        static readonly string[] KindLabels = { "系统", "项目", "高级", "诊断" };
        static readonly string[] KnownInclude = { "display", "quality", "audio", "performance" };
        static readonly string[] KnownIncludeLabels = { "显示", "画质", "音频", "性能" };

        const string VisibleIfHelp =
            "有一项布尔设置 project.shadow（开启阴影）。另一项「阴影距离」的显示 key 填 project.shadow，显示值填 true。阴影关掉时，阴影距离整行消失；勾上之后才出现。如果改成填在可用条件里，阴影距离会一直显示，只是在阴影关掉时不能拖。";

        enum NodeKind
        {
            System,
            Panel,
            Tab,
            Group,
            Item
        }

        SettingObject _asset;
        bool _parseBlocked;
        string _parseError;
        string _rawJson = string.Empty;
        string _search = string.Empty;
        Vector2 _leftScroll;
        Vector2 _rightScroll;

        int _version = 2;
        bool _upgradedFromArray;
        bool _systemEnabled = true;
        bool _systemAdvanced = true;
        readonly List<string> _include = new List<string>();
        readonly List<string> _hide = new List<string>();
        readonly List<TabEdit> _tabs = new List<TabEdit>();

        bool _usePanel = true;
        KeyCode _toggleKey = KeyCode.F1;
        float _winX = 80f;
        float _winY = 60f;
        float _winW = 920f;
        float _winH = 620f;
        bool _usePercentage = true;
        float _heightPct = 0.82f;
        bool _showAdvancedByDefault;
        bool _showPreview;

        NodeKind _kind = NodeKind.System;
        int _ti;
        int _gi;
        int _ii;
        string _extraInclude = string.Empty;

        [MenuItem("Tools/NonsensicalKit/设置 JSON 编辑器")]
        public static void OpenMenu()
        {
            SettingObject selected = Selection.activeObject as SettingObject;
            Open(selected);
        }

        public static void Open(SettingObject asset)
        {
            SettingJsonEditorWindow window = GetWindow<SettingJsonEditorWindow>("设置配置");
            window.minSize = new Vector2(980f, 640f);
            if (asset != null)
            {
                window.TrySetAsset(asset, true);
            }
        }

        void OnEnable()
        {
            saveChangesMessage = "设置 JSON 有未保存的修改，关闭前要写回资源吗？";
            if (_asset == null)
            {
                string guid = SessionState.GetString(SessionKey, string.Empty);
                if (!string.IsNullOrEmpty(guid))
                {
                    _asset = AssetDatabase.LoadAssetAtPath<SettingObject>(AssetDatabase.GUIDToAssetPath(guid));
                }
            }

            if (_asset == null)
            {
                _asset = FindDefaultAsset();
            }

            if (_asset != null && _tabs.Count == 0 && string.IsNullOrEmpty(_parseError))
            {
                Load();
            }
        }

        public override void SaveChanges()
        {
            if (!WriteToAsset())
            {
                return;
            }

            base.SaveChanges();
        }

        void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            DrawTree();
            DrawDetail();
            EditorGUILayout.EndHorizontal();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            SettingObject next = (SettingObject)EditorGUILayout.ObjectField(
                _asset, typeof(SettingObject), false, GUILayout.MinWidth(260f));
            if (EditorGUI.EndChangeCheck())
            {
                TrySetAsset(next, true);
            }

            using (new EditorGUI.DisabledScope(_asset == null || _parseBlocked))
            {
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                {
                    SaveChanges();
                }
            }

            using (new EditorGUI.DisabledScope(_asset == null))
            {
                if (GUILayout.Button("还原", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                {
                    if (!hasUnsavedChanges || EditorUtility.DisplayDialog("还原", "放弃当前修改并重新读取资源？", "还原", "取消"))
                    {
                        Load();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            string state = hasUnsavedChanges ? "未保存" : "已与资源一致";
            GUILayout.Label(state, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            if (_asset != null)
            {
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(_asset), EditorStyles.miniLabel);
            }

            if (!string.IsNullOrEmpty(_parseError))
            {
                EditorGUILayout.HelpBox(_parseError, MessageType.Error);
            }
            else if (_upgradedFromArray)
            {
                EditorGUILayout.HelpBox("这份配置是旧版数组。保存后会写成 version 2（页签 / 分组）。", MessageType.Info);
            }

            if (EditorApplication.isPlaying && _asset != null)
            {
                EditorGUILayout.HelpBox("正在播放。保存只改资源，重新进入播放模式后运行中的设置页才会换上新配置。", MessageType.None);
            }
        }

        void DrawTree()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300f));
            _search = EditorGUILayout.TextField(_search ?? string.Empty, EditorStyles.toolbarSearchField);
            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, EditorStyles.helpBox);
            if (Node("系统页开关", 0, _kind == NodeKind.System))
            {
                _kind = NodeKind.System;
            }

            if (Node("面板", 0, _kind == NodeKind.Panel))
            {
                _kind = NodeKind.Panel;
            }

            string query = string.IsNullOrEmpty(_search) ? string.Empty : _search.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                DrawTreeFull();
            }
            else
            {
                DrawTreeSearch(query);
            }

            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("添加页签"))
            {
                AddTab();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawTreeFull()
        {
            for (int t = 0; t < _tabs.Count; t++)
            {
                TabEdit tab = _tabs[t];
                string tabTitle = (string.IsNullOrEmpty(tab.Label) ? tab.Id : tab.Label) + "  ·  " + KindLabels[(int)tab.Kind];
                if (Node(tabTitle, 0, _kind == NodeKind.Tab && _ti == t))
                {
                    _kind = NodeKind.Tab;
                    _ti = t;
                }

                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    GroupEdit group = tab.Groups[g];
                    string groupTitle = string.IsNullOrEmpty(group.Label) ? group.Id : group.Label;
                    if (Node(groupTitle, 1, _kind == NodeKind.Group && _ti == t && _gi == g))
                    {
                        _kind = NodeKind.Group;
                        _ti = t;
                        _gi = g;
                    }

                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        ItemEdit item = group.Items[i];
                        string itemTitle = ItemTitle(item);
                        if (Node(itemTitle, 2, _kind == NodeKind.Item && _ti == t && _gi == g && _ii == i))
                        {
                            _kind = NodeKind.Item;
                            _ti = t;
                            _gi = g;
                            _ii = i;
                        }
                    }
                }
            }
        }

        void DrawTreeSearch(string query)
        {
            for (int t = 0; t < _tabs.Count; t++)
            {
                TabEdit tab = _tabs[t];
                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    GroupEdit group = tab.Groups[g];
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        ItemEdit item = group.Items[i];
                        string blob = (item.Label + " " + item.Key + " " + group.Label + " " + tab.Label).ToLowerInvariant();
                        if (blob.IndexOf(query, StringComparison.Ordinal) < 0)
                        {
                            continue;
                        }

                        string title = ItemTitle(item) + "    " + tab.Label + " / " + group.Label;
                        if (Node(title, 0, _kind == NodeKind.Item && _ti == t && _gi == g && _ii == i))
                        {
                            _kind = NodeKind.Item;
                            _ti = t;
                            _gi = g;
                            _ii = i;
                        }
                    }
                }
            }
        }

        void DrawDetail()
        {
            EditorGUILayout.BeginVertical();
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
            List<string> issues = CollectIssues();
            for (int i = 0; i < issues.Count; i++)
            {
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
            }

            switch (_kind)
            {
                case NodeKind.System:
                    DrawSystem();
                    break;
                case NodeKind.Panel:
                    DrawPanel();
                    break;
                case NodeKind.Tab:
                    DrawTab();
                    break;
                case NodeKind.Group:
                    DrawGroup();
                    break;
                case NodeKind.Item:
                    DrawItem();
                    break;
            }

            EditorGUILayout.Space(8f);
            _showPreview = EditorGUILayout.Foldout(_showPreview, "保存时写入的 JSON");
            if (_showPreview && !_parseBlocked)
            {
                string preview = SettingJsonText.Build(CaptureDoc());
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(preview, GUILayout.MinHeight(160f));
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("复制 JSON", GUILayout.Width(100f)))
                {
                    EditorGUIUtility.systemCopyBuffer = preview;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawSystem()
        {
            EditorGUILayout.LabelField("系统页", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "显示、画质、音频、性能这些页由程序生成，不用在 JSON 里逐项填写。留空表示全部启用。高级页和诊断页不受这个列表限制，高级页看下面的开关。",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _systemEnabled = EditorGUILayout.Toggle("启用系统设置", _systemEnabled);
            _systemAdvanced = EditorGUILayout.Toggle("启用高级页", _systemAdvanced);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("启用的系统分类", EditorStyles.boldLabel);
            for (int i = 0; i < KnownInclude.Length; i++)
            {
                bool on = _include.Contains(KnownInclude[i]);
                bool next = EditorGUILayout.ToggleLeft(KnownIncludeLabels[i] + "    " + KnownInclude[i], on);
                if (next == on)
                {
                    continue;
                }

                if (next)
                {
                    _include.Add(KnownInclude[i]);
                }
                else
                {
                    _include.Remove(KnownInclude[i]);
                }

                MarkDirty();
            }

            EditorGUILayout.BeginHorizontal();
            _extraInclude = EditorGUILayout.TextField("其他分类 id", _extraInclude ?? string.Empty);
            if (GUILayout.Button("加入", GUILayout.Width(48f)) && !string.IsNullOrEmpty(_extraInclude))
            {
                string id = _extraInclude.Trim();
                if (!_include.Contains(id))
                {
                    _include.Add(id);
                    MarkDirty();
                }

                _extraInclude = string.Empty;
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();

            int removeInclude = -1;
            for (int i = 0; i < _include.Count; i++)
            {
                bool known = false;
                for (int k = 0; k < KnownInclude.Length; k++)
                {
                    if (KnownInclude[k] == _include[i])
                    {
                        known = true;
                        break;
                    }
                }

                if (known)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_include[i]);
                if (GUILayout.Button("移除", GUILayout.Width(48f)))
                {
                    removeInclude = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (removeInclude >= 0)
            {
                _include.RemoveAt(removeInclude);
                MarkDirty();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("隐藏的设置项", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("填写设置项 key，运行时不显示。例如 quality.shadowDistance", EditorStyles.wordWrappedMiniLabel);
            DrawStringList(_hide, "隐藏 key");
        }

        void DrawPanel()
        {
            EditorGUILayout.LabelField("面板", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("这部分存在资源上，不进 JSON。控制运行时设置窗口怎么打开、开多大。", MessageType.None);
            EditorGUI.BeginChangeCheck();
            _usePanel = EditorGUILayout.Toggle("创建运行时面板", _usePanel);
            _toggleKey = (KeyCode)EditorGUILayout.EnumPopup("开关快捷键", _toggleKey);
            _winX = EditorGUILayout.FloatField("窗口 X", _winX);
            _winY = EditorGUILayout.FloatField("窗口 Y", _winY);
            _winW = EditorGUILayout.FloatField("窗口宽", _winW);
            _winH = EditorGUILayout.FloatField("窗口高", _winH);
            _usePercentage = EditorGUILayout.Toggle("高度按屏幕比例", _usePercentage);
            if (_usePercentage)
            {
                _heightPct = EditorGUILayout.Slider("高度比例", _heightPct, 0.4f, 1f);
            }

            _showAdvancedByDefault = EditorGUILayout.Toggle("默认展开高级", _showAdvancedByDefault);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }
        }

        void DrawTab()
        {
            TabEdit tab = CurrentTab();
            if (tab == null)
            {
                EditorGUILayout.LabelField("这个页签已经不在了。");
                return;
            }

            DrawMoveBar("页签", () => MoveTab(-1), () => MoveTab(1), ConfirmDeleteTab);
            EditorGUI.BeginChangeCheck();
            tab.Id = EditorGUILayout.TextField("Id", tab.Id ?? string.Empty);
            tab.Label = EditorGUILayout.TextField("名称", tab.Label ?? string.Empty);
            tab.Order = EditorGUILayout.IntField("排序", tab.Order);
            tab.Kind = (SettingTabKind)EditorGUILayout.Popup("类别", (int)tab.Kind, KindLabels);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            if (GUILayout.Button("添加分组", GUILayout.Width(100f)))
            {
                AddGroup();
            }
        }

        void DrawGroup()
        {
            GroupEdit group = CurrentGroup();
            if (group == null)
            {
                EditorGUILayout.LabelField("这个分组已经不在了。");
                return;
            }

            DrawMoveBar("分组", () => MoveGroup(-1), () => MoveGroup(1), ConfirmDeleteGroup);
            EditorGUI.BeginChangeCheck();
            group.Id = EditorGUILayout.TextField("Id", group.Id ?? string.Empty);
            group.Label = EditorGUILayout.TextField("名称", group.Label ?? string.Empty);
            group.Description = EditorGUILayout.TextField("说明", group.Description ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            if (GUILayout.Button("添加设置项", GUILayout.Width(100f)))
            {
                AddItem();
            }
        }

        void DrawItem()
        {
            ItemEdit item = CurrentItem();
            if (item == null)
            {
                EditorGUILayout.LabelField("这个设置项已经不在了。");
                return;
            }

            DrawMoveBar("设置项", () => MoveItem(-1), () => MoveItem(1), DeleteItem);
            EditorGUI.BeginChangeCheck();
            item.Type = (SettingItemType)EditorGUILayout.Popup("类型", (int)item.Type, TypeLabels);
            if (item.Type != SettingItemType.Info)
            {
                item.Key = EditorGUILayout.TextField("Key", item.Key ?? string.Empty);
            }

            item.Label = EditorGUILayout.TextField("名称", item.Label ?? string.Empty);
            item.Description = EditorGUILayout.TextField("说明", item.Description ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            DrawItemTypeFields(item);
            EditorGUILayout.Space(6f);
            EditorGUI.BeginChangeCheck();
            if (item.Type != SettingItemType.Info && item.Type != SettingItemType.Button)
            {
                item.ApplyMode = (SettingApplyMode)EditorGUILayout.Popup("生效方式", (int)item.ApplyMode, ApplyLabels);
            }

            item.Risk = (SettingRiskLevel)EditorGUILayout.Popup("风险", (int)item.Risk, RiskLabels);
            if (item.Type != SettingItemType.Info && item.Type != SettingItemType.Button)
            {
                item.ReadOnly = EditorGUILayout.Toggle("只读", item.ReadOnly);
            }

            if (item.Risk == SettingRiskLevel.Danger || item.Type == SettingItemType.Button ||
                !string.IsNullOrEmpty(item.ConfirmMessage))
            {
                item.ConfirmMessage = EditorGUILayout.TextField("确认文案", item.ConfirmMessage ?? string.Empty);
            }

            DrawTitleWithHelp("显示条件", VisibleIfHelp);
            EditorGUILayout.LabelField("左侧 key 的当前值等于右侧时才显示。右侧留空视为 true。", EditorStyles.wordWrappedMiniLabel);
            item.VisibleIfKey = EditorGUILayout.TextField("显示 key", item.VisibleIfKey ?? string.Empty);
            item.VisibleIfValue = EditorGUILayout.TextField("显示值", item.VisibleIfValue ?? string.Empty);
            EditorGUILayout.LabelField("可用条件", EditorStyles.boldLabel);
            item.EnabledIfKey = EditorGUILayout.TextField("可用 key", item.EnabledIfKey ?? string.Empty);
            item.EnabledIfValue = EditorGUILayout.TextField("可用值", item.EnabledIfValue ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }
        }

        static void DrawTitleWithHelp(string title, string help)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(title, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            GUIContent source = EditorGUIUtility.IconContent("_Help");
            GUIContent icon = source != null && source.image != null
                ? new GUIContent(source.image, help)
                : new GUIContent("?", help);
            GUILayout.Label(icon, GUILayout.Width(18f), GUILayout.Height(18f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DrawItemTypeFields(ItemEdit item)
        {
            EditorGUI.BeginChangeCheck();
            int intDefault = ParseInt(item.DefaultValue);
            float floatDefault = ParseFloat(item.DefaultValue);
            switch (item.Type)
            {
                case SettingItemType.Bool:
                    bool on = item.DefaultValue == "true" || item.DefaultValue == "1";
                    bool nextOn = EditorGUILayout.Toggle("默认", on);
                    if (EditorGUI.EndChangeCheck())
                    {
                        item.DefaultValue = nextOn ? "true" : "false";
                        MarkDirty();
                    }

                    return;
                case SettingItemType.Int:
                    intDefault = EditorGUILayout.IntField("默认", intDefault);
                    item.Min = EditorGUILayout.FloatField("最小", item.Min);
                    item.Max = EditorGUILayout.FloatField("最大", item.Max);
                    item.Step = EditorGUILayout.FloatField("步进", item.Step);
                    item.Unit = EditorGUILayout.TextField("单位", item.Unit ?? string.Empty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        item.DefaultValue = intDefault.ToString(CultureInfo.InvariantCulture);
                        MarkDirty();
                    }

                    return;
                case SettingItemType.Float:
                    floatDefault = EditorGUILayout.FloatField("默认", floatDefault);
                    item.Min = EditorGUILayout.FloatField("最小", item.Min);
                    item.Max = EditorGUILayout.FloatField("最大", item.Max);
                    item.Step = EditorGUILayout.FloatField("步进", item.Step);
                    item.Unit = EditorGUILayout.TextField("单位", item.Unit ?? string.Empty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        item.DefaultValue = floatDefault.ToString(CultureInfo.InvariantCulture);
                        MarkDirty();
                    }

                    return;
                case SettingItemType.String:
                    item.DefaultValue = EditorGUILayout.TextField("默认", item.DefaultValue ?? string.Empty);
                    item.Unit = EditorGUILayout.TextField("单位", item.Unit ?? string.Empty);
                    break;
                case SettingItemType.Enum:
                    item.DefaultValue = EditorGUILayout.TextField("默认值", item.DefaultValue ?? string.Empty);
                    DrawOptions(item);
                    break;
                case SettingItemType.Button:
                    item.ButtonAction = EditorGUILayout.TextField("按钮动作", item.ButtonAction ?? string.Empty);
                    EditorGUILayout.LabelField("运行时用这个动作名接收点击。Key 可与动作名相同。", EditorStyles.wordWrappedMiniLabel);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }
        }

        void DrawOptions(ItemEdit item)
        {
            EditorGUILayout.LabelField("选项", EditorStyles.boldLabel);
            int remove = -1;
            for (int i = 0; i < item.Options.Count; i++)
            {
                SettingEnumOption opt = item.Options[i];
                EditorGUILayout.BeginHorizontal();
                opt.Value = EditorGUILayout.TextField(opt.Value ?? string.Empty);
                opt.Label = EditorGUILayout.TextField(opt.Label ?? string.Empty);
                if (GUILayout.Button("删除", GUILayout.Width(48f)))
                {
                    remove = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (remove >= 0)
            {
                item.Options.RemoveAt(remove);
                MarkDirty();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("添加选项", GUILayout.Width(100f)))
            {
                item.Options.Add(new SettingEnumOption(string.Empty, string.Empty));
                MarkDirty();
            }
        }

        void DrawStringList(List<string> list, string fieldLabel)
        {
            int remove = -1;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                string next = EditorGUILayout.TextField(fieldLabel, list[i] ?? string.Empty);
                if (next != (list[i] ?? string.Empty))
                {
                    list[i] = next;
                    MarkDirty();
                }

                if (GUILayout.Button("删除", GUILayout.Width(48f)))
                {
                    remove = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (remove >= 0)
            {
                list.RemoveAt(remove);
                MarkDirty();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("添加一行", GUILayout.Width(100f)))
            {
                list.Add(string.Empty);
                MarkDirty();
            }
        }

        void DrawMoveBar(string title, Action up, Action down, Action remove)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("上移", GUILayout.Width(48f)))
            {
                up();
            }

            if (GUILayout.Button("下移", GUILayout.Width(48f)))
            {
                down();
            }

            if (GUILayout.Button("删除", GUILayout.Width(48f)))
            {
                remove();
            }

            EditorGUILayout.EndHorizontal();
        }

        void AddTab()
        {
            TabEdit tab = new TabEdit();
            tab.Id = Unique("project.page", TabIdExists);
            tab.Label = "新页面";
            tab.Order = 100;
            tab.Kind = SettingTabKind.Project;
            GroupEdit group = new GroupEdit();
            group.Id = "main";
            group.Label = "通用";
            tab.Groups.Add(group);
            _tabs.Add(tab);
            _kind = NodeKind.Tab;
            _ti = _tabs.Count - 1;
            MarkDirty();
        }

        void AddGroup()
        {
            TabEdit tab = CurrentTab();
            if (tab == null)
            {
                return;
            }

            GroupEdit group = new GroupEdit();
            group.Id = Unique("group", id => GroupIdExists(tab, id));
            group.Label = "新分组";
            tab.Groups.Add(group);
            _kind = NodeKind.Group;
            _gi = tab.Groups.Count - 1;
            MarkDirty();
        }

        void AddItem()
        {
            GroupEdit group = CurrentGroup();
            if (group == null && _kind == NodeKind.Item)
            {
                group = CurrentGroup();
            }

            if (group == null)
            {
                return;
            }

            ItemEdit item = new ItemEdit();
            item.Key = Unique("project.item", KeyExists);
            item.Label = "新设置项";
            item.Type = SettingItemType.Bool;
            item.DefaultValue = "false";
            item.Min = 0f;
            item.Max = 1f;
            int insert = _kind == NodeKind.Item ? _ii + 1 : group.Items.Count;
            group.Items.Insert(insert, item);
            _kind = NodeKind.Item;
            _ii = insert;
            MarkDirty();
        }

        void MoveTab(int dir)
        {
            int n = _ti + dir;
            if (n < 0 || n >= _tabs.Count)
            {
                return;
            }

            TabEdit tmp = _tabs[_ti];
            _tabs[_ti] = _tabs[n];
            _tabs[n] = tmp;
            _ti = n;
            MarkDirty();
        }

        void MoveGroup(int dir)
        {
            TabEdit tab = CurrentTab();
            if (tab == null)
            {
                return;
            }

            int n = _gi + dir;
            if (n < 0 || n >= tab.Groups.Count)
            {
                return;
            }

            GroupEdit tmp = tab.Groups[_gi];
            tab.Groups[_gi] = tab.Groups[n];
            tab.Groups[n] = tmp;
            _gi = n;
            MarkDirty();
        }

        void MoveItem(int dir)
        {
            GroupEdit group = CurrentGroup();
            if (group == null)
            {
                return;
            }

            int n = _ii + dir;
            if (n < 0 || n >= group.Items.Count)
            {
                return;
            }

            ItemEdit tmp = group.Items[_ii];
            group.Items[_ii] = group.Items[n];
            group.Items[n] = tmp;
            _ii = n;
            MarkDirty();
        }

        void ConfirmDeleteTab()
        {
            TabEdit tab = CurrentTab();
            if (tab == null)
            {
                return;
            }

            string name = string.IsNullOrEmpty(tab.Label) ? tab.Id : tab.Label;
            if (!EditorUtility.DisplayDialog("删除页签", "删除「" + name + "」以及里面的分组和设置项？", "删除", "取消"))
            {
                return;
            }

            _tabs.RemoveAt(_ti);
            _kind = NodeKind.System;
            MarkDirty();
        }

        void ConfirmDeleteGroup()
        {
            TabEdit tab = CurrentTab();
            GroupEdit group = CurrentGroup();
            if (tab == null || group == null)
            {
                return;
            }

            string name = string.IsNullOrEmpty(group.Label) ? group.Id : group.Label;
            if (!EditorUtility.DisplayDialog("删除分组", "删除「" + name + "」以及里面的设置项？", "删除", "取消"))
            {
                return;
            }

            tab.Groups.RemoveAt(_gi);
            _kind = NodeKind.Tab;
            MarkDirty();
        }

        void DeleteItem()
        {
            GroupEdit group = CurrentGroup();
            if (group == null || _ii < 0 || _ii >= group.Items.Count)
            {
                return;
            }

            group.Items.RemoveAt(_ii);
            if (group.Items.Count == 0)
            {
                _kind = NodeKind.Group;
            }
            else if (_ii >= group.Items.Count)
            {
                _ii = group.Items.Count - 1;
            }

            MarkDirty();
        }

        TabEdit CurrentTab()
        {
            if (_ti < 0 || _ti >= _tabs.Count)
            {
                return null;
            }

            return _tabs[_ti];
        }

        GroupEdit CurrentGroup()
        {
            TabEdit tab = CurrentTab();
            if (tab == null || _gi < 0 || _gi >= tab.Groups.Count)
            {
                return null;
            }

            return tab.Groups[_gi];
        }

        ItemEdit CurrentItem()
        {
            GroupEdit group = CurrentGroup();
            if (group == null || _ii < 0 || _ii >= group.Items.Count)
            {
                return null;
            }

            return group.Items[_ii];
        }

        void TrySetAsset(SettingObject next, bool prompt)
        {
            if (next == _asset)
            {
                return;
            }

            if (prompt && hasUnsavedChanges)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "设置配置未保存",
                    "当前修改还没写回资源。",
                    "保存",
                    "取消",
                    "不保存");
                if (choice == 1)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                if (choice == 0 && !WriteToAsset())
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            _asset = next;
            if (_asset != null)
            {
                SessionState.SetString(SessionKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_asset)));
                Load();
            }
            else
            {
                _tabs.Clear();
                hasUnsavedChanges = false;
            }

            GUIUtility.ExitGUI();
        }

        void Load()
        {
            _parseBlocked = false;
            _parseError = null;
            _upgradedFromArray = false;
            _tabs.Clear();
            _include.Clear();
            _hide.Clear();
            _version = 2;
            _systemEnabled = true;
            _systemAdvanced = true;
            if (_asset == null)
            {
                hasUnsavedChanges = false;
                return;
            }

            SerializedObject so = new SerializedObject(_asset);
            SerializedProperty jsonProp = so.FindProperty("m_config.m_SettingJson");
            SerializedProperty useProp = so.FindProperty("m_config.m_UseSettingPanel");
            SerializedProperty panel = so.FindProperty("m_config.m_Panel");
            string json = jsonProp != null ? jsonProp.stringValue : string.Empty;
            _rawJson = json ?? string.Empty;
            if (useProp != null)
            {
                _usePanel = useProp.boolValue;
            }

            ReadPanel(panel);

            if (!string.IsNullOrWhiteSpace(json))
            {
                object root = MiniJson.Deserialize(json);
                if (root == null)
                {
                    _parseBlocked = true;
                    _parseError = "JSON 无法解析，没有载入。请在资源里修正这段文本后再打开。";
                    hasUnsavedChanges = false;
                    return;
                }

                _upgradedFromArray = root is List<object>;
            }

            SettingSchema schema = SettingSchemaParser.Parse(json);
            _version = schema.Version <= 0 ? 2 : schema.Version;
            Dictionary<string, object> system = SettingSchemaParser.ParseSystemSection(json);
            if (system != null)
            {
                _systemEnabled = ReadBool(system, "enabled", true);
                _systemAdvanced = ReadBool(system, "advanced", true);
                ReadStringList(system, "include", _include);
                ReadStringList(system, "hide", _hide);
            }

            for (int t = 0; t < schema.Tabs.Count; t++)
            {
                _tabs.Add(TabEdit.From(schema.Tabs[t]));
            }

            _kind = NodeKind.System;
            hasUnsavedChanges = false;
        }

        bool WriteToAsset()
        {
            if (_asset == null)
            {
                EditorUtility.DisplayDialog("无法保存", "先指定一个 SettingConfig 资源。", "确定");
                return false;
            }

            if (_parseBlocked)
            {
                EditorUtility.DisplayDialog("无法保存", "当前 JSON 没解析成功，保存会把原内容盖掉。", "确定");
                return false;
            }

            SerializedObject so = new SerializedObject(_asset);
            SerializedProperty jsonProp = so.FindProperty("m_config.m_SettingJson");
            if (jsonProp == null)
            {
                EditorUtility.DisplayDialog("无法保存", "资源上找不到 m_SettingJson。", "确定");
                return false;
            }

            jsonProp.stringValue = SettingJsonText.Build(CaptureDoc());
            SerializedProperty useProp = so.FindProperty("m_config.m_UseSettingPanel");
            if (useProp != null)
            {
                useProp.boolValue = _usePanel;
            }

            WritePanel(so.FindProperty("m_config.m_Panel"));
            so.ApplyModifiedProperties();
            _upgradedFromArray = false;
            hasUnsavedChanges = false;
            return true;
        }

        Doc CaptureDoc()
        {
            Doc doc = new Doc();
            doc.Version = _version <= 0 ? 2 : _version;
            doc.SystemEnabled = _systemEnabled;
            doc.SystemAdvanced = _systemAdvanced;
            doc.Include = _include;
            doc.Hide = _hide;
            doc.Tabs = _tabs;
            return doc;
        }

        void ReadPanel(SerializedProperty panel)
        {
            if (panel == null)
            {
                return;
            }

            _toggleKey = (KeyCode)panel.FindPropertyRelative("m_ToggleKey").intValue;
            _winX = panel.FindPropertyRelative("m_X").floatValue;
            _winY = panel.FindPropertyRelative("m_Y").floatValue;
            _winW = panel.FindPropertyRelative("m_W").floatValue;
            _winH = panel.FindPropertyRelative("m_H").floatValue;
            _usePercentage = panel.FindPropertyRelative("m_UsePercentage").boolValue;
            _heightPct = panel.FindPropertyRelative("m_WindowHeightPercentage").floatValue;
            _showAdvancedByDefault = panel.FindPropertyRelative("m_ShowAdvancedByDefault").boolValue;
        }

        void WritePanel(SerializedProperty panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.FindPropertyRelative("m_ToggleKey").intValue = (int)_toggleKey;
            panel.FindPropertyRelative("m_X").floatValue = _winX;
            panel.FindPropertyRelative("m_Y").floatValue = _winY;
            panel.FindPropertyRelative("m_W").floatValue = _winW;
            panel.FindPropertyRelative("m_H").floatValue = _winH;
            panel.FindPropertyRelative("m_UsePercentage").boolValue = _usePercentage;
            panel.FindPropertyRelative("m_WindowHeightPercentage").floatValue = _heightPct;
            panel.FindPropertyRelative("m_ShowAdvancedByDefault").boolValue = _showAdvancedByDefault;
        }

        List<string> CollectIssues()
        {
            List<string> issues = new List<string>();
            HashSet<string> tabIds = new HashSet<string>();
            HashSet<string> keys = new HashSet<string>();
            for (int t = 0; t < _tabs.Count; t++)
            {
                TabEdit tab = _tabs[t];
                if (string.IsNullOrEmpty(tab.Id))
                {
                    issues.Add("有页签没有 Id。");
                }
                else if (!tabIds.Add(tab.Id))
                {
                    issues.Add("页签 Id 重复：" + tab.Id);
                }

                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    GroupEdit group = tab.Groups[g];
                    if (string.IsNullOrEmpty(group.Id))
                    {
                        issues.Add("页签「" + tab.Label + "」里有分组没有 Id。");
                    }

                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        ItemEdit item = group.Items[i];
                        if (item.Type != SettingItemType.Info && string.IsNullOrEmpty(item.Key))
                        {
                            issues.Add("「" + item.Label + "」没有 Key。");
                        }

                        if (!string.IsNullOrEmpty(item.Key) && !keys.Add(item.Key))
                        {
                            issues.Add("Key 重复：" + item.Key);
                        }

                        if (item.Type == SettingItemType.Enum && item.Options.Count == 0)
                        {
                            issues.Add("枚举「" + item.Label + "」还没有选项。");
                        }

                        if ((item.Type == SettingItemType.Int || item.Type == SettingItemType.Float) && item.Min > item.Max)
                        {
                            issues.Add("「" + item.Label + "」的最小值大于最大值。");
                        }
                    }
                }
            }

            return issues;
        }

        void MarkDirty()
        {
            hasUnsavedChanges = true;
        }

        static bool Node(string text, int indent, bool selected)
        {
            Rect row = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint && selected)
            {
                EditorGUI.DrawRect(row, new Color(0.18f, 0.42f, 0.78f, 1f));
            }

            GUIStyle style = selected ? EditorStyles.whiteLabel : EditorStyles.label;
            GUI.Label(new Rect(row.x + 6f + indent * 14f, row.y, row.width - 8f, row.height), text, style);
            return GUI.Button(row, GUIContent.none, GUIStyle.none);
        }

        static string ItemTitle(ItemEdit item)
        {
            string name = string.IsNullOrEmpty(item.Label) ? item.Key : item.Label;
            int typeIndex = (int)item.Type;
            string type = typeIndex >= 0 && typeIndex < TypeLabels.Length ? TypeLabels[typeIndex] : string.Empty;
            return type + "  " + name;
        }

        bool TabIdExists(string id)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        static bool GroupIdExists(TabEdit tab, string id)
        {
            for (int i = 0; i < tab.Groups.Count; i++)
            {
                if (tab.Groups[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        bool KeyExists(string key)
        {
            for (int t = 0; t < _tabs.Count; t++)
            {
                TabEdit tab = _tabs[t];
                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    GroupEdit group = tab.Groups[g];
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        if (group.Items[i].Key == key)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static string Unique(string prefix, Func<string, bool> exists)
        {
            if (!exists(prefix))
            {
                return prefix;
            }

            for (int i = 2; i < 100; i++)
            {
                string id = prefix + i;
                if (!exists(id))
                {
                    return id;
                }
            }

            return prefix + "x";
        }

        static SettingObject FindDefaultAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:SettingObject");
            SettingObject fallback = null;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]).Replace('\\', '/');
                SettingObject asset = AssetDatabase.LoadAssetAtPath<SettingObject>(path);
                if (asset == null || path.IndexOf("Samples~", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (path.EndsWith("Assets/Resources/SettingConfig.asset", StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }

                if (fallback == null)
                {
                    fallback = asset;
                }
            }

            return fallback;
        }

        static bool ReadBool(Dictionary<string, object> dict, string key, bool fallback)
        {
            object value;
            if (!dict.TryGetValue(key, out value) || value == null)
            {
                return fallback;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            bool parsed;
            return bool.TryParse(value.ToString(), out parsed) ? parsed : fallback;
        }

        static void ReadStringList(Dictionary<string, object> dict, string key, List<string> into)
        {
            object value;
            if (!dict.TryGetValue(key, out value))
            {
                return;
            }

            List<object> list = value as List<object>;
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    into.Add(list[i].ToString());
                }
            }
        }

        static int ParseInt(string text)
        {
            int value;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        static float ParseFloat(string text)
        {
            float value;
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0f;
        }

        sealed class Doc
        {
            public int Version = 2;
            public bool SystemEnabled = true;
            public bool SystemAdvanced = true;
            public List<string> Include;
            public List<string> Hide;
            public List<TabEdit> Tabs;
        }

        sealed class TabEdit
        {
            public string Id = string.Empty;
            public string Label = string.Empty;
            public int Order = 100;
            public SettingTabKind Kind = SettingTabKind.Project;
            public readonly List<GroupEdit> Groups = new List<GroupEdit>();

            public static TabEdit From(SettingTabDef source)
            {
                TabEdit tab = new TabEdit();
                tab.Id = source.Id ?? string.Empty;
                tab.Label = source.Label ?? string.Empty;
                tab.Order = source.Order;
                tab.Kind = source.Kind;
                if (source.Groups == null)
                {
                    return tab;
                }

                for (int i = 0; i < source.Groups.Count; i++)
                {
                    tab.Groups.Add(GroupEdit.From(source.Groups[i]));
                }

                return tab;
            }
        }

        sealed class GroupEdit
        {
            public string Id = string.Empty;
            public string Label = string.Empty;
            public string Description = string.Empty;
            public readonly List<ItemEdit> Items = new List<ItemEdit>();

            public static GroupEdit From(SettingGroupDef source)
            {
                GroupEdit group = new GroupEdit();
                group.Id = source.Id ?? string.Empty;
                group.Label = source.Label ?? string.Empty;
                group.Description = source.Description ?? string.Empty;
                if (source.Items == null)
                {
                    return group;
                }

                for (int i = 0; i < source.Items.Count; i++)
                {
                    group.Items.Add(ItemEdit.From(source.Items[i]));
                }

                return group;
            }
        }

        sealed class ItemEdit
        {
            public string Key = string.Empty;
            public SettingItemType Type = SettingItemType.String;
            public string Label = string.Empty;
            public string Description = string.Empty;
            public string DefaultValue = string.Empty;
            public float Min;
            public float Max = 1f;
            public float Step;
            public string Unit = string.Empty;
            public readonly List<SettingEnumOption> Options = new List<SettingEnumOption>();
            public string ButtonAction = string.Empty;
            public SettingApplyMode ApplyMode = SettingApplyMode.Immediate;
            public SettingRiskLevel Risk = SettingRiskLevel.Normal;
            public string ConfirmMessage = string.Empty;
            public string VisibleIfKey = string.Empty;
            public string VisibleIfValue = string.Empty;
            public string EnabledIfKey = string.Empty;
            public string EnabledIfValue = string.Empty;
            public bool ReadOnly;

            public static ItemEdit From(SettingItemDef source)
            {
                ItemEdit item = new ItemEdit();
                item.Key = source.Key ?? string.Empty;
                item.Type = source.Type;
                item.Label = source.Label ?? string.Empty;
                item.Description = source.Description ?? string.Empty;
                item.DefaultValue = source.DefaultValue ?? string.Empty;
                item.Min = source.Min;
                item.Max = source.Max;
                item.Step = source.Step;
                item.Unit = source.Unit ?? string.Empty;
                item.ButtonAction = source.ButtonAction ?? string.Empty;
                item.ApplyMode = source.ApplyMode;
                item.Risk = source.Risk;
                item.ConfirmMessage = source.ConfirmMessage ?? string.Empty;
                item.VisibleIfKey = source.VisibleIfKey ?? string.Empty;
                item.VisibleIfValue = source.VisibleIfValue ?? string.Empty;
                item.EnabledIfKey = source.EnabledIfKey ?? string.Empty;
                item.EnabledIfValue = source.EnabledIfValue ?? string.Empty;
                item.ReadOnly = source.ReadOnly;
                if (source.Options == null)
                {
                    return item;
                }

                for (int i = 0; i < source.Options.Count; i++)
                {
                    SettingEnumOption opt = source.Options[i];
                    item.Options.Add(new SettingEnumOption(opt.Value ?? string.Empty, opt.Label ?? string.Empty));
                }

                return item;
            }
        }

        static class SettingJsonText
        {
            public static string Build(Doc doc)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\n");
                sb.Append("  \"version\": ").Append(doc.Version <= 0 ? 2 : doc.Version).Append(",\n");
                sb.Append("  \"system\": {\n");
                sb.Append("    \"enabled\": ").Append(doc.SystemEnabled ? "true" : "false").Append(",\n");
                sb.Append("    \"advanced\": ").Append(doc.SystemAdvanced ? "true" : "false").Append(",\n");
                sb.Append("    \"include\": ").Append(StringArray(doc.Include)).Append(",\n");
                sb.Append("    \"hide\": ").Append(StringArray(doc.Hide)).Append("\n");
                sb.Append("  },\n");
                sb.Append("  \"tabs\": [\n");
                List<TabEdit> tabs = doc.Tabs ?? new List<TabEdit>();
                for (int t = 0; t < tabs.Count; t++)
                {
                    WriteTab(sb, tabs[t], t + 1 < tabs.Count);
                }

                sb.Append("  ]\n");
                sb.Append("}\n");
                return sb.ToString();
            }

            static void WriteTab(StringBuilder sb, TabEdit tab, bool comma)
            {
                sb.Append("    {\n");
                sb.Append("      \"id\": ").Append(Esc(tab.Id)).Append(",\n");
                sb.Append("      \"label\": ").Append(Esc(tab.Label)).Append(",\n");
                sb.Append("      \"order\": ").Append(tab.Order.ToString(CultureInfo.InvariantCulture)).Append(",\n");
                sb.Append("      \"kind\": ").Append(Esc(tab.Kind.ToString().ToLowerInvariant())).Append(",\n");
                sb.Append("      \"groups\": [\n");
                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    WriteGroup(sb, tab.Groups[g], g + 1 < tab.Groups.Count);
                }

                sb.Append("      ]\n");
                sb.Append("    }");
                if (comma)
                {
                    sb.Append(',');
                }

                sb.Append('\n');
            }

            static void WriteGroup(StringBuilder sb, GroupEdit group, bool comma)
            {
                sb.Append("        {\n");
                sb.Append("          \"id\": ").Append(Esc(group.Id)).Append(",\n");
                sb.Append("          \"label\": ").Append(Esc(group.Label)).Append(",\n");
                if (!string.IsNullOrEmpty(group.Description))
                {
                    sb.Append("          \"description\": ").Append(Esc(group.Description)).Append(",\n");
                }

                sb.Append("          \"items\": [\n");
                for (int i = 0; i < group.Items.Count; i++)
                {
                    WriteItem(sb, group.Items[i], i + 1 < group.Items.Count);
                }

                sb.Append("          ]\n");
                sb.Append("        }");
                if (comma)
                {
                    sb.Append(',');
                }

                sb.Append('\n');
            }

            static void WriteItem(StringBuilder sb, ItemEdit item, bool comma)
            {
                List<string> fields = new List<string>();
                if (item.Type != SettingItemType.Info || !string.IsNullOrEmpty(item.Key))
                {
                    fields.Add("\"key\": " + Esc(item.Key));
                }

                fields.Add("\"type\": " + Esc(TypeName(item.Type)));
                fields.Add("\"label\": " + Esc(item.Label));
                if (!string.IsNullOrEmpty(item.Description))
                {
                    fields.Add("\"description\": " + Esc(item.Description));
                }

                if (item.Type == SettingItemType.Bool || item.Type == SettingItemType.Int ||
                    item.Type == SettingItemType.Float || item.Type == SettingItemType.String ||
                    item.Type == SettingItemType.Enum)
                {
                    fields.Add("\"default\": " + FormatDefault(item));
                }

                if (item.Type == SettingItemType.Int || item.Type == SettingItemType.Float)
                {
                    fields.Add("\"min\": " + Num(item.Min));
                    fields.Add("\"max\": " + Num(item.Max));
                    if (Math.Abs(item.Step) > 0.0000001f)
                    {
                        fields.Add("\"step\": " + Num(item.Step));
                    }
                }

                if (!string.IsNullOrEmpty(item.Unit))
                {
                    fields.Add("\"unit\": " + Esc(item.Unit));
                }

                if (item.Type == SettingItemType.Enum)
                {
                    fields.Add(OptionsField(item.Options));
                }

                if (item.Type == SettingItemType.Button || !string.IsNullOrEmpty(item.ButtonAction))
                {
                    fields.Add("\"buttonAction\": " + Esc(item.ButtonAction));
                }

                if (item.ApplyMode == SettingApplyMode.Restart)
                {
                    fields.Add("\"apply\": \"restart\"");
                }

                if (item.Risk == SettingRiskLevel.Advanced)
                {
                    fields.Add("\"risk\": \"advanced\"");
                }
                else if (item.Risk == SettingRiskLevel.Danger)
                {
                    fields.Add("\"risk\": \"danger\"");
                }

                if (!string.IsNullOrEmpty(item.ConfirmMessage))
                {
                    fields.Add("\"confirm\": " + Esc(item.ConfirmMessage));
                }

                string visible = FormatCond(item.VisibleIfKey, item.VisibleIfValue);
                if (visible != null)
                {
                    fields.Add("\"visibleIf\": " + Esc(visible));
                }

                string enabled = FormatCond(item.EnabledIfKey, item.EnabledIfValue);
                if (enabled != null)
                {
                    fields.Add("\"enabledIf\": " + Esc(enabled));
                }

                if (item.ReadOnly)
                {
                    fields.Add("\"readOnly\": true");
                }

                sb.Append("            {\n");
                for (int i = 0; i < fields.Count; i++)
                {
                    sb.Append("              ").Append(fields[i]);
                    if (i + 1 < fields.Count)
                    {
                        sb.Append(',');
                    }

                    sb.Append('\n');
                }

                sb.Append("            }");
                if (comma)
                {
                    sb.Append(',');
                }

                sb.Append('\n');
            }

            static string OptionsField(List<SettingEnumOption> options)
            {
                if (options == null || options.Count == 0)
                {
                    return "\"options\": []";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("\"options\": [\n");
                for (int i = 0; i < options.Count; i++)
                {
                    SettingEnumOption opt = options[i];
                    sb.Append("                { \"value\": ").Append(Esc(opt.Value));
                    sb.Append(", \"label\": ").Append(Esc(opt.Label)).Append(" }");
                    if (i + 1 < options.Count)
                    {
                        sb.Append(',');
                    }

                    sb.Append('\n');
                }

                sb.Append("              ]");
                return sb.ToString();
            }

            static string FormatDefault(ItemEdit item)
            {
                if (item.Type == SettingItemType.Bool)
                {
                    bool on = item.DefaultValue == "true" || item.DefaultValue == "1";
                    return on ? "true" : "false";
                }

                if (item.Type == SettingItemType.Int)
                {
                    return ParseInt(item.DefaultValue).ToString(CultureInfo.InvariantCulture);
                }

                if (item.Type == SettingItemType.Float)
                {
                    return Num(ParseFloat(item.DefaultValue));
                }

                return Esc(item.DefaultValue);
            }

            static string FormatCond(string key, string value)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(value))
                {
                    return key;
                }

                return key + "=" + value;
            }

            static string StringArray(List<string> values)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('[');
                if (values != null)
                {
                    bool first = true;
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (string.IsNullOrEmpty(values[i]))
                        {
                            continue;
                        }

                        if (!first)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(Esc(values[i]));
                        first = false;
                    }
                }

                sb.Append(']');
                return sb.ToString();
            }

            static string TypeName(SettingItemType type)
            {
                switch (type)
                {
                    case SettingItemType.Bool:
                        return "bool";
                    case SettingItemType.Int:
                        return "int";
                    case SettingItemType.Float:
                        return "float";
                    case SettingItemType.Enum:
                        return "enum";
                    case SettingItemType.Button:
                        return "button";
                    case SettingItemType.Info:
                        return "info";
                    default:
                        return "string";
                }
            }

            static string Num(float value)
            {
                return value.ToString("0.###", CultureInfo.InvariantCulture);
            }

            static string Esc(string text)
            {
                if (text == null)
                {
                    text = string.Empty;
                }

                StringBuilder sb = new StringBuilder(text.Length + 2);
                sb.Append('"');
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    switch (c)
                    {
                        case '"':
                            sb.Append("\\\"");
                            break;
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        default:
                            if (c < 32)
                            {
                                sb.Append("\\u").Append(((int)c).ToString("x4"));
                            }
                            else
                            {
                                sb.Append(c);
                            }

                            break;
                    }
                }

                sb.Append('"');
                return sb.ToString();
            }
        }
    }
}
