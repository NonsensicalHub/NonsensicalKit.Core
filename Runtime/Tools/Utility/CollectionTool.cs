using System;
using System.Collections;
using System.Collections.Generic;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 字典快捷操作工具类
    /// </summary>
    public static class CollectionTool
    {
        public static IList Resize(IList list, Type type, int size)
        {
            int delta = size;
            if (list != null)
            {
                delta = size - list.Count;
            }

            bool remove = delta < 0;

            IList newList = (list != null) ? (IList)Activator.CreateInstance(type, list) : (IList)Activator.CreateInstance(type);

            Type elementType = type.GetGenericArguments()[0];

            if (remove)
            {
                for (int i = 0; i < -delta; ++i)
                {
                    newList.RemoveAt(newList.Count - 1);
                }
            }
            else
            {
                for (int i = 0; i < delta; ++i)
                {
                    newList.Add(GetDefault(elementType));
                }
            }

            return newList;
        }

        private static object GetDefault(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        public static void Fill<T>(this IList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = value;
            }
        }

        public static T SafeGet<T>(this IList<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                return default;
            }
            else
            {
                return list[index];
            }
        }

        public static void ListAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<TValue>() { value });
            }
        }

        public static void ActionAdd<TKey>(this Dictionary<TKey, Action> dictionary, TKey key, Action value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] += value;
            }
        }

        public static void ActionAdd<TKey, TValue>(this Dictionary<TKey, Action<TValue>> dictionary, TKey key, Action<TValue> value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] += value;
            }
        }

        public static void ActionAdd<TKey, TValue1, TValue2>(this Dictionary<TKey, Action<TValue1, TValue2>> dictionary, TKey key,
            Action<TValue1, TValue2> value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] += value;
            }
        }
    }
}
