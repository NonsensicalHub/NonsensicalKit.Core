using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    [Obsolete("Use ComponentPoolMk2<TComponent> instead.")]
    public class ComponentPool_MK2<TComponent> : ComponentPoolMk2<TComponent> where TComponent : Component
    {
        public ComponentPool_MK2(TComponent prefab, Action<TComponent> resetAction = null, Action<TComponent> initAction = null,
            Action<ComponentPoolMk2<TComponent>, TComponent> createAction = null) : base(prefab, resetAction, initAction, createAction)
        {
        }
    }

    public class ComponentPoolMk2<TComponent> where TComponent : Component
    {
        private readonly TComponent _prefab; //预制体
        private readonly Queue<TComponent> _queue; //待使用的对象
        private readonly List<TComponent> _actives; //使用中的对象
        private readonly Action<TComponent> _resetAction; //返回池中后调用
        private readonly Action<TComponent> _initAction; //取出时调用
        private readonly Action<ComponentPoolMk2<TComponent>, TComponent> _createAction; //首次生成时调用

        public ComponentPoolMk2(TComponent prefab,
            Action<TComponent> resetAction = null,
            Action<TComponent> initAction = null,
            Action<ComponentPoolMk2<TComponent>, TComponent> createAction = null)
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
}
