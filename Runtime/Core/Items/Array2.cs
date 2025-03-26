using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现二维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct Array2<T>
    {
        public T[] m_Array;

        public readonly int Length0;
        public readonly int Length1;

        public readonly int Step0;

        public Array2(int length0, int length1)
        {
            m_Array = new T[length0 * length1];
            Length0 = length0;
            Length1 = length1;

            Step0 = length1;
        }

        public void Reset(T state = default)
        {
            if (m_Array == null) return;
            for (int i = 0; i < m_Array.Length; i++)
            {
                m_Array[i] = state;
            }
        }

        public Array2<T> CopyToNewArray(int length0, int length1)
        {
            var newArray = new Array2<T>(length0, length1);

            int min0 = length0 < Length0 ? length0 : Length0;
            int min1 = length1 < Length1 ? length1 : Length1;

            for (int i = 0; i < min0; i++)
            {
                for (int j = 0; j < min1; j++)
                {
                    newArray[i, j] = this[i, j];
                }
            }

            return newArray;
        }

        public T this[int index0, int index1]
        {
            get => m_Array[index0 * Step0 + index1];
            set => m_Array[index0 * Step0 + index1] = value;
        }

        public T this[int index]
        {
            get => m_Array[index];
            set => m_Array[index] = value;
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
