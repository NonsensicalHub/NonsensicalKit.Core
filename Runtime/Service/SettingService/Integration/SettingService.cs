using System;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Setting;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    /// <summary>
    /// Core 框架适配：通过 ServiceCore 启动，内部委托给独立 SettingManager。
    /// </summary>
    public class SettingService : IClassService
    {
        public bool IsReady { get; private set; }
        public Action InitCompleted { get; set; }

        public SettingManager Manager { get; private set; }

        public SettingService()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetConfig);
        }

        private void OnGetConfig(ConfigService service)
        {
            SettingConfig settingConfig;
            if (!service.TryGetConfig(out settingConfig))
            {
                LogCore.Error("未找到设置的配置文件，请使用 ScriptableObjects/SettingConfig 创建");
                return;
            }

            Manager = new SettingManager();
            SettingPanelConfig panel = settingConfig.m_Panel != null
                ? settingConfig.m_Panel.ToRuntime()
                : new SettingPanelConfig();

            Manager.Initialize(settingConfig.m_SettingJson, panel, settingConfig.m_UseSettingPanel);

            IsReady = true;
            if (InitCompleted != null)
            {
                InitCompleted();
            }
        }

        public string GetSettingValue(string settingName)
        {
            return Manager != null ? Manager.GetString(settingName) : string.Empty;
        }

        public bool GetBool(string key)
        {
            return Manager != null && Manager.GetBool(key);
        }

        public int GetInt(string key)
        {
            return Manager != null ? Manager.GetInt(key) : 0;
        }

        public float GetFloat(string key)
        {
            return Manager != null ? Manager.GetFloat(key) : 0f;
        }

        public void SetSetting(string settingName, string settingValue)
        {
            if (Manager == null)
            {
                return;
            }

            Manager.SetPending(settingName, new SettingValue(settingValue));
            Manager.ApplyPending(true);
        }

        public void Save()
        {
            if (Manager == null)
            {
                return;
            }

            Manager.ApplyPending(true);
        }

        public void ResetToDefault()
        {
            if (Manager == null)
            {
                return;
            }

            Manager.ResetToDefaults();
            Manager.ApplyPending(true);
        }

        public void AddSettingListener(string settingName, Action<string> listener)
        {
            if (Manager == null || listener == null)
            {
                return;
            }

            Manager.AddListener(settingName, delegate(string key, SettingValue value)
            {
                listener(value.AsString());
            });
        }

        public void AddSettingListener(string settingName, Action<string, SettingValue> listener)
        {
            if (Manager == null)
            {
                return;
            }

            Manager.AddListener(settingName, listener);
        }

        public void RemoveSettingListener(string settingName, Action<string, SettingValue> listener)
        {
            if (Manager == null)
            {
                return;
            }

            Manager.RemoveListener(settingName, listener);
        }

        public void RegisterApplier(string key, Action<SettingValue> applier)
        {
            if (Manager != null)
            {
                Manager.RegisterApplier(key, applier);
            }
        }

        public void AddButtonListener(string action, Action<string> listener)
        {
            if (Manager != null)
            {
                Manager.AddButtonListener(action, listener);
            }
        }
    }
}
