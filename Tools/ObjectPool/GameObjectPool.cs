using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class GameObjectPool
    {
        protected readonly GameObject Prefab; //预制体
        protected readonly Queue<GameObject> Queue = new(); //待使用的对象
        public Action<GameObject> ResetAction { protected get; set; } //返回池中后调用
        public Action<GameObject> InitAction { protected get; set; } //取出时调用
        public Action<GameObjectPool, GameObject> CreateAction { protected get; set; } //首次生成时调用

        public GameObjectPool(GameObject prefab, Action<GameObjectPool, GameObject> createAction)
        {
            Prefab = prefab;
            ResetAction = DefaultReset;
            InitAction = DefaultInit;
            CreateAction = createAction;
        }

        public GameObjectPool(GameObject prefab, Action<GameObject> resetAction = null,
            Action<GameObject> initAction = null,
            Action<GameObjectPool, GameObject> createAction = null)
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
        public virtual GameObject New()
        {
            GameObject newComponent;
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
        public virtual void Store(GameObject obj)
        {
            if (Queue.Contains(obj) == false)
            {
                ResetAction?.Invoke(obj);
                Queue.Enqueue(obj);

                OnStore(obj);
            }
        }

        protected virtual void OnNew(GameObject obj)
        {
        }

        protected virtual void OnStore(GameObject obj)
        {
        }

        private static void DefaultReset(GameObject go)
        {
            go.SetActive(false);
        }

        private static void DefaultInit(GameObject go)
        {
            go.SetActive(true);
        }
    }
}
