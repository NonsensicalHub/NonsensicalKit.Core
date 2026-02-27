using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using Newtonsoft.Json;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonsensicalKit.Core.Service.Setting
{
    [CreateAssetMenu(fileName = "SettingConfig", menuName = "ScriptableObjects/SettingConfig")]
    public class SettingObject : ConfigObject, ISerializationCallbackReceiver
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

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (m_config.guiPanelConfig.m_CustomSkin != null)
            {
                m_config.guiPanelConfig.m_GuiSkinPath = GetResourcePath(m_config.guiPanelConfig.m_CustomSkin);
            }
#endif
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (!m_config.guiPanelConfig.m_CustomSkin && string.IsNullOrEmpty(m_config.guiPanelConfig.m_GuiSkinPath) == false)
            {
                m_config.guiPanelConfig.m_CustomSkin = Resources.Load<GUISkin>(m_config.guiPanelConfig.m_GuiSkinPath);
            }
#endif
        }

#if UNITY_EDITOR
        [Button("同步GUI设置项至Items")]
        public void SyncGUISetting()
        {
            if (string.IsNullOrEmpty(m_config.GUISettingTemplate)) return;

            var guiSetting = JsonConvert.DeserializeObject<GUISettingItem[]>(m_config.GUISettingTemplate);
            if (guiSetting == null) return;

            // 获取当前已存在的 Name 集合
            var existingItems = (m_config.m_Items ?? Array.Empty<SettingItem>())
                .Where(item => item?.Name != null)
                .ToDictionary(item => item.Name, item => item, StringComparer.OrdinalIgnoreCase);

            var newItems = new List<SettingItem>();
            var skippedKeys = new List<string>(); // 记录被跳过的 key

            foreach (var item in guiSetting)
            {
                if (item.type == "button" || string.IsNullOrEmpty(item.key)) continue;
                if (existingItems.ContainsKey(item.key))
                {
                    skippedKeys.Add(item.key);
                    continue;
                }

                newItems.Add(new SettingItem
                {
                    Name = item.key,
                    InitialValue = item.value
                });
            }

            if (skippedKeys.Count > 0)
            {
                string skippedList = string.Join(", ", skippedKeys);
                Debug.LogError($"[同步设置项时存在重复项] 跳过 {skippedKeys.Count} 个重复项: {skippedList}");
            }

            if (newItems.Count == 0) return;

            // 合并数组
            var original = m_config.m_Items ?? Array.Empty<SettingItem>();
            var merged = new SettingItem[original.Length + newItems.Count];
            original.AsSpan().CopyTo(merged);
            newItems.ToArray().AsSpan().CopyTo(merged.AsSpan(original.Length));

            m_config.m_Items = merged;
        }

        private string GetResourcePath(Object selectedObj)
        {
            // 获取编辑器中选中的资源
            //var selectedObj = Selection.activeObject;
            if (selectedObj == null)
            {
                Debug.LogError("请选中一个资源！");
                return null;
            }

            // 获取资源的绝对路径（如：Assets/Resources/Prefabs/Player.prefab）
            string assetPath = AssetDatabase.GetAssetPath(selectedObj);

            // 检查资源是否在 Resources 文件夹下
            if (!assetPath.Contains("/Resources/"))
            {
                Debug.LogError("选中的资源不在 Resources 文件夹下！");
                return null;
            }

            // 提取 Resources 相对路径（去掉 Assets/Resources/ 和后缀名）
            var resourcesIndex = assetPath.IndexOf("/Resources/", StringComparison.Ordinal) + "/Resources/".Length;
            var relativePath = assetPath.Substring(resourcesIndex);
            relativePath = Path.ChangeExtension(relativePath, null);

            return relativePath;
        }
#endif
    }

    [Serializable]
    public class SettingConfig : ConfigData
    {
        public SettingItem[] m_Items;
        public bool m_UseGUISetting = true;

        public GUIPanelConfig guiPanelConfig;
        [TextArea(15, 30)] public string GUISettingTemplate = "[{\"key\":\"targetFrameRate\",\"type\":\"string\",\"label\":\"帧率设置\",\"description\":\"调整程序运行帧率 (0-120)\",\"value\":60,\"range\":{\"I1\":0,\"I2\":120}},{\"key\":\"showFPS\",\"type\":\"bool\",\"label\":\"显示FPS\",\"description\":\"在屏幕上显示帧率信息\",\"value\":false},{\"key\":\"graphicsQuality\",\"type\":\"enum\",\"label\":\"图形质量\",\"description\":\"选择游戏图形质量\",\"value\":\"中\",\"options\":[\"极低\",\"低\",\"中\",\"高\"]},{\"key\":\"resetSettings\",\"type\":\"button\",\"label\":\"重置设置\",\"description\":\"将所有设置恢复到默认值\",\"buttonAction\":\"resetSettings\"},{\"key\":\"exitApplication\",\"type\":\"button\",\"label\":\"退出程序\",\"description\":\"退出程序\",\"buttonAction\":\"exitApplication\"}]";
    }

    [Serializable]
    public class SettingItem
    {
        public string Name;
        public string InitialValue;
    }

    [Serializable]
    public class GUIPanelConfig
    {
        public KeyCode m_ToggleKey = KeyCode.F1;

        [JsonIgnore] public GUISkin m_CustomSkin;

        public string m_GuiSkinPath;

        [Tooltip("窗口高度使用百分比")] public bool m_UsePercentage = true;

        public float m_WindowHeightPercentage = 0.8f;
        public Rect m_WindowRect = new Rect(50, 50, 400, 600);

        public GUISkin GetSkin()
        {
            return m_CustomSkin != null ? m_CustomSkin : Resources.Load<GUISkin>(m_GuiSkinPath);
        }
    }
}