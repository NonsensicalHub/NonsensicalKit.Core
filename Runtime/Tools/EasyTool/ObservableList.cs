using System;
using System.Collections;
using System.Collections.Generic;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 可观察链表，可以监听各种改变的事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableList<T> : IList<T>
    {
        /// <summary>
        /// 当被整体替换时
        /// </summary>
        public Action<List<T>, List<T>> OnListChanged;

        public Action<T> OnAdd;
        public Action<int, T> OnInsert;
        public Action<T> OnRemove;
        public Action<int> OnRemoveAt;
        public Action<int, T, T> OnValueChanged;
        public Action OnClear;

        //预先初始化，防止空异常
        private List<T> _list = new();

        public List<T> List
        {
            get => _list;
            set
            {
                if (!ReferenceEquals(_list, value))
                {
                    var old = _list;
                    _list = value;

                    ListChanged(old, _list);
                }
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ListChanged(List<T> oldValue, List<T> newValue)
        {
            OnListChanged?.Invoke(oldValue, newValue);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            OnAdd?.Invoke(item);
        }

        public void Clear()
        {
            _list.Clear();
            OnClear?.Invoke();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (_list.Remove(item))
            {
                OnRemove?.Invoke(item);
                return true;
            }

            return false;
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnInsert?.Invoke(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            OnRemoveAt(index);
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                var old = _list[index];
                _list[index] = value;
                OnValueChanged(index, old, value);
            }
        }
    }
}
