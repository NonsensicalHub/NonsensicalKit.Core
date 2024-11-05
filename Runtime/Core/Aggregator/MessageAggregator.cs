using System;
using System.Collections.Generic;

namespace NonsensicalKit.Core
{
    /* 经简单测试，循环十万次调用单一方法时，publish的时间消耗是直接引用调用的20倍，但大量调用时间仍在可接受范围内
     * 消息聚合器应当只用于模块之间的通信，且当通信过于频繁时不应使用，模块内部使用应直接引用的方式进行值的传递
     *
     * TODO:避免在publish中执行了Subscribe或Unsubscribe导致不可预料的情况
     * TODO:事件队列
     */

    /// <summary>
    /// 消息聚合器，最多支持三个参数的泛型
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class MessageAggregator<T1, T2, T3>
    {
        public static MessageAggregator<T1, T2, T3> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageAggregator<T1, T2, T3>();
                }

                return _instance;
            }
        }

        private static MessageAggregator<T1, T2, T3> _instance;

        private readonly Dictionary<int, Action<T1, T2, T3>> _messages = new Dictionary<int, Action<T1, T2, T3>>();
        private readonly Dictionary<int, Dictionary<string, Action<T1, T2, T3>>> _IDMessages = new Dictionary<int, Dictionary<string, Action<T1, T2, T3>>>();
        private readonly Dictionary<string, Action<T1, T2, T3>> _strMessages = new Dictionary<string, Action<T1, T2, T3>>();

        private readonly Dictionary<string, Dictionary<string, Action<T1, T2, T3>>> _strIDMessages =
            new Dictionary<string, Dictionary<string, Action<T1, T2, T3>>>();

        private MessageAggregator()
        {
        }

        #region int

        public void Subscribe(int name, Action<T1, T2, T3> handler)
        {
            if (!_messages.ContainsKey(name))
            {
                _messages.Add(name, handler);
            }
            else
            {
                _messages[name] += handler;
            }
        }

        public void Unsubscribe(int name, Action<T1, T2, T3> handler)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name] -= handler;

                if (_messages[name] == null)
                {
                    _messages.Remove(name);
                }
            }
        }

        public void Publish(int name, T1 value1, T2 value2, T3 value3)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name](value1, value2, value3);
            }
        }

        public void Subscribe(int name, string id, Action<T1, T2, T3> handler)
        {
            if (!_IDMessages.ContainsKey(name))
            {
                _IDMessages.Add(name, new Dictionary<string, Action<T1, T2, T3>>());
            }


            if (_IDMessages[name].ContainsKey(id))
            {
                _IDMessages[name][id] += handler;
            }
            else
            {
                _IDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(int name, string id, Action<T1, T2, T3> handler)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id] -= handler;
                    if (_IDMessages[name][id] == null)
                    {
                        _IDMessages[name].Remove(id);
                        if (_IDMessages[name] == null)
                        {
                            _IDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(int name, string id, T1 value1, T2 value2, T3 value3)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id](value1, value2, value3);
                }
            }
        }

        #endregion

        #region string

        public void Subscribe(string name, Action<T1, T2, T3> handler)
        {
            if (!_strMessages.ContainsKey(name))
            {
                _strMessages.Add(name, handler);
            }
            else
            {
                _strMessages[name] += handler;
            }
        }

        public void Unsubscribe(string name, Action<T1, T2, T3> handler)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name] -= handler;

                if (_strMessages[name] == null)
                {
                    _strMessages.Remove(name);
                }
            }
        }

        public void Publish(string name, T1 value1, T2 value2, T3 value3)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name](value1, value2, value3);
            }
        }

        public void Subscribe(string name, string id, Action<T1, T2, T3> handler)
        {
            if (!_strIDMessages.ContainsKey(name))
            {
                _strIDMessages.Add(name, new Dictionary<string, Action<T1, T2, T3>>());
            }

            if (_strIDMessages[name].ContainsKey(id))
            {
                _strIDMessages[name][id] += handler;
            }
            else
            {
                _strIDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(string name, string id, Action<T1, T2, T3> handler)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id] -= handler;
                    if (_strIDMessages[name][id] == null)
                    {
                        _strIDMessages[name].Remove(id);
                        if (_strIDMessages[name] == null)
                        {
                            _strIDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(string name, string id, T1 value1, T2 value2, T3 value3)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id](value1, value2, value3);
                }
            }
        }

        #endregion
    }

    public class MessageAggregator<T1, T2>
    {
        public static MessageAggregator<T1, T2> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageAggregator<T1, T2>();
                }

                return _instance;
            }
        }

        private static MessageAggregator<T1, T2> _instance;

        private readonly Dictionary<int, Action<T1, T2>> _messages = new Dictionary<int, Action<T1, T2>>();
        private readonly Dictionary<int, Dictionary<string, Action<T1, T2>>> _IDMessages = new Dictionary<int, Dictionary<string, Action<T1, T2>>>();
        private readonly Dictionary<string, Action<T1, T2>> _strMessages = new Dictionary<string, Action<T1, T2>>();
        private readonly Dictionary<string, Dictionary<string, Action<T1, T2>>> _strIDMessages = new Dictionary<string, Dictionary<string, Action<T1, T2>>>();

        private MessageAggregator()
        {
        }

        #region int

        public void Subscribe(int name, Action<T1, T2> handler)
        {
            if (!_messages.ContainsKey(name))
            {
                _messages.Add(name, handler);
            }
            else
            {
                _messages[name] += handler;
            }
        }

        public void Unsubscribe(int name, Action<T1, T2> handler)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name] -= handler;

                if (_messages[name] == null)
                {
                    _messages.Remove(name);
                }
            }
        }

        public void Publish(int name, T1 value1, T2 value2)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name](value1, value2);
            }
        }

        public void Subscribe(int name, string id, Action<T1, T2> handler)
        {
            if (!_IDMessages.ContainsKey(name))
            {
                _IDMessages.Add(name, new Dictionary<string, Action<T1, T2>>());
            }


            if (_IDMessages[name].ContainsKey(id))
            {
                _IDMessages[name][id] += handler;
            }
            else
            {
                _IDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(int name, string id, Action<T1, T2> handler)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id] -= handler;
                    if (_IDMessages[name][id] == null)
                    {
                        _IDMessages[name].Remove(id);
                        if (_IDMessages[name] == null)
                        {
                            _IDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(int name, string id, T1 value1, T2 value2)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id](value1, value2);
                }
            }
        }

        #endregion

        #region string

        public void Subscribe(string name, Action<T1, T2> handler)
        {
            if (!_strMessages.ContainsKey(name))
            {
                _strMessages.Add(name, handler);
            }
            else
            {
                _strMessages[name] += handler;
            }
        }

        public void Unsubscribe(string name, Action<T1, T2> handler)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name] -= handler;

                if (_strMessages[name] == null)
                {
                    _strMessages.Remove(name);
                }
            }
        }

        public void Publish(string name, T1 value1, T2 value2)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name](value1, value2);
            }
        }

        public void Subscribe(string name, string id, Action<T1, T2> handler)
        {
            if (!_strIDMessages.ContainsKey(name))
            {
                _strIDMessages.Add(name, new Dictionary<string, Action<T1, T2>>());
            }

            if (_strIDMessages[name].ContainsKey(id))
            {
                _strIDMessages[name][id] += handler;
            }
            else
            {
                _strIDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(string name, string id, Action<T1, T2> handler)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id] -= handler;
                    if (_strIDMessages[name][id] == null)
                    {
                        _strIDMessages[name].Remove(id);
                        if (_strIDMessages[name] == null)
                        {
                            _strIDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(string name, string id, T1 value1, T2 value2)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id](value1, value2);
                }
            }
        }

        #endregion
    }

    public class MessageAggregator<T>
    {
        public static MessageAggregator<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageAggregator<T>();
                }

                return _instance;
            }
        }

        private static MessageAggregator<T> _instance;

        private readonly Dictionary<int, Action<T>> _messages = new Dictionary<int, Action<T>>();
        private readonly Dictionary<int, Dictionary<string, Action<T>>> _IDMessages = new Dictionary<int, Dictionary<string, Action<T>>>();
        private readonly Dictionary<string, Action<T>> _strMessages = new Dictionary<string, Action<T>>();
        private readonly Dictionary<string, Dictionary<string, Action<T>>> _strIDMessages = new Dictionary<string, Dictionary<string, Action<T>>>();

        private MessageAggregator()
        {
        }

        #region int

        public void Subscribe(int name, Action<T> handler)
        {
            if (!_messages.ContainsKey(name))
            {
                _messages.Add(name, handler);
            }
            else
            {
                _messages[name] += handler;
            }
        }

        public void Unsubscribe(int name, Action<T> handler)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name] -= handler;

                if (_messages[name] == null)
                {
                    _messages.Remove(name);
                }
            }
        }

        public void Publish(int name, T value)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name](value);
            }
        }

        public void Subscribe(int name, string id, Action<T> handler)
        {
            if (!_IDMessages.ContainsKey(name))
            {
                _IDMessages.Add(name, new Dictionary<string, Action<T>>());
            }


            if (_IDMessages[name].ContainsKey(id))
            {
                _IDMessages[name][id] += handler;
            }
            else
            {
                _IDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(int name, string id, Action<T> handler)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id] -= handler;
                    if (_IDMessages[name][id] == null)
                    {
                        _IDMessages[name].Remove(id);
                        if (_IDMessages[name] == null)
                        {
                            _IDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(int name, string id, T value)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id](value);
                }
            }
        }

        #endregion

        #region string

        public void Subscribe(string name, Action<T> handler)
        {
            if (!_strMessages.ContainsKey(name))
            {
                _strMessages.Add(name, handler);
            }
            else
            {
                _strMessages[name] += handler;
            }
        }

        public void Unsubscribe(string name, Action<T> handler)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name] -= handler;

                if (_strMessages[name] == null)
                {
                    _strMessages.Remove(name);
                }
            }
        }

        public void Publish(string name, T value)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name](value);
            }
        }

        public void Subscribe(string name, string id, Action<T> handler)
        {
            if (!_strIDMessages.ContainsKey(name))
            {
                _strIDMessages.Add(name, new Dictionary<string, Action<T>>());
            }

            if (_strIDMessages[name].ContainsKey(id))
            {
                _strIDMessages[name][id] += handler;
            }
            else
            {
                _strIDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(string name, string id, Action<T> handler)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id] -= handler;
                    if (_strIDMessages[name][id] == null)
                    {
                        _strIDMessages[name].Remove(id);
                        if (_strIDMessages[name] == null)
                        {
                            _strIDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(string name, string id, T value)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id](value);
                }
            }
        }

        #endregion
    }

    public class MessageAggregator
    {
        public static MessageAggregator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageAggregator();
                }

                return _instance;
            }
        }

        private static MessageAggregator _instance;

        private readonly Dictionary<int, Action> _messages = new Dictionary<int, Action>();
        private readonly Dictionary<int, Dictionary<string, Action>> _IDMessages = new Dictionary<int, Dictionary<string, Action>>();
        private readonly Dictionary<string, Action> _strMessages = new Dictionary<string, Action>();
        private readonly Dictionary<string, Dictionary<string, Action>> _strIDMessages = new Dictionary<string, Dictionary<string, Action>>();

        private MessageAggregator()
        {
        }

        #region int

        public void Subscribe(int name, Action handler)
        {
            if (!_messages.ContainsKey(name))
            {
                _messages.Add(name, handler);
            }
            else
            {
                _messages[name] += handler;
            }
        }

        public void Unsubscribe(int name, Action handler)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name] -= handler;

                if (_messages[name] == null)
                {
                    _messages.Remove(name);
                }
            }
        }

        public void Publish(int name)
        {
            if (_messages.ContainsKey(name))
            {
                _messages[name]();
            }
        }

        public void Subscribe(int name, string id, Action handler)
        {
            if (!_IDMessages.ContainsKey(name))
            {
                _IDMessages.Add(name, new Dictionary<string, Action>());
            }


            if (_IDMessages[name].ContainsKey(id))
            {
                _IDMessages[name][id] += handler;
            }
            else
            {
                _IDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(int name, string id, Action handler)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id] -= handler;
                    if (_IDMessages[name][id] == null)
                    {
                        _IDMessages[name].Remove(id);
                        if (_IDMessages[name] == null)
                        {
                            _IDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(int name, string id)
        {
            if (_IDMessages.ContainsKey(name))
            {
                if (_IDMessages[name].ContainsKey(id))
                {
                    _IDMessages[name][id]();
                }
            }
        }

        #endregion

        #region string

        public void Subscribe(string name, Action handler)
        {
            if (!_strMessages.ContainsKey(name))
            {
                _strMessages.Add(name, handler);
            }
            else
            {
                _strMessages[name] += handler;
            }
        }

        public void Unsubscribe(string name, Action handler)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name] -= handler;

                if (_strMessages[name] == null)
                {
                    _strMessages.Remove(name);
                }
            }
        }

        public void Publish(string name)
        {
            if (_strMessages.ContainsKey(name))
            {
                _strMessages[name]();
            }
        }

        public void Subscribe(string name, string id, Action handler)
        {
            if (!_strIDMessages.ContainsKey(name))
            {
                _strIDMessages.Add(name, new Dictionary<string, Action>());
            }

            if (_strIDMessages[name].ContainsKey(id))
            {
                _strIDMessages[name][id] += handler;
            }
            else
            {
                _strIDMessages[name][id] = handler;
            }
        }

        public void Unsubscribe(string name, string id, Action handler)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id] -= handler;
                    if (_strIDMessages[name][id] == null)
                    {
                        _strIDMessages[name].Remove(id);
                        if (_strIDMessages[name] == null)
                        {
                            _strIDMessages.Remove(name);
                        }
                    }
                }
            }
        }

        public void PublishWithID(string name, string id)
        {
            if (_strIDMessages.ContainsKey(name))
            {
                if (_strIDMessages[name].ContainsKey(id))
                {
                    _strIDMessages[name][id]();
                }
            }
        }

        #endregion
    }
}