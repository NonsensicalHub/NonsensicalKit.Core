using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现三维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct Array3<T>
    {
        public T[] m_Array;

        public readonly int Length0;
        public readonly int Length1;
        public readonly int Length2;

        public readonly int Step0;
        public readonly int Step1;

        public Array3(int length0, int length1, int length2)
        {
            m_Array = new T[length0 * length1 * length2];

            Length0 = length0;
            Length1 = length1;
            Length2 = length2;

            Step0 = length1 * length2;
            Step1 = length2;
        }
    
        public Array3<T> CopyToNewArray(int length0, int length1, int length2)
        {
            Array3<T> newArray3 = new Array3<T>(length0, length1, length2);
            int minL0 = Math.Min(Length0, length0);
            int minL1 = Math.Min(Length1, length1);
            int minL2 = Math.Min(Length2, length2);
            for (int i = 0; i < minL0; i++)
            {
                for (int j = 0; j < minL1; j++)
                {
                    for (int k = 0; k < minL2; k++)
                    {
                        newArray3[i, j, k] = this[i, j, k];
                    }
                }
            }

            return newArray3;
        }

        public void Reset(T state = default)
        {
            for (int i = 0; i < m_Array.Length; i++)
            {
                m_Array[i] = state;
            }
        }

        public T this[int index0, int index1, int index2]
        {
            get => m_Array[index0 * Step0 + index1 * Step1 + index2];
            set => m_Array[index0 * Step0 + index1 * Step1 + index2] = value;
        }

        public T this[Int3 int3]
        {
            get => m_Array[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3];
            set => m_Array[int3.I1 * Step0 + int3.I2 * Step1 + int3.I3] = value;
        }

        public T this[int index]
        {
            get => m_Array[index];
            set => m_Array[index] = value;
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

        public void SafeSet(int index0, int index1, int index2, T value)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0)
            {
                return;
            }

            var index = index0 * Step0 + index1 * Step1 + index2;
            if (index < m_Array.Length)
            {
                m_Array[index] = value;
            }
        }

        public T SafeGet(int index0, int index1, int index2)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0)
            {
                return default;
            }

            var index = index0 * Step0 + index1 * Step1 + index2;
            return index >= m_Array.Length ? default : m_Array[index];
        }
    }
}
