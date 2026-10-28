using System;
using System.Collections.Generic;

namespace NonsensicalKit.Setting
{
    [Serializable]
    public class SettingEnumOption
    {
        public string Value;
        public string Label;

        public SettingEnumOption()
        {
        }

        public SettingEnumOption(string value, string label)
        {
            Value = value;
            Label = label;
        }
    }

    [Serializable]
    public class SettingItemDef
    {
        public string Key;
        public SettingItemType Type = SettingItemType.String;
        public string Label;
        public string Description;
        public string DefaultValue = string.Empty;
        public float Min;
        public float Max = 1f;
        public float Step;
        public string Unit = string.Empty;
        public List<SettingEnumOption> Options = new List<SettingEnumOption>();
        public string ButtonAction = string.Empty;
        public SettingApplyMode ApplyMode = SettingApplyMode.Immediate;
        public SettingRiskLevel Risk = SettingRiskLevel.Normal;
        public string ConfirmMessage = string.Empty;
        public string VisibleIfKey = string.Empty;
        public string VisibleIfValue = string.Empty;
        public string EnabledIfKey = string.Empty;
        public string EnabledIfValue = string.Empty;
        public bool ReadOnly;
        public bool IsSystem;
        public string GroupId = string.Empty;
        public string TabId = string.Empty;

        /// <summary>系统项由 Provider 实现；项目项由 RegisterApplier 实现。</summary>
        public Action<SettingValue> Applier;

        public Func<SettingValue> Reader;

        public Func<bool> IsSupported;
    }

    [Serializable]
    public class SettingGroupDef
    {
        public string Id;
        public string Label;
        public string Description = string.Empty;
        public List<SettingItemDef> Items = new List<SettingItemDef>();
    }

    [Serializable]
    public class SettingTabDef
    {
        public string Id;
        public string Label;
        public int Order;
        public SettingTabKind Kind = SettingTabKind.Project;
        public List<SettingGroupDef> Groups = new List<SettingGroupDef>();
    }

    [Serializable]
    public class SettingSchema
    {
        public int Version = 2;
        public List<SettingTabDef> Tabs = new List<SettingTabDef>();
        public Dictionary<string, SettingItemDef> ItemsByKey = new Dictionary<string, SettingItemDef>();

        public void RebuildIndex()
        {
            ItemsByKey.Clear();
            for (int t = 0; t < Tabs.Count; t++)
            {
                SettingTabDef tab = Tabs[t];
                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    SettingGroupDef group = tab.Groups[g];
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        SettingItemDef item = group.Items[i];
                        item.TabId = tab.Id;
                        item.GroupId = group.Id;
                        if (!string.IsNullOrEmpty(item.Key) && item.Type != SettingItemType.Button &&
                            item.Type != SettingItemType.Info)
                        {
                            ItemsByKey[item.Key] = item;
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class SettingPanelConfig
    {
        public UnityEngine.KeyCode ToggleKey = UnityEngine.KeyCode.F1;
        public float WindowX = 80f;
        public float WindowY = 60f;
        public float WindowWidth = 920f;
        public float WindowHeight = 620f;
        public bool UseHeightPercentage = true;
        public float HeightPercentage = 0.82f;
        public bool ShowAdvancedByDefault;
    }
}
