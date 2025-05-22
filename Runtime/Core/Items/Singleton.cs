using System;

namespace NonsensicalKit.Core
{
    /// <summary>
    /// 不继承MonoBehaviour的单例类基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                return _instance ??= Activator.CreateInstance(typeof(T), true) as T;
            }
        }

        protected Singleton()
        {
            _instance ??= this as T;
        }
    }
}
