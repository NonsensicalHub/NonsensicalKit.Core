using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现三维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array3<T>
    {
        private readonly T[] _array3;

        public readonly int Length0;
        public readonly int Length1;
        public readonly int Length2;

        private readonly int _step0;
        private readonly int _step1;

        public Array3(int _length0, int _length1, int _length2)
        {
            _array3 = new T[_length0 * _length1 * _length2];
            this.Length0 = _length0;
            Length1 = _length1;
            Length2 = _length2;

            _step0 = _length1 * _length2;
            _step1 = _length2;
        }

        public void Reset(T state)
        {
            for (int i = 0; i < _array3.Length; i++)
            {
                _array3[i] = state;
            }
        }

        public T this[int index0, int index1, int index2]
        {
            get
            {
                return _array3[index0 * _step0 + index1 * _step1 + index2];
            }
            set
            {
                _array3[index0 * _step0 + index1 * _step1 + index2] = value;

            }
        }

        public T this[Int3 int3]
        {
            get
            {
                return _array3[int3.I1 * _step0 + int3.I2 * _step1 + int3.I3];
            }
            set
            {
                _array3[int3.I1 * _step0 + int3.I2 * _step1 + int3.I3] = value;

            }
        }

        public T this[int index]
        {
            get
            {
                return _array3[index];
            }
            set
            {
                _array3[index] = value;
            }
        }

        public Tuple<int, int, int> GetValue(int index)
        {
            var x = index / _step0;
            var y = (index - x * _step0) / _step1;
            var z = index - x * _step0 - y * _step1;
            return Tuple.Create(x, y, z);
        }

        public int GetIndex(int index0, int index1, int index2)
        {
            return index0 * _step0 + index1 * _step1 + index2;
        }
    }
}
