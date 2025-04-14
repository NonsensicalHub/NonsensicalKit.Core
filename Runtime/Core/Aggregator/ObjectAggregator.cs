using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 对象聚合器
    /// </summary>
    public class ObjectAggregator<T>
    {
        public static ObjectAggregator<T> Instance => _instance ??= new ObjectAggregator<T>();
        private static ObjectAggregator<T> _instance;

        private T _field;
        private Func<T> _fallBack;
        private Action<T> _listener;

        private readonly Dictionary<int, T> _intFields = new();
        private readonly Dictionary<int, Func<T>> _intFallBacks = new();
        private readonly Dictionary<int, Action<T>> _intFieldListeners = new();

        private readonly Dictionary<string, T> _strFields = new();
        private readonly Dictionary<string, Func<T>> _strFallBacks = new();
        private readonly Dictionary<string, Action<T>> _strFieldListeners = new();

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

        [return: MaybeNull]
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

        public bool TryGet([MaybeNull] out T value)
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
            foreach (var delegates in ds)
            {
                var item = (Func<T>)delegates;
                if (item != null)
                {
                    list.Add(item());
                }
            }

            return list;
        }

        public void Register([DisallowNull] Func<T> fallback)
        {
            _fallBack += fallback;
            _listener?.Invoke(fallback());
        }

        public void Unregister([DisallowNull] Func<T> fallback)
        {
            _fallBack -= fallback;
        }

        public void AddListener([DisallowNull] Action<T> listener)
        {
            _listener += listener;
        }

        public void RemoveListener([DisallowNull] Action<T> listener)
        {
            _listener -= listener;
        }

        #endregion


        #region int

        public void Set(int name, T value)
        {
            _intFields[name] = value;
            if (_intFieldListeners.ContainsKey(name))
            {
                _intFieldListeners[name]?.Invoke(value);
            }
        }

        [return: MaybeNull]
        public T Get(int name)
        {
            if (_intFields.ContainsKey(name) && _intFields[name] != null)
            {
                return _intFields[name];
            }
            else if (_intFallBacks.TryGetValue(name, out var back))
            {
                return back();
            }
            else
            {
                return default;
            }
        }

        public bool TryGet(int name, [MaybeNull] out T value)
        {
            if (_intFields.ContainsKey(name) && _intFields[name] != null)
            {
                value = _intFields[name];
                return true;
            }
            else if (_intFallBacks.TryGetValue(name, out var back))
            {
                value = back();
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
            if (_intFallBacks.TryGetValue(name, out var back))
            {
                var ds = back.GetInvocationList();
                foreach (var delegates in ds)
                {
                    var item = (Func<T>)delegates;
                    if (item != null)
                    {
                        list.Add(item());
                    }
                }
            }

            return list;
        }

        public void Register(int name, [DisallowNull] Func<T> fallback)
        {
            if (!_intFallBacks.TryAdd(name, fallback))
            {
                _intFallBacks[name] += fallback;
            }

            if (_intFieldListeners.TryGetValue(name, out var listener))
            {
                listener?.Invoke(fallback());
            }
        }

        public void Unregister(int name, [DisallowNull] Func<T> fallback)
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

        public void AddListener(int name, [DisallowNull] Action<T> listener)
        {
            if (!_intFieldListeners.TryAdd(name, listener))
            {
                _intFieldListeners[name] += listener;
            }
        }

        public void RemoveListener(int name, [DisallowNull] Action<T> listener)
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

        public void Set([DisallowNull] string name, T value)
        {
            _strFields[name] = value;
            if (_strFieldListeners.TryGetValue(name, out var listener))
            {
                listener?.Invoke(value);
            }
        }

        [return: MaybeNull]
        public T Get([DisallowNull] string name)
        {
            if (_strFields.ContainsKey(name) && _strFields[name] != null)
            {
                return _strFields[name];
            }
            else if (_strFallBacks.TryGetValue(name, out var back))
            {
                return back();
            }
            else
            {
                return default;
            }
        }

        public bool TryGet([DisallowNull] string name, [MaybeNull] out T value)
        {
            if (_strFields.ContainsKey(name) && _strFields[name] != null)
            {
                value = _strFields[name];
                return true;
            }
            else if (_strFallBacks.TryGetValue(name, out var back))
            {
                value = back();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public List<T> GetAll([DisallowNull] string name)
        {
            List<T> list = new List<T>();
            if (_strFallBacks.TryGetValue(name, out var back))
            {
                var ds = back.GetInvocationList();
                foreach (var delegates in ds)
                {
                    var item = (Func<T>)delegates;
                    if (item != null)
                    {
                        list.Add(item());
                    }
                }
            }

            return list;
        }

        public void Register([DisallowNull] string name, [DisallowNull] Func<T> fallback)
        {
            if (!_strFallBacks.TryAdd(name, fallback))
            {
                _strFallBacks[name] += fallback;
            }

            if (_strFieldListeners.TryGetValue(name, out var listener))
            {
                listener?.Invoke(fallback());
            }
        }

        public void Unregister([DisallowNull] string name, [DisallowNull] Func<T> fallback)
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

        public void AddListener([DisallowNull] string name, [DisallowNull] Action<T> listener)
        {
            if (!_strFieldListeners.TryAdd(name, listener))
            {
                _strFieldListeners[name] += listener;
            }
        }

        public void RemoveListener([DisallowNull] string name, [DisallowNull] Action<T> listener)
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
