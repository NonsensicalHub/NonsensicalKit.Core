using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace NonsensicalKit.Setting.Json
{
    public static class SettingSchemaParser
    {
        public static SettingSchema Parse(string json)
        {
            SettingSchema schema = new SettingSchema();
            if (string.IsNullOrEmpty(json))
            {
                return schema;
            }

            object root = MiniJson.Deserialize(json);
            if (root is List<object>)
            {
                // 旧版扁平数组：包装为一个项目标签页
                SettingTabDef tab = new SettingTabDef
                {
                    Id = "project.general",
                    Label = "项目",
                    Order = 100,
                    Kind = SettingTabKind.Project
                };
                SettingGroupDef group = new SettingGroupDef
                {
                    Id = "general",
                    Label = "通用"
                };
                List<object> list = (List<object>)root;
                for (int i = 0; i < list.Count; i++)
                {
                    SettingItemDef item = ParseItem(list[i] as Dictionary<string, object>);
                    if (item != null)
                    {
                        group.Items.Add(item);
                    }
                }

                tab.Groups.Add(group);
                schema.Tabs.Add(tab);
                schema.Version = 1;
                schema.RebuildIndex();
                return schema;
            }

            Dictionary<string, object> obj = root as Dictionary<string, object>;
            if (obj == null)
            {
                Debug.LogWarning("[Setting] JSON 根节点无效");
                return schema;
            }

            schema.Version = GetInt(obj, "version", 2);

            // system 段：include/advanced/hide/overrides 由 SettingManager 合并时处理
            if (obj.ContainsKey("tabs"))
            {
                List<object> tabs = obj["tabs"] as List<object>;
                if (tabs != null)
                {
                    for (int i = 0; i < tabs.Count; i++)
                    {
                        SettingTabDef tab = ParseTab(tabs[i] as Dictionary<string, object>);
                        if (tab != null)
                        {
                            schema.Tabs.Add(tab);
                        }
                    }
                }
            }

            schema.RebuildIndex();
            return schema;
        }

        public static Dictionary<string, object> ParseSystemSection(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            object root = MiniJson.Deserialize(json);
            Dictionary<string, object> obj = root as Dictionary<string, object>;
            if (obj == null || !obj.ContainsKey("system"))
            {
                return null;
            }

            return obj["system"] as Dictionary<string, object>;
        }

        public static string LoadStreamingAssetsJson()
        {
            string dir = Path.Combine(Application.streamingAssetsPath, "Settings");
            if (!Directory.Exists(dir))
            {
                return null;
            }

            string[] files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
            if (files == null || files.Length == 0)
            {
                return null;
            }

            Array.Sort(files);
            // 合并多个文件的 tabs：简单拼接为最后一个覆盖同 id
            SettingSchema merged = new SettingSchema();
            Dictionary<string, SettingTabDef> tabMap = new Dictionary<string, SettingTabDef>();
            for (int f = 0; f < files.Length; f++)
            {
                string text;
                try
                {
                    text = File.ReadAllText(files[f]);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Setting] 读取 StreamingAssets 失败: " + files[f] + " " + e.Message);
                    continue;
                }

                SettingSchema part = Parse(text);
                for (int t = 0; t < part.Tabs.Count; t++)
                {
                    SettingTabDef tab = part.Tabs[t];
                    tabMap[tab.Id] = tab;
                }
            }

            foreach (KeyValuePair<string, SettingTabDef> kv in tabMap)
            {
                merged.Tabs.Add(kv.Value);
            }

            merged.RebuildIndex();
            return MiniJson.Serialize(SchemaToDict(merged));
        }

        public static SettingSchema MergeSchemas(SettingSchema primary, SettingSchema overlay)
        {
            if (primary == null)
            {
                primary = new SettingSchema();
            }

            if (overlay == null)
            {
                primary.RebuildIndex();
                return primary;
            }

            Dictionary<string, SettingTabDef> map = new Dictionary<string, SettingTabDef>();
            for (int i = 0; i < primary.Tabs.Count; i++)
            {
                map[primary.Tabs[i].Id] = primary.Tabs[i];
            }

            for (int i = 0; i < overlay.Tabs.Count; i++)
            {
                map[overlay.Tabs[i].Id] = overlay.Tabs[i];
            }

            SettingSchema result = new SettingSchema { Version = Math.Max(primary.Version, overlay.Version) };
            foreach (KeyValuePair<string, SettingTabDef> kv in map)
            {
                result.Tabs.Add(kv.Value);
            }

            result.Tabs.Sort(CompareTabOrder);
            result.RebuildIndex();
            return result;
        }

        private static int CompareTabOrder(SettingTabDef a, SettingTabDef b)
        {
            return a.Order.CompareTo(b.Order);
        }

        private static SettingTabDef ParseTab(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            SettingTabDef tab = new SettingTabDef
            {
                Id = GetString(dict, "id", "tab"),
                Label = GetString(dict, "label", "未命名"),
                Order = GetInt(dict, "order", 100),
                Kind = ParseTabKind(GetString(dict, "kind", "project"))
            };

            if (dict.ContainsKey("groups"))
            {
                List<object> groups = dict["groups"] as List<object>;
                if (groups != null)
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        SettingGroupDef group = ParseGroup(groups[i] as Dictionary<string, object>);
                        if (group != null)
                        {
                            tab.Groups.Add(group);
                        }
                    }
                }
            }

            return tab;
        }

        private static SettingGroupDef ParseGroup(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            SettingGroupDef group = new SettingGroupDef
            {
                Id = GetString(dict, "id", "group"),
                Label = GetString(dict, "label", "分组"),
                Description = GetString(dict, "description", string.Empty)
            };

            if (dict.ContainsKey("items"))
            {
                List<object> items = dict["items"] as List<object>;
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        SettingItemDef item = ParseItem(items[i] as Dictionary<string, object>);
                        if (item != null)
                        {
                            group.Items.Add(item);
                        }
                    }
                }
            }

            return group;
        }

        private static SettingItemDef ParseItem(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            SettingItemDef item = new SettingItemDef
            {
                Key = GetString(dict, "key", string.Empty),
                Type = ParseItemType(GetString(dict, "type", "string")),
                Label = GetString(dict, "label", GetString(dict, "key", "未命名")),
                Description = GetString(dict, "description", string.Empty),
                Unit = GetString(dict, "unit", string.Empty),
                ButtonAction = GetString(dict, "buttonAction", string.Empty),
                ConfirmMessage = GetString(dict, "confirm", string.Empty),
                ApplyMode = ParseApplyMode(GetString(dict, "apply", "immediate")),
                Risk = ParseRisk(GetString(dict, "risk", "normal")),
                ReadOnly = GetBool(dict, "readOnly", false),
                Step = GetFloat(dict, "step", 0f)
            };

            // default / value
            if (dict.ContainsKey("default"))
            {
                item.DefaultValue = ValueToString(dict["default"]);
            }
            else if (dict.ContainsKey("value"))
            {
                item.DefaultValue = ValueToString(dict["value"]);
            }

            // range: v2 min/max 或旧版 range.I1/I2
            if (dict.ContainsKey("min") || dict.ContainsKey("max"))
            {
                item.Min = GetFloat(dict, "min", 0f);
                item.Max = GetFloat(dict, "max", 1f);
            }
            else if (dict.ContainsKey("range"))
            {
                Dictionary<string, object> range = dict["range"] as Dictionary<string, object>;
                if (range != null)
                {
                    item.Min = GetFloat(range, "I1", GetFloat(range, "min", 0f));
                    item.Max = GetFloat(range, "I2", GetFloat(range, "max", 1f));
                }
            }

            // visibleIf / enabledIf: "key=value"
            ParseCondition(GetString(dict, "visibleIf", string.Empty), out item.VisibleIfKey, out item.VisibleIfValue);
            ParseCondition(GetString(dict, "enabledIf", string.Empty), out item.EnabledIfKey, out item.EnabledIfValue);

            if (dict.ContainsKey("options"))
            {
                List<object> options = dict["options"] as List<object>;
                if (options != null)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        if (options[i] is string)
                        {
                            string s = (string)options[i];
                            item.Options.Add(new SettingEnumOption(s, s));
                        }
                        else
                        {
                            Dictionary<string, object> od = options[i] as Dictionary<string, object>;
                            if (od != null)
                            {
                                string v = GetString(od, "value", string.Empty);
                                string l = GetString(od, "label", v);
                                item.Options.Add(new SettingEnumOption(v, l));
                            }
                        }
                    }
                }
            }

            return item;
        }

        private static void ParseCondition(string expr, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;
            if (string.IsNullOrEmpty(expr))
            {
                return;
            }

            int idx = expr.IndexOf('=');
            if (idx <= 0)
            {
                key = expr.Trim();
                value = "true";
                return;
            }

            key = expr.Substring(0, idx).Trim();
            value = expr.Substring(idx + 1).Trim();
        }

        private static SettingItemType ParseItemType(string t)
        {
            if (string.IsNullOrEmpty(t))
            {
                return SettingItemType.String;
            }

            switch (t.ToLowerInvariant())
            {
                case "bool":
                    return SettingItemType.Bool;
                case "int":
                    return SettingItemType.Int;
                case "float":
                    return SettingItemType.Float;
                case "enum":
                    return SettingItemType.Enum;
                case "button":
                    return SettingItemType.Button;
                case "info":
                    return SettingItemType.Info;
                default:
                    return SettingItemType.String;
            }
        }

        private static SettingApplyMode ParseApplyMode(string t)
        {
            if (string.Equals(t, "restart", StringComparison.OrdinalIgnoreCase))
            {
                return SettingApplyMode.Restart;
            }

            return SettingApplyMode.Immediate;
        }

        private static SettingRiskLevel ParseRisk(string t)
        {
            if (string.Equals(t, "danger", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t, "high", StringComparison.OrdinalIgnoreCase))
            {
                return SettingRiskLevel.Danger;
            }

            if (string.Equals(t, "advanced", StringComparison.OrdinalIgnoreCase))
            {
                return SettingRiskLevel.Advanced;
            }

            return SettingRiskLevel.Normal;
        }

        private static SettingTabKind ParseTabKind(string t)
        {
            if (string.Equals(t, "system", StringComparison.OrdinalIgnoreCase))
            {
                return SettingTabKind.System;
            }

            if (string.Equals(t, "advanced", StringComparison.OrdinalIgnoreCase))
            {
                return SettingTabKind.Advanced;
            }

            if (string.Equals(t, "diagnostics", StringComparison.OrdinalIgnoreCase))
            {
                return SettingTabKind.Diagnostics;
            }

            return SettingTabKind.Project;
        }

        private static string ValueToString(object v)
        {
            if (v == null)
            {
                return string.Empty;
            }

            if (v is bool)
            {
                return ((bool)v) ? "true" : "false";
            }

            if (v is double)
            {
                return ((double)v).ToString(CultureInfo.InvariantCulture);
            }

            if (v is long)
            {
                return ((long)v).ToString(CultureInfo.InvariantCulture);
            }

            return v.ToString();
        }

        private static string GetString(Dictionary<string, object> d, string key, string def)
        {
            object v;
            if (!d.TryGetValue(key, out v) || v == null)
            {
                return def;
            }

            return v.ToString();
        }

        private static int GetInt(Dictionary<string, object> d, string key, int def)
        {
            object v;
            if (!d.TryGetValue(key, out v) || v == null)
            {
                return def;
            }

            if (v is long)
            {
                return (int)(long)v;
            }

            if (v is double)
            {
                return (int)(double)v;
            }

            int result;
            return int.TryParse(v.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                ? result
                : def;
        }

        private static float GetFloat(Dictionary<string, object> d, string key, float def)
        {
            object v;
            if (!d.TryGetValue(key, out v) || v == null)
            {
                return def;
            }

            if (v is double)
            {
                return (float)(double)v;
            }

            if (v is long)
            {
                return (long)v;
            }

            float result;
            return float.TryParse(v.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                ? result
                : def;
        }

        private static bool GetBool(Dictionary<string, object> d, string key, bool def)
        {
            object v;
            if (!d.TryGetValue(key, out v) || v == null)
            {
                return def;
            }

            if (v is bool)
            {
                return (bool)v;
            }

            bool result;
            return bool.TryParse(v.ToString(), out result) ? result : def;
        }

        private static Dictionary<string, object> SchemaToDict(SettingSchema schema)
        {
            // 仅用于 StreamingAssets 合并后的中间表示，Manager 直接用 Schema 对象
            List<object> tabs = new List<object>();
            for (int t = 0; t < schema.Tabs.Count; t++)
            {
                SettingTabDef tab = schema.Tabs[t];
                Dictionary<string, object> td = new Dictionary<string, object>();
                td["id"] = tab.Id;
                td["label"] = tab.Label;
                td["order"] = tab.Order;
                td["kind"] = tab.Kind.ToString().ToLowerInvariant();
                List<object> groups = new List<object>();
                for (int g = 0; g < tab.Groups.Count; g++)
                {
                    SettingGroupDef group = tab.Groups[g];
                    Dictionary<string, object> gd = new Dictionary<string, object>();
                    gd["id"] = group.Id;
                    gd["label"] = group.Label;
                    List<object> items = new List<object>();
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        SettingItemDef item = group.Items[i];
                        Dictionary<string, object> id = new Dictionary<string, object>();
                        id["key"] = item.Key;
                        id["type"] = item.Type.ToString().ToLowerInvariant();
                        id["label"] = item.Label;
                        id["description"] = item.Description;
                        id["default"] = item.DefaultValue;
                        id["apply"] = item.ApplyMode == SettingApplyMode.Restart ? "restart" : "immediate";
                        id["risk"] = item.Risk.ToString().ToLowerInvariant();
                        items.Add(id);
                    }

                    gd["items"] = items;
                    groups.Add(gd);
                }

                td["groups"] = groups;
                tabs.Add(td);
            }

            Dictionary<string, object> root = new Dictionary<string, object>();
            root["version"] = schema.Version;
            root["tabs"] = tabs;
            return root;
        }
    }
}
