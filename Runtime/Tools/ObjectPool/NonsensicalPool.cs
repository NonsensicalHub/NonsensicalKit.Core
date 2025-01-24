using System;
using System.Collections.Generic;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class NonsensicalPool<TObj> where TObj : IPoolObject
    {
        private readonly Queue<TObj> _queue; //待使用的对象
        private readonly Func<TObj> _getNewObj; //获取新对象的方法

        public NonsensicalPool(Func<TObj> getNewObj)
        {
            _getNewObj = getNewObj;

            _queue = new Queue<TObj>();
        }

        public NonsensicalPool(Queue<TObj> queue) { _queue = queue; }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public TObj New()
        {
            if (_queue.Count > 0)
            {
                TObj t = _queue.Dequeue();
                t.Out();
                return t;
            }
            else
            {
                TObj t = _getNewObj();
                t.Init();
                t.Out();

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(TObj obj)
        {
            if (_queue.Contains(obj) == false)
            {
                obj.In();
                _queue.Enqueue(obj);
            }
        }
    }

    public interface IPoolObject
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init();

        /// <summary>
        /// 出池
        /// </summary>
        public void Out();

        /// <summary>
        /// 入池
        /// </summary>
        public void In();
    }
}
