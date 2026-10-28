using System;
using System.Collections.Generic;
using NonsensicalKit.Setting.Json;
using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 设置持久化。默认 PlayerPrefs，键带版本号。
    /// </summary>
    public class SettingStore
    {
        private const string StoreKey = "NONSENSICAL_SETTING_V2";
        private const string PendingRestartKey = "NONSENSICAL_SETTING_PENDING_RESTART";
        private const int StoreVersion = 2;

        public Dictionary<string, string> Load()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string raw = PlayerPrefs.GetString(StoreKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return result;
            }

            object root = MiniJson.Deserialize(raw);
            Dictionary<string, object> obj = root as Dictionary<string, object>;
            if (obj == null)
            {
                return result;
            }

            object valuesObj;
            if (!obj.TryGetValue("values", out valuesObj))
            {
                return result;
            }

            Dictionary<string, object> values = valuesObj as Dictionary<string, object>;
            if (values == null)
            {
                return result;
            }

            foreach (KeyValuePair<string, object> kv in values)
            {
                result[kv.Key] = kv.Value != null ? kv.Value.ToString() : string.Empty;
            }

            return result;
        }

        public void Save(Dictionary<string, string> values)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["version"] = StoreVersion;
            Dictionary<string, object> map = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> kv in values)
            {
                map[kv.Key] = kv.Value;
            }

            payload["values"] = map;
            PlayerPrefs.SetString(StoreKey, MiniJson.Serialize(payload));
            PlayerPrefs.Save();
        }

        public List<string> LoadPendingRestartKeys()
        {
            List<string> result = new List<string>();
            string raw = PlayerPrefs.GetString(PendingRestartKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return result;
            }

            object root = MiniJson.Deserialize(raw);
            List<object> list = root as List<object>;
            if (list == null)
            {
                return result;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    result.Add(list[i].ToString());
                }
            }

            return result;
        }

        public void SavePendingRestartKeys(List<string> keys)
        {
            List<object> list = new List<object>();
            if (keys != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    list.Add(keys[i]);
                }
            }

            PlayerPrefs.SetString(PendingRestartKey, MiniJson.Serialize(list));
            PlayerPrefs.Save();
        }

        public void ClearAll()
        {
            PlayerPrefs.DeleteKey(StoreKey);
            PlayerPrefs.DeleteKey(PendingRestartKey);
            // 清理旧版键
            PlayerPrefs.DeleteKey("NONSENSICALSETTING");
            PlayerPrefs.Save();
        }
    }
}
