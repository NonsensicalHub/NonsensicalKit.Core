using NonsensicalKit.Core;
using NonsensicalKit.Setting;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    /// <summary>
    /// 项目侧监听基类：订阅指定 key 与按钮 action。
    /// </summary>
    public abstract class SettingListenerBase : NonsensicalMono
    {
        [SerializeField] private string[] m_settingKeys;
        [SerializeField] private string[] m_buttonActions;

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<SettingService>(OnGetService);
        }

        private void OnGetService(SettingService service)
        {
            if (m_settingKeys != null)
            {
                for (int i = 0; i < m_settingKeys.Length; i++)
                {
                    string key = m_settingKeys[i];
                    service.AddSettingListener(key, OnSettingValueChanged);
                }
            }

            if (m_buttonActions != null)
            {
                for (int i = 0; i < m_buttonActions.Length; i++)
                {
                    service.AddButtonListener(m_buttonActions[i], OnButtonClick);
                }
            }
        }

        protected abstract void OnSettingValueChanged(string key, SettingValue value);
        protected abstract void OnButtonClick(string actionKey);
    }
}
