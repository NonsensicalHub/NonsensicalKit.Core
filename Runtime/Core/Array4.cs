using System;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 使用一维数组实现四维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array4<T>
    {
        private readonly T[] _array4;

        public readonly int Length0;
        public readonly int Length1;
        public readonly int Length2;
        public readonly int Length3;

        private readonly int _step0;
        private readonly int _step1;
        private readonly int _step2;

        public Array4(int _length0, int _length1, int _length2, int _length3)
        {
            _array4 = new T[_length0 * _length1 * _length2 * _length3];
            Length0 = _length0;
            Length1 = _length1;
            Length2 = _length2;
            Length3 = _length3;

            _step0 = _length1 * _length2 * _length3;
            _step1 = _length2 * _length3;
            _step2 = _length3;
        }

        public T this[int index0, int index1, int index2, int index3]
        {
            get
            {
                return _array4[index0 * _step0 + index1 * _step1 + index2 * _step2 + index3];
            }
            set
            {
                _array4[index0 * _step0 + index1 * _step1 + index2 * _step2 + index3] = value;
            }
        }

        public T this[Int3 int3, int index3]
        {
            get
            {
                return _array4[int3.I1 * _step0 + int3.I2 * _step1 + int3.I3 * _step2 + index3];
            }
            set
            {
                _array4[int3.I1 * _step0 + int3.I2 * _step1 + int3.I3 * _step2 + index3] = value;
            }
        }

        public T this[int index]
        {
            get
            {
                return _array4[index];
            }
            set
            {
                _array4[index] = value;
            }
        }

        public Tuple<int, int, int, int> GetValue(int index)
        {
            var x = index / _step0;
            var y = (index - x * _step0) / _step1;
            var z = (index - x * _step0 - y * _step1) / _step2;
            var w = index - x * _step0 - y * _step1 - z * _step2;
            return Tuple.Create(x, y, z, w);
        }

        public int GetIndex(int index0, int index1, int index2, int index3)
        {
            return index0 * _step0 + index1 * _step1 + index2 * _step2 + index3;
        }
    }
}
