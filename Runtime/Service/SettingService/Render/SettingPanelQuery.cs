using System;
using System.Collections.Generic;
using System.Text;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// IMGUI 与 UI Toolkit 共用的页签过滤和搜索。列表由调用方复用。
    /// </summary>
    static class SettingPanelQuery
    {
        public static bool HasTabs(SettingSchema schema, SettingTabKind kind)
        {
            if (schema == null || schema.Tabs == null)
            {
                return false;
            }

            List<SettingTabDef> tabs = schema.Tabs;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CopyTabs(SettingSchema schema, SettingTabKind kind, List<SettingTabDef> into)
        {
            into.Clear();
            if (schema == null || schema.Tabs == null)
            {
                return;
            }

            List<SettingTabDef> tabs = schema.Tabs;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].Kind == kind)
                {
                    into.Add(tabs[i]);
                }
            }
        }

        public static void CollectVisible(SettingManager manager, SettingGroupDef group, string search,
            List<SettingItemDef> into)
        {
            into.Clear();
            if (manager == null || group == null || group.Items == null)
            {
                return;
            }

            bool filtering = !string.IsNullOrEmpty(search);
            for (int i = 0; i < group.Items.Count; i++)
            {
                SettingItemDef item = group.Items[i];
                if (!manager.EvaluateVisible(item))
                {
                    continue;
                }

                if (filtering && !Matches(item, search))
                {
                    continue;
                }

                into.Add(item);
            }
        }

        public static SettingTabDef FindTab(SettingSchema schema, string id)
        {
            if (schema == null || schema.Tabs == null || string.IsNullOrEmpty(id))
            {
                return null;
            }

            List<SettingTabDef> tabs = schema.Tabs;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].Id == id)
                {
                    return tabs[i];
                }
            }

            return null;
        }

        public static string CategoryTitle(SettingTabKind kind)
        {
            switch (kind)
            {
                case SettingTabKind.System:
                    return "系统设置";
                case SettingTabKind.Project:
                    return "项目设置";
                case SettingTabKind.Advanced:
                    return "高级";
                case SettingTabKind.Diagnostics:
                    return "诊断信息";
                default:
                    return "设置";
            }
        }

        public static string DangerMessage(List<SettingItemDef> dangers)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("以下高危设置将被应用，请确认：");
            for (int i = 0; i < dangers.Count; i++)
            {
                sb.Append(" · ").Append(dangers[i].Label);
                if (!string.IsNullOrEmpty(dangers[i].ConfirmMessage))
                {
                    sb.Append(" — ").Append(dangers[i].ConfirmMessage);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        static bool Matches(SettingItemDef item, string search)
        {
            return Contains(item.Label, search) || Contains(item.Key, search) || Contains(item.Description, search);
        }

        static bool Contains(string text, string search)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
