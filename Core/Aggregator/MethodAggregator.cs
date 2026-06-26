using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NonsensicalKit.Core
{
    #region TODO:可优化方向
    public abstract class BaseMethodAggregator<TResult>
    {
        protected readonly Dictionary<string, Delegate> _handlers = new();

        protected void AddHandler(string name, Delegate handler) => _handlers[name] = handler;

        protected TResult Execute(string name, params object[] args)
        {
            if (_handlers.TryGetValue(name, out var del))
                return (TResult)del.DynamicInvoke(args);
            throw new KeyNotFoundException(name);
        }
    }

    public class TestMethodAggregator<TValue1, TValue2, TValue3, TResult> : BaseMethodAggregator<TResult>
    {
        public void AddHandler(string name, [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
           base.AddHandler(name,handler);
        }
    }
    #endregion
  
    public class MethodAggregator<TValue1, TValue2, TValue3, TResult>
    {
        public static MethodAggregator<TValue1, TValue2, TValue3, TResult> Instance =>
            LazyInstance.Value;

        //线程安全
        private static readonly Lazy<MethodAggregator<TValue1, TValue2, TValue3, TResult>> LazyInstance =
            new(() => new MethodAggregator<TValue1, TValue2, TValue3, TResult>());


        private readonly Dictionary<int, Func<TValue1, TValue2, TValue3, TResult>> _method = new();

        private readonly Dictionary<int, Dictionary<string, Func<TValue1, TValue2, TValue3, TResult>>>
            _idMethod = new();

        private readonly Dictionary<string, Func<TValue1, TValue2, TValue3, TResult>> _strMethod = new();

        private readonly Dictionary<string, Dictionary<string, Func<TValue1, TValue2, TValue3, TResult>>> _strIDMethod =
            new();

        private MethodAggregator() { } //私有构造函数避免被外部实例化

        #region int

        public void AddHandler(int name, [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (!_method.TryAdd(name, handler))
            {
                _method[name] += handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (_method.ContainsKey(name))
            {
                _method[name] -= handler;

                if (_method[name] == null)
                {
                    _method.Remove(name, out _);
                }
            }
        }

        public TResult Execute(int name, TValue1 v1, TValue2 v2, TValue3 v3, TResult defaultValue = default)
        {
            if (_method.TryGetValue(name, out var method))
            {
                return method(v1, v2, v3);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll(int name, TValue1 v1, TValue2 v2, TValue3 v3)
        {
            if (_method.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue1, TValue2, TValue3, TResult>>()
                    .Select(m => m(v1, v2, v3));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler(int name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (!_idMethod.ContainsKey(name))
            {
                _idMethod.Add(name, new Dictionary<string, Func<TValue1, TValue2, TValue3, TResult>>());
            }


            if (_idMethod[name].ContainsKey(id))
            {
                _idMethod[name][id] += handler;
            }
            else
            {
                _idMethod[name][id] = handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    _idMethod[name][id] -= handler;
                    if (_idMethod[name][id] == null)
                    {
                        _idMethod[name].Remove(id);
                        if (_idMethod[name] == null)
                        {
                            _idMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID(int name, [DisallowNull] string id, TValue1 v1, TValue2 v2, TValue3 v3)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    return _idMethod[name][id](v1, v2, v3);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(int name, [DisallowNull] string id, TValue1 v1, TValue2 v2,
            TValue3 v3)
        {
            if (_idMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue1, TValue2, TValue3, TResult>>()
                        .Select(m => m(v1, v2, v3));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion

        #region string

        public void AddHandler([DisallowNull] string name,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (!_strMethod.TryAdd(name, handler))
            {
                _strMethod[name] += handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (_strMethod.ContainsKey(name))
            {
                _strMethod[name] -= handler;

                if (_strMethod[name] == null)
                {
                    _strMethod.Remove(name);
                }
            }
        }

        public TResult Execute([DisallowNull] string name, TValue1 v1, TValue2 v2, TValue3 v3,
            TResult defaultValue = default)
        {
            if (_strMethod.TryGetValue(name, out var method))
            {
                return method(v1, v2, v3);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll([DisallowNull] string name, TValue1 v1, TValue2 v2, TValue3 v3)
        {
            if (_strMethod.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue1, TValue2, TValue3, TResult>>()
                    .Select(m => m(v1, v2, v3));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (!_strIDMethod.ContainsKey(name))
            {
                _strIDMethod.Add(name, new Dictionary<string, Func<TValue1, TValue2, TValue3, TResult>>());
            }

            if (_strIDMethod[name].ContainsKey(id))
            {
                _strIDMethod[name][id] += handler;
            }
            else
            {
                _strIDMethod[name][id] = handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TValue3, TResult> handler)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    _strIDMethod[name][id] -= handler;
                    if (_strIDMethod[name][id] == null)
                    {
                        _strIDMethod[name].Remove(id);
                        if (_strIDMethod[name] == null)
                        {
                            _strIDMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID([DisallowNull] string name, [DisallowNull] string id, TValue1 v1, TValue2 v2,
            TValue3 v3)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    return _strIDMethod[name][id](v1, v2, v3);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(string name, [DisallowNull] string id, TValue1 v1, TValue2 v2,
            TValue3 v3)
        {
            if (_strIDMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue1, TValue2, TValue3, TResult>>()
                        .Select(m => m(v1, v2, v3));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion
    }

    public class MethodAggregator<TValue1, TValue2, TResult>
    {
        public static MethodAggregator<TValue1, TValue2, TResult> Instance =>
            LazyInstance.Value;

        //线程安全
        private static readonly Lazy<MethodAggregator<TValue1, TValue2, TResult>> LazyInstance =
            new(() => new MethodAggregator<TValue1, TValue2, TResult>());

        private readonly Dictionary<int, Func<TValue1, TValue2, TResult>> _method = new();
        private readonly Dictionary<int, Dictionary<string, Func<TValue1, TValue2, TResult>>> _idMethod = new();
        private readonly Dictionary<string, Func<TValue1, TValue2, TResult>> _strMethod = new();
        private readonly Dictionary<string, Dictionary<string, Func<TValue1, TValue2, TResult>>> _strIDMethod = new();

        private MethodAggregator() { } //私有构造函数避免被外部实例化

        #region int

        public void AddHandler(int name, [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (!_method.TryAdd(name, handler))
            {
                _method[name] += handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (_method.ContainsKey(name))
            {
                _method[name] -= handler;

                if (_method[name] == null)
                {
                    _method.Remove(name);
                }
            }
        }

        public TResult Execute(int name, TValue1 v1, TValue2 v2, TResult defaultValue = default)
        {
            if (_method.TryGetValue(name, out var method))
            {
                return method(v1, v2);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll(int name, TValue1 v1, TValue2 v2)
        {
            if (_method.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue1, TValue2, TResult>>()
                    .Select(m => m(v1, v2));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler(int name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (!_idMethod.ContainsKey(name))
            {
                _idMethod.Add(name, new Dictionary<string, Func<TValue1, TValue2, TResult>>());
            }


            if (_idMethod[name].ContainsKey(id))
            {
                _idMethod[name][id] += handler;
            }
            else
            {
                _idMethod[name][id] = handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    _idMethod[name][id] -= handler;
                    if (_idMethod[name][id] == null)
                    {
                        _idMethod[name].Remove(id);
                        if (_idMethod[name] == null)
                        {
                            _idMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID(int name, [DisallowNull] string id, TValue1 v1, TValue2 v2)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    return _idMethod[name][id](v1, v2);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(int name, [DisallowNull] string id, TValue1 v1, TValue2 v2)
        {
            if (_idMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue1, TValue2, TResult>>()
                        .Select(m => m(v1, v2));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion

        #region string

        public void AddHandler([DisallowNull] string name, [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (!_strMethod.TryAdd(name, handler))
            {
                _strMethod[name] += handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (_strMethod.ContainsKey(name))
            {
                _strMethod[name] -= handler;

                if (_strMethod[name] == null)
                {
                    _strMethod.Remove(name);
                }
            }
        }

        public TResult Execute([DisallowNull] string name, TValue1 v1, TValue2 v2, TResult defaultValue = default)
        {
            if (_strMethod.TryGetValue(name, out var method))
            {
                return method(v1, v2);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll([DisallowNull] string name, TValue1 v1, TValue2 v2)
        {
            if (_strMethod.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue1, TValue2, TResult>>()
                    .Select(m => m(v1, v2));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (!_strIDMethod.ContainsKey(name))
            {
                _strIDMethod.Add(name, new Dictionary<string, Func<TValue1, TValue2, TResult>>());
            }

            if (_strIDMethod[name].ContainsKey(id))
            {
                _strIDMethod[name][id] += handler;
            }
            else
            {
                _strIDMethod[name][id] = handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue1, TValue2, TResult> handler)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    _strIDMethod[name][id] -= handler;
                    if (_strIDMethod[name][id] == null)
                    {
                        _strIDMethod[name].Remove(id);
                        if (_strIDMethod[name] == null)
                        {
                            _strIDMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID([DisallowNull] string name, [DisallowNull] string id, TValue1 v1,
            TValue2 v2)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    return _strIDMethod[name][id](v1, v2);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(string name, [DisallowNull] string id, TValue1 v1, TValue2 v2)
        {
            if (_strIDMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue1, TValue2, TResult>>()
                        .Select(m => m(v1, v2));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion
    }


    public class MethodAggregator<TValue, TResult>
    {
        public static MethodAggregator<TValue, TResult> Instance =>
            LazyInstance.Value;

        //线程安全
        private static readonly Lazy<MethodAggregator<TValue, TResult>> LazyInstance =
            new(() => new MethodAggregator<TValue, TResult>());

        private readonly Dictionary<int, Func<TValue, TResult>> _method = new();
        private readonly Dictionary<int, Dictionary<string, Func<TValue, TResult>>> _idMethod = new();
        private readonly Dictionary<string, Func<TValue, TResult>> _strMethod = new();
        private readonly Dictionary<string, Dictionary<string, Func<TValue, TResult>>> _strIDMethod = new();

        private MethodAggregator() { } //私有构造函数避免被外部实例化

        #region int

        public void AddHandler(int name, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (!_method.TryAdd(name, handler))
            {
                _method[name] += handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (_method.ContainsKey(name))
            {
                _method[name] -= handler;

                if (_method[name] == null)
                {
                    _method.Remove(name);
                }
            }
        }

        public TResult Execute(int name, TValue value, TResult defaultValue = default)
        {
            if (_method.TryGetValue(name, out var method))
            {
                return method(value);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll(int name, TValue v)
        {
            if (_method.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue, TResult>>()
                    .Select(m => m(v));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler(int name, [DisallowNull] string id, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (!_idMethod.ContainsKey(name))
            {
                _idMethod.Add(name, new Dictionary<string, Func<TValue, TResult>>());
            }


            if (_idMethod[name].ContainsKey(id))
            {
                _idMethod[name][id] += handler;
            }
            else
            {
                _idMethod[name][id] = handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] string id, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    _idMethod[name][id] -= handler;
                    if (_idMethod[name][id] == null)
                    {
                        _idMethod[name].Remove(id);
                        if (_idMethod[name] == null)
                        {
                            _idMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID(int name, [DisallowNull] string id, TValue value)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    return _idMethod[name][id](value);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(int name, [DisallowNull] string id, TValue value)
        {
            if (_idMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue, TResult>>()
                        .Select(m => m(value));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion

        #region string

        public void AddHandler([DisallowNull] string name, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (!_strMethod.TryAdd(name, handler))
            {
                _strMethod[name] += handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] Func<TValue, TResult> handler)
        {
            if (_strMethod.ContainsKey(name))
            {
                _strMethod[name] -= handler;

                if (_strMethod[name] == null)
                {
                    _strMethod.Remove(name);
                }
            }
        }

        public TResult Execute([DisallowNull] string name, TValue value, TResult defaultValue = default)
        {
            if (_strMethod.TryGetValue(name, out var method))
            {
                return method(value);
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll([DisallowNull] string name, TValue v)
        {
            if (_strMethod.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TValue, TResult>>()
                    .Select(m => m(v));
            return Enumerable.Empty<TResult>();
        }

        public void AddHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue, TResult> handler)
        {
            if (!_strIDMethod.ContainsKey(name))
            {
                _strIDMethod.Add(name, new Dictionary<string, Func<TValue, TResult>>());
            }

            if (_strIDMethod[name].ContainsKey(id))
            {
                _strIDMethod[name][id] += handler;
            }
            else
            {
                _strIDMethod[name][id] = handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TValue, TResult> handler)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    _strIDMethod[name][id] -= handler;
                    if (_strIDMethod[name][id] == null)
                    {
                        _strIDMethod[name].Remove(id);
                        if (_strIDMethod[name] == null)
                        {
                            _strIDMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID([DisallowNull] string name, [DisallowNull] string id, TValue value)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    return _strIDMethod[name][id](value);
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(string name, [DisallowNull] string id, TValue value)
        {
            if (_strIDMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TValue, TResult>>()
                        .Select(m => m(value));
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion
    }

    public class MethodAggregator<TResult>
    {
        public static MethodAggregator<TResult> Instance =>
            LazyInstance.Value;

        //线程安全
        private static readonly Lazy<MethodAggregator<TResult>> LazyInstance =
            new(() => new MethodAggregator<TResult>());

        private readonly Dictionary<int, Func<TResult>> _method = new();
        private readonly Dictionary<int, Dictionary<string, Func<TResult>>> _idMethod = new();
        private readonly Dictionary<string, Func<TResult>> _strMethod = new();
        private readonly Dictionary<string, Dictionary<string, Func<TResult>>> _strIDMethod = new();

        private MethodAggregator() { } //私有构造函数避免被外部实例化

        #region int

        public void AddHandler(int name, [DisallowNull] Func<TResult> handler)
        {
            if (!_method.TryAdd(name, handler))
            {
                _method[name] += handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] Func<TResult> handler)
        {
            if (_method.ContainsKey(name))
            {
                _method[name] -= handler;

                if (_method[name] == null)
                {
                    _method.Remove(name);
                }
            }
        }

        public TResult Execute(int name, TResult defaultValue = default)
        {
            if (_method.TryGetValue(name, out var method))
            {
                return method();
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll(int name)
        {
            if (_method.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TResult>>()
                    .Select(m => m());
            return Enumerable.Empty<TResult>();
        }


        public void AddHandler(int name, [DisallowNull] string id, [DisallowNull] Func<TResult> handler)
        {
            if (!_idMethod.ContainsKey(name))
            {
                _idMethod.Add(name, new Dictionary<string, Func<TResult>>());
            }


            if (_idMethod[name].ContainsKey(id))
            {
                _idMethod[name][id] += handler;
            }
            else
            {
                _idMethod[name][id] = handler;
            }
        }

        public void RemoveHandler(int name, [DisallowNull] string id, [DisallowNull] Func<TResult> handler)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    _idMethod[name][id] -= handler;
                    if (_idMethod[name][id] == null)
                    {
                        _idMethod[name].Remove(id);
                        if (_idMethod[name] == null)
                        {
                            _idMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID(int name, [DisallowNull] string id)
        {
            if (_idMethod.ContainsKey(name))
            {
                if (_idMethod[name].ContainsKey(id))
                {
                    return _idMethod[name][id]();
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(int name, [DisallowNull] string id)
        {
            if (_idMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TResult>>()
                        .Select(m => m());
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion

        #region string

        public void AddHandler([DisallowNull] string name, [DisallowNull] Func<TResult> handler)
        {
            if (!_strMethod.TryAdd(name, handler))
            {
                _strMethod[name] += handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] Func<TResult> handler)
        {
            if (_strMethod.ContainsKey(name))
            {
                _strMethod[name] -= handler;

                if (_strMethod[name] == null)
                {
                    _strMethod.Remove(name);
                }
            }
        }

        public TResult Execute([DisallowNull] string name, TResult defaultValue = default)
        {
            if (_strMethod.TryGetValue(name, out var method))
            {
                return method();
            }

            return defaultValue;
        }

        public IEnumerable<TResult> ExecuteAll([DisallowNull] string name)
        {
            if (_strMethod.TryGetValue(name, out var method))
                return method.GetInvocationList().Cast<Func<TResult>>()
                    .Select(m => m());
            return Enumerable.Empty<TResult>();
        }


        public void AddHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TResult> handler)
        {
            if (!_strIDMethod.ContainsKey(name))
            {
                _strIDMethod.Add(name, new Dictionary<string, Func<TResult>>());
            }

            if (_strIDMethod[name].ContainsKey(id))
            {
                _strIDMethod[name][id] += handler;
            }
            else
            {
                _strIDMethod[name][id] = handler;
            }
        }

        public void RemoveHandler([DisallowNull] string name, [DisallowNull] string id,
            [DisallowNull] Func<TResult> handler)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    _strIDMethod[name][id] -= handler;
                    if (_strIDMethod[name][id] == null)
                    {
                        _strIDMethod[name].Remove(id);
                        if (_strIDMethod[name] == null)
                        {
                            _strIDMethod.Remove(name);
                        }
                    }
                }
            }
        }

        public TResult ExecuteWithID([DisallowNull] string name, [DisallowNull] string id)
        {
            if (_strIDMethod.ContainsKey(name))
            {
                if (_strIDMethod[name].ContainsKey(id))
                {
                    return _strIDMethod[name][id]();
                }
            }

            return default;
        }

        public IEnumerable<TResult> ExecuteAllWithID(string name, [DisallowNull] string id)
        {
            if (_strIDMethod.TryGetValue(name, out var idDic))
            {
                if (idDic.TryGetValue(id, out var method))
                {
                    return method.GetInvocationList().Cast<Func<TResult>>()
                        .Select(m => m());
                }
            }

            return Enumerable.Empty<TResult>();
        }

        #endregion
    }
}
