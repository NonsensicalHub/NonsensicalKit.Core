using System;
using System.Collections.Generic;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 字典快捷操作工具类
    /// </summary>
    public static class CollectionTool
    {
        public static T SafeGet<T>(this IList<T> list, int index)
        {
            if (index<0||index>= list.Count)
            {
                return default;
            }
            else
            {
                return list[index];
            }
        }

        public static void ListAdd<Key, Value>(this Dictionary<Key, List<Value>> dictionary, Key key, Value value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<Value>() { value });
            }
        }

        public static void ActionAdd<Key>(this Dictionary<Key, Action> dictionary, Key key, Action value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void ActionAdd<Key, Value>(this Dictionary<Key, Action<Value>> dictionary, Key key, Action<Value> value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void ActionAdd<Key, Value1,Value2>(this Dictionary<Key, Action<Value1,Value2>> dictionary, Key key, Action<Value1, Value2> value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
