using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class ReflectionTool
    {
        private static Assembly[] _assemblyBuffer;

        /// <summary>
        /// 通过类型名从所有程序集中获取类型对象
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            Type type;
            for (int i = 0; i < _assemblyBuffer.Length; i++)
            {
                type = _assemblyBuffer[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取继承了某个类的所有非抽象类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetConcreteTypes<T>()
        {
            return GetConcreteTypes(typeof(T));
        }

        /// <summary>
        /// 获取继承了某个类的所有非抽象类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetConcreteTypes(Type type)
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            List<Type> types = new List<Type>();
            foreach (var assembly in _assemblyBuffer)
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                {
                    foreach (Type t in assemblyTypes)
                    {
                        if (type.IsAssignableFrom(t) && !t.IsAbstract)
                        {
                            types.Add(t);
                        }
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// 获取继承了某两个类型的所有非抽象类
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static List<Type> GetConcreteTypes<T1, T2>()
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            List<Type> types = new List<Type>();
            foreach (var assembly in _assemblyBuffer)
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                {
                    foreach (Type t in assemblyTypes)
                    {
                        if (t.IsAbstract)
                        {
                            continue;
                        }

                        if (typeof(T1).IsAssignableFrom(t) == false)
                        {
                            continue;
                        }

                        if (typeof(T2).IsAssignableFrom(t) == false)
                        {
                            continue;
                        }

                        types.Add(t);
                    }
                }
            }

            return types;
        }

        public static List<Type> GetEnumByAttribute<T>() where T : Attribute
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            List<Type> enums = new List<Type>();
            foreach (var assembly in _assemblyBuffer)
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                {
                    foreach (Type t in assemblyTypes)
                    {
                        if (t.IsEnum && t.GetCustomAttribute(typeof(T)) != null)
                        {
                            enums.Add(t);
                        }
                    }
                }
            }

            return enums;
        }


        /// <summary>
        /// 获取继承了某个类的所有非抽象类的类名字符串
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetConcreteTypesString(Type type)
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            List<string> types = new List<string>();
            foreach (var assembly in _assemblyBuffer)
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                {
                    foreach (Type t in assemblyTypes)
                    {
                        if (type.IsAssignableFrom(t) && !t.IsAbstract)
                        {
                            types.Add(t.Name);
                        }
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// 获取继承了某个类的所有非抽象类的类名字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetConcreteTypesString<T>()
        {
            return GetConcreteTypesString(typeof(T));
        }

        /// <summary>
        /// 获取继承了某两个类的所有非抽象类的类名字符串
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static List<string> GetConcreteTypesString<T1, T2>()
        {
            if (_assemblyBuffer == null)
            {
                _assemblyBuffer = AppDomain.CurrentDomain.GetAssemblies();
            }

            List<string> types = new List<string>();
            foreach (var assembly in _assemblyBuffer)
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Could not load types from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                {
                    foreach (Type t in assemblyTypes)
                    {
                        if (t.IsAbstract)
                        {
                            continue;
                        }

                        if (typeof(T1).IsAssignableFrom(t) == false)
                        {
                            continue;
                        }

                        if (typeof(T2).IsAssignableFrom(t) == false)
                        {
                            continue;
                        }

                        types.Add(t.Name);
                    }
                }
            }

            return types;
        }
    }
}
