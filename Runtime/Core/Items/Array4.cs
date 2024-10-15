using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现四维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array4<T>
    {
        public readonly T[] TArray;

        public readonly int Length0;
        public readonly int Length1;
        public readonly int Length2;
        public readonly int Length3;

        public readonly int Step0;
        public readonly int Step1;
        public readonly int Step2;

        public Array4(int length0, int length1, int length2, int length3)
        {
            TArray = new T[length0 * length1 * length2 * length3];

            Length0 = length0;
            Length1 = length1;
            Length2 = length2;
            Length3 = length3;

            Step0 = length1 * length2 * length3;
            Step1 = length2 * length3;
            Step2 = length3;
        }

        public T this[int index0, int index1, int index2, int index3]
        {
            get
            {
                return TArray[index0 * Step0 + index1 * Step1 + index2 * Step2 + index3];
            }
            set
            {
                TArray[index0 * Step0 + index1 * Step1 + index2 * Step2 + index3] = value;
            }
        }

        public T this[Int3 int3, int index3]
        {
            get
            {
                return TArray[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3 * Step2 + index3];
            }
            set
            {
                TArray[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3 * Step2 + index3] = value;
            }
        }

        public T this[int index]
        {
            get
            {
                return TArray[index];
            }
            set
            {
                TArray[index] = value;
            }
        }

        public Tuple<int, int, int, int> GetIndexTuple(int index)
        {
            var x = index / Step0;
            var y = (index - x * Step0) / Step1;
            var z = (index - x * Step0 - y * Step1) / Step2;
            var w = index - x * Step0 - y * Step1 - z * Step2;
            return Tuple.Create(x, y, z, w);
        }

        public int GetIndex(int index0, int index1, int index2, int index3)
        {
            return index0 * Step0 + index1 * Step1 + index2 * Step2 + index3;
        }
    }
}
