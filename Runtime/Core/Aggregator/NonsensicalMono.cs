using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 简化Aggregator的操作以及自动化注销操作
    /// </summary>
    public abstract class NonsensicalMono : MonoBehaviour
    {
        private readonly List<SubscribeInfo> _subscribeInfos = new();
        private readonly List<RegisterInfo> _registerInfos = new();
        private readonly List<ListenerInfo> _listenerInfos = new();
        private readonly List<HandlerInfo> _handlerInfos = new();

        protected virtual void OnDestroy()
        {
            //TODO:或许可以不使用反射，而是动态管理一个Action来进行自动注销（需要测试是否能正常提前注销）

            foreach (var info in _subscribeInfos)
            {
                // 根据参数数量选择聚合器泛型版本
                Type baseType = info.Types.Length switch
                {
                    0 => typeof(MessageAggregator),
                    1 => typeof(MessageAggregator<>),
                    2 => typeof(MessageAggregator<,>),
                    3 => typeof(MessageAggregator<,,>),
                    _ => null
                };

                Type funcType = info.Types.Length switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>).MakeGenericType(info.Types),
                    2 => typeof(Action<,>).MakeGenericType(info.Types),
                    3 => typeof(Action<,,>).MakeGenericType(info.Types),
                    _ => null
                };
                Type[] ts;
                object[] os;

                if (info.UseInt)
                {
                    if (info.UseID)
                    {
                        ts = new[] { typeof(int), typeof(string), funcType };
                        os = new[] { info.Index, info.ID, info.Func };
                    }
                    else
                    {
                        ts = new[] { typeof(int), funcType };
                        os = new[] { info.Index, info.Func };
                    }
                }
                else
                {
                    if (info.UseID)
                    {
                        ts = new[] { typeof(string), typeof(string), funcType };
                        os = new[] { info.Str, info.ID, info.Func };
                    }
                    else
                    {
                        ts = new[] { typeof(string), funcType };
                        os = new[] { info.Str, info.Func };
                    }
                }

                if (baseType == null) continue;

                AggregatorInvoker.Invoke(
                    baseType,
                    "Unsubscribe",
                    info.Types,
                    ts,
                    os
                );
            }

            _subscribeInfos.Clear();

            foreach (var info in _registerInfos)
            {
                Type[] ts;
                object[] os;

                switch (info.KeyType)
                {
                    case 0: // 类型键
                        ts = new[] { typeof(Func<>).MakeGenericType(info.Type) };
                        os = new[] { info.Func };
                        break;

                    case 1: // 字符串键
                        ts = new[] { typeof(string), typeof(Func<>).MakeGenericType(info.Type) };
                        os = new[] { info.Str, info.Func };
                        break;

                    case 2: // 数字键
                        ts = new[] { typeof(int), typeof(Func<>).MakeGenericType(info.Type) };
                        os = new[] { info.Index, info.Func };
                        break;

                    default:
                        continue;
                }

                AggregatorInvoker.Invoke(
                    typeof(ObjectAggregator<>),
                    "Unregister",
                    new[] { info.Type },
                    ts,
                    os
                );
            }

            _registerInfos.Clear();

            foreach (var info in _listenerInfos)
            {
                Type[] ts;
                object[] os;

                switch (info.KeyType)
                {
                    case 0: // 类型键
                        ts = new[] { typeof(Action<>).MakeGenericType(info.Type) };
                        os = new[] { info.Func };
                        break;

                    case 1: // 字符串键
                        ts = new[] { typeof(string), typeof(Action<>).MakeGenericType(info.Type) };
                        os = new[] { info.Str, info.Func };
                        break;

                    case 2: // 数字键
                        ts = new[] { typeof(int), typeof(Action<>).MakeGenericType(info.Type) };
                        os = new[] { info.Index, info.Func };
                        break;

                    default:
                        continue;
                }

                AggregatorInvoker.Invoke(
                    typeof(ObjectAggregator<>),
                    "RemoveListener",
                    new[] { info.Type },
                    ts,
                    os
                );
            }

            _listenerInfos.Clear();

            foreach (var info in _handlerInfos)
            {
                Type[] genericArgs = info.Types;
                int argCount = genericArgs.Length;

                Type baseAggregatorType;
                switch (argCount)
                {
                    case 1: baseAggregatorType = typeof(MethodAggregator<>); break;
                    case 2: baseAggregatorType = typeof(MethodAggregator<,>); break;
                    case 3: baseAggregatorType = typeof(MethodAggregator<,,>); break;
                    case 4: baseAggregatorType = typeof(MethodAggregator<,,,>); break;
                    default: continue;
                }

                // 构造 Func<...> 类型
                Type funcType;
                if (argCount == 1)
                    funcType = typeof(Func<>).MakeGenericType(genericArgs);
                else if (argCount == 2)
                    funcType = typeof(Func<,>).MakeGenericType(genericArgs);
                else if (argCount == 3)
                    funcType = typeof(Func<,,>).MakeGenericType(genericArgs);
                else
                    funcType = typeof(Func<,,,>).MakeGenericType(genericArgs);

                Type[] ts;
                object[] os;

                if (info.UseInt)
                {
                    if (info.UseID)
                    {
                        ts = new[] { typeof(int), typeof(string), funcType };
                        os = new[] { info.Index, info.ID, info.Func };
                    }
                    else
                    {
                        ts = new[] { typeof(int), funcType };
                        os = new[] { info.Index, info.Func };
                    }
                }
                else
                {
                    if (info.UseID)
                    {
                        ts = new[] { typeof(string), typeof(string), funcType };
                        os = new[] { info.Str, info.ID, info.Func };
                    }
                    else
                    {
                        ts = new[] { typeof(string), funcType };
                        os = new[] { info.Str, info.Func };
                    }
                }

                AggregatorInvoker.Invoke(
                    baseAggregatorType,
                    "RemoveHandler",
                    genericArgs,
                    ts,
                    os
                );
            }

            _handlerInfos.Clear();
        }

        protected void AddSubscribeInfo(SubscribeInfo info)
        {
            _subscribeInfos.Add(info);
        }

        protected void AddRegisterInfo(RegisterInfo info)
        {
            _registerInfos.Add(info);
        }

        protected void AddListenerInfo(ListenerInfo info)
        {
            _listenerInfos.Add(info);
        }

        protected void AddHandlerInfo(HandlerInfo info)
        {
            _handlerInfos.Add(info);
        }

        protected class SubscribeInfo
        {
            public readonly bool UseInt;
            public readonly bool UseID;
            public readonly Type[] Types;
            public readonly int Index;
            public readonly string Str;
            public readonly string ID;
            public readonly object Func;

            public SubscribeInfo(int index, object func, params Type[] types)
            {
                UseID = false;
                UseInt = true;
                Types = types;
                Index = index;
                Func = func;
                ID = null;
                Str = null;
            }

            public SubscribeInfo(string str, object func, params Type[] types)
            {
                UseID = false;
                UseInt = false;
                Types = types;
                Str = str;
                Func = func;
                ID = null;
                Index = 0;
            }

            public SubscribeInfo(int index, string id, object func, params Type[] types)
            {
                UseID = true;
                UseInt = true;
                Types = types;
                Index = index;
                Func = func;
                ID = id;
                Str = null;
            }

            public SubscribeInfo(string str, string id, object func, params Type[] types)
            {
                UseID = true;
                UseInt = false;
                Types = types;
                Str = str;
                Func = func;
                ID = id;
                Index = 0;
            }
        }

        protected class RegisterInfo
        {
            public readonly int KeyType; //0代表类型键，1代表字符串键，2代表数字键
            public readonly int Index;
            public readonly Type Type;
            public readonly string Str;
            public readonly object Func;

            public RegisterInfo(object func, Type type)
            {
                KeyType = 0;
                Index = 0;
                Str = null;
                Func = func;
                Type = type;
            }

            public RegisterInfo(string str, object func, Type type)
            {
                KeyType = 1;
                Index = 0;
                Str = str;
                Func = func;
                Type = type;
            }

            public RegisterInfo(int index, object func, Type type)
            {
                KeyType = 2;
                Index = index;
                Str = null;
                Func = func;
                Type = type;
            }
        }

        protected class ListenerInfo
        {
            public readonly int KeyType; //0代表类型键，1代表字符串键，2代表数字键
            public readonly int Index;
            public readonly Type Type;
            public readonly string Str;
            public readonly object Func;

            public ListenerInfo(object func, Type type)
            {
                KeyType = 0;
                Index = 0;
                Str = null;
                Func = func;
                Type = type;
            }

            public ListenerInfo(string str, object func, Type type)
            {
                KeyType = 1;
                Index = 0;
                Str = str;
                Func = func;
                Type = type;
            }

            public ListenerInfo(int index, object func, Type type)
            {
                KeyType = 2;
                Index = index;
                Str = null;
                Func = func;
                Type = type;
            }
        }

        protected class HandlerInfo
        {
            public readonly bool UseInt;
            public readonly bool UseID;
            public readonly Type[] Types;
            public readonly int Index;
            public readonly string Str;
            public readonly string ID;
            public readonly object Func;

            public HandlerInfo(int index, object func, params Type[] types)
            {
                UseID = false;
                UseInt = true;
                Types = types;
                Index = index;
                Func = func;
                ID = null;
                Str = null;
            }

            public HandlerInfo(string str, object func, params Type[] types)
            {
                UseID = false;
                UseInt = false;
                Types = types;
                Str = str;
                Func = func;
                ID = null;
                Index = 0;
            }

            public HandlerInfo(int index, string id, object func, params Type[] types)
            {
                UseID = true;
                UseInt = true;
                Types = types;
                Index = index;
                Func = func;
                ID = id;
                Str = null;
            }

            public HandlerInfo(string str, string id, object func, params Type[] types)
            {
                UseID = true;
                UseInt = false;
                Types = types;
                Str = str;
                Func = func;
                ID = id;
                Index = 0;
            }
        }

        #region Subscribe

        // --- Subscribe (int key)
        protected void Subscribe<T1, T2, T3>(int index, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func, typeof(T1), typeof(T2), typeof(T3)));
        }

        protected void Subscribe<T1, T2>(int index, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func, typeof(T1), typeof(T2)));
        }

        protected void Subscribe<T>(int index, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func, typeof(T)));
        }

        protected void Subscribe(int index, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func));
        }

        // --- Subscribe (Enum)
        protected void Subscribe<T1, T2, T3>(Enum index, Action<T1, T2, T3> func)
        {
            Subscribe(Convert.ToInt32(index), func);
        }

        protected void Subscribe<T1, T2>(Enum index, Action<T1, T2> func)
        {
            Subscribe(Convert.ToInt32(index), func);
        }

        protected void Subscribe<T>(Enum index, Action<T> func)
        {
            Subscribe(Convert.ToInt32(index), func);
        }

        protected void Subscribe(Enum index, Action func)
        {
            Subscribe(Convert.ToInt32(index), func);
        }

        // --- Subscribe with ID (int)
        protected void Subscribe<T1, T2, T3>(int index, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func, typeof(T1), typeof(T2), typeof(T3)));
        }

        protected void Subscribe<T1, T2>(int index, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func, typeof(T1), typeof(T2)));
        }

        protected void Subscribe<T>(int index, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func, typeof(T)));
        }

        protected void Subscribe(int index, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func));
        }

        protected void Subscribe<T1, T2, T3>(Enum index, string id, Action<T1, T2, T3> func)
        {
            Subscribe(Convert.ToInt32(index), id, func);
        }

        protected void Subscribe<T1, T2>(Enum index, string id, Action<T1, T2> func)
        {
            Subscribe(Convert.ToInt32(index), id, func);
        }

        protected void Subscribe<T>(Enum index, string id, Action<T> func)
        {
            Subscribe(Convert.ToInt32(index), id, func);
        }

        protected void Subscribe(Enum index, string id, Action func)
        {
            Subscribe(Convert.ToInt32(index), id, func);
        }


        // --- Subscribe (string key)
        protected void Subscribe<T1, T2, T3>(string str, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func, typeof(T1), typeof(T2), typeof(T3)));
        }

        protected void Subscribe<T1, T2>(string str, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func, typeof(T1), typeof(T2)));
        }

        protected void Subscribe<T>(string str, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func, typeof(T)));
        }

        protected void Subscribe(string str, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func));
        }

        // --- Subscribe (string + id)
        protected void Subscribe<T1, T2, T3>(string str, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func, typeof(T1), typeof(T2), typeof(T3)));
        }

        protected void Subscribe<T1, T2>(string str, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func, typeof(T1), typeof(T2)));
        }

        protected void Subscribe<T>(string str, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func, typeof(T)));
        }

        protected void Subscribe(string str, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func));
        }

        #endregion

        #region Unsubscribe

        protected void Unsubscribe<T1, T2, T3>(int index, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && func == (Action<T1, T2, T3>)info.Func);
        }

        protected void Unsubscribe<T1, T2>(int index, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && func == (Action<T1, T2>)info.Func);
        }

        protected void Unsubscribe<T>(int index, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && func == (Action<T>)info.Func);
        }

        protected void Unsubscribe(int index, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(index, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && func == (Action)info.Func);
        }

        protected void Unsubscribe<T1, T2, T3>(Enum index, Action<T1, T2, T3> func)
        {
            Unsubscribe(Convert.ToInt32(index), func);
        }

        protected void Unsubscribe<T1, T2>(Enum index, Action<T1, T2> func)
        {
            Unsubscribe(Convert.ToInt32(index), func);
        }

        protected void Unsubscribe<T>(Enum index, Action<T> func)
        {
            Unsubscribe(Convert.ToInt32(index), func);
        }

        protected void Unsubscribe(Enum index, Action func)
        {
            Unsubscribe(Convert.ToInt32(index), func);
        }

        protected void Unsubscribe<T1, T2, T3>(int index, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, id, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id &&
                func == (Action<T1, T2, T3>)info.Func);
        }

        protected void Unsubscribe<T1, T2>(int index, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, id, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id && func == (Action<T1, T2>)info.Func);
        }

        protected void Unsubscribe<T>(int index, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, id, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id && func == (Action<T>)info.Func);
        }

        protected void Unsubscribe(int index, string id, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(index, id, func);
            RemoveSubscribeInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id && func == (Action)info.Func);
        }

        protected void Unsubscribe<T1, T2, T3>(Enum index, string id, Action<T1, T2, T3> func)
        {
            Unsubscribe(Convert.ToInt32(index), id, func);
        }

        protected void Unsubscribe<T1, T2>(Enum index, string id, Action<T1, T2> func)
        {
            Unsubscribe(Convert.ToInt32(index), id, func);
        }

        protected void Unsubscribe<T>(Enum index, string id, Action<T> func)
        {
            Unsubscribe(Convert.ToInt32(index), id, func);
        }

        protected void Unsubscribe(Enum index, string id, Action func)
        {
            Unsubscribe(Convert.ToInt32(index), id, func);
        }

        protected void Unsubscribe<T1, T2, T3>(string str, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(str, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && func == (Action<T1, T2, T3>)info.Func);
        }

        protected void Unsubscribe<T1, T2>(string str, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(str, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && func == (Action<T1, T2>)info.Func);
        }

        protected void Unsubscribe<T>(string str, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(str, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && func == (Action<T>)info.Func);
        }

        protected void Unsubscribe(string str, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(str, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && func == (Action)info.Func);
        }

        protected void Unsubscribe<T1, T2, T3>(string str, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(str, id, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id &&
                func == (Action<T1, T2, T3>)info.Func);
        }

        protected void Unsubscribe<T1, T2>(string str, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(str, id, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id && func == (Action<T1, T2>)info.Func);
        }

        protected void Unsubscribe<T>(string str, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(str, id, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id && func == (Action<T>)info.Func);
        }

        protected void Unsubscribe(string str, string id, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(str, id, func);
            RemoveSubscribeInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id && func == (Action)info.Func);
        }

        private void RemoveSubscribeInfoMatch(Predicate<SubscribeInfo> match)
        {
            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (match(_subscribeInfos[i]))
                {
                    _subscribeInfos.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region Publish

        protected void Publish<T1, T2, T3>(int index, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(index, data1, data2, data3);
        }

        protected void Publish<T1, T2>(int index, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(index, data1, data2);
        }

        protected void Publish<T>(int index, T data)
        {
            MessageAggregator<T>.Instance.Publish(index, data);
        }

        protected void Publish(int index)
        {
            MessageAggregator.Instance.Publish(index);
        }

        protected void Publish<T1, T2, T3>(Enum index, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(Convert.ToInt32(index), data1, data2, data3);
        }

        protected void Publish<T1, T2>(Enum index, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(Convert.ToInt32(index), data1, data2);
        }

        protected void Publish<T>(Enum index, T data)
        {
            MessageAggregator<T>.Instance.Publish(Convert.ToInt32(index), data);
        }

        protected void Publish(Enum index)
        {
            MessageAggregator.Instance.Publish(Convert.ToInt32(index));
        }

        protected void PublishWithID<T1, T2, T3>(Enum index, string id, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(Convert.ToInt32(index), id, data1, data2, data3);
        }

        protected void PublishWithID<T1, T2>(Enum index, string id, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(Convert.ToInt32(index), id, data1, data2);
        }

        protected void PublishWithID<T>(Enum index, string id, T data)
        {
            MessageAggregator<T>.Instance.PublishWithID(Convert.ToInt32(index), id, data);
        }

        protected void PublishWithID(Enum index, string id)
        {
            MessageAggregator.Instance.PublishWithID(Convert.ToInt32(index), id);
        }

        protected void PublishWithID<T1, T2, T3>(int index, string id, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(index, id, data1, data2, data3);
        }

        protected void PublishWithID<T1, T2>(int index, string id, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(index, id, data1, data2);
        }

        protected void PublishWithID<T>(int index, string id, T data)
        {
            MessageAggregator<T>.Instance.PublishWithID(index, id, data);
        }

        protected void PublishWithID(int index, string id)
        {
            MessageAggregator.Instance.PublishWithID(index, id);
        }

        protected void Publish<T1, T2, T3>(string str, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(str, data1, data2, data3);
        }

        protected void Publish<T1, T2>(string str, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(str, data1, data2);
        }

        protected void Publish<T>(string str, T data)
        {
            MessageAggregator<T>.Instance.Publish(str, data);
        }

        protected void Publish(string str)
        {
            MessageAggregator.Instance.Publish(str);
        }

        protected void PublishWithID<T1, T2, T3>(string str, string id, T1 data1, T2 data2, T3 data3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(str, id, data1, data2, data3);
        }

        protected void PublishWithID<T1, T2>(string str, string id, T1 data1, T2 data2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(str, id, data1, data2);
        }

        protected void PublishWithID<T>(string str, string id, T data)
        {
            MessageAggregator<T>.Instance.PublishWithID(str, id, data);
        }

        protected void PublishWithID(string str, string id)
        {
            MessageAggregator.Instance.PublishWithID(str, id);
        }

        #endregion

        #region Register / Unregister (ObjectAggregator)

        protected void Register<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(func);
            _registerInfos.Add(new RegisterInfo(func, typeof(T)));
        }

        protected void Register<T>(string str, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(str, func);
            _registerInfos.Add(new RegisterInfo(str, func, typeof(T)));
        }

        protected void Register<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(index, func);
            _registerInfos.Add(new RegisterInfo(index, func, typeof(T)));
        }

        protected void Register<T>(Enum index, Func<T> func)
        {
            Register(Convert.ToInt32(index), func);
        }

        protected void Unregister<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(func);
            RemoveRegisterInfoMatch(info => info.KeyType == 0 && info.Type == typeof(T) && (Func<T>)info.Func == func);
        }

        protected void Unregister<T>(string str, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(str, func);
            RemoveRegisterInfoMatch(info =>
                info.KeyType == 1 && info.Type == typeof(T) && info.Str == str && (Func<T>)info.Func == func);
        }

        protected void Unregister<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(index, func);
            RemoveRegisterInfoMatch(info =>
                info.KeyType == 2 && info.Type == typeof(T) && info.Index == index && (Func<T>)info.Func == func);
        }

        protected void Unregister<T>(Enum index, Func<T> func)
        {
            Unregister(Convert.ToInt32(index), func);
        }

        private void RemoveRegisterInfoMatch(Predicate<RegisterInfo> match)
        {
            for (int i = 0; i < _registerInfos.Count; i++)
            {
                if (match(_registerInfos[i]))
                {
                    _registerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region AddListener / RemoveListener (ObjectAggregator)

        protected void AddListener<T>(Action<T> listener)
        {
            ObjectAggregator<T>.Instance.AddListener(listener);
            _listenerInfos.Add(new ListenerInfo(listener, typeof(T)));
        }

        protected void AddListener<T>(string str, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.AddListener(str, listener);
            _listenerInfos.Add(new ListenerInfo(str, listener, typeof(T)));
        }

        protected void AddListener<T>(int index, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.AddListener(index, listener);
            _listenerInfos.Add(new ListenerInfo(index, listener, typeof(T)));
        }

        protected void AddListener<T>(Enum index, Action<T> listener)
        {
            AddListener(Convert.ToInt32(index), listener);
        }

        protected void RemoveListener<T>(Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 0 && info.Type == typeof(T) && (Action<T>)info.Func == listener);
        }

        protected void RemoveListener<T>(string str, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(str, listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 1 && info.Type == typeof(T) && info.Str == str && (Action<T>)info.Func == listener);
        }

        protected void RemoveListener<T>(int index, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(index, listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 2 && info.Type == typeof(T) && info.Index == index && (Action<T>)info.Func == listener);
        }

        protected void RemoveListener<T>(Enum index, Action<T> listener)
        {
            RemoveListener(Convert.ToInt32(index), listener);
        }

        private void RemoveListenerInfoMatch(Predicate<ListenerInfo> match)
        {
            for (int i = 0; i < _listenerInfos.Count; i++)
            {
                if (match(_listenerInfos[i]))
                {
                    _listenerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region AddHandler

//
// AddHandler - 1 generic (Func<TResult>)
//
        protected void AddHandler<TResult>(int index, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(index, handler);
            _handlerInfos.Add(new HandlerInfo(index, handler, typeof(TResult)));
        }

        protected void AddHandler<TResult>(string str, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler, typeof(TResult)));
        }

        protected void AddHandler<TResult>(int index, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler, typeof(TResult)));
        }

        protected void AddHandler<TResult>(string str, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler, typeof(TResult)));
        }

        protected void AddHandler<TResult>(Enum index, Func<TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TResult>(Enum index, string id, Func<TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

//
// AddHandler - 2 generics (Func<TValue, TResult>)
//
        protected void AddHandler<TValue, TResult>(int index, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(index, handler);
            _handlerInfos.Add(new HandlerInfo(index, handler, typeof(TValue), typeof(TResult)));
        }

        protected void AddHandler<TValue, TResult>(string str, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler, typeof(TValue), typeof(TResult)));
        }

        protected void AddHandler<TValue, TResult>(int index, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler, typeof(TValue), typeof(TResult)));
        }

        protected void AddHandler<TValue, TResult>(string str, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler, typeof(TValue), typeof(TResult)));
        }

        protected void AddHandler<TValue, TResult>(Enum index, Func<TValue, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TValue, TResult>(Enum index, string id, Func<TValue, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

//
// AddHandler - 3 generics (Func<T1, T2, TResult>)
//
        protected void AddHandler<T1, T2, TResult>(int index, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(index, handler);
            _handlerInfos.Add(new HandlerInfo(index, handler, typeof(T1), typeof(T2), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, TResult>(string str, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler, typeof(T1), typeof(T2), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, TResult>(int index, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler, typeof(T1), typeof(T2), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, TResult>(string str, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler, typeof(T1), typeof(T2), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, TResult>(Enum index, Func<T1, T2, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<T1, T2, TResult>(Enum index, string id, Func<T1, T2, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

//
// AddHandler - 4 generics (Func<T1, T2, T3, TResult>)
//
        protected void AddHandler<T1, T2, T3, TResult>(int index, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(index, handler);
            _handlerInfos.Add(new HandlerInfo(index, handler, typeof(T1), typeof(T2), typeof(T3), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(string str, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler, typeof(T1), typeof(T2), typeof(T3), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(int index, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler, typeof(T1), typeof(T2), typeof(T3), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(string str, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler, typeof(T1), typeof(T2), typeof(T3), typeof(TResult)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(Enum index, Func<T1, T2, T3, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<T1, T2, T3, TResult>(Enum index, string id, Func<T1, T2, T3, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

        #endregion

        #region RemoveHandler

//
// RemoveHandler - 1 generic
//
        protected void RemoveHandler<TResult>(int index, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(index, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && handler == (info.Func as Func<TResult>));
        }

        protected void RemoveHandler<TResult>(string str, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(str, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && handler == (info.Func as Func<TResult>));
        }

        protected void RemoveHandler<TResult>(int index, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(index, id, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id &&
                handler == (info.Func as Func<TResult>));
        }

        protected void RemoveHandler<TResult>(string str, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(str, id, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id &&
                handler == (info.Func as Func<TResult>));
        }

        protected void RemoveHandler<TResult>(Enum index, Func<TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TResult>(Enum index, string id, Func<TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

//
// RemoveHandler - 2 generics
//
        protected void RemoveHandler<TValue, TResult>(int index, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && handler == (info.Func as Func<TValue, TResult>));
        }

        protected void RemoveHandler<TValue, TResult>(string str, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && handler == (info.Func as Func<TValue, TResult>));
        }

        protected void RemoveHandler<TValue, TResult>(int index, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, id, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id &&
                handler == (info.Func as Func<TValue, TResult>));
        }

        protected void RemoveHandler<TValue, TResult>(string str, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, id, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id &&
                handler == (info.Func as Func<TValue, TResult>));
        }

        protected void RemoveHandler<TValue, TResult>(Enum index, Func<TValue, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TValue, TResult>(Enum index, string id, Func<TValue, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

//
// RemoveHandler - 3 generics
//
        protected void RemoveHandler<T1, T2, TResult>(int index, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(index, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index && handler == (info.Func as Func<T1, T2, TResult>));
        }

        protected void RemoveHandler<T1, T2, TResult>(string str, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(str, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && handler == (info.Func as Func<T1, T2, TResult>));
        }

        protected void RemoveHandler<T1, T2, TResult>(int index, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(index, id, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id &&
                handler == (info.Func as Func<T1, T2, TResult>));
        }

        protected void RemoveHandler<T1, T2, TResult>(string str, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(str, id, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id &&
                handler == (info.Func as Func<T1, T2, TResult>));
        }

        protected void RemoveHandler<T1, T2, TResult>(Enum index, Func<T1, T2, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<T1, T2, TResult>(Enum index, string id, Func<T1, T2, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

//
// RemoveHandler - 4 generics
//
        protected void RemoveHandler<T1, T2, T3, TResult>(int index, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(index, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && !info.UseID && info.Index == index &&
                handler == (info.Func as Func<T1, T2, T3, TResult>));
        }

        protected void RemoveHandler<T1, T2, T3, TResult>(string str, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(str, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && !info.UseID && info.Str == str && handler == (info.Func as Func<T1, T2, T3, TResult>));
        }

        protected void RemoveHandler<T1, T2, T3, TResult>(int index, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(index, id, handler);
            RemoveHandlerInfoMatch(info =>
                info.UseInt && info.UseID && info.Index == index && info.ID == id &&
                handler == (info.Func as Func<T1, T2, T3, TResult>));
        }

        protected void RemoveHandler<T1, T2, T3, TResult>(string str, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(str, id, handler);
            RemoveHandlerInfoMatch(info =>
                !info.UseInt && info.UseID && info.Str == str && info.ID == id &&
                handler == (info.Func as Func<T1, T2, T3, TResult>));
        }

        protected void RemoveHandler<T1, T2, T3, TResult>(Enum index, Func<T1, T2, T3, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<T1, T2, T3, TResult>(Enum index, string id, Func<T1, T2, T3, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

        private void RemoveHandlerInfoMatch(Predicate<HandlerInfo> match)
        {
            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (match(_handlerInfos[i]))
                {
                    _handlerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region Execute

//
// Execute - 1 generic (Func<TResult>)
//
        protected TResult Execute<TResult>(int index) => MethodAggregator<TResult>.Instance.Execute(index);

        protected TResult Execute<TResult>(string str) => MethodAggregator<TResult>.Instance.Execute(str);

        protected TResult Execute<TResult>(int index, string id) =>
            MethodAggregator<TResult>.Instance.ExecuteWithID(index, id);

        protected TResult Execute<TResult>(string str, string id) =>
            MethodAggregator<TResult>.Instance.ExecuteWithID(str, id);

        protected TResult Execute<TResult>(Enum index) =>
            MethodAggregator<TResult>.Instance.Execute(Convert.ToInt32(index));

        protected TResult Execute<TResult>(Enum index, string id) =>
            MethodAggregator<TResult>.Instance.ExecuteWithID(Convert.ToInt32(index), id);

        //
// Execute - 2 generics (Func<TValue, TResult>)
//
        protected TResult Execute<TValue, TResult>(int index, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.Execute(index, value);

        protected TResult Execute<TValue, TResult>(Enum index, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.Execute(Convert.ToInt32(index), value);

        protected TResult Execute<TValue, TResult>(string str, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.Execute(str, value);

        protected TResult Execute<TValue, TResult>(int index, string id, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.ExecuteWithID(index, id, value);

        protected TResult Execute<TValue, TResult>(string str, string id, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.ExecuteWithID(str, id, value);

        protected TResult Execute<TValue, TResult>(Enum index, string id, TValue value) =>
            MethodAggregator<TValue, TResult>.Instance.ExecuteWithID(Convert.ToInt32(index), id, value);

        //
// Execute - 3 generics (Func<T1, T2, TResult>)
//
        protected TResult Execute<T1, T2, TResult>(int index, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.Execute(index, v1, v2);

        protected TResult Execute<T1, T2, TResult>(Enum index, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.Execute(Convert.ToInt32(index), v1, v2);

        protected TResult Execute<T1, T2, TResult>(string str, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.Execute(str, v1, v2);

        protected TResult Execute<T1, T2, TResult>(int index, string id, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.ExecuteWithID(index, id, v1, v2);

        protected TResult Execute<T1, T2, TResult>(Enum index, string id, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.ExecuteWithID(Convert.ToInt32(index), id, v1, v2);

        protected TResult Execute<T1, T2, TResult>(string str, string id, T1 v1, T2 v2) =>
            MethodAggregator<T1, T2, TResult>.Instance.ExecuteWithID(str, id, v1, v2);

        //
// Execute - 4 generics (Func<T1, T2, T3, TResult>)
//
        protected TResult Execute<T1, T2, T3, TResult>(int index, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.Execute(index, v1, v2, v3);

        protected TResult Execute<T1, T2, T3, TResult>(Enum index, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.Execute(Convert.ToInt32(index), v1, v2, v3);


        protected TResult Execute<T1, T2, T3, TResult>(string str, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.Execute(str, v1, v2, v3);

        protected TResult Execute<T1, T2, T3, TResult>(int index, string id, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.ExecuteWithID(index, id, v1, v2, v3);

        protected TResult Execute<T1, T2, T3, TResult>(Enum index, string id, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.ExecuteWithID(Convert.ToInt32(index), id, v1, v2, v3);

        protected TResult Execute<T1, T2, T3, TResult>(string str, string id, T1 v1, T2 v2, T3 v3) =>
            MethodAggregator<T1, T2, T3, TResult>.Instance.ExecuteWithID(str, id, v1, v2, v3);

        #endregion

        #region AggregatorInvoker helper (reflection + caching)

        // A small helper that centralizes reflection invocation for aggregator types.
        // Caches MethodInfo to reduce repeated reflection cost.
        internal static class AggregatorInvoker
        {
            // Simple cache for MethodInfo lookup: key => MethodInfo
            // Key format: aggregatorFullName|methodName|paramType1;paramType2;...
            private static readonly Dictionary<string, MethodInfo> MethodCache = new Dictionary<string, MethodInfo>();

            public static void Invoke(Type genericBaseType, string methodName, Type[] genericArgs, Type[] paramTypes,
                object[] parameters)
            {
                if (genericBaseType == null)
                {
                    Debug.LogWarning("AggregatorInvoker: genericBaseType is null.");
                    return;
                }

                try
                {
                    Type targetType;
                    if (genericBaseType.IsGenericTypeDefinition)
                    {
                        // MakeGenericType - ensure genericArgs provided
                        if (genericArgs == null || genericArgs.Length == 0)
                        {
                            // if base is generic but no args given, we cannot make it
                            Debug.LogWarning($"AggregatorInvoker: generic args required for {genericBaseType}.");
                            return;
                        }

                        targetType = genericBaseType.MakeGenericType(genericArgs);
                    }
                    else
                    {
                        targetType = genericBaseType;
                    }

                    // Get instance
                    PropertyInfo instField = targetType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                    if (instField == null)
                    {
                        Debug.LogWarning($"AggregatorInvoker: Instance field not found on {targetType}.");
                        return;
                    }

                    object instance = instField.GetValue(null);
                    if (instance == null)
                    {
                        Debug.LogWarning($"AggregatorInvoker: Instance is null for {targetType}.");
                        return;
                    }

                    // Build cache key
                    string key = BuildCacheKey(targetType, methodName, paramTypes);

                    MethodInfo method;
                    lock (MethodCache)
                    {
                        if (!MethodCache.TryGetValue(key, out method))
                        {
                            method = targetType.GetMethod(methodName, paramTypes);
                            if (method == null)
                            {
                                Debug.LogWarning(
                                    $"AggregatorInvoker: Method {methodName} not found on {targetType} with specified parameter signature.");
                                MethodCache[key] = null; // remember miss to avoid repeated attempts
                            }
                            else
                            {
                                MethodCache[key] = method;
                            }
                        }
                    }

                    if (method == null)
                    {
                        // already warned above
                        return;
                    }

                    method.Invoke(instance, parameters);
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogError(
                        $"AggregatorInvoker: target invocation exception invoking {methodName} on {genericBaseType}: {tie.InnerException?.Message ?? tie.Message}\n{tie.InnerException?.StackTrace}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"AggregatorInvoker: error invoking {methodName} on {genericBaseType}: {e}");
                }
            }

            private static string BuildCacheKey(Type targetType, string methodName, Type[] paramTypes)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(targetType.FullName);
                sb.Append("|");
                sb.Append(methodName);
                sb.Append("|");
                if (paramTypes != null)
                {
                    for (int i = 0; i < paramTypes.Length; i++)
                    {
                        if (paramTypes[i] == null) sb.Append("null");
                        else sb.Append(paramTypes[i].AssemblyQualifiedName);
                        if (i < paramTypes.Length - 1) sb.Append(";");
                    }
                }

                return sb.ToString();
            }
        }

        #endregion
    }
}
