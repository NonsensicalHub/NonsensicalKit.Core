using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
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
}
