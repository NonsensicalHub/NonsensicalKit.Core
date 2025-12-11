using System;
using System.Collections.Generic;
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
            foreach (var info in _subscribeInfos)
            {
                info.Recycle.Invoke();
            }

            _subscribeInfos.Clear();

            foreach (var info in _registerInfos)
            {
                info.Recycle.Invoke();
            }

            _registerInfos.Clear();

            foreach (var info in _listenerInfos)
            {
                info.Recycle.Invoke();
            }

            _listenerInfos.Clear();

            foreach (var info in _handlerInfos)
            {
                info.Recycle.Invoke();
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
            public readonly int Index;
            public readonly string Str;
            public readonly string ID;
            public readonly object Func;
            public readonly Action Recycle;

            public SubscribeInfo(int index, object func, Action recycle)
            {
                UseID = false;
                UseInt = true;
                Index = index;
                Func = func;
                ID = null;
                Str = null;
                Recycle = recycle;
            }

            public SubscribeInfo(string str, object func, Action recycle)
            {
                UseID = false;
                UseInt = false;
                Str = str;
                Func = func;
                ID = null;
                Index = 0;
                Recycle = recycle;
            }

            public SubscribeInfo(int index, string id, object func, Action recycle)
            {
                UseID = true;
                UseInt = true;
                Index = index;
                Func = func;
                ID = id;
                Str = null;
                Recycle = recycle;
            }

            public SubscribeInfo(string str, string id, object func, Action recycle)
            {
                UseID = true;
                UseInt = false;
                Str = str;
                Func = func;
                ID = id;
                Index = 0;
                Recycle = recycle;
            }
        }

        protected class RegisterInfo
        {
            public readonly int KeyType; //0代表类型键，1代表字符串键，2代表数字键
            public readonly int Index;
            public readonly string Str;
            public readonly object Func;
            public readonly Action Recycle;

            public RegisterInfo(object func, Action recycle)
            {
                KeyType = 0;
                Index = 0;
                Str = null;
                Func = func;
                Recycle = recycle;
            }

            public RegisterInfo(string str, object func, Action recycle)
            {
                KeyType = 1;
                Index = 0;
                Str = str;
                Func = func;
                Recycle = recycle;
            }

            public RegisterInfo(int index, object func, Action recycle)
            {
                KeyType = 2;
                Index = index;
                Str = null;
                Func = func;
                Recycle = recycle;
            }
        }

        protected class ListenerInfo
        {
            public readonly int KeyType; //0代表类型键，1代表字符串键，2代表数字键
            public readonly int Index;
            public readonly string Str;
            public readonly object Func;
            public readonly Action Recycle;

            public ListenerInfo(object func, Action recycle)
            {
                KeyType = 0;
                Index = 0;
                Str = null;
                Func = func;
                Recycle = recycle;
            }

            public ListenerInfo(string str, object func, Action recycle)
            {
                KeyType = 1;
                Index = 0;
                Str = str;
                Func = func;
                Recycle = recycle;
            }

            public ListenerInfo(int index, object func, Action recycle)
            {
                KeyType = 2;
                Index = index;
                Str = null;
                Func = func;
                Recycle = recycle;
            }
        }

        protected class HandlerInfo
        {
            public readonly bool UseInt;
            public readonly bool UseID;
            public readonly int Index;
            public readonly string Str;
            public readonly string ID;
            public readonly object Func;
            public readonly Action Recycle;

            public HandlerInfo(int index, object func, Action recycle)
            {
                UseID = false;
                UseInt = true;
                Index = index;
                Func = func;
                ID = null;
                Str = null;
                Recycle = recycle;
            }

            public HandlerInfo(string str, object func, Action recycle)
            {
                UseID = false;
                UseInt = false;
                Str = str;
                Func = func;
                ID = null;
                Index = 0;
                Recycle = recycle;
            }

            public HandlerInfo(int index, string id, object func, Action recycle)
            {
                UseID = true;
                UseInt = true;
                Index = index;
                Func = func;
                ID = id;
                Str = null;
                Recycle = recycle;
            }

            public HandlerInfo(string str, string id, object func, Action recycle)
            {
                UseID = true;
                UseInt = false;
                Str = str;
                Func = func;
                ID = id;
                Index = 0;
                Recycle = recycle;
            }
        }

        #region Subscribe

        // --- Subscribe (int key)
        protected void Subscribe<T1, T2, T3>(int index, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func,
                () => MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, func)));
        }

        protected void Subscribe<T1, T2>(int index, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func,
                () => MessageAggregator<T1, T2>.Instance.Unsubscribe(index, func)));
        }

        protected void Subscribe<T>(int index, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func,
                () => MessageAggregator<T>.Instance.Unsubscribe(index, func)));
        }

        protected void Subscribe(int index, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, func);
            _subscribeInfos.Add(new SubscribeInfo(index, func,
                () => MessageAggregator.Instance.Unsubscribe(index, func)));
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
            _subscribeInfos.Add(new SubscribeInfo(index, id, func,
                () => MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, id, func)));
        }

        protected void Subscribe<T1, T2>(int index, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func,
                () => MessageAggregator<T1, T2>.Instance.Unsubscribe(index, id, func)));
        }

        protected void Subscribe<T>(int index, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func,
                () => MessageAggregator<T>.Instance.Unsubscribe(index, id, func)));
        }

        protected void Subscribe(int index, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(index, id, func);
            _subscribeInfos.Add(new SubscribeInfo(index, id, func,
                () => MessageAggregator.Instance.Unsubscribe(index, id, func)));
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
            _subscribeInfos.Add(new SubscribeInfo(str, func,
                () => MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(str, func)));
        }

        protected void Subscribe<T1, T2>(string str, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func,
                () => MessageAggregator<T1, T2>.Instance.Unsubscribe(str, func)));
        }

        protected void Subscribe<T>(string str, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, func);
            _subscribeInfos.Add(
                new SubscribeInfo(str, func, () => MessageAggregator<T>.Instance.Unsubscribe(str, func)));
        }

        protected void Subscribe(string str, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, func);
            _subscribeInfos.Add(new SubscribeInfo(str, func, () => MessageAggregator.Instance.Unsubscribe(str, func)));
        }

        // --- Subscribe (string + id)
        protected void Subscribe<T1, T2, T3>(string str, string id, Action<T1, T2, T3> func)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func,
                () => MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(str,id,  func)));
        }

        protected void Subscribe<T1, T2>(string str, string id, Action<T1, T2> func)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func,
                () => MessageAggregator<T1, T2>.Instance.Unsubscribe(str, id, func)));
        }

        protected void Subscribe<T>(string str, string id, Action<T> func)
        {
            MessageAggregator<T>.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func,
                () => MessageAggregator<T>.Instance.Unsubscribe(str,id, func)));
        }

        protected void Subscribe(string str, string id, Action func)
        {
            MessageAggregator.Instance.Subscribe(str, id, func);
            _subscribeInfos.Add(new SubscribeInfo(str, id, func,
                () => MessageAggregator.Instance.Unsubscribe(str,  id, func)));
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
            _registerInfos.Add(new RegisterInfo(func, () => ObjectAggregator<T>.Instance.Unregister(func)));
        }

        protected void Register<T>(string str, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(str, func);
            _registerInfos.Add(new RegisterInfo(str, func, () => ObjectAggregator<T>.Instance.Unregister(func)));
        }

        protected void Register<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(index, func);
            _registerInfos.Add(new RegisterInfo(index, func, () => ObjectAggregator<T>.Instance.Unregister(func)));
        }

        protected void Register<T>(Enum index, Func<T> func)
        {
            Register(Convert.ToInt32(index), func);
        }

        protected void Unregister<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(func);
            RemoveRegisterInfoMatch(info => info.KeyType == 0 && (Func<T>)info.Func == func);
        }

        protected void Unregister<T>(string str, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(str, func);
            RemoveRegisterInfoMatch(info =>
                info.KeyType == 1 && info.Str == str && (Func<T>)info.Func == func);
        }

        protected void Unregister<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(index, func);
            RemoveRegisterInfoMatch(info =>
                info.KeyType == 2 && info.Index == index && (Func<T>)info.Func == func);
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
            _listenerInfos.Add(new ListenerInfo(listener, () => ObjectAggregator<T>.Instance.RemoveListener(listener)));
        }

        protected void AddListener<T>(string str, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.AddListener(str, listener);
            _listenerInfos.Add(new ListenerInfo(str, listener,
                () => ObjectAggregator<T>.Instance.RemoveListener(listener)));
        }

        protected void AddListener<T>(int index, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.AddListener(index, listener);
            _listenerInfos.Add(new ListenerInfo(index, listener,
                () => ObjectAggregator<T>.Instance.RemoveListener(listener)));
        }

        protected void AddListener<T>(Enum index, Action<T> listener)
        {
            AddListener(Convert.ToInt32(index), listener);
        }

        protected void RemoveListener<T>(Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 0 && (Action<T>)info.Func == listener);
        }

        protected void RemoveListener<T>(string str, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(str, listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 1 && info.Str == str && (Action<T>)info.Func == listener);
        }

        protected void RemoveListener<T>(int index, Action<T> listener)
        {
            ObjectAggregator<T>.Instance.RemoveListener(index, listener);
            RemoveListenerInfoMatch(info =>
                info.KeyType == 2 && info.Index == index && (Action<T>)info.Func == listener);
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
            _handlerInfos.Add(new HandlerInfo(index, handler,
                () => MethodAggregator<TResult>.Instance.RemoveHandler(index, handler)));
        }

        protected void AddHandler<TResult>(string str, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler,
                () => MethodAggregator<TResult>.Instance.RemoveHandler(str, handler)));
        }

        protected void AddHandler<TResult>(int index, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler,
                () => MethodAggregator<TResult>.Instance.RemoveHandler(id, handler)));
        }

        protected void AddHandler<TResult>(string str, string id, Func<TResult> handler)
        {
            MethodAggregator<TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler,
                () => MethodAggregator<TResult>.Instance.RemoveHandler(str, id, handler)));
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
            _handlerInfos.Add(new HandlerInfo(index, handler,
                () => MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, handler)));
        }

        protected void AddHandler<TValue, TResult>(string str, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler,
                () => MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, handler)));
        }

        protected void AddHandler<TValue, TResult>(int index, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler,
                () => MethodAggregator<TValue, TResult>.Instance.RemoveHandler(index, id, handler)));
        }

        protected void AddHandler<TValue, TResult>(string str, string id, Func<TValue, TResult> handler)
        {
            MethodAggregator<TValue, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler,
                () => MethodAggregator<TValue, TResult>.Instance.RemoveHandler(str, id, handler)));
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
            _handlerInfos.Add(new HandlerInfo(index, handler,
                () => MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(index, handler)));
        }

        protected void AddHandler<T1, T2, TResult>(string str, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler,
                () => MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(str, handler)));
        }

        protected void AddHandler<T1, T2, TResult>(int index, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler,
                () => MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(index, id, handler)));
        }

        protected void AddHandler<T1, T2, TResult>(string str, string id, Func<T1, T2, TResult> handler)
        {
            MethodAggregator<T1, T2, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler,
                () => MethodAggregator<T1, T2, TResult>.Instance.RemoveHandler(str, id, handler)));
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
            _handlerInfos.Add(new HandlerInfo(index, handler,
                () => MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(index, handler)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(string str, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(str, handler);
            _handlerInfos.Add(new HandlerInfo(str, handler,
                () => MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(str, handler)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(int index, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(index, id, handler);
            _handlerInfos.Add(new HandlerInfo(index, id, handler,
                () => MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(index, id, handler)));
        }

        protected void AddHandler<T1, T2, T3, TResult>(string str, string id, Func<T1, T2, T3, TResult> handler)
        {
            MethodAggregator<T1, T2, T3, TResult>.Instance.AddHandler(str, id, handler);
            _handlerInfos.Add(new HandlerInfo(str, id, handler,
                () => MethodAggregator<T1, T2, T3, TResult>.Instance.RemoveHandler(str, id, handler)));
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
    }
}
