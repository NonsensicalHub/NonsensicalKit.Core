using System;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现可序列化四维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct Array4<T>
    {
        public T[] m_Array;

        public int m_Length0;
        public int m_Length1;
        public int m_Length2;
        public int m_Length3;

        public int Length0 => m_Length0;
        public int Length1 => m_Length1;
        public int Length2 => m_Length2;
        public int Length3 => m_Length3;

        public int m_Step0;
        public int m_Step1;
        public int m_Step2;
        
        public Int4 Size=>new(Length0, Length1, Length2, Length3);
        public int Length => m_Array.Length;

        public Array4(Int4 length) : this(length.I1, length.I2, length.I3, length.I4) { }

        public Array4(int length0, int length1, int length2, int length3)
        {
            m_Array = new T[length0 * length1 * length2 * length3];

            m_Length0 = length0;
            m_Length1 = length1;
            m_Length2 = length2;
            m_Length3 = length3;

            m_Step0 = length1 * length2 * length3;
            m_Step1 = length2 * length3;
            m_Step2 = length3;
        }

        public T this[int index0, int index1, int index2, int index3]
        {
            get => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2 + index3];
            set => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2 + index3] = value;
        }

        public T this[int index0, int index1, int index2]
        {
            get => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2];
            set => m_Array[index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2] = value;
        }

        public T this[Int3 int3, int index3]
        {
            get => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3 * m_Step2 + index3];
            set => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3 * m_Step2 + index3] = value;
        }

        public T this[Int3 int3]
        {
            get => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3 * m_Step2];
            set => m_Array[int3.I1 * m_Step0 + int3.I2 * m_Step1 + int3.I3 * m_Step2] = value;
        }

        public T this[Vector3Int int3]
        {
            get => m_Array[int3.x * m_Step0 + int3.y * m_Step1 + int3.z * m_Step2];
            set => m_Array[int3.x * m_Step0 + int3.y * m_Step1 + int3.z * m_Step2] = value;
        }

        public T this[int index]
        {
            get => m_Array[index];
            set => m_Array[index] = value;
        }

        public Tuple<int, int, int, int> GetIndexTuple(int index)
        {
            var x = index / m_Step0;
            var y = (index - x * m_Step0) / m_Step1;
            var z = (index - x * m_Step0 - y * m_Step1) / m_Step2;
            var w = index - x * m_Step0 - y * m_Step1 - z * m_Step2;
            return Tuple.Create(x, y, z, w);
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


        public int GetIndex(int index0, int index1, int index2, int index3)
        {
            return index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2 + index3;
        }

        public void SafeSet(int index0, int index1, int index2, int index3, T value)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0 || index3 < 0)
            {
                return;
            }

            var index = index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2 + index3;
            if (index < m_Array.Length)
            {
                m_Array[index] = value;
            }
        }

        public T SafeGet(int index0, int index1, int index2, int index3)
        {
            if (index0 < 0 || index1 < 0 || index2 < 0 || index3 < 0)
            {
                return default;
            }

            var index = index0 * m_Step0 + index1 * m_Step1 + index2 * m_Step2 + index3;
            return index >= m_Array.Length ? default : m_Array[index];
        }
    }
}
