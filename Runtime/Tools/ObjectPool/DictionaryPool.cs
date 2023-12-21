using System.Collections.Generic;

namespace NonsensicalKit.Tools.ObjectPool
{
    public static class DictionaryPool<T1, T2>
    {
        private static Stack<Dictionary<T1, T2>> _stack = new Stack<Dictionary<T1, T2>>();

        public static Dictionary<T1, T2> Get()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            return new Dictionary<T1, T2>();
        }

        public static void Set(Dictionary<T1, T2> list)
        {
            list.Clear();
            _stack.Push(list);
        }
    }
}
