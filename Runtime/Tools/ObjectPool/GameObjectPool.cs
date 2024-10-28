using System;
using System.Collections.Generic;
using UnityEngine;
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
            set { _firstInitAction = value; }
        }

        private GameObject _prefab; //预制体
        private Queue<GameObject> _queue; //待使用的对象
        private Action<GameObject> _resetAction; //返回池中后调用
        private Action<GameObject> initAction; //取出时调用
        private Action<GameObjectPool, GameObject> _firstInitAction; //首次生成时调用

        public GameObjectPool(GameObject prefab)
        {
            this._prefab = prefab;
            _queue = new Queue<GameObject>();
            _resetAction = DefaultReset;
            initAction = DefaultInit;
        }

        public GameObjectPool(GameObject prefab, Action<GameObject>
            ResetAction = null, Action<GameObject> InitAction = null, Action<GameObjectPool, GameObject> FirstInitAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<GameObject>();
            _resetAction = ResetAction;
            initAction = InitAction;
            _firstInitAction = FirstInitAction;
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
                initAction?.Invoke(t);
                return t;
            }
            else
            {
                GameObject t = GameObject.Instantiate(_prefab);
                _firstInitAction?.Invoke(this, t);
                initAction?.Invoke(t);

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
        private GameObject _prefab; //预制体
        private Queue<GameObject> _queue; //待使用的对象
        private List<GameObject> _actives; //使用中的对象
        private Action<GameObject> _resetAction; //返回池中后调用
        private Action<GameObject> _initAction; //取出时调用
        private Action<GameObjectPool_MK2, GameObject> _firstInitAction; //首次生成时调用

        public GameObjectPool_MK2(GameObject prefab,
            Action<GameObject> ResetAction = null,
            Action<GameObject> InitAction = null,
            Action<GameObjectPool_MK2, GameObject> FirstInitAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<GameObject>();
            _actives = new List<GameObject>();
            _resetAction = ResetAction;
            _initAction = InitAction;
            _firstInitAction = FirstInitAction;
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
                _firstInitAction?.Invoke(this, t);
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

    public class ComponentPool<C> where C : Component
    {
        private C _prefab; //预制体
        private Queue<C> _queue; //待使用的对象
        private Action<C> _resetAction; //返回池中后调用
        private Action<C> _initAction; //取出时调用
        private Action<ComponentPool<C>, C> _firstInitAction; //首次生成时调用

        public ComponentPool(C prefab)
        {
            this._prefab = prefab;
            _queue = new Queue<C>();
            _resetAction = DefaultReset;
            _initAction = DefaultInit;
        }

        public ComponentPool(C prefab, Action<C>
            ResetAction = null, Action<C> InitAction = null, Action<ComponentPool<C>, C> FirstInitAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<C>();
            _resetAction = ResetAction;
            _initAction = InitAction;
            _firstInitAction = FirstInitAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public C New()
        {
            if (_queue.Count > 0)
            {
                C t = _queue.Dequeue();
                _initAction?.Invoke(t);
                return t;
            }
            else
            {
                C t = Object.Instantiate(_prefab);
                _firstInitAction?.Invoke(this, t);
                _initAction?.Invoke(t);

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(C obj)
        {
            if (_queue.Contains(obj) == false)
            {
                _resetAction?.Invoke(obj);
                _queue.Enqueue(obj);
            }
        }

        public static void DefaultReset(C go)
        {
            go.gameObject.SetActive(false);
        }

        public static void DefaultInit(C go)
        {
            go.gameObject.SetActive(true);
        }
    }


    public class ComponentPool_MK2<C> where C : Component
    {
        private C _prefab; //预制体
        private Queue<C> _queue; //待使用的对象
        private List<C> _actives; //使用中的对象
        private Action<C> _resetAction; //返回池中后调用
        private Action<C> _initAction; //取出时调用
        private Action<ComponentPool_MK2<C>, C> _firstInitAction; //首次生成时调用

        public ComponentPool_MK2(C prefab,
            Action<C> ResetAction = null,
            Action<C> InitAction = null,
            Action<ComponentPool_MK2<C>, C> FirstInitAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<C>();
            _actives = new List<C>();
            _resetAction = ResetAction;
            _initAction = InitAction;
            _firstInitAction = FirstInitAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public C New()
        {
            C newC;
            if (_queue.Count > 0)
            {
                newC = _queue.Dequeue();
            }
            else
            {
                newC = Object.Instantiate(_prefab);
                _firstInitAction?.Invoke(this, newC);
            }

            _initAction?.Invoke(newC);
            _actives.Add(newC);
            return newC;
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(C obj)
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

    [Serializable]
    public class SerializableGameobjectPool
    {
        [SerializeField] private GameObject m_prefab; //预制体
        [SerializeField] private Transform m_birthPoint; //出生点
        [SerializeField] private List<GameObject> m_using=new List<GameObject>(); //使用中对象
        [SerializeField] private List<GameObject> m_storage=new List<GameObject>(); //存储对象

        public GameObject Prefab => m_prefab;

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