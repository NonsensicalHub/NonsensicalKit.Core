using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using UnityEngine;

namespace  NonsensicalKit.Core.Service.Setting
{
    public abstract class GUISettingListenerBase : NonsensicalMono
    {
        [SerializeField] private string[] m_settingKeys;
        [SerializeField] private string[] m_addlistenButtonActions;

        protected virtual void Awake()
        {
            ServiceCore.SafeGet<NonsensicalKit.Core.Service.Setting.SettingService>(OnGetService);
        }

        private void OnGetService(NonsensicalKit.Core.Service.Setting.SettingService service)
        {
            foreach (var key in m_settingKeys)
            {
                service.AddGUISettingListener(key, OnSettingValueChanged);
            }

            foreach (var action in m_addlistenButtonActions)
            {
                Subscribe<string>("OnSettingButtonCLick", action, OnAddListenButtonClick);
            }
        }

        protected abstract void OnSettingValueChanged(string key, GUISettingItem value2);
        protected abstract void OnAddListenButtonClick(string actionKey);
    }
}