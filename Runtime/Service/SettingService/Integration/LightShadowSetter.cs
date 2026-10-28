using NonsensicalKit.Setting;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    /// <summary>
    /// 示例：监听设置项并应用到 Light.shadows。
    /// 建议在 JSON 中配置 key=lightShadow，类型 enum，值为 0/1/2。
    /// </summary>
    public class LightShadowSetter : MonoBehaviour
    {
        [SerializeField] private string m_settingName = "lightShadow";

        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
            ServiceCore.SafeGet<SettingService>(OnGetService);
        }

        private void OnGetService(SettingService service)
        {
            service.AddSettingListener(m_settingName, OnSettingChanged);
        }

        private void OnSettingChanged(string key, SettingValue value)
        {
            if (_light == null)
            {
                return;
            }

            switch (value.AsInt())
            {
                case 0:
                    _light.shadows = LightShadows.None;
                    break;
                case 1:
                    _light.shadows = LightShadows.Hard;
                    break;
                case 2:
                    _light.shadows = LightShadows.Soft;
                    break;
            }
        }
    }
}
