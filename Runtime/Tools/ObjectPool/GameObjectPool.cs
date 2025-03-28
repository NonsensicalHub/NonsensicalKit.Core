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
}
