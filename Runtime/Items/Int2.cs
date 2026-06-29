using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector2Int的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public static readonly Int2 Zero = new Int2(0, 0);
        public static readonly Int2 One = new Int2(1, 1);
        
        [SerializeField] private int m_i1;
        [SerializeField] private int m_i2;

        public int I1 { get => m_i1; set => m_i1 = value; }
        public int I2 { get => m_i2; set => m_i2 = value; }

        [JsonIgnore] public int X => I1;
        [JsonIgnore] public int Y => I2;

        public Int2(int i1, int i2)
        {
            m_i1 = i1;
            m_i2 = i2;
        }

        public Int2(Float2 float2)
        {
            m_i1 = (int)float2.F1;
            if (float2.F1 - m_i1 >= 0.5f)
            {
                m_i1++;
            }

            m_i2 = (int)float2.F2;
            if (float2.F2 - m_i2 >= 0.5f)
            {
                m_i2++;
            }
        }

        public static Int2 operator +(Int2 a, Int2 b)
        {
            Int2 c = new Int2
            {
                I1 = a.I1 + b.I1,
                I2 = a.I2 + b.I2
            };
            return c;
        }

        public static Int2 operator -(Int2 a, Int2 b)
        {
            Int2 c = new Int2
            {
                I1 = a.I1 - b.I1,
                I2 = a.I2 - b.I2
            };
            return c;
        }

        public static Int2 operator -(Int2 a)
        {
            Int2 c = new Int2
            {
                I1 = -a.I1,
                I2 = -a.I2
            };
            return c;
        }

        /// <summary>
        /// 检测是否超出最大值
        /// </summary>
        /// <param name="max1"></param>
        /// <param name="max2"></param>
        /// <returns></returns>
        public bool CheckBound(int max1, int max2)
        {
            if (I1 < 0 || I2 < 0 || I1 > max1 || I2 > max2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override string ToString()
        {
            return $"({I1},{I2})";
        }

        public bool Equals(Int2 obj)
        {
            return I1 == obj.I1 &&
                   I2 == obj.I2;
        }

        public override bool Equals(object obj)
        {
            return obj is Int2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_i1, m_i2);
        }
    }
}
