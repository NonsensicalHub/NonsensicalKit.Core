using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class ComponentPool<TComponent> where TComponent : Component
    {
        protected readonly TComponent Prefab; //预制体
        protected readonly Queue<TComponent> Queue = new(); //待使用的对象
        public Action<TComponent> ResetAction { protected get; set; } //返回池中后调用
        public Action<TComponent> InitAction { protected get; set; } //取出时调用
        public Action<ComponentPool<TComponent>, TComponent> CreateAction { protected get; set; } //首次生成时调用

        public ComponentPool(TComponent prefab, Action<ComponentPool<TComponent>, TComponent> createAction)
        {
            Prefab = prefab;
            ResetAction = DefaultReset;
            InitAction = DefaultInit;
            CreateAction = createAction;
        }

        public ComponentPool(TComponent prefab, Action<TComponent> resetAction = null,
            Action<TComponent> initAction = null,
            Action<ComponentPool<TComponent>, TComponent> createAction = null)
        {
            Prefab = prefab;
            ResetAction = resetAction;
            InitAction = initAction;
            CreateAction = createAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public virtual TComponent New()
        {
            TComponent newComponent;
            if (Queue.Count > 0)
            {
                newComponent = Queue.Dequeue();
            }
            else
            {
                newComponent = Object.Instantiate(Prefab);
                CreateAction?.Invoke(this, newComponent);
            }

            InitAction?.Invoke(newComponent);
            OnNew(newComponent);
            return newComponent;
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Store(TComponent obj)
        {
            if (Queue.Contains(obj) == false)
            {
                ResetAction?.Invoke(obj);
                Queue.Enqueue(obj);

                OnStore(obj);
            }
        }

        protected virtual void OnNew(TComponent obj)
        {
        }

        protected virtual void OnStore(TComponent obj)
        {
        }

        private static void DefaultReset(TComponent go)
        {
            go.gameObject.SetActive(false);
        }

        private static void DefaultInit(TComponent go)
        {
            go.gameObject.SetActive(true);
        }
    }
}
