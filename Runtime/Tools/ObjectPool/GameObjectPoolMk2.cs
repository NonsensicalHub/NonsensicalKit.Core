using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    [Obsolete("Use GameObjectPoolMk2 instead.")]
    public class GameObjectPool_MK2 : GameObjectPoolMk2
    {
        public GameObjectPool_MK2(GameObject prefab, Action<GameObject> resetAction = null, Action<GameObject> initAction = null,
            Action<GameObjectPoolMk2, GameObject> createAction = null) : base(prefab, resetAction, initAction, createAction)
        {
        }
    }

    /// <summary>
    /// 除了缓存对象外，还管理了使用中对象
    /// </summary>
    public class GameObjectPoolMk2
    {
        private readonly GameObject _prefab; //预制体
        private readonly Queue<GameObject> _queue; //待使用的对象
        private readonly List<GameObject> _actives; //使用中的对象
        private readonly Action<GameObject> _resetAction; //返回池中后调用
        private readonly Action<GameObject> _initAction; //取出时调用
        private readonly Action<GameObjectPoolMk2, GameObject> _createAction; //首次生成时调用

        public GameObjectPoolMk2(GameObject prefab,
            Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null,
            Action<GameObjectPoolMk2, GameObject> createAction = null)
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
}
