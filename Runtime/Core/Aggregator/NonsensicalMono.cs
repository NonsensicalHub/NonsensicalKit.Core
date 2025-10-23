using System;
using System.Collections.Generic;
using System.Reflection;
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
            foreach (var subscribeInfo in _subscribeInfos)
            {
                bool isInt = subscribeInfo.UseInt;
                bool useID = subscribeInfo.UseID;
                Type[] types = subscribeInfo.Types;
                Type messageAggregator;
                object instance;
                Type action;
                switch (types.Length)
                {
                    case 0:
                    {
                        messageAggregator = typeof(MessageAggregator);
                        instance = messageAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        action = typeof(Action);
                    }
                        break;
                    case 1:
                    {
                        messageAggregator = typeof(MessageAggregator<>).MakeGenericType(types);
                        instance = messageAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        action = typeof(Action<>).MakeGenericType(types);
                    }
                        break;
                    case 2:
                    {
                        messageAggregator = typeof(MessageAggregator<,>).MakeGenericType(types);
                        instance = messageAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        action = typeof(Action<,>).MakeGenericType(types);
                    }
                        break;
                    case 3:
                    {
                        messageAggregator = typeof(MessageAggregator<,,>).MakeGenericType(types);
                        instance = messageAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        action = typeof(Action<,,>).MakeGenericType(types);
                    }
                        break;
                    default:
                        continue;
                }

                Type[] ts = new Type[useID ? 3 : 2];
                object[] os = new object[useID ? 3 : 2];
                if (isInt)
                {
                    ts[0] = typeof(int);
                    os[0] = subscribeInfo.Index;
                }
                else
                {
                    ts[0] = typeof(string);
                    os[0] = subscribeInfo.Str;
                }

                if (useID)
                {
                    ts[1] = typeof(string);
                    ts[2] = action;
                    os[1] = subscribeInfo.ID;
                    os[2] = subscribeInfo.Func;
                }
                else
                {
                    ts[1] = action;
                    os[1] = subscribeInfo.Func;
                }

                MethodInfo unsubMethod = messageAggregator.GetMethod("Unsubscribe", ts);
                unsubMethod.Invoke(instance, os);
            }

            _subscribeInfos.Clear();
            foreach (var registerInfo in _registerInfos)
            {
                int keytype = registerInfo.KeyType;
                Type type = registerInfo.Type;

                var objectAggregator = typeof(ObjectAggregator<>).MakeGenericType(type);
                var instance = objectAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                    .GetValue(null);
                var func = typeof(Func<>).MakeGenericType(type);

                Type[] ts = new Type[keytype == 0 ? 1 : 2];
                object[] os = new object[keytype == 0 ? 1 : 2];
                if (keytype == 0)
                {
                    ts[0] = func;
                    os[0] = registerInfo.Func;
                }
                else if (keytype == 1)
                {
                    ts[0] = typeof(string);
                    os[0] = registerInfo.Str;

                    ts[1] = func;
                    os[1] = registerInfo.Func;
                }
                else
                {
                    ts[0] = typeof(int);
                    os[0] = registerInfo.Index;

                    ts[1] = func;
                    os[1] = registerInfo.Func;
                }

                MethodInfo unregister = objectAggregator.GetMethod("Unregister", ts);
                unregister.Invoke(instance, os);
            }

            _registerInfos.Clear();
            foreach (var listenerInfo in _listenerInfos)
            {
                int keytype = listenerInfo.KeyType;
                Type type = listenerInfo.Type;

                var objectAggregator = typeof(ObjectAggregator<>).MakeGenericType(type);
                var instance = objectAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                    .GetValue(null);
                var action = typeof(Action<>).MakeGenericType(type);

                Type[] ts = new Type[keytype == 0 ? 1 : 2];
                object[] os = new object[keytype == 0 ? 1 : 2];
                if (keytype == 0)
                {
                    ts[0] = action;
                    os[0] = listenerInfo.Func;
                }
                else if (keytype == 1)
                {
                    ts[0] = typeof(string);
                    os[0] = listenerInfo.Str;

                    ts[1] = action;
                    os[1] = listenerInfo.Func;
                }
                else
                {
                    ts[0] = typeof(int);
                    os[0] = listenerInfo.Index;

                    ts[1] = action;
                    os[1] = listenerInfo.Func;
                }

                MethodInfo removeListener = objectAggregator.GetMethod("RemoveListener", ts);
                removeListener.Invoke(instance, os);
            }

            _listenerInfos.Clear();
            foreach (var handlerInfos in _handlerInfos)
            {
                bool isInt = handlerInfos.UseInt;
                bool useID = handlerInfos.UseID;
                Type[] types = handlerInfos.Types;
                Type methodAggregator;
                object instance;
                Type func;
                switch (types.Length)
                {
                    case 1:
                    {
                        methodAggregator = typeof(MethodAggregator<>).MakeGenericType(types);
                        instance = methodAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        func = typeof(Func<>).MakeGenericType(types);
                    }
                        break;
                    case 2:
                    {
                        methodAggregator = typeof(MethodAggregator<,>).MakeGenericType(types);
                        instance = methodAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        func = typeof(Func<,>).MakeGenericType(types);
                    }
                        break;
                    case 3:
                    {
                        methodAggregator = typeof(MethodAggregator<,,>).MakeGenericType(types);
                        instance = methodAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        func = typeof(Func<,,>).MakeGenericType(types);
                    }
                        break;
                    case 4:
                    {
                        methodAggregator = typeof(MethodAggregator<,,,>).MakeGenericType(types);
                        instance = methodAggregator.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                        func = typeof(Func<,,,>).MakeGenericType(types);
                    }
                        break;
                    default:
                        continue;
                }

                Type[] ts = new Type[useID ? 3 : 2];
                object[] os = new object[useID ? 3 : 2];
                if (isInt)
                {
                    ts[0] = typeof(int);
                    os[0] = handlerInfos.Index;
                }
                else
                {
                    ts[0] = typeof(string);
                    os[0] = handlerInfos.Str;
                }

                if (useID)
                {
                    ts[1] = typeof(string);
                    ts[2] = func;
                    os[1] = handlerInfos.ID;
                    os[2] = handlerInfos.Func;
                }
                else
                {
                    ts[1] = func;
                    os[1] = handlerInfos.Func;
                }

                MethodInfo removeMethod = methodAggregator.GetMethod("RemoveHandler", ts);
                removeMethod.Invoke(instance, os);
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

        protected void Subscribe<T1, T2, T3>(int index, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, func);

            SubscribeInfo temp = new SubscribeInfo(index, func, typeof(T1), typeof(T2), typeof(T3));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T1, T2>(int index, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, func);

            SubscribeInfo temp = new SubscribeInfo(index, func, typeof(T1), typeof(T2));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T>(int index, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, func);

            SubscribeInfo temp = new SubscribeInfo(index, func, typeof(T));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe(int index, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, func);

            SubscribeInfo temp = new SubscribeInfo(index, func);
            _subscribeInfos.Add(temp);
        }

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

        protected void Subscribe<T1, T2, T3>(int index, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, id, func);

            SubscribeInfo temp = new SubscribeInfo(index, id, func, typeof(T1), typeof(T2), typeof(T3));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T1, T2>(int index, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, id, func);

            SubscribeInfo temp = new SubscribeInfo(index, id, func, typeof(T1), typeof(T2));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T>(int index, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, id, func);

            SubscribeInfo temp = new SubscribeInfo(index, id, func, typeof(T));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe(int index, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, id, func);

            SubscribeInfo temp = new SubscribeInfo(index, id, func);
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe(Enum index, string id, Action func)
        {
            Subscribe(Convert.ToInt32(index), id, func);
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

        protected void Subscribe<T1, T2, T3>(string str, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(str, func);

            SubscribeInfo temp = new SubscribeInfo(str, func, typeof(T1), typeof(T2), typeof(T3));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T1, T2>(string str, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, func);

            SubscribeInfo temp = new SubscribeInfo(str, func, typeof(T1), typeof(T2));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T>(string str, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, func);

            SubscribeInfo temp = new SubscribeInfo(str, func, typeof(T));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe(string str, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, func);

            SubscribeInfo temp = new SubscribeInfo(str, func);
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T1, T2, T3>(string str, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(str, id, func);

            SubscribeInfo temp = new SubscribeInfo(str, id, func, typeof(T1), typeof(T2), typeof(T3));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T1, T2>(string str, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, id, func);

            SubscribeInfo temp = new SubscribeInfo(str, id, func, typeof(T1), typeof(T2));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe<T>(string str, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, id, func);

            SubscribeInfo temp = new SubscribeInfo(str, id, func, typeof(T));
            _subscribeInfos.Add(temp);
        }

        protected void Subscribe(string str, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, id, func);

            SubscribeInfo temp = new SubscribeInfo(str, id, func);
            _subscribeInfos.Add(temp);
        }

        #endregion

        #region Unsubscribe

        protected void Unsubscribe<T1, T2, T3>(int index, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2, T3>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T1, T2>(int index, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T>(int index, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe(int index, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(index, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
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

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2, T3>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T1, T2>(int index, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T>(int index, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action<T>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe(int index, string id, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(index, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    index == _subscribeInfos[i].Index &&
                    func == (_subscribeInfos[i].Func as Action))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
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

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2, T3>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T1, T2>(string str, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(str, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T>(string str, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(str, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe(string str, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(str, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && !_subscribeInfos[i].UseID && str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T1, T2, T3>(string str, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(str, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2, T3>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T1, T2>(string str, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(str, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T1, T2>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe<T>(string str, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Unsubscribe(str, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action<T>))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void Unsubscribe(string str, string id, Action func)
        {
            MessageAggregator.Instance.Unsubscribe(str, id, func);

            for (int i = 0; i < _subscribeInfos.Count; i++)
            {
                if (!_subscribeInfos[i].UseInt && _subscribeInfos[i].UseID && id == _subscribeInfos[i].ID &&
                    str == _subscribeInfos[i].Str &&
                    func == (_subscribeInfos[i].Func as Action))
                {
                    _subscribeInfos.RemoveAt(i);
                    return;
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

        #region Register

        protected void Register<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(func);

            RegisterInfo temp = new RegisterInfo(func, typeof(T));
            _registerInfos.Add(temp);
        }

        protected void Register<T>(string id, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(id, func);

            RegisterInfo temp = new RegisterInfo(id, func, typeof(T));
            _registerInfos.Add(temp);
        }

        protected void Register<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(index, func);

            RegisterInfo temp = new RegisterInfo(index, func, typeof(T));
            _registerInfos.Add(temp);
        }

        protected void Register<T>(Enum index, Func<T> func)
        {
            Register(Convert.ToInt32(index), func);
        }

        #endregion

        #region Unregister

        protected void Unregister<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(func);

            for (int i = 0; i < _registerInfos.Count; i++)
            {
                if (_registerInfos[i].KeyType == 0 && func.Equals(_registerInfos[i].Func as Func<T>))
                {
                    _registerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void Unregister<T>(string id, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(id, func);

            for (int i = 0; i < _registerInfos.Count; i++)
            {
                if (_registerInfos[i].KeyType == 1 && id == _registerInfos[i].Str &&
                    func.Equals(_registerInfos[i].Func as Func<T>))
                {
                    _registerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void Unregister<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(index, func);

            for (int i = 0; i < _registerInfos.Count; i++)
            {
                if (_registerInfos[i].KeyType == 2 && index == _registerInfos[i].Index &&
                    func.Equals(_registerInfos[i].Func as Func<T>))
                {
                    _registerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void Unregister<T>(Enum index, Func<T> func)
        {
            Unregister(Convert.ToInt32(index), func);
        }

        #endregion

        #region AddListener

        protected void AddListener<T>(Action<T> func)
        {
            ObjectAggregator<T>.Instance.AddListener(func);

            ListenerInfo temp = new ListenerInfo(func, typeof(T));
            _listenerInfos.Add(temp);
        }

        protected void AddListener<T>(string id, Action<T> func)
        {
            ObjectAggregator<T>.Instance.AddListener(id, func);

            ListenerInfo temp = new ListenerInfo(id, func, typeof(T));
            _listenerInfos.Add(temp);
        }

        protected void AddListener<T>(int index, Action<T> func)
        {
            ObjectAggregator<T>.Instance.AddListener(index, func);

            ListenerInfo temp = new ListenerInfo(index, func, typeof(T));
            _listenerInfos.Add(temp);
        }

        protected void AddListener<T>(Enum index, Action<T> func)
        {
            AddListener(Convert.ToInt32(index), func);
        }

        #endregion

        #region RemoveListener

        protected void RemoveListener<T>(Action<T> func)
        {
            ObjectAggregator<T>.Instance.RemoveListener(func);

            for (int i = 0; i < _listenerInfos.Count; i++)
            {
                if (_listenerInfos[i].KeyType == 0 && func.Equals(_listenerInfos[i].Func as Action<T>))
                {
                    _listenerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void RemoveListener<T>(string id, Action<T> func)
        {
            ObjectAggregator<T>.Instance.RemoveListener(id, func);

            for (int i = 0; i < _listenerInfos.Count; i++)
            {
                if (_listenerInfos[i].KeyType == 1 && id == _listenerInfos[i].Str &&
                    func.Equals(_listenerInfos[i].Func as Action<T>))
                {
                    _listenerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void RemoveListener<T>(int index, Action<T> func)
        {
            ObjectAggregator<T>.Instance.RemoveListener(index, func);

            for (int i = 0; i < _listenerInfos.Count; i++)
            {
                if (_listenerInfos[i].KeyType == 2 && index == _listenerInfos[i].Index &&
                    func.Equals(_listenerInfos[i].Func as Action<T>))
                {
                    _listenerInfos.RemoveAt(i);
                    break;
                }
            }
        }

        protected void RemoveListener<T>(Enum index, Action<T> func)
        {
            RemoveListener(Convert.ToInt32(index), func);
        }

        #endregion

        #region AddHandler

        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(int index,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.AddHandler(index, handler);

            HandlerInfo temp = new HandlerInfo(index, handler, typeof(TValue1), typeof(TValue2), typeof(TValue3),
                typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(int index, Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.AddHandler(index, handler);

            HandlerInfo temp = new HandlerInfo(index, handler, typeof(TValue1), typeof(TValue2), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue, TResult>(int index, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(index, handler);

            HandlerInfo temp = new HandlerInfo(index, handler, typeof(TValue), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TResult>(int index, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(index, handler);

            HandlerInfo temp = new HandlerInfo(index, handler, typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(Enum index,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(Enum index, Func<TValue1, TValue2, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TValue, TResult>(Enum index, Func<TValue, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TResult>(Enum index, Func<TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), handler);
        }

        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(string str,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.AddHandler(str, handler);

            HandlerInfo temp = new HandlerInfo(str, handler, typeof(TValue1), typeof(TValue2), typeof(TValue3),
                typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(string str, Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.AddHandler(str, handler);

            HandlerInfo temp = new HandlerInfo(str, handler, typeof(TValue1), typeof(TValue2), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue, TResult>(string str, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, handler);

            HandlerInfo temp = new HandlerInfo(str, handler, typeof(TValue), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TResult>(string str, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, handler);

            HandlerInfo temp = new HandlerInfo(str, handler, typeof(TResult));
            _handlerInfos.Add(temp);
        }


        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(int index, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.AddHandler(index, id, handler);

            HandlerInfo temp = new HandlerInfo(index, id, handler, typeof(TValue1), typeof(TValue2), typeof(TValue3),
                typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(int index, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.AddHandler(index, id, handler);

            HandlerInfo temp = new HandlerInfo(index, id, handler, typeof(TValue1), typeof(TValue2), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue, TResult>(int index, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(index, id, handler);

            HandlerInfo temp = new HandlerInfo(index, id, handler, typeof(TValue), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TResult>(int index, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(index, id, handler);

            HandlerInfo temp = new HandlerInfo(index, id, handler, typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(Enum index, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(Enum index, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

        protected void AddHandler<TValue, TResult>(Enum index, string id, Func<TValue, TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

        protected void AddHandler<TResult>(Enum index, string id, Func<TResult> handler)
        {
            AddHandler(Convert.ToInt32(index), id, handler);
        }

        protected void AddHandler<TValue1, TValue2, TValue3, TResult>(string str, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.AddHandler(str, id, handler);

            HandlerInfo temp = new HandlerInfo(str, id, handler, typeof(TValue1), typeof(TValue2), typeof(TValue3),
                typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue1, TValue2, TResult>(string str, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.AddHandler(str, id, handler);

            HandlerInfo temp = new HandlerInfo(str, id, handler, typeof(TValue1), typeof(TValue2), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TValue, TResult>(string str, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, id, handler);

            HandlerInfo temp = new HandlerInfo(str, id, handler, typeof(TValue), typeof(TResult));
            _handlerInfos.Add(temp);
        }

        protected void AddHandler<TResult>(string str, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, id, handler);

            HandlerInfo temp = new HandlerInfo(str, id, handler, typeof(TResult));
            _handlerInfos.Add(temp);
        }

        #endregion

        #region RemoveHandler

        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(int index,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.RemoveHandler(index, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TValue3, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(int index, Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.RemoveHandler(index, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue, TResult>(int index, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TResult>(int index, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(index, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && !_handlerInfos[i].UseID && index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }


        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(Enum index,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(Enum index, Func<TValue1, TValue2, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TValue, TResult>(Enum index, Func<TValue, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TResult>(Enum index, Func<TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), handler);
        }

        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(string str,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.RemoveHandler(str, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TValue3, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(string str, Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.RemoveHandler(str, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue, TResult>(string str, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    handler == (_handlerInfos[i].Func as Func<TValue, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TResult>(string str, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(str, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && !_handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    handler == (_handlerInfos[i].Func as Func<TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }


        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(int index, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.RemoveHandler(index, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TValue3, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(int index, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.RemoveHandler(index, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue, TResult>(int index, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TValue, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TResult>(int index, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(index, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (_handlerInfos[i].UseInt && _handlerInfos[i].UseID && index == _handlerInfos[i].Index &&
                    handler == (_handlerInfos[i].Func as Func<TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }


        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(Enum index, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(Enum index, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

        protected void RemoveHandler<TValue, TResult>(Enum index, string id, Func<TValue, TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

        protected void RemoveHandler<TResult>(Enum index, string id, Func<TResult> handler)
        {
            RemoveHandler(Convert.ToInt32(index), id, handler);
        }

        protected void RemoveHandler<TValue1, TValue2, TValue3, TResult>(string str, string id,
            Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TValue3, TResult>.Instance.RemoveHandler(str, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    id == _handlerInfos[i].ID &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TValue3, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue1, TValue2, TResult>(string str, string id,
            Func<TValue1, TValue2, TResult> handler)
        {
            MethodAggregator<TValue1, TValue2, TResult>.Instance.RemoveHandler(str, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    id == _handlerInfos[i].ID &&
                    handler == (_handlerInfos[i].Func as Func<TValue1, TValue2, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TValue, TResult>(string str, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    id == _handlerInfos[i].ID &&
                    handler == (_handlerInfos[i].Func as Func<TValue, TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        protected void RemoveHandler<TResult>(string str, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.RemoveHandler(str, id, handler);

            for (int i = 0; i < _handlerInfos.Count; i++)
            {
                if (!_handlerInfos[i].UseInt && _handlerInfos[i].UseID &&
                    str == _handlerInfos[i].Str &&
                    id == _handlerInfos[i].ID &&
                    handler == (_handlerInfos[i].Func as Func<TResult>))
                {
                    _handlerInfos.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion
    }
}
