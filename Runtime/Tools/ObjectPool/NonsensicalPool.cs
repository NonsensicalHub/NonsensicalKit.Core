using System;
using System.Collections.Generic;

namespace NonsensicalKit.Tools.ObjectPool
{
    public class NonsensicalPool<Obj> where Obj : IPoolObject
    {
        private Queue<Obj> _queue;    //待使用的对象
        private Func<Obj> _getNewObj;    //获取新对象的方法

        public NonsensicalPool(Func<Obj> getNewObj)
        {
            this._getNewObj = getNewObj;

            _queue = new Queue<Obj>();
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public Obj New()
        {
            if (_queue.Count > 0)
            {
                Obj t = _queue.Dequeue();
                t.Out();
                return t;
            }
            else
            {
                Obj t = _getNewObj();
                t.Init();
                t.Out();

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(Obj obj)
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
        public abstract void Init();
        /// <summary>
        /// 出池
        /// </summary>
        public abstract void Out();
        /// <summary>
        /// 入池
        /// </summary>
        public abstract void In();
    }
}
