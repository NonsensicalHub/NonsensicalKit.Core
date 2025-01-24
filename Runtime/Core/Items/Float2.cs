using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector2的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Float2
    {
        public static readonly Float2 Zero = new Float2(0, 0);
        public static readonly Float2 One = new Float2(1, 1);

        [SerializeField] private float m_f1;
        [SerializeField] private float m_f2;

        public float F1 { get => m_f1; set => m_f1 = value; }
        public float F2 { get => m_f2; set => m_f2 = value; }

        [JsonIgnore] public float X => F1;
        [JsonIgnore] public float Y => F2;

        public Float2(float f1, float f2)
        {
            m_f1 = f1;
            m_f2 = f2;
        }

        public Float2(Vector2 vector2)
        {
            m_f1 = vector2.x;
            m_f2 = vector2.y;
        }

        public Vector3 ToVector3()
        {
            return new Vector2(F1, F2);
        }

        public static Float2 operator *(Float2 a, float b)
        {
            Float2 c = new Float2
            {
                F1 = a.F1 * b,
                F2 = a.F2 * b
            };
            return c;
        }

        public static Float2 operator /(Float2 a, float b)
        {
            Float2 c = new Float2
            {
                F1 = a.F1 / b,
                F2 = a.F2 / b
            };
            return c;
        }

        public static Float2 operator +(Float2 a, Float2 b)
        {
            Float2 c = new Float2
            {
                F1 = a.F1 + b.F1,
                F2 = a.F2 + b.F2
            };
            return c;
        }

        public static Float2 operator -(Float2 a, Float2 b)
        {
            Float2 c = new Float2
            {
                F1 = a.F1 - b.F1,
                F2 = a.F2 - b.F2
            };
            return c;
        }

        public static Float2 operator -(Float2 a)
        {
            Float2 c = new Float2
            {
                F1 = -a.F1,
                F2 = -a.F2,
            };
            return c;
        }

        public override string ToString()
        {
            return $"({F1},{F2})";
        }

        public static explicit operator Float2(Vector2 v)
        {
            return new Float2(v.x, v.y);
        }

        /// <summary>
        /// 距离
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float Distance(Float2 pos1, Float2 pos2)
        {
            return Mathf.Sqrt(DistanceSquare(pos1, pos2));
        }

        /// <summary>
        /// 距离的平方，当只需要进行大小比较时，使用此方法能减少开方的性能消耗
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float DistanceSquare(Float2 pos1, Float2 pos2)
        {
            float f1Offset = pos1.F1 - pos2.F1;
            float f2Offset = pos1.F2 - pos2.F2;
            float temp = f1Offset * f1Offset + f2Offset * f2Offset;
            return temp;
        }
    }
}
