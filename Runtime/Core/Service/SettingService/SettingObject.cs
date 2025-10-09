using System;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    [CreateAssetMenu(fileName = "SettingConfig", menuName = "ScriptableObjects/SettingConfig")]
    public class SettingObject : ConfigObject
    {
        [SerializeField] private SettingConfig m_config;

        public override ConfigData GetData()
        {
            return m_config;
        }

        public override void SetData(ConfigData cd)
        {
            if (CheckType<SettingConfig>(cd))
            {
                m_config = cd as SettingConfig;
            }
        }
    }

    [Serializable]
    public class SettingConfig : ConfigData
    {
        public SettingItem[] Items;
    }

    [Serializable]
    public class SettingItem
    {
        public string Name;
        public string InitialValue;
    }
}
