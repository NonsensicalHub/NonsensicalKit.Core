using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class GameObjectPoolMk3 : GameObjectPoolMk2
    {
        public Action<GameObject> ReinitAction { private get; set; } //不返回池中直接重新使用时调用

        private List<GameObject> _cache;
        private int _catchIndex;

        public GameObjectPoolMk3(GameObject prefab, Action<GameObjectPool, GameObject> createAction) : base(
            prefab, createAction)
        {
        }

        public GameObjectPoolMk3(GameObject prefab, Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null, Action<GameObject> reinitAction = null,
            Action<GameObjectPool, GameObject> createAction = null) : base(prefab, resetAction, initAction,
            createAction)
        {
            ReinitAction = reinitAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public override GameObject New()
        {
            GameObject newC;
            if ((_cache != null) && (_catchIndex < _cache.Count))
            {
                newC = _cache[_catchIndex];
                _catchIndex++;
                ReinitAction?.Invoke(newC);
            }
            else if (Queue.Count > 0)
            {
                newC = Queue.Dequeue();
                InitAction?.Invoke(newC);
            }
            else
            {
                newC = Object.Instantiate(Prefab);
                CreateAction?.Invoke(this, newC);
                InitAction?.Invoke(newC);
            }

            Actives.Add(newC);

            return newC;
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

            _cache = Actives.Clone();
            Actives.Clear();
        }

        /// <summary>
        /// 和Flush配套使用，在不需要全部回收，但希望避免重复回收-使用造成不必要消耗时使用
        /// </summary>
        /// <param name="obj"></param>
        public void Cache(GameObject obj)
        {
            if (_cache == null)
            {
                _cache = new List<GameObject>();
            }

            _cache.Add(obj);
            Actives.Remove(obj);
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
                ResetAction?.Invoke(_cache[_catchIndex]);
                Queue.Enqueue(_cache[_catchIndex]);
            }

            _catchIndex = 0;
            _cache.Clear();
            _cache = null;
        }

        public override void Clear()
        {
            if (_cache != null)
            {
                Flush();
            }

            base.Clear();
        }
    }
}
