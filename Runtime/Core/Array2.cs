using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 使用一维数组实现二维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Array2<T>
    {
        public readonly int Length0;
        public readonly int Length1;

        private readonly T[] _array2;

        private readonly int _step0;

        public Array2(int _length0, int _length1)
        {
            _array2 = new T[_length0 * _length1];
            this.Length0 = _length0;
            Length1 = _length1;

            _step0 = _length1;
        }

        public void Reset()
        {
            Reset(default);
        }

        public void Reset(T state)
        {
            for (int i = 0; i < _array2.Length; i++)
            {
                _array2[i] = state;
            }
        }

        public T this[int index0, int index1]
        {
            get
            {
                return _array2[index0 * _step0 + index1 ];
            }
            set
            {
                _array2[index0 * _step0 + index1 ] = value;

            }
        }

        public T this[int index]
        {
            get
            {
                return _array2[index];
            }
            set
            {
                _array2[index] = value;
            }
        }

        public Tuple<int, int> GetIndexTuple(int index)
        {
            var x = index / _step0;
            var y = index - x * _step0;
            return Tuple.Create(x, y);
        }

        public int GetIndex(int index0, int index1)
        {
            return index0 * _step0 + index1 ;
        }
    }
}
