using System;
using System.Collections.Generic;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 控制反转容器InversionOfControlContainer
    /// 对MessageAggregator和ObjectAggregator的封装
    /// </summary>
    public static class IOCC
    {
        #region MessageAggregator
        public static void Subscribe(int index, Action handler)
        {
            MessageAggregator.Instance.Subscribe(index, handler);
        }
        public static void Subscribe<T>(int index, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(index, handler);
        }
        public static void Subscribe<T1, T2>(int index, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, handler);
        }
        public static void Subscribe<T1, T2, T3>(int index, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, handler);
        }

        public static void Subscribe(Enum index, Action handler)
        {
            MessageAggregator.Instance.Subscribe(Convert.ToInt32(index), handler);
        }
        public static void Subscribe<T>(Enum index, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(Convert.ToInt32(index), handler);
        }
        public static void Subscribe<T1, T2>(Enum index, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(Convert.ToInt32(index), handler);
        }
        public static void Subscribe<T1, T2, T3>(Enum index, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(Convert.ToInt32(index), handler);
        }

        public static void Subscribe(int index, string id, Action handler)
        {
            MessageAggregator.Instance.Subscribe(index, id, handler);
        }
        public static void Subscribe<T>(int index, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(index, id, handler);
        }
        public static void Subscribe<T1, T2>(int index, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(index, id, handler);
        }
        public static void Subscribe<T1, T2, T3>(int index, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(index, id, handler);
        }

        public static void Subscribe(Enum index, string id, Action handler)
        {
            MessageAggregator.Instance.Subscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Subscribe<T>(Enum index, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Subscribe<T1, T2>(Enum index, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Subscribe<T1, T2, T3>(Enum index, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(Convert.ToInt32(index), id, handler);
        }

        public static void Unsubscribe(int index, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(index, handler);
        }
        public static void Unsubscribe<T>(int index, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, handler);
        }
        public static void Unsubscribe<T1, T2>(int index, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, handler);
        }
        public static void Unsubscribe<T1, T2, T3>(int index, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, handler);
        }

        public static void Unsubscribe(Enum index, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(Convert.ToInt32(index), handler);
        }
        public static void Unsubscribe<T>(Enum index, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(Convert.ToInt32(index), handler);
        }
        public static void Unsubscribe<T1, T2>(Enum index, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(Convert.ToInt32(index), handler);
        }
        public static void Unsubscribe<T1, T2, T3>(Enum index, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(Convert.ToInt32(index), handler);
        }

        public static void Unsubscribe(int index, string id, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(index, id, handler);
        }
        public static void Unsubscribe<T>(int index, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(index, id, handler);
        }
        public static void Unsubscribe<T1, T2>(int index, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(index, id, handler);
        }
        public static void Unsubscribe<T1, T2, T3>(int index, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(index, id, handler);
        }

        public static void Unsubscribe(Enum index, string id, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Unsubscribe<T>(Enum index, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Unsubscribe<T1, T2>(Enum index, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(Convert.ToInt32(index), id, handler);
        }
        public static void Unsubscribe<T1, T2, T3>(Enum index, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(Convert.ToInt32(index), id, handler);
        }

        public static void Publish(int index)
        {
            MessageAggregator.Instance.Publish(index);
        }
        public static void Publish<T>(int index, T value)
        {
            MessageAggregator<T>.Instance.Publish(index, value);
        }
        public static void Publish<T1, T2>(int index, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(index, value1, value2);
        }
        public static void Publish<T1, T2, T3>(int index, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(index, value1, value2, value3);
        }

        public static void Publish(Enum index)
        {
            MessageAggregator.Instance.Publish(Convert.ToInt32(index));
        }
        public static void Publish<T>(Enum index, T value)
        {
            MessageAggregator<T>.Instance.Publish(Convert.ToInt32(index), value);
        }
        public static void Publish<T1, T2>(Enum index, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(Convert.ToInt32(index), value1, value2);
        }
        public static void Publish<T1, T2, T3>(Enum index, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(Convert.ToInt32(index), value1, value2, value3);
        }

        public static void PublishWithID(int index, string id)
        {
            MessageAggregator.Instance.PublishWithID(index, id);
        }
        public static void PublishWithID<T>(int index, string id, T value)
        {
            MessageAggregator<T>.Instance.PublishWithID(index, id, value);
        }
        public static void PublishWithID<T1, T2>(int index, string id, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(index, id, value1, value2);
        }
        public static void PublishWithID<T1, T2, T3>(int index, string id, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(index, id, value1, value2, value3);
        }

        public static void PublishWithID(Enum index, string id)
        {
            MessageAggregator.Instance.PublishWithID(Convert.ToInt32(index), id);
        }
        public static void PublishWithID<T>(Enum index, string id, T value)
        {
            MessageAggregator<T>.Instance.PublishWithID(Convert.ToInt32(index), id, value);
        }
        public static void PublishWithID<T1, T2>(Enum index, string id, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(Convert.ToInt32(index), id, value1, value2);
        }
        public static void PublishWithID<T1, T2, T3>(Enum index, string id, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(Convert.ToInt32(index), id, value1, value2, value3);
        }

        public static void Subscribe(string name, Action handler)
        {
            MessageAggregator.Instance.Subscribe(name, handler);
        }
        public static void Subscribe<T>(string name, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(name, handler);
        }
        public static void Subscribe<T1, T2>(string name, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(name, handler);
        }
        public static void Subscribe<T1, T2, T3>(string name, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(name, handler);
        }

        public static void Subscribe(string name, string id, Action handler)
        {
            MessageAggregator.Instance.Subscribe(name, id, handler);
        }
        public static void Subscribe<T>(string name, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Subscribe(name, id, handler);
        }
        public static void Subscribe<T1, T2>(string name, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Subscribe(name, id, handler);
        }
        public static void Subscribe<T1, T2, T3>(string name, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Subscribe(name, id, handler);
        }

        public static void Unsubscribe(string name, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(name, handler);
        }
        public static void Unsubscribe<T>(string name, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(name, handler);
        }
        public static void Unsubscribe<T1, T2>(string name, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(name, handler);
        }
        public static void Unsubscribe<T1, T2, T3>(string name, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(name, handler);
        }

        public static void Unsubscribe(string name, string id, Action handler)
        {
            MessageAggregator.Instance.Unsubscribe(name, id, handler);
        }
        public static void Unsubscribe<T>(string name, string id, Action<T> handler)
        {
            MessageAggregator<T>.Instance.Unsubscribe(name, id, handler);
        }
        public static void Unsubscribe<T1, T2>(string name, string id, Action<T1, T2> handler)
        {
            MessageAggregator<T1, T2>.Instance.Unsubscribe(name, id, handler);
        }
        public static void Unsubscribe<T1, T2, T3>(string name, string id, Action<T1, T2, T3> handler)
        {
            MessageAggregator<T1, T2, T3>.Instance.Unsubscribe(name, id, handler);
        }

        public static void Publish(string name)
        {
            MessageAggregator.Instance.Publish(name);
        }
        public static void Publish<T>(string name, T value)
        {
            MessageAggregator<T>.Instance.Publish(name, value);
        }
        public static void Publish<T1, T2>(string name, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.Publish(name, value1, value2);
        }
        public static void Publish<T1, T2, T3>(string name, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.Publish(name, value1, value2, value3);
        }

        public static void PublishWithID(string name, string id)
        {
            MessageAggregator.Instance.PublishWithID(name, id);
        }
        public static void PublishWithID<T>(string name, string id, T value)
        {
            MessageAggregator<T>.Instance.PublishWithID(name, id, value);
        }
        public static void PublishWithID<T1, T2>(string name, string id, T1 value1, T2 value2)
        {
            MessageAggregator<T1, T2>.Instance.PublishWithID(name, id, value1, value2);
        }
        public static void PublishWithID<T1, T2, T3>(string name, string id, T1 value1, T2 value2, T3 value3)
        {
            MessageAggregator<T1, T2, T3>.Instance.PublishWithID(name, id, value1, value2, value3);
        }
        #endregion

        #region ObjectAggregator
        public static void Set<T>(T value)
        {
            ObjectAggregator<T>.Instance.Set(value);
        }
        public static T Get<T>()
        {
            return ObjectAggregator<T>.Instance.Get();
        }
        public static bool TryGet<T>(out T value)
        {
            return ObjectAggregator<T>.Instance.TryGet(out value);
        }
        public static List<T> GetAll<T>()
        {
            return ObjectAggregator<T>.Instance.GetAll();
        }
        public static void Register<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(func);
        }
        public static void Unregister<T>(Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(func);
        }
        public static void AddListener<T>(Action<T> handler)
        {
            ObjectAggregator<T>.Instance.AddListener(handler);
        }
        public static void RemoveListener<T>(Action<T> handler)
        {
            ObjectAggregator<T>.Instance.RemoveListener(handler);
        }

        public static void Set<T>(int index, T value)
        {
            ObjectAggregator<T>.Instance.Set(index, value);
        }
        public static T Get<T>(int index)
        {
            return ObjectAggregator<T>.Instance.Get(index);
        }
        public static bool TryGet<T>(int index, out T value)
        {
            return ObjectAggregator<T>.Instance.TryGet(index, out value);
        }
        public static List<T> GetAll<T>(int index)
        {
            return ObjectAggregator<T>.Instance.GetAll(index);
        }
        public static void Register<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(index, func);
        }
        public static void Unregister<T>(int index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(index, func);
        }
        public static void AddListener<T>(int index, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.AddListener(index, handler);
        }
        public static void RemoveListener<T>(int index, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.RemoveListener(index, handler);
        }

        public static void Set<T>(Enum index, T value)
        {
            ObjectAggregator<T>.Instance.Set(Convert.ToInt32(index), value);
        }
        public static T Get<T>(Enum index)
        {
            return ObjectAggregator<T>.Instance.Get(Convert.ToInt32(index));
        }
        public static bool TryGet<T>(Enum index, out T value)
        {
            return ObjectAggregator<T>.Instance.TryGet(Convert.ToInt32(index), out value);
        }
        public static List<T> GetAll<T>(Enum index)
        {
            return ObjectAggregator<T>.Instance.GetAll(Convert.ToInt32(index));
        }
        public static void Register<T>(Enum index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(Convert.ToInt32(index), func);
        }
        public static void Unregister<T>(Enum index, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(Convert.ToInt32(index), func);
        }
        public static void AddListener<T>(Enum index, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.AddListener(Convert.ToInt32(index), handler);
        }   
        public static void RemoveListener<T>(Enum index, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.RemoveListener(Convert.ToInt32(index), handler);
        }

        public static void Set<T>(string name, T value)
        {
            ObjectAggregator<T>.Instance.Set(name, value);
        }
        public static T Get<T>(string name)
        {
            return ObjectAggregator<T>.Instance.Get(name);
        }
        public static bool TryGet<T>(string name, out T value)
        {
            return ObjectAggregator<T>.Instance.TryGet(name, out value);
        }
        public static List<T> GetAll<T>(string name)
        {
            return ObjectAggregator<T>.Instance.GetAll(name);
        }
        public static void Register<T>(string name, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Register(name, func);
        }
        public static void Unregister<T>(string name, Func<T> func)
        {
            ObjectAggregator<T>.Instance.Unregister(name, func);
        }
        public static void AddListener<T>(string name, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.AddListener(name, handler);
        }
        public static void RemoveListener<T>(string name, Action<T> handler)
        {
            ObjectAggregator<T>.Instance.RemoveListener(name, handler);
        }
        #endregion
    }
}
