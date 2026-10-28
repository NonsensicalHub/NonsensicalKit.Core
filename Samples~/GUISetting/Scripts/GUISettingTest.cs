using NonsensicalKit.Core.Service.Setting;
using NonsensicalKit.Setting;
using NonsensicalKit.Tools.GUITool;
using TMPro;
using UnityEngine;

public class GUISettingTest : SettingListenerBase
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private GameObject A;
    [SerializeField] private GUIFPS m_fps;

    protected override void OnSettingValueChanged(string key, SettingValue value)
    {
        switch (key)
        {
            case "display.showFPS":
                if (m_fps != null)
                {
                    m_fps.gameObject.SetActive(value.AsBool());
                }

                if (m_text != null)
                {
                    m_text.text = "显示帧率: " + value.AsBool();
                }

                break;
        }
    }

    protected override void OnButtonClick(string actionKey)
    {
        switch (actionKey)
        {
            case "hideObj":
                if (A != null)
                {
                    A.SetActive(!A.activeInHierarchy);
                }

                break;
        }
    }
}
