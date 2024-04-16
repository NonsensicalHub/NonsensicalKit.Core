using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Setting;
using NonsensicalKit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NonsensicalKit.Core.Service
{
    public static class ServiceCore
    {
        private static readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        private static List<string> _runningService;

        public static Action AfterServiceCoreInit;

        /// <summary>
        /// 场景加载前创建服务，但是由于服务本身可能需要时间进行初始化（比如通过网络获取数据），所以并不能保证场景加载完成后服务一定可以使用
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateServices()
        {
            var setting = NonsensicalSetting.LoadSetting();
            if (setting == null)
            {
                return;
            }

            _runningService = setting.RunningServices.ToList();

            var allServiceTypes = ReflectionTool.GetConcreteTypes<IClassService>();
            foreach (Type type in allServiceTypes)
            {
                if (_runningService.Contains(type.Name))
                {
                    LogCore.Info($"服务[{type.Name}]开始初始化");
                    var newService = Activator.CreateInstance(type) as IClassService;
                    _services.Add(type, newService);

                    if (newService.IsReady)
                    {
                        LogCore.Info($"服务[{type.Name}]完成初始化");

                        var context = typeof(ServiceContext<>).MakeGenericType(type);
                        var info = context.GetField("Callback", BindingFlags.Public | BindingFlags.Static);
                        var callback = info.GetValue(null);
                        if (callback != null)
                        {
                            var method = callback.GetType().GetMethod("Invoke");
                            method?.Invoke(callback, new object[] { newService });
                        }
                    }
                    else
                    {
                        var context = typeof(ServiceContext<>).MakeGenericType(type);
                        var constructor = context.GetConstructor(new Type[2] { typeof(Type), typeof(IClassService) });
                        constructor.Invoke(new object[2] { type, newService });
                    }
                }
            }

            var allMonoServiceTypes = ReflectionTool.GetConcreteTypes<MonoBehaviour, IMonoService>();
            foreach (Type type in allMonoServiceTypes)
            {
                if (_runningService.Contains(type.Name))
                {
                    LogCore.Info($"服务[{type.Name}]开始初始化");
                    var prefabAttr = type.GetCustomAttribute<ServicePrefabAttribute>();
                    GameObject gameObject = null;

                    if (prefabAttr != null)
                    {
                        var prefab = Resources.Load<GameObject>(prefabAttr.PrefabPath);

                        if (prefab != null)
                        {
                            gameObject = UnityEngine.Object.Instantiate(prefab);
                        }
                        else
                        {
                            throw new TODOException($"无法实例化服务[{type}]，Resources路径“{prefabAttr.PrefabPath}”上未找到预制体");
                        }
                    }
                    if (gameObject == null)
                    {
                        gameObject = new GameObject();
                        gameObject.AddComponent(type);
                        gameObject.name = type.Name;
                    }
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);

                    var newService = gameObject.GetComponent(type) as IMonoService;

                    _services.Add(type, newService);

                    if (newService.IsReady)
                    {
                        LogCore.Info($"服务[{type.Name}]完成初始化");

                        var context = typeof(ServiceContext<>).MakeGenericType(type);
                        var info = context.GetField("Callback", BindingFlags.Public | BindingFlags.Static);
                        var callback = info.GetValue(null);
                        if (callback != null)
                        {
                            var method = callback.GetType().GetMethod("Invoke");
                            method?.Invoke(callback, new object[] { newService });
                        }
                    }
                    else
                    {
                        var context = typeof(ServiceContext<>).MakeGenericType(type);
                        var constructor = context.GetConstructor(new Type[2] { typeof(Type), typeof(IMonoService) });
                        constructor.Invoke(new object[2] { type, newService });
                    }
                }
            }

            AfterServiceCoreInit?.Invoke();
        }

        /// <summary>
        /// 直接获取，需要确保目标管理类已经加载完成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : class, IService
        {
            if (_services.ContainsKey(typeof(T)))
            {
                return _services[typeof(T)] as T;
            }
            else
            {
                throw new TODOException($"服务[{typeof(T)}]未创建或未初始化完成");
            }
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (_services.ContainsKey(typeof(T)))
            {
                service = _services[typeof(T)] as T;
                return true;
            }
            else
            {
                service = null;
                return false;
            }
        }

        /// <summary>
        /// 安全获取，如果目标管理类未加载完成，则会在加载完成后执行回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public static void SafeGet<T>(Action<T> callback) where T : class, IService
        {
            if (!_runningService.Contains(typeof(T).Name))
            {
                throw new TODOException($"服务[{typeof(T).Name}]未配置");
            }

            if (_services.ContainsKey(typeof(T)))
            {
                if (_services[typeof(T)].IsReady)
                {
                    callback(_services[typeof(T)] as T);
                }
            }

            ServiceContext<T>.Callback += callback;
        }

        private class ServiceContext<T> where T : class, IService
        {
            public static ServiceContext<T> Instance;
            public static Action<T> Callback;

            public Type Type;
            public IService Service;

            public ServiceContext(Type type, IService service)
            {
                Type = type;
                Service = service;
                Service.InitCompleted += OnInitCompleted;
                Instance = this;
            }

            public void OnInitCompleted()
            {
                Service.InitCompleted -= OnInitCompleted;
                Callback?.Invoke(Service as T);
                LogCore.Info($"服务[{Type.Name}]完成初始化");
                Dispose();
            }

            private void Dispose()
            {
                Callback = null;
                Instance = null;
            }
        }
    }
}
