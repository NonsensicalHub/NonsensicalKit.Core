using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    /// <summary>
    /// 添加缓冲-使用-回收逻辑
    /// </summary>
    public class GameObjectPoolMk3
    {
        private readonly GameObject _prefab; //预制体
        private readonly Queue<GameObject> _queue; //待使用的对象
        private readonly List<GameObject> _actives; //使用中的对象
        private readonly Action<GameObject> _resetAction; //返回池中后调用
        private readonly Action<GameObject> _initAction; //取出时调用
        private readonly Action<GameObject> _reinitAction; //不返回池中直接重新使用时调用
        private readonly Action<GameObjectPoolMk3, GameObject> _createAction; //首次生成时调用

        private List<GameObject> _cache;
        private int _catchIndex;

        public GameObjectPoolMk3(GameObject prefab,
            Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null,
            Action<GameObject> reinitAction = null,
            Action<GameObjectPoolMk3, GameObject> createAction = null)
        {
            this._prefab = prefab;
            _queue = new Queue<GameObject>();
            _actives = new List<GameObject>();
            _resetAction = resetAction;
            _initAction = initAction;
            _reinitAction= reinitAction;
            _createAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            GameObject newC;
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
        public void Cache(GameObject obj)
        {
            if (_cache==null)
            {
                _cache=new List<GameObject>();
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
}
