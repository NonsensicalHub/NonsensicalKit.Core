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
        public static T Clone<T>(this T list) where T : IList
        {
            T clonedList = (T)Activator.CreateInstance(list.GetType());
            foreach (var obj in list)
            {
                clonedList.Add(obj);
            }

            return clonedList;
        }

        public static T[] Clone<T>(this T[] array)
        {
            T[] clonedArray = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                clonedArray[i] = array[i];
            }

            return clonedArray;
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            List<T> clonedList = new List<T>(list.Count);
            foreach (var obj in list)
            {
                clonedList.Add(obj);
            }

            return clonedList;
        }

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

        public static void Add<T>(this IList<T> list, T value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                list.Add(value);
            }
        }

        public static void Fill<T>(this IList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = value;
            }
        }

        public static void FillNew<T>(this IList<T> list) where T : new()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new T();
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

        public static void DicAdd<TKey,  TKey2,TValue>(this Dictionary<TKey, Dictionary<TKey2,TValue>> dictionary, TKey key, TKey2 key2,TValue value)
        {
            if (dictionary.ContainsKey(key)==false)
            {
                dictionary.Add(key, new Dictionary<TKey2, TValue>() );
            }

            if (dictionary[key].ContainsKey(key2))
            {
                dictionary[key][key2]= value;
            }
            else
            {
                dictionary[key].Add(key2, value);
            }
        }
        
        public static bool ListContains<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            return dictionary.TryGetValue(key, out var value1) && value1.Contains(value);
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
