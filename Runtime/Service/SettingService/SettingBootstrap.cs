using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 独立启动组件：不依赖 NonsensicalKit.Core，可直接挂到场景中用于 2018.4+ 项目。
    /// </summary>
    public class SettingBootstrap : MonoBehaviour
    {
        [TextArea(12, 40)] [SerializeField] private string m_settingJson =
            "{\n  \"version\": 2,\n  \"system\": { \"enabled\": true, \"advanced\": true },\n  \"tabs\": []\n}";

        [SerializeField] private KeyCode m_toggleKey = KeyCode.F1;
        [SerializeField] private bool m_dontDestroyOnLoad = true;
        [SerializeField] private bool m_showAdvancedByDefault;

        private SettingManager _manager;

        private void Awake()
        {
            if (m_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            _manager = new SettingManager();
            SettingPanelConfig panel = new SettingPanelConfig
            {
                ToggleKey = m_toggleKey,
                ShowAdvancedByDefault = m_showAdvancedByDefault
            };
            _manager.Initialize(m_settingJson, panel);
        }

        public SettingManager Manager
        {
            get { return _manager; }
        }
    }
}
