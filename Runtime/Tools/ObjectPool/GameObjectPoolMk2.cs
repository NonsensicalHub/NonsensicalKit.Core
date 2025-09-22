using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class GameObjectPoolMk2 : GameObjectPool
    {
        public List<GameObject> Actives { get; } = new(); //使用中的对象

        public GameObjectPoolMk2(GameObject prefab, Action<GameObjectPool, GameObject> createAction) : base(
            prefab, createAction)
        {
        }

        public GameObjectPoolMk2(GameObject prefab, Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null,
            Action<GameObjectPool, GameObject> createAction = null) : base(prefab, resetAction,
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

        protected override void OnNew(GameObject obj)
        {
            base.OnNew(obj);

            Actives.Add(obj);
        }

        protected override void OnStore(GameObject obj)
        {
            base.OnStore(obj);
            if (Actives.Contains(obj))
            {
                Actives.Remove(obj);
            }
        }
    }
}
