using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector4或Color的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Float4
    {
        public static readonly Float4 Zero = new Float4(0, 0, 0, 0);
        public static readonly Float4 One = new Float4(1, 1, 1, 1);

        [SerializeField] private float m_f1;
        [SerializeField] private float m_f2;
        [SerializeField] private float m_f3;
        [SerializeField] private float m_f4;

        public float F1 { get => m_f1; set => m_f1 = value; }
        public float F2 { get => m_f2; set => m_f2 = value; }
        public float F3 { get => m_f3; set => m_f3 = value; }
        public float F4 { get => m_f4; set => m_f4 = value; }

        [JsonIgnore] public float X => F1;
        [JsonIgnore] public float Y => F2;
        [JsonIgnore] public float Z => F3;
        [JsonIgnore] public float W => F4;

        [JsonIgnore] public float R => F1;
        [JsonIgnore] public float G => F2;
        [JsonIgnore] public float B => F3;
        [JsonIgnore] public float A => F4;

        public Float4(float f1, float f2, float f3, float f4)
        {
            m_f1 = f1;
            m_f2 = f2;
            m_f3 = f3;
            m_f4 = f4;
        }

        public Float4(Vector4 vector4)
        {
            m_f1 = vector4.x;
            m_f2 = vector4.y;
            m_f3 = vector4.z;
            m_f4 = vector4.w;
        }

        public Float4(Color color)
        {
            m_f1 = color.r;
            m_f2 = color.g;
            m_f3 = color.b;
            m_f4 = color.a;
        }

        public Vector3 ToVector4()
        {
            return new Vector4(F1, F2, F3, F4);
        }

        public Color ToColor()
        {
            return new Color(F1, F2, F3, F4);
        }

        public static Float4 operator *(Float4 a, float b)
        {
            Float4 c = new Float4
            {
                F1 = a.F1 * b,
                F2 = a.F2 * b,
                F3 = a.F3 * b,
                F4 = a.F4 * b
            };
            return c;
        }

        public static Float4 operator /(Float4 a, float b)
        {
            Float4 c = new Float4
            {
                F1 = a.F1 / b,
                F2 = a.F2 / b,
                F3 = a.F3 / b,
                F4 = a.F4 / b
            };
            return c;
        }

        public static Float4 operator +(Float4 a, Float4 b)
        {
            Float4 c = new Float4
            {
                F1 = a.F1 + b.F1,
                F2 = a.F2 + b.F2,
                F3 = a.F3 + b.F3,
                F4 = a.F4 + b.F4
            };
            return c;
        }

        public static Float4 operator -(Float4 a, Float4 b)
        {
            Float4 c = new Float4
            {
                F1 = a.F1 - b.F1,
                F2 = a.F2 - b.F2,
                F3 = a.F3 - b.F3,
                F4 = a.F4 - b.F4
            };
            return c;
        }

        public static Float4 operator -(Float4 a)
        {
            Float4 c = new Float4
            {
                F1 = -a.F1,
                F2 = -a.F2,
                F3 = -a.F3,
                F4 = -a.F4
            };
            return c;
        }

        public override string ToString()
        {
            return $"({F1},{F2},{F3},{F4})";
        }

        public static explicit operator Float4(Vector4 v)
        {
            return new Float4(v.x, v.y, v.z, v.w);
        }

        public static explicit operator Float4(Color c)
        {
            return new Float4(c.r, c.g, c.b, c.a);
        }

        /// <summary>
        /// 距离
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float Distance(Float4 pos1, Float4 pos2)
        {
            return Mathf.Sqrt(DistanceSquare(pos1, pos2));
        }

        /// <summary>
        /// 距离的平方，当只需要进行大小比较时，使用此方法能减少开方的性能消耗
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float DistanceSquare(Float4 pos1, Float4 pos2)
        {
            float f1Offset = pos1.F1 - pos2.F1;
            float f2Offset = pos1.F2 - pos2.F2;
            float f3Offset = pos1.F3 - pos2.F3;
            float f4Offset = pos1.F4 - pos2.F4;
            float temp = f1Offset * f1Offset + f2Offset * f2Offset + f3Offset * f3Offset + f4Offset * f4Offset;
            return temp;
        }
    }
}
