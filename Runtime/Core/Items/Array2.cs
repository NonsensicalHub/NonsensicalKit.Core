using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现二维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array2<T>
    {
        public readonly T[] TArray;

        public readonly int Length0;
        public readonly int Length1;

        public readonly int Step0;

        public Array2(int length0, int length1)
        {
            TArray = new T[length0 * length1];
            Length0 = length0;
            Length1 = length1;

            Step0 = length1;
        }

        public void Reset()
        {
            Reset(default);
        }

        public void Reset(T state)
        {
            for (int i = 0; i < TArray.Length; i++)
            {
                TArray[i] = state;
            }
        }

        public T this[int index0, int index1]
        {
            get { return TArray[index0 * Step0 + index1]; }
            set { TArray[index0 * Step0 + index1] = value; }
        }

        public T this[int index]
        {
            get { return TArray[index]; }
            set { TArray[index] = value; }
        }

        public Tuple<int, int> GetIndexTuple(int index)
        {
            var x = index / Step0;
            var y = index - x * Step0;
            return Tuple.Create(x, y);
        }

        public int GetIndex(int index0, int index1)
        {
            return index0 * Step0 + index1;
        }
    }
}