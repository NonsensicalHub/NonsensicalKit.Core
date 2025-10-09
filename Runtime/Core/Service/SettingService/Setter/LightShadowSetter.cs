using System;
using UnityEngine;

namespace NonsensicalKit.Core.Service.Setting
{
    public class LightShadowSetter : MonoBehaviour
    {
        [SerializeField] private string m_settingName="lightShadow";

        private Light _light;
        
        private void Awake()
        {
            _light = GetComponent<Light>();

            ServiceCore.SafeGet<SettingService>(OnGetService);
        }

        private void OnGetService(SettingService service)
        {
            service .AddSettingListener(m_settingName,OnSettingChanged);
        }
        
        private void OnSettingChanged(string settingName)
        {
            if (int.TryParse(settingName, out int intValue))
            {
                switch (intValue)
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
}