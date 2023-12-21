using UnityEngine;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 继承NonsensicalMono的单例类基类，泛型T为子类类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : NonsensicalMono where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var instance = NonsensicalInstance.Instance;
                    if (instance != null)
                    {
                        instance.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        private static T _instance;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
                return;
            }

            _instance = this as T;
        }
    }
}
