using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector4Int的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Int4 : IEquatable<Int4>
    {
        public static readonly Int4 Zero = new Int4(0, 0, 0, 0);
        public static readonly Int4 One = new Int4(1, 1, 1, 1);

        [SerializeField] private int m_i1;
        [SerializeField] private int m_i2;
        [SerializeField] private int m_i3;
        [SerializeField] private int m_i4;

        public int I1 { get => m_i1; set => m_i1 = value; }
        public int I2 { get => m_i2; set => m_i2 = value; }
        public int I3 { get => m_i3; set => m_i3 = value; }
        public int I4 { get => m_i4; set => m_i4 = value; }

        [JsonIgnore] public int X => I1;
        [JsonIgnore] public int Y => I2;
        [JsonIgnore] public int Z => I3;
        [JsonIgnore] public int W => I4;

        public Int4(int i1, int i2, int i3, int i4)
        {
            m_i1 = i1;
            m_i2 = i2;
            m_i3 = i3;
            m_i4 = i4;
        }

        public Int4(Float4 float4)
        {
            m_i1 = (int)float4.F1;
            if (float4.F1 - m_i1 >= 0.5f)
            {
                m_i1++;
            }

            m_i2 = (int)float4.F2;
            if (float4.F2 - m_i2 >= 0.5f)
            {
                m_i2++;
            }

            m_i3 = (int)float4.F3;
            if (float4.F3 - m_i3 >= 0.5f)
            {
                m_i3++;
            }

            m_i4 = (int)float4.F4;
            if (float4.F4 - m_i4 >= 0.5f)
            {
                m_i4++;
            }
        }

        public static Int4 operator +(Int4 a, Int4 b)
        {
            Int4 c = new Int4
            {
                I1 = a.I1 + b.I1,
                I2 = a.I2 + b.I2,
                I3 = a.I3 + b.I3,
                I4 = a.I4 + b.I4,
            };
            return c;
        }

        public static Int4 operator -(Int4 a, Int4 b)
        {
            Int4 c = new Int4
            {
                I1 = a.I1 - b.I1,
                I2 = a.I2 - b.I2,
                I3 = a.I3 - b.I3,
                I4 = a.I4 - b.I4,
            };
            return c;
        }

        public static Int4 operator -(Int4 a)
        {
            Int4 c = new Int4
            {
                I1 = -a.I1,
                I2 = -a.I2,
                I3 = -a.I3,
                I4 = -a.I4,
            };
            return c;
        }

        /// <summary>
        /// 检测是否超出最大值
        /// </summary>
        /// <returns></returns>
        public bool CheckBound(int max1, int max2, int max3, int max4)
        {
            if (I1 < 0 || I2 < 0 || I3 < 0 || I4 < 0 || I1 > max1 || I2 > max2 || I3 > max3 || I4 > max4)
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
            return $"({I1},{I2},{I3},{I4})";
        }

        public bool Equals(Int4 obj)
        {
            return I1 == obj.I1 &&
                   I2 == obj.I2 &&
                   I3 == obj.I3 &&
                   I4 == obj.I4;
        }

        public override bool Equals(object obj)
        {
            return obj is Int4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_i1, m_i2, m_i3, m_i4);
        }
    }
}
