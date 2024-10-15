using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.ObjectPool
{
    /// <summary>
    /// GameObject对象池
    /// </summary>
    public class GameObjectPool
    {
        public Action<GameObjectPool, GameObject> FirstInitAction { set { _firstInitAction = value; } }

        private GameObject _prefab;  //预制体
        private Queue<GameObject> _queue;    //待使用的对象
        private Action<GameObject> _resetAction; //返回池中后调用
        private Action<GameObject> initAction;  //取出时调用
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

    public class GameObjectPool_MK2
    {
        private GameObject _prefab;  //预制体
        private Queue<GameObject> _queue;    //待使用的对象
        private List<GameObject> _actives;    //使用中的对象
        private Action<GameObject> _resetAction; //返回池中后调用
        private Action<GameObject> _initAction;  //取出时调用
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
                GameObject t = GameObject.Instantiate(_prefab);
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
        private C _prefab;  //预制体
        private Queue<C> _queue;    //待使用的对象
        private Action<C> _resetAction; //返回池中后调用
        private Action<C> _initAction;  //取出时调用
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
                C t = GameObject.Instantiate(_prefab);
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
        private C _prefab;  //预制体
        private Queue<C> _queue;    //待使用的对象
        private List<C> _actives;    //使用中的对象
        private Action<C> _resetAction; //返回池中后调用
        private Action<C> _initAction;  //取出时调用
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
                newC = GameObject.Instantiate(_prefab);
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

    [System.Serializable]
    public class SerializableGameobjectPool
    {
        [SerializeField] private GameObject m_prefab;  //预制体
        [SerializeField] private Transform m_pool;
        [SerializeField] private List<GameObject> m_using;  //使用中对象
        [SerializeField] private List<GameObject> m_cache;  //缓存对象

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            GameObject newGo;
            if (m_cache.Count > 0)
            {
                newGo = m_cache[0];
                m_cache.RemoveAt(0);
            }
            else
            {
                newGo = GameObject.Instantiate(m_prefab, m_pool);
            }
            newGo.SetActive(true);
            m_using.Add(newGo);
            return newGo;
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="go"></param>
        public void Store(GameObject go)
        {
            if (m_using.Contains(go) && (m_cache.Contains(go) == false))
            {
                m_using.Remove(go);
                m_cache.Add(go);
                go.gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            foreach (var item in m_using)
            {
                item.SetActive(false);
                m_cache.Add(item);
            }
            m_using.Clear();
        }

        public void Clean()
        {
            foreach (var item in m_using)
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
            foreach (var item in m_cache)
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
            m_using.Clear();
            m_cache.Clear();
        }
    }
}
