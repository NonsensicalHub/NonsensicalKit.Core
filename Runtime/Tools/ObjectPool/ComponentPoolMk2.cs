using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.ObjectPool
{
    /// <summary>
    /// 管理激活中的对象
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public class ComponentPoolMk2<TComponent> : ComponentPool<TComponent> where TComponent : Component
    {
        public List<TComponent> Actives { get; } = new(); //使用中的对象

        public ComponentPoolMk2(TComponent prefab, Action<ComponentPool<TComponent>, TComponent> createAction) : base(
            prefab, createAction)
        {
        }

        public ComponentPoolMk2(TComponent prefab, Action<TComponent> resetAction = null,
            Action<TComponent> initAction = null,
            Action<ComponentPool<TComponent>, TComponent> createAction = null) : base(prefab, resetAction,
            initAction,
            createAction)
        {
        }

        public virtual void Clear()
        {
            foreach (var item in Actives)
            {
                ResetAction?.Invoke(item);
                Queue.Enqueue(item);
            }

            Actives.Clear();
        }

        protected override void OnNew(TComponent obj)
        {
            base.OnNew(obj);

            Actives.Add(obj);
        }

        protected override void OnStore(TComponent obj)
        {
            base.OnStore(obj);
            if (Actives.Contains(obj))
            {
                Actives.Remove(obj);
            }
        }
    }
}
