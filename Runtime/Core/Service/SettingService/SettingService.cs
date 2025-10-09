using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    public class SettingService : IClassService
    {
        public bool IsReady { get; private set; }
        public Action InitCompleted { get; set; }

        private readonly Dictionary<string, string> _settingTemplate= new();
        private readonly Dictionary<string, string> _currentSettings= new();

        private readonly Dictionary<string, Action<string>> _onSettingChanged = new();

        public SettingService()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetConfig);
        }

        private void OnGetConfig(ConfigService service)
        {
            if (service.TryGetConfig<SettingConfig>(out var settingConfig))
            {

                foreach (var item in settingConfig.Items)
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
                SetSetting(key,_settingTemplate[key]);
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
    }
}
