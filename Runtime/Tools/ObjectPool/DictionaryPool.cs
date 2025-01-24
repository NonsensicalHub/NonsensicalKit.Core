using System.Collections.Generic;

namespace NonsensicalKit.Tools.ObjectPool
{
    public static class DictionaryPool<T1, T2>
    {
        private static readonly Stack<Dictionary<T1, T2>> Stack = new();

        public static Dictionary<T1, T2> Get()
        {
            if (Stack.Count > 0)
            {
                return Stack.Pop();
            }

            return new Dictionary<T1, T2>();
        }

        public static void Set(Dictionary<T1, T2> list)
        {
            list.Clear();
            Stack.Push(list);
        }
    }
}
