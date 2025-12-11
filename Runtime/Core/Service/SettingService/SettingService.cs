using System;
using System.Collections.Generic;
using System.Linq;
using Core.Service.SettingService.GUISetting;
using Newtonsoft.Json;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.EasyTool;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    public class SettingService : IClassService
    {
        public bool IsReady { get; private set; }
        public Action InitCompleted { get; set; }

        private readonly Dictionary<string, string> _settingTemplate = new();
        private readonly Dictionary<string, string> _currentSettings = new();

        private readonly Dictionary<string, Action<string>> _onSettingChanged = new();
        private readonly Dictionary<string, List<Action<string, GUISettingItem>>> _onGUISettingChanged = new();

        private bool _useGUISetting;

        public SettingService()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetConfig);
        }

        private void OnGetConfig(ConfigService service)
        {
            if (service.TryGetConfig<SettingConfig>(out var settingConfig))
            {
                foreach (var item in settingConfig.m_Items)
                {
                    if (_settingTemplate.TryAdd(item.Name, item.InitialValue))
                    {
                        _currentSettings.Add(item.Name, item.InitialValue);
                    }
                    else
                    {
                        LogCore.Error($"重复的设置项：{item.Name}");
                    }
                }

                var settingStr = PlayerPrefs.GetString($"NONSENSICALSETTING", string.Empty);
                if (string.IsNullOrEmpty(settingStr) == false)
                {
                    var settingItems = JsonTool.DeserializeObject<string[]>(settingStr);

                    for (int i = 0; i < settingItems.Length; i += 2)
                    {
                        if (_currentSettings.ContainsKey(settingItems[i]))
                        {
                            _currentSettings[settingItems[i]] = settingItems[i + 1];

                            if (_onSettingChanged.ContainsKey(settingItems[i]))
                            {
                                _onSettingChanged[settingItems[i]]?.Invoke(settingItems[i + 1]);
                            }
                        }
                    }
                }

                _useGUISetting = settingConfig.m_UseGUISetting;
                if (settingConfig.m_UseGUISetting == true)
                {
                    CreateSettingGUI(settingConfig);
                    CreateSettingData(settingConfig);
                }


                IsReady = true;
                InitCompleted?.Invoke();
            }
            else
            {
                LogCore.Error("未找到设置的配置文件，使用 ScriptableObjects/SettingConfig 创建");
            }
        }

        public void ResetToDefault()
        {
            foreach (var key in _settingTemplate.Keys)
            {
                SetSetting(key, _settingTemplate[key]);
            }
        }

        public string GetSettingValue(string settingName)
        {
            return _currentSettings.TryGetValue(settingName, out var value) ? value : string.Empty;
        }

        public void SetSetting(string settingName, string settingValue)
        {
            if (_currentSettings.ContainsKey(settingName))
            {
                _currentSettings[settingName] = settingValue;
                if (_onSettingChanged.TryGetValue(settingName, out var value))
                {
                    value?.Invoke(settingValue);
                }

                //通知GUI设置项修改
                if (_useGUISetting && _onGUISettingChanged.TryGetValue(settingName, out var guiValues))
                {
                    //需注意值校验
                    _settingsDict[settingName].SetInitialValue(settingValue);
                    _settingsDict[settingName].value = settingValue;
                    foreach (var action in guiValues)
                    {
                        action?.Invoke(settingName, _settingsDict[settingName]);
                    }
                }
            }
        }

        public void Save()
        {
            string[] settingItems = new string[_currentSettings.Count * 2];

            int index = 0;

            foreach (var setting in _currentSettings)
            {
                settingItems[index++] = setting.Key;
                settingItems[index++] = setting.Value;
            }

            PlayerPrefs.SetString($"NONSENSICALSETTING", JsonTool.SerializeObject(settingItems));
        }

        //会在初次有值时调用一次listener
        public void AddSettingListener(string settingName, Action<string> listener)
        {
            if (_currentSettings.TryGetValue(settingName, out var setting))
            {
                _onSettingChanged.ActionAdd<string, string>(settingName, listener);
                listener?.Invoke(setting);
            }
        }

        public void RemoveSettingListener(string settingName, Action<string> listener)
        {
            if (_onSettingChanged.ContainsKey(settingName))
            {
                _onSettingChanged[settingName] -= listener;
            }
        }


        #region GUISetting Manager

        private List<GUISettingItem> _settingsDataWrapper;
        private Dictionary<string, GUISettingItem> _settingsDict;
        private readonly Dictionary<string, GUISettingItem> _settingsTempDict = new();

        #region 监听器

        public void AddGUISettingListener(string settingName, Action<string, GUISettingItem> listener)
        {
            if (_useGUISetting == false)
            {
                Debug.LogWarning("GUI设置未启用,不支持添加监听");
                return;
            }
            _onGUISettingChanged.ListAdd<string, Action<string, GUISettingItem>>(settingName, listener);
            listener?.Invoke(settingName, _settingsDict[settingName]);
        }

        public void RemoveGUISettingListener(string settingName, Action<string, GUISettingItem> listener)
        {
            if (_onGUISettingChanged.TryGetValue(settingName, out var value))
            {
                value.Remove(listener); // listener;
            }
        }

        #endregion

        public void RefreshGUIData()
        {
            if (_settingsDataWrapper == null) return;
            foreach (var item in _settingsDataWrapper.Where(item => item.type != "button"))
            {
                item.SetInitialValue(_currentSettings.TryGetValue(item.key, out var setting) ? setting : item.value);
                item.value = item.GetCacheValue()?.ToString(); //同步值到Value
            }
        }

        public List<GUISettingItem> GetSettingsDataWrapper() //GUI Panel获取设置项数据
        {
            return _settingsDataWrapper;
        }

        public void SetValue(string key, object value) //GUI panel设置值
        {
            if (_settingsDict.TryGetValue(key, out GUISettingItem item))
            {
                bool valueChanged = false;
                switch (item.InternalType)
                {
                    case SettingItemTypeInternal.Bool:
                        if (value is bool boolVal && item.boolValue != boolVal)
                        {
                            item.boolValue = boolVal;
                            valueChanged = true;
                        }

                        break;
                    case SettingItemTypeInternal.String:
                        if (value is string strVal && item.stringValue != strVal)
                        {
                            item.stringValue = strVal;
                            valueChanged = true;
                        }

                        break;
                    case SettingItemTypeInternal.Float:
                        if (value is float floatVal && !Mathf.Approximately(item.floatValue, floatVal))
                        {
                            item.floatValue = floatVal;
                            valueChanged = true;
                        }
                        else if (value is int intVal)
                        {
                            float fVal = intVal;
                            if (!Mathf.Approximately(item.floatValue, fVal))
                            {
                                item.floatValue = fVal;
                                valueChanged = true;
                            }
                        }
                        else if (value is double doubleVal)
                        {
                            float fVal = (float)doubleVal;
                            if (!Mathf.Approximately(item.floatValue, fVal))
                            {
                                item.floatValue = fVal;
                                valueChanged = true;
                            }
                        }

                        break;
                    case SettingItemTypeInternal.Enum:
                        if (value is int enumIntVal)
                        {
                            int clampedIndex = Mathf.Clamp(enumIntVal, 0, item.options.Count - 1);
                            string newValue = item.options[clampedIndex];
                            if (item.stringValue != newValue)
                            {
                                item.stringValue = newValue;
                                item.SelectedOptionIndex = clampedIndex;
                                valueChanged = true;
                            }
                        }
                        else if (value is string enumStrVal)
                        {
                            int index = item.options.IndexOf(enumStrVal);

                            if (index != -1)
                            {
                                valueChanged = true;
                            }
                            else
                            {
                                Debug.LogWarning($"Attempted to set invalid enum value '{enumStrVal}' for key '{key}'. Valid options: {string.Join(", ", item.options)}");
                            }
                        }

                        break;
                    case SettingItemTypeInternal.Button:
                        Debug.LogWarning($"SetValue called on button type setting '{key}'. Buttons don't store values.");
                        break;
                }

                if (valueChanged)
                {
                    if (_settingsTempDict.TryAdd(key, item) == false)
                    {
                        _settingsTempDict[key] = item;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Setting with key '{key}' not found.");
            }
        }

        public void HandleButtonAction(string actionKey)
        {
            switch (actionKey)
            {
                case "resetSettings":
                    Debug.Log("Resetting settings to defaults...");
                    foreach (GUISettingItem item in _settingsDataWrapper)
                    {
                        item.value = _settingTemplate[item.key];
                        _currentSettings[item.key] = item.value;
                        Publisher(item);
                        //IOCC.PublishWithID<string, object, GUISettingItem>("OnSettingValueChanged", item.key, item.key, item.value, item);
                    }

                    break;

                case "applySettings":
                    Debug.Log("Apply settings");
                    foreach (var item in _settingsTempDict)
                    {
                        //IOCC.PublishWithID("OnSettingValueChanged", item.Key, item.Key, item.Value.Item1, item.Value.Item2);
                        Publisher(item.Value);
                    }

                    _settingsTempDict.Clear();
                    SaveGUISetting();
                    break;
                case "cancel":
                    _settingsTempDict.Clear();
                    break;
                case "exitApplication":
                    if (Application.isEditor)
                    {
#if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
#endif
                    }
                    else
                    {
                        Application.Quit();
                    }

                    break;
                default:
                    IOCC.PublishWithID("OnSettingButtonCLick", actionKey, actionKey);
                    break;
            }
        }

        private void Publisher(GUISettingItem item)
        {
            if (_onGUISettingChanged.TryGetValue(item.key, out var guiValues))
            {
                foreach (var action in guiValues)
                {
                    action?.Invoke(item.key, item);
                }
            }
        }

        private void SaveGUISetting()
        {
            //同步缓存数据到主缓存
            foreach (var item in _settingsDataWrapper.Where(item => item.InternalType != SettingItemTypeInternal.Button))
            {
                item.value = item.GetCacheValue()?.ToString();
                _currentSettings[item.key] = item.value;
            }

            Save();
        }

        private void CreateSettingGUI(SettingConfig settingConfig)
        {
            var guiManager = new GameObject("GUISettingService");
            var panel = guiManager.AddComponent<RuntimeSettingsPanel>();

            panel.SetSettingService(this);
            panel.Init(settingConfig.guiPanelConfig);
            guiManager.AddComponent<DontDestroyGameObject>();
        }

        private void CreateSettingData(SettingConfig settingConfig)
        {
            //加载并缓存设置项
            _settingsDataWrapper = new List<GUISettingItem>();
            _settingsDict = new Dictionary<string, GUISettingItem>();

            if (string.IsNullOrEmpty(settingConfig.GUISettingTemplate)) return;
            _settingsDataWrapper = JsonConvert.DeserializeObject<List<GUISettingItem>>(settingConfig.GUISettingTemplate);

            if (_settingsDataWrapper is { Count: > 0 })
            {
                foreach (var item in _settingsDataWrapper)
                {
                    item.InternalType = GetInternalTypeFromString(item.type);
                    if (item.type == "button") continue;

                    if (_currentSettings.ContainsKey(item.key) == false) Debug.LogWarning($"未在Item中找到设置项：{item.key},请检查!!!");
                    //同步保存到数据至缓存数据
                    item.SetInitialValue(_currentSettings.TryGetValue(item.key, out var setting) ? setting : item.value);
                    item.value = item.GetCacheValue()?.ToString(); //同步值到Value

                    if (item.InternalType == SettingItemTypeInternal.Enum && item.options.Count > 0)
                    {
                        if (item.options.Contains(item.stringValue))
                        {
                            item.SelectedOptionIndex = item.options.IndexOf(item.stringValue);
                        }
                        else
                        {
                            Debug.LogWarning($"Enum value '{item.stringValue}' not found in options for key '{item.key}'. Defaulting to first option.");
                            item.SelectedOptionIndex = 0;
                            item.stringValue = item.options.Count > 0 ? item.options[0] : "";
                        }
                    }

                    _settingsDict[item.key] = item;
                }
            }
        }

        private SettingItemTypeInternal GetInternalTypeFromString(string typeString)
        {
            switch (typeString?.ToLowerInvariant())
            {
                case "bool": return SettingItemTypeInternal.Bool;
                case "string": return SettingItemTypeInternal.String;
                case "float": return SettingItemTypeInternal.Float;
                case "enum": return SettingItemTypeInternal.Enum;
                case "button": return SettingItemTypeInternal.Button;
                default:
                    Debug.LogWarning($"Unknown type string '{typeString}', defaulting to String.");
                    return SettingItemTypeInternal.String;
            }
        }

        #endregion
    }
}