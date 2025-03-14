using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    /// <summary>
    /// GameObject对象池，只管理缓存对象
    /// </summary>
    public class GameObjectPool
    {
        public Action<GameObjectPool, GameObject> FirstInitAction
        {
            set => _createAction = value;
        }

        private readonly GameObject _prefab; //预制体
        private readonly Queue<GameObject> _queue; //待使用的对象
        private readonly Action<GameObject> _resetAction; //返回池中后调用
        private readonly Action<GameObject> _initAction; //取出时调用
        private Action<GameObjectPool, GameObject> _createAction; //首次生成时调用

        public GameObjectPool(GameObject prefab)
        {
            _prefab = prefab;
            _queue = new Queue<GameObject>();
            _resetAction = DefaultReset;
            _initAction = DefaultInit;
        }

        public GameObjectPool(GameObject prefab, Action<GameObject> resetAction = null, Action<GameObject> initAction = null,
            Action<GameObjectPool, GameObject> createAction = null)
        {
            _prefab = prefab;
            _queue = new Queue<GameObject>();
            _resetAction = resetAction;
            _initAction = initAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            if (_queue.Count > 0)
            {
                GameObject t = _queue.Dequeue();
                _initAction?.Invoke(t);
                return t;
            }
            else
            {
                GameObject t = Object.Instantiate(_prefab);
                _createAction?.Invoke(this, t);
                _initAction?.Invoke(t);

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(GameObject obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
            }
        }

        public static void DefaultReset(GameObject go)
        {
            go.SetActive(false);
        }

        public static void DefaultInit(GameObject go)
        {
            go.SetActive(true);
        }
    }

    /// <summary>
    /// 除了缓存对象外，还管理了使用中对象
    /// </summary>
    public class GameObjectPool_MK2
    {
        private readonly GameObject _prefab; //预制体
        private readonly Queue<GameObject> _queue; //待使用的对象
        private readonly List<GameObject> _actives; //使用中的对象
        private readonly Action<GameObject> _resetAction; //返回池中后调用
        private readonly Action<GameObject> _initAction; //取出时调用
        private readonly Action<GameObjectPool_MK2, GameObject> _createAction; //首次生成时调用

        public GameObjectPool_MK2(GameObject prefab,
            Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null,
            Action<GameObjectPool_MK2, GameObject> createAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<GameObject>();
            _actives = new List<GameObject>();
            _resetAction = resetAction;
            _initAction = initAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            if (_queue.Count > 0)
            {
                GameObject t = _queue.Dequeue();
                _actives.Add(t);
                _initAction?.Invoke(t);
                return t;
            }
            else
            {
                GameObject t = Object.Instantiate(_prefab);
                _actives.Add(t);
                _createAction?.Invoke(this, t);
                _initAction?.Invoke(t);

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(GameObject obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
                if (_actives.Contains(obj))
                {
                    _actives.Remove(obj);
                }
            }
        }

        public void Clear()
        {
            foreach (var item in _actives)
            {
                _resetAction?.Invoke(item);
                _queue.Enqueue(item);
            }

            _actives.Clear();
        }
    }

    public class ComponentPool<TComponent> where TComponent : Component
    {
        private readonly TComponent _prefab; //预制体
        private readonly Queue<TComponent> _queue; //待使用的对象
        private readonly Action<TComponent> _resetAction; //返回池中后调用
        private readonly Action<TComponent> _initAction; //取出时调用
        private readonly Action<ComponentPool<TComponent>, TComponent> _createAction; //首次生成时调用

        public ComponentPool(TComponent prefab)
        {
            this._prefab = prefab;
            _queue = new Queue<TComponent>();
            _resetAction = DefaultReset;
            _initAction = DefaultInit;
        }

        public ComponentPool(TComponent prefab, Action<TComponent> resetAction = null, Action<TComponent> initAction = null,
            Action<ComponentPool<TComponent>, TComponent> createAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<TComponent>();
            _resetAction = resetAction;
            _initAction = initAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public TComponent New()
        {
            if (_queue.Count > 0)
            {
                TComponent t = _queue.Dequeue();
                _initAction?.Invoke(t);
                return t;
            }
            else
            {
                TComponent t = Object.Instantiate(_prefab);
                _createAction?.Invoke(this, t);
                _initAction?.Invoke(t);

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(TComponent obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
            }
        }

        public static void DefaultReset(TComponent go)
        {
            go.gameObject.SetActive(false);
        }

        public static void DefaultInit(TComponent go)
        {
            go.gameObject.SetActive(true);
        }
    }

    public class ComponentPool_MK2<TComponent> where TComponent : Component
    {
        private readonly TComponent _prefab; //预制体
        private readonly Queue<TComponent> _queue; //待使用的对象
        private readonly List<TComponent> _actives; //使用中的对象
        private readonly Action<TComponent> _resetAction; //返回池中后调用
        private readonly Action<TComponent> _initAction; //取出时调用
        private readonly Action<ComponentPool_MK2<TComponent>, TComponent> _createAction; //首次生成时调用

        public ComponentPool_MK2(TComponent prefab,
            Action<TComponent> resetAction = null,
            Action<TComponent> initAction = null,
            Action<ComponentPool_MK2<TComponent>, TComponent> createAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<TComponent>();
            _actives = new List<TComponent>();
            _resetAction = resetAction;
            _initAction = initAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public TComponent New()
        {
            TComponent newC;
            if (_queue.Count > 0)
            {
                newC = _queue.Dequeue();
            }
            else
            {
                newC = Object.Instantiate(_prefab);
                _createAction?.Invoke(this, newC);
            }

            _initAction?.Invoke(newC);
            _actives.Add(newC);
            return newC;
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(TComponent obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
                if (_actives.Contains(obj))
                {
                    _actives.Remove(obj);
                }
            }
        }

        public void Clear()
        {
            foreach (var item in _actives)
            {
                _resetAction?.Invoke(item);
                _queue.Enqueue(item);
            }

            _actives.Clear();
        }
    }
    
    /// <summary>
    /// 添加缓冲-使用-回收逻辑
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public class ComponentPool_MK3<TComponent> where TComponent : Component
    {
        private readonly TComponent _prefab; //预制体
        private readonly Queue<TComponent> _queue; //待使用的对象
        private readonly List<TComponent> _actives; //使用中的对象
        private readonly Action<TComponent> _resetAction; //返回池中后调用
        private readonly Action<TComponent> _initAction; //取出时调用
        private readonly Action<TComponent> _reinitAction; //不返回池中直接重新使用时调用
        private readonly Action<ComponentPool_MK3<TComponent>, TComponent> _createAction; //首次生成时调用

        private List<TComponent> _cache;
        private int _catchIndex;

        public ComponentPool_MK3(TComponent prefab,
            Action<TComponent> resetAction = null,
            Action<TComponent> initAction = null,
            Action<TComponent> reinitAction = null,
            Action<ComponentPool_MK3<TComponent>, TComponent> createAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<TComponent>();
            _actives = new List<TComponent>();
            _resetAction = resetAction;
            _initAction = initAction;
            _reinitAction= reinitAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public TComponent New()
        {
            TComponent newC;
            if ((_cache != null) && (_catchIndex < _cache.Count))
            {
                newC = _cache[_catchIndex];
                _catchIndex++;
                _reinitAction?.Invoke(newC);
            }
            else if (_queue.Count > 0)
            {
                newC = _queue.Dequeue();
                _initAction?.Invoke(newC);
            }
            else
            {
                newC = Object.Instantiate(_prefab);
                _createAction?.Invoke(this, newC);
                _initAction?.Invoke(newC);
            }
            _actives.Add(newC);

            return newC;
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(TComponent obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
                if (_actives.Contains(obj))
                {
                    _actives.Remove(obj);
                }
            }
        }
        
        /// <summary>
        /// 和Flush配套使用
        /// 缓存当前使用的对象，在取出对象时优先使用缓存的对象，在需要全部回收后重新配置时使用，避免回收后马上取用造成的不必要性能消耗
        /// 使用方法为先调用Cache缓存当前激活对象，然后正常使用New获取和使用对象，最后使用Flush回收未被使用的缓存激活对象
        /// </summary>
        public void Cache()
        {
            if (_cache is { Count: > 0 })
            {
                Flush();
            }
            _cache = _actives.Clone();
            _actives.Clear();
        }
        
        /// <summary>
        /// 和Flush配套使用，在不需要全部回收，但希望避免重复回收-使用造成不必要消耗时使用
        /// </summary>
        /// <param name="obj"></param>
        public void Cache(TComponent obj)
        {
            if (_cache==null)
            {
                _cache=new List<TComponent>();
            }
            _cache.Add(obj);
            _actives.Remove(obj);
        }

        /// <summary>
        /// 和Catch配套使用，回收未取用的缓存
        /// </summary>
        public void Flush()
        {
            if (_cache == null)
            {
                return;
            }

            for (; _catchIndex < _cache.Count; _catchIndex++)
            {
                _resetAction?.Invoke(_cache[_catchIndex]);
                _queue.Enqueue(_cache[_catchIndex]);
            }

            _catchIndex = 0;
            _cache.Clear();
            _cache = null;
        }

        public void Clear()
        {
            foreach (var item in _actives)
            {
                _resetAction?.Invoke(item);
                _queue.Enqueue(item);
            }

            _actives.Clear();
        }
    }

    [Serializable]
    public class SerializableGameObjectPool
    {
        [SerializeField] private GameObject m_prefab; //预制体
        [SerializeField] private Transform m_birthPoint; //出生点
        [SerializeField] private List<GameObject> m_using = new List<GameObject>(); //使用中对象
        [SerializeField] private List<GameObject> m_storage = new List<GameObject>(); //存储对象

        public GameObject Prefab
        {
            get => m_prefab;
            set => m_prefab = value;
        }

        private List<GameObject> _cache;
        private int _catchIndex;

        public void Init(GameObject prefab, Transform birthPoint)
        {
            m_prefab = prefab;
            m_birthPoint = birthPoint;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            GameObject newGo;
            if ((_cache != null) && (_catchIndex < _cache.Count))
            {
                newGo = _cache[_catchIndex];
                _catchIndex++;
                m_using.Add(newGo);
            }
            else if (m_storage.Count > 0)
            {
                newGo = m_storage[0];
                m_storage.RemoveAt(0);
                newGo.SetActive(true);
                m_using.Add(newGo);
            }
            else
            {
                newGo = Object.Instantiate(m_prefab, m_birthPoint);
                newGo.SetActive(true);
                m_using.Add(newGo);
            }

            return newGo;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="go"></param>
        public void Store(GameObject go)
        {
            if (m_using.Contains(go) && (m_storage.Contains(go) == false))
            {
                m_using.Remove(go);
                m_storage.Add(go);
                go.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 和Flush配套使用
        /// 缓存当前使用的对象，在取出对象时优先使用缓存的对象，在需要全部回收后重新配置时使用，避免回收后马上取用造成的不必要性能消耗
        /// </summary>
        public void Cache()
        {
            _cache = m_using;
            _catchIndex = 0;
            m_using = new List<GameObject>();
        }

        /// <summary>
        /// 和Catch配套使用，回收未取用的缓存
        /// </summary>
        public void Flush()
        {
            if (_cache == null)
            {
                return;
            }

            for (; _catchIndex < _cache.Count; _catchIndex++)
            {
                _cache[_catchIndex].SetActive(false);
                m_storage.Add(_cache[_catchIndex]);
            }

            _cache.Clear();
            _cache = null;
        }

        /// <summary>
        /// 把使用中的对象全部回收
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_using)
            {
                item.SetActive(false);
                m_storage.Add(item);
            }

            m_using.Clear();
        }

        /// <summary>
        /// 销毁所有对象
        /// </summary>
        public void Clean()
        {
            foreach (var item in m_using)
            {
                item.Destroy();
            }

            foreach (var item in m_storage)
            {
                item.Destroy();
            }

            m_using.Clear();
            m_storage.Clear();
            _cache = null;
        }
    }
}
