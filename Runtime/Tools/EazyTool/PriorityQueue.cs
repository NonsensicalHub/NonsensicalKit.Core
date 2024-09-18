using NonsensicalKit.Tools;
using System;
using System.Text;

namespace NonsensicalKit.Tools.EazyTool
{
    /// <summary>
    /// 可自定义判断方法的优先队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        public delegate bool CompareEventHandler(T t1, T t2);

        public event CompareEventHandler Compare;

        //优先队列中元素个数，同时是队尾索引
        private int _number;
        private T[] _queue;
        private int _size;

        /// <summary>
        /// 当前容量
        /// </summary>
        public int Size { get { return _size; } }
        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        public bool IsEmpty { get { return _number == 0; } }
        /// <summary>
        /// 返回队列元素个数
        /// </summary>
        public int Count { get { return _number; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compare">堆顶的条件</param>
        /// <param name="size">初始尺寸</param>
        public PriorityQueue(CompareEventHandler compare, int size = 55)
        {
            Compare = compare;
            this._size = size;
            _queue = new T[size + 1];
            /* 主体从1开始索引
             * 从0开始索引时，子节点为2*n+1和2*n+2，父节点为(n-1)/2
             * 从1开始索引时，子节点为2*n和2*n+1，父节点为n/2
             * 从1开始索引可以使每次找父子节点操作少一个加减操作
             */
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="val"></param>
        public void Push(T val)
        {
            if (_number + 1 > _size)
            {   //容量不够时翻倍
                T[] newqueue = new T[_size * 2 + 1];
                _queue.CopyTo(newqueue, 0);
                _queue = newqueue;
                _size *= 2;
            }
            _queue[++_number] = val;
            //将队尾元素上浮到合适位置
            Swim(_number);
        }

        /// <summary>
        /// 取出最大元素
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            //队首元素就是最大值
            T max = _queue[1];
            //将队尾元素放入队首
            Swap(1, _number);
            //删除队尾元素
            _number--;
            //恢复堆的有序性
            Sink(1);
            return max;
        }

        /// <summary>
        /// 获取数组对象
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            T[] values = new T[_number];
            Array.Copy(_queue, 1, values, 0, _number);

            return values;
        }

        /// <summary>
        /// 获取排序好的数组
        /// </summary>
        /// <returns></returns>
        public T[] ToSortedArray()
        {
            T[] values = new T[_number];
            Array.Copy(_queue, 1, values, 0, _number);

            for (int i = _number - 1; i > 0; i--)
            {
                MathTool.Swap(values, 0, i);

                int index = 0;
                while (2 * index + 1 < i)
                {
                    int child = 2 * index + 1;

                    if (child + 1 < i)
                    {
                        if (Compare(values[index], values[child]) && Compare(values[index], values[child + 1]))
                            break;
                        if (Compare(values[child + 1], values[child])) child++;
                        MathTool.Swap(values, index, child);
                        index = child;
                    }
                    else
                    {
                        if (Compare(values[index], values[child]))
                            break;
                        MathTool.Swap(values, index, child);
                        index = child;
                    }
                }
            }
            return values;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= _number; i++)
            {
                sb.Append(_queue[i]).Append(" ");
            }
            return sb.ToString();
        }

        //下沉操作
        private void Sink(int k)
        {
            //左子节点是否存在
            while (2 * k <= _number)
            {
                int child = 2 * k;

                //判断是否需要下沉
                if (CompareValue(k, child) && CompareValue(k, child + 1))
                    break;
                //找出子节点的较大值
                if (CompareValue(child + 1, child)) child++;
                //交换位置
                Swap(k, child);
                k = child;
            }
        }

        //进行判断
        private bool CompareValue(int i1, int i2)
        {
            if (i1 > _number)
            {
                return false;
            }
            else if (i2 > _number)
            {
                return true;
            }
            else
            {
                return Compare(_queue[i1], _queue[i2]);
            }
        }

        //上浮操作
        private void Swim(int k)
        {
            //当前节点大于父节点，则交换，直至堆恢复有序
            while (k > 1 && CompareValue(k, k / 2))
            {

                Swap(k, k / 2);
                k = k / 2;
            }
        }

