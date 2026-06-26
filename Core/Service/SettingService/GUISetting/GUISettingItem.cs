using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NonsensicalKit.Core;
using UnityEngine;

namespace  NonsensicalKit.Core.Service.Setting
{
    // 定义设置项的类型枚举 (用于内部处理)
    public enum SettingItemTypeInternal
    {
        Bool,
        String,
        Float, // 使用 Float 替代 Int
        Enum,
        Button
    }

    [Serializable]
    public class GUISettingItem
    {
        public string key;
        public string type; // JSON中的字符串类型 ("bool", "float", etc.)
        public string label; // 显示名称
        public string description; // 描述信息
        public string value;
        public Int2 range; // float范围
    
        // Enum 特有
        public List<string> options = new List<string>();
        public string buttonAction = "";

        [JsonIgnore] public bool boolValue;
        [JsonIgnore] public float floatValue;
        [JsonIgnore] public string stringValue = "";


        [NonSerialized] // 运行时计算，不序列化到JSON
        public int SelectedOptionIndex = 0;


        // 内部使用的类型枚举 (方便代码处理)
        [NonSerialized] public SettingItemTypeInternal InternalType;

        // 构造函数 (可选)
        public GUISettingItem(string key, string label, string type)
        {
            this.key = key;
            this.label = label;
            this.type = type.ToLowerInvariant(); // 统一转小写便于比较

            // 初始化内部类型
            switch (this.type)
            {
                case "bool": InternalType = SettingItemTypeInternal.Bool; break;
                case "string": InternalType = SettingItemTypeInternal.String; break;
                case "float": InternalType = SettingItemTypeInternal.Float; break;
                case "enum": InternalType = SettingItemTypeInternal.Enum; break;
                case "button": InternalType = SettingItemTypeInternal.Button; break;
                default:
                    InternalType = SettingItemTypeInternal.String;
                    Debug.LogWarning($"Unknown setting type '{this.type}' for key '{key}', defaulting to String.");
                    break;
            }
        }

        public GUISettingItem()
        {
        }

        /// <summary>
        /// 根据Item的类型设置其内部值
        /// </summary>
        /// <param name="jsonValue"></param>
        public void SetInitialValue(object jsonValue)
        {
            switch (InternalType)
            {
                case SettingItemTypeInternal.Bool:
                    if (jsonValue is bool b) boolValue = b;
                    else if (jsonValue is string s && bool.TryParse(s, out bool parsedBool)) boolValue = parsedBool;
                    else Debug.LogWarning($"Failed to parse boolean value for key '{key}'. Value: {jsonValue}");
                    break;
                case SettingItemTypeInternal.Float:
                    if (jsonValue is double d) floatValue = (float)d; // JSON.NET 默认将数字解析为 double
                    else if (jsonValue is long l) floatValue = l; // 整数
                    else if (jsonValue is string s && float.TryParse(s, out float parsedFloat)) floatValue = parsedFloat;
                    else Debug.LogWarning($"Failed to parse float value for key '{key}'. Value: {jsonValue}");
                    break;
                case SettingItemTypeInternal.String:
                    if (jsonValue != null) stringValue = jsonValue.ToString();
                    break;
                case SettingItemTypeInternal.Enum:
                    if (jsonValue != null)
                    {
                        string valStr = jsonValue.ToString();
                        stringValue = valStr; // stringValue 也存储选中的字符串
                        if (options.Contains(valStr))
                        {
                            SelectedOptionIndex = options.IndexOf(valStr);
                        }
                        else
                        {
                            Debug.LogWarning($"Enum value '{valStr}' not found in options for key '{key}'. Defaulting to first option.");
                            SelectedOptionIndex = 0;
                            if (options.Count > 0) stringValue = options[0];
                        }
                    }

                    break;
                case SettingItemTypeInternal.Button:
                    // Button 不存储 value
                    break;
            }
        }

        // 辅助方法：获取要保存到 JSON 的 value 对象
        public object GetCacheValue()
        {
            switch (InternalType)
            {
                case SettingItemTypeInternal.Bool: return boolValue;
                case SettingItemTypeInternal.Float: return floatValue;
                case SettingItemTypeInternal.String: return stringValue;
                case SettingItemTypeInternal.Enum: return stringValue; // 保存选中的字符串
                case SettingItemTypeInternal.Button: return null; // Button 不保存 value
                default: return stringValue;
            }
        }
    
        public GUISettingItem Clone()
        {
            return new GUISettingItem
            {
                key = this.key,
                value = this.value,
                type = this.type,
                InternalType = this.InternalType,
                boolValue = this.boolValue,
                stringValue = this.stringValue,
                floatValue = this.floatValue,
                options = this.options != null ? new List<string>(this.options) : null,
                SelectedOptionIndex = this.SelectedOptionIndex
            };
        }
    }
}