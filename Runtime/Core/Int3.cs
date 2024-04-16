namespace NonsensicalKit.Core
{
    /// <summary>
    /// 限定值为Int的三位向量
    /// 新版本Unity可用Vector3Int
    /// </summary>
    public struct Int3
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
        public int I3 { get; set; }

        public int X => I1;
        public int Y => I2;
        public int Z => I3;

        public Int3(int i1, int i2, int i3)
        {
            I1 = i1;
            I2 = i2;
            I3 = i3;
        }


        public Int3(Float3 _float3)
        {
            I1 = (int)_float3.F1;
            if (_float3.F1 - I1 > 0.5f)
            {
                I1++;
            }


            I2 = (int)_float3.F2;
            if (_float3.F2 - I2 > 0.5f)
            {
                I2++;
            }


            I3 = (int)_float3.F3;
            if (_float3.F3 - I3 > 0.5f)
            {
                I3++;
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

        public int GetValue(Int3 a)
        {
            if (a.I1 != 0)
            {
                return this.I1;
            }
            else if (a.I2 != 0)
            {
                return this.I2;
            }
            else if (a.I3 != 0)
            {
                return this.I3;
            }
            else
            {
                return 0;
            }
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
    }
}