        //交换元素
        private void Swap(int i1, int i2)
        {
            T tmp = _queue[i1];
            _queue[i1] = _queue[i2];
            _queue[i2] = tmp;
        }
    }

    /// <summary>
    /// 大顶堆优先队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MaxHeapPriorityQueue<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// 当前容量
        /// </summary>
        public int Size { get { return _size; } }
        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        public bool isEmpty { get { return _count == 0; } }
        /// <summary>
        /// 返回队列元素个数
        /// </summary>
        public int Count { get { return _count; } }

        private int _count;
        private T[] _queue;
        private int _size;


        public MaxHeapPriorityQueue(int size = 50)
        {
            this._size = size;
            _queue = new T[size + 1];
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="val"></param>
        public void Push(T val)
        {
            if (_count + 1 > _size)
            {
                T[] newqueue = new T[_size * 2 + 1];
                _queue.CopyTo(newqueue, 0);
                _queue = newqueue;
                _size *= 2;
            }
            _queue[++_count] = val;
            Swim(_count);
        }

        /// <summary>
        /// 取出最大元素
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            T max = _queue[1];
            Swap(1, _count);
            _count--;
            Sink(1);
            return max;
        }

        public T[] ToArray()
        {
            T[] values = new T[_count];
            Array.Copy(_queue, 1, values, 0, _count);

            return values;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= _count; i++)
            {
                sb.Append(_queue[i]).Append(" ");
            }
            return sb.ToString();
        }

        private void Sink(int k)
        {
            while (2 * k <= _count)
            {
                int child = 2 * k;

                if (Less(child, k) && Less(child + 1, k))
                    break;
                if (Less(child, child + 1)) child++;
                Swap(k, child);
                k = child;
            }
        }

        private bool Less(int i1, int i2)
        {
            if (i1 > _count)
            {
                return true;
            }
            else if (i2 > _count)
            {
                return false;
            }
            else
            {
                return _queue[i1].CompareTo(_queue[i2]) < 0;
            }
        }

        private void Swim(int k)
        {
            while (k > 1 && Less(k / 2, k))
            {

                Swap(k, k / 2);
                k = k / 2;
            }
        }

        private void Swap(int i1, int i2)
        {
            T tmp = _queue[i1];
            _queue[i1] = _queue[i2];
            _queue[i2] = tmp;
        }
    }

    /// <summary>
    /// 小顶堆优先队列
    /// 和大顶堆基本一致，仅在判断大小时相反
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeapPriorityQueue<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// 当前容量
        /// </summary>
        public int Size { get { return _size; } }
        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        public bool isEmpty { get { return _count == 0; } }
        /// <summary>
        /// 返回队列元素个数
        /// </summary>
        public int Count { get { return _count; } }

        private int _count;
        private T[] _queue;
        private int _size;

        public HeapPriorityQueue(int size = 50)
        {
            this._size = size;
            _queue = new T[size + 1];
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="val"></param>
        public void Push(T val)
        {
            if (_count + 1 > _size)
            {
                T[] newqueue = new T[_size * 2 + 1];
                _queue.CopyTo(newqueue, 0);
                _queue = newqueue;
                _size *= 2;
            }
            _queue[++_count] = val;
            Swim(_count);
        }

        /// <summary>
        /// 取出最小元素
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            T max = _queue[1];
            Swap(1, _count);
            _count--;
            Sink(1);
            return max;
        }

        public T[] ToArray()
        {
            T[] values = new T[_count];
            Array.Copy(_queue, 1, values, 0, _count);

            return values;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= _count; i++)
            {
                sb.Append(_queue[i]).Append(" ");
            }
            return sb.ToString();
        }

        private void Sink(int k)
        {
            while (2 * k <= _count)
            {
                int child = 2 * k;

                if (Greater(child, k) && Greater(child + 1, k))
                    break;
                if (Greater(child, child + 1)) child++;
                Swap(k, child);
                k = child;
            }
        }


        private bool Greater(int i1, int i2)
        {
            if (i1 > _count)
            {
                return true;
            }
            else if (i2 > _count)
            {
                return false;
            }
            else
            {
                return _queue[i1].CompareTo(_queue[i2]) > 0;
            }
        }

        private void Swim(int k)
        {
            while (k > 1 && Greater(k / 2, k))
            {
                Swap(k, k / 2);
                k = k / 2;
            }
        }

        private void Swap(int i1, int i2)
        {
            T tmp = _queue[i1];
            _queue[i1] = _queue[i2];
            _queue[i2] = tmp;
        }
    }
}
