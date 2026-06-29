using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector3Int的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Int3 : IEquatable<Int3>
    {
        public static readonly Int3 Zero = new Int3(0, 0, 0);
        public static readonly Int3 One = new Int3(1, 1, 1);

        [SerializeField] private int m_i1;
        [SerializeField] private int m_i2;
        [SerializeField] private int m_i3;

        public int I1 { get => m_i1; set => m_i1 = value; }
        public int I2 { get => m_i2; set => m_i2 = value; }
        public int I3 { get => m_i3; set => m_i3 = value; }

        [JsonIgnore] public int X => I1;
        [JsonIgnore] public int Y => I2;
        [JsonIgnore] public int Z => I3;

        public Int3(int i1, int i2, int i3)
        {
            m_i1 = i1;
            m_i2 = i2;
            m_i3 = i3;
        }

        public Int3(Float3 float3)
        {
            m_i1 = (int)float3.F1;
            if (float3.F1 - m_i1 >= 0.5f)
            {
                m_i1++;
            }

            m_i2 = (int)float3.F2;
            if (float3.F2 - m_i2 >= 0.5f)
            {
                m_i2++;
            }

            m_i3 = (int)float3.F3;
            if (float3.F3 - m_i3 >= 0.5f)
            {
                m_i3++;
            }
        }


        public int this[int index]
        {
            get
            {
                return index switch
                {
                    1 => m_i2,
                    2 => m_i3,
                    _ => m_i1
                };
            }
            set
            {
                switch (index)
                {
                    default: m_i1 = value; break;
                    case 1: m_i2 = value; break;
                    case 2: m_i3 = value; break;
                }
            }
        }

        public static Int3 operator +(Int3 a, Int3 b)
        {
            Int3 c = new Int3
            {
                I1 = a.I1 + b.I1,
                I2 = a.I2 + b.I2,
                I3 = a.I3 + b.I3
            };
            return c;
        }

        public static Int3 operator -(Int3 a, Int3 b)
        {
            Int3 c = new Int3
            {
                I1 = a.I1 - b.I1,
                I2 = a.I2 - b.I2,
                I3 = a.I3 - b.I3
            };
            return c;
        }

        public static Int3 operator -(Int3 a)
        {
            Int3 c = new Int3
            {
                I1 = -a.I1,
                I2 = -a.I2,
                I3 = -a.I3
            };
            return c;
        }
        public static bool operator ==(Int3 a,Int3 b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Int3 a,Int3 b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 检测是否超出最大值
        /// </summary>
        /// <param name="max1"></param>
        /// <param name="max2"></param>
        /// <param name="max3"></param>
        /// <returns></returns>
        public bool CheckBound(int max1, int max2, int max3)
        {
            if (I1 < 0 || I2 < 0 || I3 < 0 || I1 > max1 || I2 > max2 || I3 > max3)
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
            return $"({I1},{I2},{I3})";
        }

        public bool Equals(Int3 obj)
        {
            return I1 == obj.I1 &&
                   I2 == obj.I2 &&
                   I3 == obj.I3;
        }

        public override bool Equals(object obj)
        {
            return obj is Int3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_i1, m_i2, m_i3);
        }
    }
}
