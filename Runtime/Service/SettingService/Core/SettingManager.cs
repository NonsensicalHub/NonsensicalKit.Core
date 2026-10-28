using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NonsensicalKit.Setting.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 独立设置管理器。不依赖 NonsensicalKit.Core，可单独用于 Unity 2018.4+。
    /// </summary>
    public class SettingManager
    {
        public static SettingManager Instance { get; private set; }

        public bool IsReady { get; private set; }
        public SettingSchema Schema { get; private set; }
        public SettingPanelConfig PanelConfig { get; private set; }
        public bool ShowAdvanced { get; set; }
        public bool HasPendingRestart { get; private set; }

        public event Action OnReady;
        public event Action<string, SettingValue> OnApplied;
        public event Action OnRestartRequired;
        public event Action OnSchemaChanged; // 预留：运行时热重载 Schema 时触发

        private readonly SettingStore _store = new SettingStore();
        private readonly Dictionary<string, SettingValue> _committed = new Dictionary<string, SettingValue>();
        private readonly Dictionary<string, SettingValue> _pending = new Dictionary<string, SettingValue>();
        private readonly Dictionary<string, SettingValue> _systemDefaults = new Dictionary<string, SettingValue>();
        private readonly Dictionary<string, Action<SettingValue>> _appliers = new Dictionary<string, Action<SettingValue>>();
        private readonly Dictionary<string, List<Action<string, SettingValue>>> _listeners =
            new Dictionary<string, List<Action<string, SettingValue>>>();
        private readonly Dictionary<string, List<Action<string>>> _buttonListeners =
            new Dictionary<string, List<Action<string>>>();
        private readonly List<ISystemSettingProvider> _providers = new List<ISystemSettingProvider>();
        private readonly List<string> _pendingRestartKeys = new List<string>();
        private readonly HashSet<string> _hiddenKeys = new HashSet<string>();
        private readonly HashSet<string> _systemInclude = new HashSet<string>();
        private bool _systemEnabled = true;
        private bool _advancedEnabled = true;

        private GameObject _panelHost;
        private ISettingPanelView _panel;

        public SettingManager()
        {
            Instance = this;
            PanelConfig = new SettingPanelConfig();
        }

        public void RegisterProvider(ISystemSettingProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            _providers.Add(provider);
        }

        public void Initialize(string projectJson, SettingPanelConfig panelConfig)
        {
            Initialize(projectJson, panelConfig, true);
        }

        public void Initialize(string projectJson, SettingPanelConfig panelConfig, bool createPanel)
        {
            if (panelConfig != null)
            {
                PanelConfig = panelConfig;
            }

            ShowAdvanced = PanelConfig.ShowAdvancedByDefault;

            if (_providers.Count == 0)
            {
                SystemSettingRegistry.RegisterDefaults(this);
            }

            Dictionary<string, object> systemSection = SettingSchemaParser.ParseSystemSection(projectJson);
            ApplySystemSection(systemSection);

            SettingSchema projectSchema = SettingSchemaParser.Parse(projectJson);
            string streamingJson = null;
            try
            {
                streamingJson = SettingSchemaParser.LoadStreamingAssetsJson();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Setting] StreamingAssets 加载异常: " + e.Message);
            }

            if (!string.IsNullOrEmpty(streamingJson))
            {
                SettingSchema overlay = SettingSchemaParser.Parse(streamingJson);
                projectSchema = SettingSchemaParser.MergeSchemas(projectSchema, overlay);
            }

            Schema = BuildFullSchema(projectSchema);
            CaptureSystemDefaults();
            LoadAndApplySaved();

            if (createPanel)
            {
                EnsurePanel();
            }

            IsReady = true;
            if (OnReady != null)
            {
                OnReady();
            }
        }

        public void RegisterApplier(string key, Action<SettingValue> applier)
        {
            if (string.IsNullOrEmpty(key) || applier == null)
            {
                return;
            }

            _appliers[key] = applier;
            SettingItemDef def;
            if (Schema != null && Schema.ItemsByKey.TryGetValue(key, out def))
            {
                def.Applier = applier;
            }

            // 若已有提交值，立即应用一次
            SettingValue current;
            if (_committed.TryGetValue(key, out current))
            {
                try
                {
                    applier(current);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Setting] Applier 执行失败 " + key + ": " + e);
                }
            }
        }

        public void AddListener(string key, Action<string, SettingValue> listener)
        {
            if (string.IsNullOrEmpty(key) || listener == null)
            {
                return;
            }

            List<Action<string, SettingValue>> list;
            if (!_listeners.TryGetValue(key, out list))
            {
                list = new List<Action<string, SettingValue>>();
                _listeners[key] = list;
            }

            list.Add(listener);
            SettingValue v;
            if (_committed.TryGetValue(key, out v))
            {
                listener(key, v);
            }
        }

        public void RemoveListener(string key, Action<string, SettingValue> listener)
        {
            List<Action<string, SettingValue>> list;
            if (_listeners.TryGetValue(key, out list))
            {
                list.Remove(listener);
            }
        }

        public void AddButtonListener(string action, Action<string> listener)
        {
            if (string.IsNullOrEmpty(action) || listener == null)
            {
                return;
            }

            List<Action<string>> list;
            if (!_buttonListeners.TryGetValue(action, out list))
            {
                list = new List<Action<string>>();
                _buttonListeners[action] = list;
            }

            list.Add(listener);
        }

        public string GetString(string key)
        {
            return GetValue(key).AsString();
        }

        public bool GetBool(string key)
        {
            return GetValue(key).AsBool();
        }

        public int GetInt(string key)
        {
            return GetValue(key).AsInt();
        }

        public float GetFloat(string key)
        {
            return GetValue(key).AsFloat();
        }

        public SettingValue GetValue(string key)
        {
            SettingValue pending;
            if (_pending.TryGetValue(key, out pending))
            {
                return pending;
            }

            SettingValue committed;
            if (_committed.TryGetValue(key, out committed))
            {
                return committed;
            }

            SettingItemDef def;
            if (Schema != null && Schema.ItemsByKey.TryGetValue(key, out def))
            {
                return new SettingValue(def.DefaultValue);
            }

            return new SettingValue(string.Empty);
        }

        public SettingValue GetCommittedValue(string key)
        {
            SettingValue committed;
            if (_committed.TryGetValue(key, out committed))
            {
                return committed;
            }

            SettingItemDef def;
            if (Schema != null && Schema.ItemsByKey.TryGetValue(key, out def))
            {
                return new SettingValue(def.DefaultValue);
            }

            return new SettingValue(string.Empty);
        }

        public bool IsDirty(string key)
        {
            SettingValue pending;
            if (!_pending.TryGetValue(key, out pending))
            {
                return false;
            }

            return pending != GetCommittedValue(key);
        }

        public int GetDirtyCount()
        {
            int count = 0;
            foreach (KeyValuePair<string, SettingValue> kv in _pending)
            {
                if (kv.Value != GetCommittedValue(kv.Key))
                {
                    count++;
                }
            }

            return count;
        }

        public int GetPendingRestartCount()
        {
            int count = 0;
            foreach (KeyValuePair<string, SettingValue> kv in _pending)
            {
                SettingItemDef def;
                if (Schema != null && Schema.ItemsByKey.TryGetValue(kv.Key, out def) &&
                    def.ApplyMode == SettingApplyMode.Restart && kv.Value != GetCommittedValue(kv.Key))
                {
                    count++;
                }
            }

            return count;
        }

        public void SetPending(string key, SettingValue value)
        {
            _pending[key] = value;
        }

        public void ClearPending()
        {
            _pending.Clear();
        }

        public bool EvaluateVisible(SettingItemDef item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.Risk == SettingRiskLevel.Advanced && !ShowAdvanced)
            {
                return false;
            }

            if (item.Risk == SettingRiskLevel.Danger && !ShowAdvanced)
            {
                return false;
            }

            if (_hiddenKeys.Contains(item.Key))
            {
                return false;
            }

            if (item.IsSupported != null && !item.IsSupported())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(item.VisibleIfKey))
            {
                string actual = GetValue(item.VisibleIfKey).AsString();
                if (!string.Equals(actual, item.VisibleIfValue, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public bool EvaluateEnabled(SettingItemDef item)
        {
            if (item == null || item.ReadOnly)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(item.EnabledIfKey))
            {
                string actual = GetValue(item.EnabledIfKey).AsString();
                if (!string.Equals(actual, item.EnabledIfValue, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 应用所有待改项。返回需要二次确认的高危项列表；若 dangerKeys 非空且 confirmed=false，则不执行。
        /// </summary>
        public List<SettingItemDef> CollectDangerPending()
        {
            List<SettingItemDef> dangers = new List<SettingItemDef>();
            foreach (KeyValuePair<string, SettingValue> kv in _pending)
            {
                if (kv.Value == GetCommittedValue(kv.Key))
                {
                    continue;
                }

                SettingItemDef def;
                if (Schema != null && Schema.ItemsByKey.TryGetValue(kv.Key, out def) &&
                    def.Risk == SettingRiskLevel.Danger)
                {
                    dangers.Add(def);
                }
            }

            return dangers;
        }

        public void ApplyPending(bool confirmedDanger)
        {
            List<SettingItemDef> dangers = CollectDangerPending();
            if (dangers.Count > 0 && !confirmedDanger)
            {
                return;
            }

            List<KeyValuePair<string, SettingValue>> qualityFirst = new List<KeyValuePair<string, SettingValue>>();
            List<KeyValuePair<string, SettingValue>> others = new List<KeyValuePair<string, SettingValue>>();

            foreach (KeyValuePair<string, SettingValue> kv in _pending)
            {
                if (kv.Value == GetCommittedValue(kv.Key))
                {
                    continue;
                }

                if (kv.Key == "quality.level")
                {
                    qualityFirst.Add(kv);
                }
                else
                {
                    others.Add(kv);
                }
            }

            bool needRestart = false;
            ApplyList(qualityFirst, ref needRestart);
            ApplyList(others, ref needRestart);

            PersistCommitted();
            _pending.Clear();

            if (needRestart)
            {
                HasPendingRestart = true;
                _store.SavePendingRestartKeys(_pendingRestartKeys);
                if (OnRestartRequired != null)
                {
                    OnRestartRequired();
                }
            }
        }

        public void CancelPending()
        {
            _pending.Clear();
        }

        public void ResetToDefaults()
        {
            _pending.Clear();
            foreach (KeyValuePair<string, SettingItemDef> kv in Schema.ItemsByKey)
            {
                SettingItemDef def = kv.Value;
                if (def.Type == SettingItemType.Button || def.Type == SettingItemType.Info)
                {
                    continue;
                }

                SettingValue defVal = new SettingValue(def.DefaultValue);
                if (def.IsSystem && _systemDefaults.ContainsKey(def.Key))
                {
                    // 项目默认优先于系统快照？方案：恢复默认用 Schema.DefaultValue
                }

                _pending[def.Key] = defVal;
            }
        }

        public void RestoreSystemDefaults()
        {
            _pending.Clear();
            foreach (KeyValuePair<string, SettingValue> kv in _systemDefaults)
            {
                _pending[kv.Key] = kv.Value;
            }
        }

        public void CommitRestoreSystemDefaults()
        {
            RestoreSystemDefaults();
            ApplyPending(true);
            // 清除已保存的系统键
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, SettingValue> kv in _committed)
            {
                SettingItemDef def;
                if (Schema.ItemsByKey.TryGetValue(kv.Key, out def) && def.IsSystem)
                {
                    toRemove.Add(kv.Key);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                _committed.Remove(toRemove[i]);
            }

            PersistCommitted();
            _pendingRestartKeys.Clear();
            HasPendingRestart = false;
            _store.SavePendingRestartKeys(_pendingRestartKeys);
        }

        public void HandleButton(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            switch (action)
            {
                case "resetSettings":
                    ResetToDefaults();
                    break;
                case "applySettings":
                    ApplyPending(true);
                    break;
                case "cancel":
                    CancelPending();
                    break;
                case "restoreSystemDefaults":
                    CommitRestoreSystemDefaults();
                    break;
                case "restartApplication":
                    // 先落盘未应用修改，再重启（重启后从 PlayerPrefs 加载）
                    ApplyPending(true);
                    RestartApplication();
                    break;
                case "exitApplication":
                    ExitApplication();
                    break;
                case "openLogFolder":
                    OpenFolder(Application.consoleLogPath);
                    break;
                case "openDataFolder":
                    OpenFolder(Application.persistentDataPath);
                    break;
                default:
                    List<Action<string>> list;
                    if (_buttonListeners.TryGetValue(action, out list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] != null)
                            {
                                list[i](action);
                            }
                        }
                    }

                    break;
            }
        }

        public void ShowPanel(bool show)
        {
            EnsurePanel();
            if (_panel != null)
            {
                _panel.SetVisible(show);
            }
        }

        public void TogglePanel()
        {
            EnsurePanel();
            if (_panel != null)
            {
                _panel.ToggleVisible();
            }
        }

        /// <summary>编辑器退出 Play 时调用，还原系统快照。</summary>
        public void RestoreSystemSnapshotForEditor()
        {
            foreach (KeyValuePair<string, SettingValue> kv in _systemDefaults)
            {
                ApplyOne(kv.Key, kv.Value, false);
            }
        }

        public Dictionary<string, SettingValue> GetSystemDefaultsSnapshot()
        {
            return new Dictionary<string, SettingValue>(_systemDefaults);
        }

        private void ApplyList(List<KeyValuePair<string, SettingValue>> list, ref bool needRestart)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string key = list[i].Key;
                SettingValue value = list[i].Value;
                SettingItemDef def;
                Schema.ItemsByKey.TryGetValue(key, out def);

                if (def != null && def.ApplyMode == SettingApplyMode.Restart)
                {
                    _committed[key] = value;
                    if (!_pendingRestartKeys.Contains(key))
                    {
                        _pendingRestartKeys.Add(key);
                    }

                    needRestart = true;
                    NotifyListeners(key, value);
                    continue;
                }

                ApplyOne(key, value, true);
                _committed[key] = value;
            }
        }

        private void ApplyOne(string key, SettingValue value, bool notify)
        {
            SettingItemDef def;
            if (Schema != null && Schema.ItemsByKey.TryGetValue(key, out def) && def.Applier != null)
            {
                try
                {
                    def.Applier(value);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Setting] 应用失败 " + key + ": " + e);
                }
            }
            else
            {
                Action<SettingValue> applier;
                if (_appliers.TryGetValue(key, out applier))
                {
                    try
                    {
                        applier(value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Setting] 应用失败 " + key + ": " + e);
                    }
                }
                else
                {
                    // 尝试系统 Provider
                    for (int i = 0; i < _providers.Count; i++)
                    {
                        ISystemSettingProvider p = _providers[i];
                        if (p.IsSupported(key))
                        {
                            try
                            {
                                p.Apply(key, value);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("[Setting] Provider 应用失败 " + key + ": " + e);
                            }

                            break;
                        }
                    }
                }
            }

            if (notify)
            {
                NotifyListeners(key, value);
                if (OnApplied != null)
                {
                    OnApplied(key, value);
                }
            }
        }

        private void NotifyListeners(string key, SettingValue value)
        {
            List<Action<string, SettingValue>> list;
            if (_listeners.TryGetValue(key, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        list[i](key, value);
                    }
                }
            }
        }

        private void PersistCommitted()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (KeyValuePair<string, SettingValue> kv in _committed)
            {
                map[kv.Key] = kv.Value.Raw;
            }

            _store.Save(map);
        }

        private void ApplySystemSection(Dictionary<string, object> section)
        {
            _systemInclude.Clear();
            _hiddenKeys.Clear();
            _systemEnabled = true;
            _advancedEnabled = true;
            if (section == null)
            {
                return;
            }

            object enabled;
            if (section.TryGetValue("enabled", out enabled) && enabled is bool)
            {
                _systemEnabled = (bool)enabled;
            }

            object advanced;
            if (section.TryGetValue("advanced", out advanced) && advanced is bool)
            {
                _advancedEnabled = (bool)advanced;
            }

            object include;
            if (section.TryGetValue("include", out include))
            {
                List<object> list = include as List<object>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null)
                        {
                            _systemInclude.Add(list[i].ToString());
                        }
                    }
                }
            }

            object hide;
            if (section.TryGetValue("hide", out hide))
            {
                List<object> list = hide as List<object>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null)
                        {
                            _hiddenKeys.Add(list[i].ToString());
                        }
                    }
                }
            }
        }

        private SettingSchema BuildFullSchema(SettingSchema projectSchema)
        {
            SettingSchema full = new SettingSchema { Version = 2 };

            if (_systemEnabled)
            {
                // 按 Provider 生成系统标签
                Dictionary<string, SettingTabDef> sysTabs = new Dictionary<string, SettingTabDef>();
                for (int i = 0; i < _providers.Count; i++)
                {
                    ISystemSettingProvider provider = _providers[i];
                    if (_systemInclude.Count > 0 && !_systemInclude.Contains(provider.CategoryId))
                    {
                        // include 为空=全部；非空则过滤。advanced/diagnostics 特殊处理
                        if (provider.TabKind != SettingTabKind.Advanced &&
                            provider.TabKind != SettingTabKind.Diagnostics)
                        {
                            continue;
                        }
                    }

                    if (provider.TabKind == SettingTabKind.Advanced && !_advancedEnabled)
                    {
                        continue;
                    }

                    string tabId;
                    string tabLabel;
                    SettingTabKind kind = provider.TabKind;
                    int order;
                    if (kind == SettingTabKind.Advanced)
                    {
                        tabId = "system.advanced." + provider.CategoryId;
                        tabLabel = provider.CategoryLabel;
                        order = 800 + provider.Order;
                    }
                    else if (kind == SettingTabKind.Diagnostics)
                    {
                        tabId = "system.diagnostics";
                        tabLabel = "诊断信息";
                        order = 900;
                    }
                    else
                    {
                        tabId = "system." + provider.CategoryId;
                        tabLabel = provider.CategoryLabel;
                        order = provider.Order;
                    }

                    SettingTabDef tab;
                    if (!sysTabs.TryGetValue(tabId, out tab))
                    {
                        tab = new SettingTabDef
                        {
                            Id = tabId,
                            Label = tabLabel,
                            Order = order,
                            Kind = kind
                        };
                        sysTabs[tabId] = tab;
                    }

                    SettingGroupDef group = new SettingGroupDef
                    {
                        Id = provider.CategoryId,
                        Label = provider.CategoryLabel
                    };

                    foreach (SettingItemDef item in provider.GetItems())
                    {
                        if (item == null || string.IsNullOrEmpty(item.Key))
                        {
                            continue;
                        }

                        if (_hiddenKeys.Contains(item.Key))
                        {
                            continue;
                        }

                        if (!provider.IsSupported(item.Key))
                        {
                            continue;
                        }

                        item.IsSystem = true;
                        string capturedKey = item.Key;
                        ISystemSettingProvider capturedProvider = provider;
                        item.Applier = delegate(SettingValue v) { capturedProvider.Apply(capturedKey, v); };
                        item.Reader = delegate { return capturedProvider.ReadCurrent(capturedKey); };
                        item.IsSupported = delegate { return capturedProvider.IsSupported(capturedKey); };
                        if (string.IsNullOrEmpty(item.DefaultValue))
                        {
                            item.DefaultValue = capturedProvider.ReadCurrent(capturedKey).Raw;
                        }

                        group.Items.Add(item);
                    }

                    if (group.Items.Count > 0)
                    {
                        tab.Groups.Add(group);
                    }
                }

                foreach (KeyValuePair<string, SettingTabDef> kv in sysTabs)
                {
                    if (kv.Value.Groups.Count > 0)
                    {
                        full.Tabs.Add(kv.Value);
                    }
                }
            }

            if (projectSchema != null)
            {
                for (int i = 0; i < projectSchema.Tabs.Count; i++)
                {
                    SettingTabDef tab = projectSchema.Tabs[i];
                    if (tab.Kind == SettingTabKind.Project || string.IsNullOrEmpty(tab.Kind.ToString()))
                    {
                        tab.Kind = SettingTabKind.Project;
                    }

                    full.Tabs.Add(tab);
                }
            }

            full.Tabs.Sort(delegate(SettingTabDef a, SettingTabDef b) { return a.Order.CompareTo(b.Order); });
            full.RebuildIndex();
            return full;
        }

        private void CaptureSystemDefaults()
        {
            _systemDefaults.Clear();
            foreach (KeyValuePair<string, SettingItemDef> kv in Schema.ItemsByKey)
            {
                SettingItemDef def = kv.Value;
                if (!def.IsSystem)
                {
                    continue;
                }

                if (def.Reader != null)
                {
                    try
                    {
                        _systemDefaults[def.Key] = def.Reader();
                    }
                    catch
                    {
                        _systemDefaults[def.Key] = new SettingValue(def.DefaultValue);
                    }
                }
                else
                {
                    _systemDefaults[def.Key] = new SettingValue(def.DefaultValue);
                }
            }
        }

        private void LoadAndApplySaved()
        {
            Dictionary<string, string> saved = _store.Load();
            foreach (KeyValuePair<string, string> kv in saved)
            {
                SettingItemDef def;
                if (Schema.ItemsByKey.TryGetValue(kv.Key, out def))
                {
                    SettingValue value = new SettingValue(kv.Value);
                    _committed[kv.Key] = value;
                }
            }

            // 填充未保存项的当前/默认值
            foreach (KeyValuePair<string, SettingItemDef> kv in Schema.ItemsByKey)
            {
                if (!_committed.ContainsKey(kv.Key))
                {
                    if (kv.Value.IsSystem && _systemDefaults.ContainsKey(kv.Key))
                    {
                        _committed[kv.Key] = _systemDefaults[kv.Key];
                    }
                    else
                    {
                        _committed[kv.Key] = new SettingValue(kv.Value.DefaultValue);
                    }
                }
            }

            // 启动时应用已保存的立即生效项
            foreach (KeyValuePair<string, SettingValue> kv in _committed)
            {
                SettingItemDef def;
                if (!Schema.ItemsByKey.TryGetValue(kv.Key, out def))
                {
                    continue;
                }

                if (def.ApplyMode == SettingApplyMode.Restart)
                {
                    continue;
                }

                // 仅当与系统默认不同，或项目项时应用
                SettingValue sysDef;
                if (def.IsSystem && _systemDefaults.TryGetValue(kv.Key, out sysDef) && sysDef == kv.Value)
                {
                    continue;
                }

                ApplyOne(kv.Key, kv.Value, true);
            }

            // 待重启项：启动时应用（上一会话保存的 restart 项）
            List<string> pendingRestart = _store.LoadPendingRestartKeys();
            _pendingRestartKeys.Clear();
            for (int i = 0; i < pendingRestart.Count; i++)
            {
                string key = pendingRestart[i];
                SettingValue value;
                if (_committed.TryGetValue(key, out value))
                {
                    ApplyOne(key, value, true);
                }
            }

            _store.SavePendingRestartKeys(new List<string>());
            HasPendingRestart = false;
        }

        private void EnsurePanel()
        {
            if (_panelHost != null)
            {
                return;
            }

            _panelHost = new GameObject("NK_SettingPanel");
            UnityEngine.Object.DontDestroyOnLoad(_panelHost);
            _panel = SettingPanelFactory.Create(_panelHost);
            _panel.Bind(this);
            Debug.Log("[Setting] 面板渲染器: " + _panel.RendererName +
                      "（" + SettingPanelFactory.ActiveRendererName + " / Unity " + Application.unityVersion + "）");
        }

        private static void RestartApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            try
            {
                string path = Process.GetCurrentProcess().MainModule != null
                    ? Process.GetCurrentProcess().MainModule.FileName
                    : null;
                if (!string.IsNullOrEmpty(path))
                {
                    Process.Start(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Setting] 重启失败: " + e.Message);
            }

            Application.Quit();
#endif
        }

        private static void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void OpenFolder(string fileOrDir)
        {
            if (string.IsNullOrEmpty(fileOrDir))
            {
                return;
            }

            string dir = fileOrDir;
            try
            {
                if (File.Exists(fileOrDir))
                {
                    dir = Path.GetDirectoryName(fileOrDir);
                }
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return;
            }

            try
            {
                Process.Start(dir);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Setting] 无法打开目录: " + e.Message);
            }
        }
    }
}
