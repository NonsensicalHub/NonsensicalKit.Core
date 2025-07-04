using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Queue<T> _objectQueue;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _onetimeInitAction;

        public ObjectPool(int initialBufferSize, Action<T>
            resetAction = null, Action<T> onetimeInitAction = null)
        {
            _objectQueue = new Queue<T>();
            _resetAction = resetAction;
            _onetimeInitAction = onetimeInitAction;
            for (int i = 0; i < initialBufferSize; i++)
            {
                Store(New());
            }
        }

        public T New()
        {
            if (_objectQueue.Count > 0)
            {
                T t = _objectQueue.Dequeue();

                return t;
            }
            else
            {
                T t = new T();
                _onetimeInitAction?.Invoke(t);

                return t;
            }
        }

        public void Store(T obj)
        {
            _resetAction?.Invoke(obj);
            _objectQueue.Enqueue(obj);
        }

        public void Clear()
        {
            _objectQueue.Clear();
        }
    }
}
