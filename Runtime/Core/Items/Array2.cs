using System;
using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现可序列化二维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct Array2<T>
    {
        public T[] m_Array;

        public int m_Length0;
        public int m_Length1;

        public int m_Step0;

        public Array2(int length0, int length1)
        {
            m_Array = new T[length0 * length1];
            m_Length0 = length0;
            m_Length1 = length1;

            m_Step0 = length1;
        }

        public void Fill(T state = default)
        {
            if (m_Array == null) return;
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

        public Array2<T> CopyToNewArray(int length0, int length1)
        {
            var newArray = new Array2<T>(length0, length1);

            int min0 = length0 < m_Length0 ? length0 : m_Length0;
            int min1 = length1 < m_Length1 ? length1 : m_Length1;

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
            get => m_Array[index0 * m_Step0 + index1];
            set => m_Array[index0 * m_Step0 + index1] = value;
        }

        public T this[int index]
        {
            get => m_Array[index];
            set => m_Array[index] = value;
        }

        public bool TryGet(int index0, int index1, out T value)
        {
            value = default;
            if (index0<0||index1<0||index0>=m_Length0||index1>=m_Length1)
            {
                return false;
            }
            value=this[index0 * m_Step0 + index1];
            return true;
        }

        public Tuple<int, int> GetIndexTuple(int index)
        {
            var x = index / m_Step0;
            var y = index - x * m_Step0;
            return Tuple.Create(x, y);
        }

        public int GetIndex(int index0, int index1)
        {
            return index0 * m_Step0 + index1;
        }
    }
}
