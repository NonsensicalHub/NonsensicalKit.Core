using System;
using System.Collections.Generic;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 对象聚合器
    /// </summary>
    public class ObjectAggregator<T>
    {
        public static ObjectAggregator<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ObjectAggregator<T>();
                }

                return _instance;
            }
        }
        private static ObjectAggregator<T> _instance;

        private T _field;
        private Func<T> _fallBack = null;
        private Action<T> _listener = null;

        private readonly Dictionary<int, T> _intFields = new Dictionary<int, T>();
        private readonly Dictionary<int, Func<T>> _intFallBacks = new Dictionary<int, Func<T>>();
        private readonly Dictionary<int, Action<T>> _intFieldListeners = new Dictionary<int, Action<T>>();

        private readonly Dictionary<string, T> _strFields = new Dictionary<string, T>();
        private readonly Dictionary<string, Func<T>> _strFallBacks = new Dictionary<string, Func<T>>();
        private readonly Dictionary<string, Action<T>> _strFieldListeners = new Dictionary<string, Action<T>>();

        /// <summary>
        /// 私有构造函数防止外部生成新对象
        /// </summary>
        private ObjectAggregator()
        {

        }

        #region Type
        public void Set(T value)
        {
            _field = value;
            _listener?.Invoke(value);
        }

        public T Get()
        {
            if (_field != null)
            {
                return _field;
            }
            else if (_fallBack != null)
            {
                return _fallBack();
            }
            else
            {
                return default(T);
            }
        }

        public bool TryGet(out T value)
        {
            if (_field != null)
            {
                value = _field;
                return true;
            }
            else if (_fallBack != null)
            {
                value = _fallBack();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public List<T> GetAll()
        {
            List<T> list = new List<T>();
            if (_fallBack == null)
            {
                return list;
            }
            var ds = _fallBack.GetInvocationList();
            foreach (Func<T> item in ds)
            {
                if (item != null)
                {
                    list.Add(item());
                }
            }
            return list;
        }

        public void Register(Func<T> fallback)
        {
            _fallBack += fallback;
        }

        public void Unregister(Func<T> fallback)
        {
            _fallBack -= fallback;
        }

        public void AddListener(Action<T> listener)
        {
            _listener += listener;
        }

        public void RemoveListener(Action<T> listener)
        {
            _listener -= listener;
        }
        #endregion


        #region int
        public void Set(int name, T value)
        {
            if (!_intFields.ContainsKey(name))
            {
                _intFields.Add(name, value);
            }
            else
            {
                _intFields[name] = value;
            }
            if (_intFieldListeners.ContainsKey(name))
            {
                _intFieldListeners[name]?.Invoke(value);
            }
        }

        public T Get(int name)
        {
            if (_intFields.ContainsKey(name) && _intFields[name] != null)
            {
                return _intFields[name];
            }
            else if (_intFallBacks.ContainsKey(name))
            {
                return _intFallBacks[name]();
            }
            else
            {
                return default;
            }
        }

        public bool TryGet(int name, out T value)
        {
            if (_intFields.ContainsKey(name) && _intFields[name] != null)
            {
                value = _intFields[name];
                return true;
            }
            else if (_intFallBacks.ContainsKey(name))
            {
                value = _intFallBacks[name]();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public List<T> GetAll(int name)
        {
            List<T> list = new List<T>();
            if (_intFallBacks.ContainsKey(name))
            {
                var ds = _intFallBacks[name].GetInvocationList();
                foreach (Func<T> item in ds)
                {
                    if (item != null)
                    {
                        list.Add(item());
                    }
                }
            }

            return list;
        }

        public void Register(int name, Func<T> fallback)
        {
            if (!_intFallBacks.ContainsKey(name))
            {
                _intFallBacks.Add(name, fallback);
            }
            else
            {
                _intFallBacks[name] += fallback;
            }
        }

        public void Unregister(int name, Func<T> fallback)
        {
            if (_intFallBacks.ContainsKey(name))
            {
                _intFallBacks[name] -= fallback;
                if (_intFallBacks[name] == null)
                {
                    _intFallBacks.Remove(name);
                }
            }
        }

        public void AddListener(int name, Action<T> listener)
        {
            if (!_intFieldListeners.ContainsKey(name))
            {
                _intFieldListeners.Add(name, listener);
            }
            else
            {
                _intFieldListeners[name] += listener;
            }
        }

        public void RemoveListener(int name, Action<T> listener)
        {
            if (_intFieldListeners.ContainsKey(name))
            {
                _intFieldListeners[name] -= listener;

                if (_intFieldListeners[name] == null)
                {
                    _intFieldListeners.Remove(name);
                }
            }
        }
        #endregion  


        #region string
        public void Set(string name, T value)
        {
            if (!_strFields.ContainsKey(name))
            {
                _strFields.Add(name, value);
            }
            else
            {
                _strFields[name] = value;
            }
            if (_strFieldListeners.ContainsKey(name))
            {
                _strFieldListeners[name]?.Invoke(value);
            }
        }

        public T Get(string name)
        {
            if (_strFields.ContainsKey(name) && _strFields[name] != null)
            {
                return _strFields[name];
            }
            else if (_strFallBacks.ContainsKey(name))
            {
                return _strFallBacks[name]();
            }
            else
            {
                return default;
            }
        }

        public bool TryGet(string name, out T value)
        {
            if (_strFields.ContainsKey(name) && _strFields[name] != null)
            {
                value = _strFields[name];
                return true;
            }
            else if (_strFallBacks.ContainsKey(name))
            {
                value = _strFallBacks[name]();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public List<T> GetAll(string name)
        {
            List<T> list = new List<T>();
            if (_strFallBacks.ContainsKey(name))
            {
                var ds = _strFallBacks[name].GetInvocationList();
                foreach (Func<T> item in ds)
                {
                    if (item != null)
                    {
                        list.Add(item());
                    }
                }
            }

            return list;
        }

        public void Register(string name, Func<T> fallback)
        {
            if (!_strFallBacks.ContainsKey(name))
            {
                _strFallBacks.Add(name, fallback);
            }
            else
            {
                _strFallBacks[name] += fallback;
            }
        }

        public void Unregister(string name, Func<T> fallback)
        {
            if (_strFallBacks.ContainsKey(name))
            {
                _strFallBacks[name] -= fallback;
                if (_strFallBacks[name] == null)
                {
                    _strFallBacks.Remove(name);
                }
            }
        }
        public void AddListener(string name, Action<T> listener)
        {
            if (!_strFieldListeners.ContainsKey(name))
            {
                _strFieldListeners.Add(name, listener);
            }
            else
            {
                _strFieldListeners[name] += listener;
            }
        }

        public void RemoveListener(string name, Action<T> listener)
        {
            if (_strFieldListeners.ContainsKey(name))
            {
                _strFieldListeners[name] -= listener;

                if (_strFieldListeners[name] == null)
                {
                    _strFieldListeners.Remove(name);
                }
            }
        }
        #endregion
    }
}
