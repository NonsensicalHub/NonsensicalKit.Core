using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现可序列化三维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct Array3<T>
    {
        public T[] m_Array;

        [FormerlySerializedAs("Length0")] public int m_Length0;
        [FormerlySerializedAs("Length1")] public int m_Length1;
        [FormerlySerializedAs("Length2")] public int m_Length2;

        [FormerlySerializedAs("Step0")] public int m_Step0;
        [FormerlySerializedAs("Step1")] public int m_Step1;

        public int Length0 => m_Length0;
        public int Length1 => m_Length1;
        public int Length2 => m_Length2;
        
        public Array3(int length0, int length1, int length2)
        {
            m_Array = new T[length0 * length1 * length2];

            m_Length0 = length0;
            m_Length1 = length1;
            m_Length2 = length2;

            m_Step0 = length1 * length2;
            m_Step1 = length2;
        }

        public Array3<T> CopyToNewArray(int length0, int length1, int length2)
        {
            Array3<T> newArray3 = new Array3<T>(length0, length1, length2);
            int minL0 = Math.Min(m_Length0, length0);
            int minL1 = Math.Min(m_Length1, length1);
            int minL2 = Math.Min(m_Length2, length2);
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

        public void Fill(T state = default)
        {
            for (int i = 0; i < m_Array.Length; i++)
            {
                m_Array[i] = state;
            }
        }


        public void Reset()
        {
            if (m_Array == null) return;
            var type = typeof(T);
            var hasEmptyConstr = type.GetConstructor(Type.EmptyTypes) != null;
            var isMonoBehaviour = type.IsSubclassOf(typeof(MonoBehaviour));
            if (hasEmptyConstr && !isMonoBehaviour)
            {
                for (int i = 0; i < m_Array.Length; i++)
                {
                    m_Array[i] = (T)Activator.CreateInstance(type);
                }
            }
            else
            {
                for (int i = 0; i < m_Array.Length; i++)
                {
                    m_Array[i] = default;
                }
            }
        }


        public T this[int index0, int index1, int index2]
        {
            get => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2];
            set => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2] = value;
        }

        public T this[Int3 int3]
        {
            get => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3];
            set => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3] = value;
        }

        public T this[int index]
        {
            get => m_Array[index];
            set => m_Array[index] = value;
        }

        public Tuple<int, int, int> GetIndexTuple(int index)
        {
            var x = index / m_Step0;
            var y = (index - x * m_Step0) / m_Step1;
            var z = index - x * m_Step0 - y * m_Step1;
            return Tuple.Create(x, y, z);
        }

        public int GetIndex(int index0, int index1, int index2)
        {
            return index0 * m_Step0 + index1 * m_Step1 + index2;
        }

        public void SafeSet(int index0, int index1, int index2, T value)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0)
            {
                return;
            }

            var index = index0 * m_Step0 + index1 * m_Step1 + index2;
            if (index < m_Array.Length)
            {
                m_Array[index] = value;
            }
        }

        public T SafeGet(Int3 int3)
        {
            return SafeGet(int3.X, int3.Y, int3.Z);
        }
        public T SafeGet(int index0, int index1, int index2)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0)
            {
                return default;
            }

            var index = index0 * m_Step0 + index1 * m_Step1 + index2;
            return index >= m_Array.Length ? default : m_Array[index];
        }
    }
}
