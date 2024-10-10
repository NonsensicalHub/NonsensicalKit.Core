using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现三维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array3<T>
    {
        public readonly T[] Array;

        public readonly int Length0;
        public readonly int Length1;
        public readonly int Length2;

        public readonly int Step0;
        public readonly int Step1;

        public Array3(int length0, int length1, int length2)
        {
            Array = new T[length0 * length1 * length2];

            Length0 = length0;
            Length1 = length1;
            Length2 = length2;

            Step0 = length1 * length2;
            Step1 = length2;
        }

        public void Reset(T state)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = state;
            }
        }

        public T this[int index0, int index1, int index2]
        {
            get
            {
                return Array[index0 * Step0 + index1 * Step1 + index2];
            }
            set
            {
                Array[index0 * Step0 + index1 * Step1 + index2] = value;

            }
        }

        public T this[Int3 int3]
        {
            get
            {
                return Array[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3];
            }
            set
            {
                Array[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3] = value;

            }
        }

        public T this[int index]
        {
            get
            {
                return Array[index];
            }
            set
            {
                Array[index] = value;
            }
        }

        public Tuple<int, int, int> GetIndexTuple(int index)
        {
            var x = index / Step0;
            var y = (index - x * Step0) / Step1;
            var z = index - x * Step0 - y * Step1;
            return Tuple.Create(x, y, z);
        }

        public int GetIndex(int index0, int index1, int index2)
        {
            return index0 * Step0 + index1 * Step1 + index2;
        }
    }
}
