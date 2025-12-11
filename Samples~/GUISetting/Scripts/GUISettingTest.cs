using System.Collections;
using System.Collections.Generic;
using Core.Service.SettingService.GUISetting;
using NonsensicalKit.Tools.GUITool;
using TMPro;
using UnityEngine;

public class GUISettingTest : GUISettingListenerBase
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private GameObject A;
    [SerializeField] private GUIFPS m_fps;

    protected override void OnSettingValueChanged(string key, GUISettingItem item)
    {
        switch (key)
        {
            case "targetFrameRate":
                Application.targetFrameRate = int.TryParse(item.GetCacheValue().ToString(), out int frameRate) ? frameRate : 60;
                m_text.text = "当前设置帧率  :" + item.value;
                break;
            case "showFPS":
                m_fps.gameObject.SetActive(bool.TryParse(item.GetCacheValue().ToString(), out bool showFPS) && showFPS);
                break;
            case "graphicsQuality":
                QualitySettings.SetQualityLevel(item.SelectedOptionIndex);
                //QualitySettings.currentLevel=(QualityLevel)item.SelectedOptionIndex;
                Debug.Log("当前设置画质  :" + QualitySettings.names[item.SelectedOptionIndex]);
                break;
        }
    }

    protected override void OnAddListenButtonClick(string actionKey)
    {
        switch (actionKey)
        {
            case "hideObj":
                A.SetActive(!A.activeInHierarchy);
                break;
        }
    }
}