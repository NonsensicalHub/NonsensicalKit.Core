using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// Vector3的可序列化替代品
    /// </summary>
    [Serializable]
    public struct Float3
    {
        public static readonly Float3 Zero = new Float3(0, 0, 0);
        public static readonly Float3 One = new Float3(1, 1, 1);

        [SerializeField] private float m_f1;
        [SerializeField] private float m_f2;
        [SerializeField] private float m_f3;

        public float F1 { get => m_f1; set => m_f1 = value; }
        public float F2 { get => m_f2; set => m_f2 = value; }
        public float F3 { get => m_f3; set => m_f3 = value; }

        [JsonIgnore] public float X { get => m_f1; set => m_f1 = value; }
        [JsonIgnore] public float Y { get => m_f2; set => m_f2 = value; }
        [JsonIgnore] public float Z { get => m_f3; set => m_f3 = value; }

        public Float3(float f1, float f2, float f3)
        {
            m_f1 = f1;
            m_f2 = f2;
            m_f3 = f3;
        }

        public Float3(Vector3 vector3)
        {
            m_f1 = vector3.x;
            m_f2 = vector3.y;
            m_f3 = vector3.z;
        }

        public float this[int index]
        {
            get
            {
                return index switch
                {
                    1 => m_f2,
                    2 => m_f3,
                    _ => m_f1
                };
            }
            set
            {
                switch (index)
                {
                    default: m_f1 = value; break;
                    case 1: m_f2 = value; break;
                    case 2: m_f3 = value; break;
                }
            }
        }

        public Vector3 ToVector3()
        {
            return new Vector3(F1, F2, F3);
        }

        public static Float3 operator *(Float3 a, float b)
        {
            Float3 c = new Float3
            {
                F1 = a.F1 * b,
                F2 = a.F2 * b,
                F3 = a.F3 * b
            };
            return c;
        }

        public static Float3 operator /(Float3 a, float b)
        {
            Float3 c = new Float3
            {
                F1 = a.F1 / b,
                F2 = a.F2 / b,
                F3 = a.F3 / b
            };
            return c;
        }

        public static Float3 operator +(Float3 a, Float3 b)
        {
            Float3 c = new Float3
            {
                F1 = a.F1 + b.F1,
                F2 = a.F2 + b.F2,
                F3 = a.F3 + b.F3
            };
            return c;
        }

        public static Float3 operator -(Float3 a, Float3 b)
        {
            Float3 c = new Float3
            {
                F1 = a.F1 - b.F1,
                F2 = a.F2 - b.F2,
                F3 = a.F3 - b.F3
            };
            return c;
        }

        public static Float3 operator -(Float3 a)
        {
            Float3 c = new Float3
            {
                F1 = -a.F1,
                F2 = -a.F2,
                F3 = -a.F3
            };
            return c;
        }

        public override string ToString()
        {
            return $"({F1},{F2},{F3})";
        }

        public static explicit operator Float3(string input)
        {
            string pattern = @"^\((-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)\)$";

            Match match = Regex.Match(input, pattern);

            if (match.Success && match.Groups.Count == 4)
            {
                float[] result = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    if (float.TryParse(match.Groups[i + 1].Value, out float value))
                    {
                        result[i] = value;
                    }
                    else
                    {
                        return Zero;
                    }
                }

                return new Float3(result[0], result[1], result[2]);
            }
            else
            {
                return Zero;
            }
        }

        public static Float3 FromObject(object input) {
            return input switch
            {
                Vector3 vector3 => (Float3)vector3,
                Float3 float3 => float3,
                string => (Float3)input.ToString(),
                _ => Zero
            };
        }

        public static explicit operator Float3(Vector3 v)
        {
            return new Float3(v.x, v.y, v.z);
        }

        /// <summary>
        /// 距离
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float Distance(Float3 pos1, Float3 pos2)
        {
            return Mathf.Sqrt(DistanceSquare(pos1, pos2));
        }

        /// <summary>
        /// 距离的平方，当只需要进行大小比较时，使用此方法能减少开方的性能消耗
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static float DistanceSquare(Float3 pos1, Float3 pos2)
        {
            float f1Offset = pos1.F1 - pos2.F1;
            float f2Offset = pos1.F2 - pos2.F2;
            float f3Offset = pos1.F3 - pos2.F3;
            float temp = f1Offset * f1Offset + f2Offset * f2Offset + f3Offset * f3Offset;
            return temp;
        }
    }
}
