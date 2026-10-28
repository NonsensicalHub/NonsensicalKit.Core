using System;
using NonsensicalKit.Core.Service.Config;
using NonsensicalKit.Setting;
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
        [Tooltip("是否创建运行时设置面板")]
        public bool m_UseSettingPanel = true;

        [Tooltip("面板开关快捷键等")]
        public SettingPanelConfigSerialized m_Panel = new SettingPanelConfigSerialized();

        [TextArea(20, 40)]
        [Tooltip("v2 JSON：system + tabs。开发侧只维护此配置，布局由面板自动生成。")]
        public string m_SettingJson =
            "{\n  \"version\": 2,\n  \"system\": {\n    \"enabled\": true,\n    \"advanced\": true,\n    \"include\": [\"display\", \"quality\", \"audio\", \"performance\"]\n  },\n  \"tabs\": [\n    {\n      \"id\": \"project.demo\",\n      \"label\": \"项目示例\",\n      \"order\": 100,\n      \"kind\": \"project\",\n      \"groups\": [\n        {\n          \"id\": \"demo\",\n          \"label\": \"示例项\",\n          \"items\": [\n            {\n              \"key\": \"hideObj\",\n              \"type\": \"button\",\n              \"label\": \"切换测试物体\",\n              \"description\": \"项目自定义按钮\",\n              \"buttonAction\": \"hideObj\"\n            }\n          ]\n        }\n      ]\n    }\n  ]\n}";
    }

    [Serializable]
    public class SettingPanelConfigSerialized
    {
        public KeyCode m_ToggleKey = KeyCode.F1;
        public float m_X = 80f;
        public float m_Y = 60f;
        public float m_W = 920f;
        public float m_H = 620f;
        public bool m_UsePercentage = true;
        public float m_WindowHeightPercentage = 0.82f;
        public bool m_ShowAdvancedByDefault;

        public SettingPanelConfig ToRuntime()
        {
            return new SettingPanelConfig
            {
                ToggleKey = m_ToggleKey,
                WindowX = m_X,
                WindowY = m_Y,
                WindowWidth = m_W,
                WindowHeight = m_H,
                UseHeightPercentage = m_UsePercentage,
                HeightPercentage = m_WindowHeightPercentage,
                ShowAdvancedByDefault = m_ShowAdvancedByDefault
            };
        }
    }
}
