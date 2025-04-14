using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NonsensicalKit.Core.Log;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonsensicalKit.Core.Service.Asset
{
    [ServicePrefab("Services/AssetBundleService")]
    public class AssetBundleService : NonsensicalMono, IMonoService
    {
        [SerializeField] private string m_runtimeBaseUrl; //基础url
        [SerializeField] private string m_editorBasePath = "..\\AssetBundles";
        [SerializeField] private int m_version = 10000;
        [SerializeField] private string[] m_preDownloadBundles; //需要提前下载的ab包
        [SerializeField] private bool m_logMessage;
        [SerializeField] private bool m_dynamicUrlBase = true;

        public bool IsReady { get; private set; }

        public Action InitCompleted { get; set; }

        public Action<string, float> OnBundleLoading { get; set; }
        public Action<string, AssetBundle> OnBundleCompleted { get; set; }
        public Action<string, string, float> OnResourceLoading { get; set; }
        public Action<string, string> OnResourceCompleted { get; set; }

        public bool IsLoading => LoadBundleCount > 0; //当前是否正在加载包

        public int LoadBundleCount { get; private set; } //当前加载的包的数量

        private readonly HashSet<string> _bundleLogBuffer = new();

        private string _rootUrl; //基础url拼接平台字符串得出的根路径

        private readonly Dictionary<string, AssetBundleContext>
            _assetBundleDic = new Dictionary<string, AssetBundleContext>(); //key是包名，value是ab包上下文信息


        protected void Awake()
        {
#if UNITY_EDITOR
            //读取当前版本并赋值到预制体中
            int version = EditorPrefs.GetInt("assetBundle_Version", 10000);
            if (m_version < version)
            {
                m_version = version;
                var service = Resources.Load<AssetBundleService>("Services/AssetBundleService");
                service.m_version = version;
                EditorUtility.SetDirty(service);
                PrefabUtility.RecordPrefabInstancePropertyModifications(service.gameObject);
                PrefabUtility.SavePrefabAsset(service.gameObject);
            }
            else if (m_version > version)
            {
                EditorPrefs.SetInt("assetBundle_Version", m_version);
            }
#endif
            AutoInit();
        }

        public void Init(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl)) return;
            _rootUrl = Path.Combine(baseUrl, PlatformInfo.GetPlatformFolderName());

            StartCoroutine(InitCor());
        }

        private void AutoInit()
        {
            if (PlatformInfo.IsEditor)
            {
                if (string.IsNullOrEmpty(m_editorBasePath))
                {
                    LogCore.Error("未配置编辑时ab包路径");
                }

                _rootUrl = Path.Combine(Application.dataPath, m_editorBasePath, PlatformInfo.GetPlatformFolderName());
                StartCoroutine(InitCor());
            }
            else if (!m_dynamicUrlBase)
            {
                if (string.IsNullOrEmpty(m_runtimeBaseUrl))
                {
                    return;
                }

                _rootUrl = Path.Combine(m_runtimeBaseUrl, PlatformInfo.GetPlatformFolderName());

                StartCoroutine(InitCor());
            }
        }

        /// <summary>
        /// 初始化管理类
        /// </summary>
        private IEnumerator InitCor()
        {
            IsReady = false;

            var assetBundleCreateRequest = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(_rootUrl, PlatformInfo.GetPlatformFolderName()));
            yield return assetBundleCreateRequest.SendWebRequest();
            AssetBundle assetBundle = (assetBundleCreateRequest.downloadHandler as DownloadHandlerAssetBundle)?.assetBundle;

            if (assetBundle is null)
            {
                LogCore.Warning("未找到AssetBundles:" + _rootUrl);
                yield break;
            }

            _assetBundleDic.Clear();

            var manifestRequest = assetBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
            yield return manifestRequest;
            AssetBundleManifest assetBundleManifest = manifestRequest.asset as AssetBundleManifest;

            if (assetBundleManifest is null)
            {
                LogCore.Warning("AssetBundles无法解析:" + _rootUrl);
                yield break;
            }

            string[] bundles = assetBundleManifest.GetAllAssetBundles();

            foreach (var item in bundles)
            {
                _assetBundleDic.Add(item, new AssetBundleContext(item, assetBundleManifest.GetDirectDependencies(item)));
            }


            IsReady = true;
            InitCompleted?.Invoke();

            PreDownload();
        }

        private void PreDownload()
        {
            if (m_preDownloadBundles.Length > 0)
            {
                StartCoroutine(PreDownloadCor());
            }
        }

        private IEnumerator PreDownloadCor()
        {
            yield return null;
            foreach (var t in m_preDownloadBundles)
            {
                //当上一次加载还未完成或者有其他加载正在进行则延迟下一次加载
                while (LoadBundleCount > 0)
                {
                    yield return null;
                }

                var crtPreDownloadBundleName = t;
                LogCore.Info("开始提前下载包：" + crtPreDownloadBundleName);
                LoadAssetBundle(crtPreDownloadBundleName);
                while (LoadBundleCount > 0)
                {
                    yield return null;
                }

                UnloadUnusedBundle(crtPreDownloadBundleName);
            }

            LogCore.Info("提前下载完成");
        }


        /// <summary>
        /// 检测某个包是否加载完成
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public bool CheckAssetBundle(string bundlePath)
        {
            bundlePath = bundlePath.ToLower();
            if (_assetBundleDic.TryGetValue(bundlePath, out var value) == false)
            {
                LogCore.Warning($"错误的包名：{bundlePath}");
                return false;
            }

            return value.IsLoadCompleted;
        }

        /// <summary>
        /// 加载AB包
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="onLoading"></param>
        /// <param name="onLoaded"></param>
        public void LoadAssetBundle(string bundlePath, Action<string, AssetBundle> onComplete = null, Action<string, float> onLoading = null,
            Action<string> onLoaded = null)
        {
            bundlePath = bundlePath.ToLower();
            if (m_logMessage && _bundleLogBuffer.Contains(bundlePath) == false)
            {
                LogCore.Info("加载ab包：" + bundlePath);
                _bundleLogBuffer.Add(bundlePath);
            }

            if (_assetBundleDic.TryGetValue(bundlePath, out var value) == false)
            {
                LogCore.Warning($"错误的包名：{bundlePath}");
                return;
            }

            if (value.IsLoadCompleted)
            {
                try
                {
                    onLoading?.Invoke(bundlePath, 1);
                    onLoaded?.Invoke(bundlePath);
                    onComplete?.Invoke(bundlePath, _assetBundleDic[bundlePath].AssetBundlePack);
                }
                catch (Exception)
                {
                    // ignored
                }

                return;
            }

            if (_assetBundleDic[bundlePath].IsLoading == false)
            {
                _assetBundleDic[bundlePath].OnLoadCompleted = onComplete;
                _assetBundleDic[bundlePath].OnLoading = onLoading;
                _assetBundleDic[bundlePath].OnLoaded = onLoaded;
                StartCoroutine(LoadAssetBundleCoroutine(bundlePath));
            }
            else
            {
                _assetBundleDic[bundlePath].OnLoadCompleted += onComplete;
                _assetBundleDic[bundlePath].OnLoading += onLoading;
                _assetBundleDic[bundlePath].OnLoaded += onLoaded;
            }
        }

        /// <summary>
        /// 加载Ab包协程
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        private IEnumerator LoadAssetBundleCoroutine(string bundlePath)
        {
            LoadBundleCount++;
            _assetBundleDic[bundlePath].IsLoading = true;

            string bundleUrl = Path.Combine(_rootUrl, bundlePath);

            var request = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, (uint)m_version, 0);

            request.SendWebRequest();
            do
            {
                yield return null;
                var crtProcess = request.downloadProgress;
                _assetBundleDic[bundlePath].OnLoading?.Invoke(bundlePath, crtProcess);
                OnBundleLoading?.Invoke(bundlePath, crtProcess);
            }
            while (!request.isDone);

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);

            if (!assetBundle)
            {
                LogCore.Error($"AB包加载失败，路径：{bundleUrl}");
                //Todo:错误处理
                yield break;
            }

            try
            {
                _assetBundleDic[bundlePath].OnLoaded?.Invoke(bundlePath);
            }
            catch (Exception)
            {
                // ignored
            }

            _assetBundleDic[bundlePath].OnLoaded = null;
            _assetBundleDic[bundlePath].IsLoaded = true;
            _assetBundleDic[bundlePath].AssetBundlePack = assetBundle;

            //加载依赖包
            //应当先加载自己再加载依赖包，防止出现互相依赖时互相等待的情况
            //但应当等待加载完成后再回调_onCompleted，保证使用时不出现引用丢失
            string[] dependencies = _assetBundleDic[bundlePath].Dependencies;
            int completeCount = 0;
            foreach (var item in dependencies)
            {
                _assetBundleDic[item].DependencyCount++;
                LoadAssetBundle(item, null, null, (_) => { completeCount++; });
            }

            while (completeCount < dependencies.Length)
            {
                yield return null;
            }

            _assetBundleDic[bundlePath].IsLoading = false;
            _assetBundleDic[bundlePath].OnLoading = null;

            _assetBundleDic[bundlePath].IsLoadCompleted = true;

            try
            {
                _assetBundleDic[bundlePath].OnLoadCompleted?.Invoke(bundlePath, assetBundle);
                OnBundleCompleted?.Invoke(bundlePath, assetBundle);
            }
            catch (Exception)
            {
                // ignored
            }

            _assetBundleDic[bundlePath].OnLoadCompleted = null;

            LoadBundleCount--;
        }

        #region LoadResource

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName">资源名</param>
        /// <param name="bundlePath">包名</param>
        /// <param name="onCompleted">完成回调</param>
        /// <param name="onLoading"></param>
        public void LoadResource<T>(string resourceName, string bundlePath, Action<string, string, T> onCompleted,
            Action<string, string, float> onLoading = null) where T : Object
        {
            bundlePath = bundlePath.ToLower();

            if (_assetBundleDic.ContainsKey(bundlePath) == false)
            {
                LogCore.Warning($"错误的包名{bundlePath}");
                return;
            }

            if (_assetBundleDic[bundlePath].IsLoadCompleted)
            {
                _assetBundleDic[bundlePath].LoadCount++;
                StartCoroutine(
                    LoadResourceCoroutine(resourceName, bundlePath, _assetBundleDic[bundlePath].AssetBundlePack, onCompleted, onLoading));
            }
            else
            {
                StartCoroutine(WaitAssetBundleLoad(resourceName, bundlePath, onCompleted, onLoading));
                LoadAssetBundle(bundlePath);
            }
        }

        /// <summary>
        /// 加载资源所在的包正在加载时，等待其加载完成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <param name="bundlePath"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onLoading"></param>
        /// <returns></returns>
        private IEnumerator WaitAssetBundleLoad<T>(string resourceName, string bundlePath, Action<string, string, T> onCompleted,
            Action<string, string, float> onLoading) where T : Object
        {
            while (_assetBundleDic[bundlePath].IsLoadCompleted == false)
            {
                yield return null;
            }

            _assetBundleDic[bundlePath].LoadCount++;
            StartCoroutine(LoadResourceCoroutine(resourceName, bundlePath, _assetBundleDic[bundlePath].AssetBundlePack, onCompleted, onLoading));
        }

        /// <summary>
        /// 加载资源协程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <param name="bundlePath"></param>
        /// <param name="assetBundle"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onLoading"></param>
        /// <returns></returns>
        private IEnumerator LoadResourceCoroutine<T>(string resourceName, string bundlePath, AssetBundle assetBundle,
            Action<string, string, T> onCompleted,
            Action<string, string, float> onLoading) where T : Object
        {
            AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T>(resourceName);

            if (onLoading == null)
            {
                yield return assetBundleRequest;
            }
            else
            {
                do
                {
                    yield return null;

                    onLoading(bundlePath, resourceName, assetBundleRequest.progress);
                    OnResourceLoading?.Invoke(bundlePath, resourceName, assetBundleRequest.progress);
                }
                while (!assetBundleRequest.isDone);
            }

            if (assetBundleRequest.asset != null)
            {
                T obj = assetBundleRequest.asset as T;
                onCompleted(bundlePath, resourceName, obj);
                OnResourceCompleted?.Invoke(bundlePath, resourceName);
            }
            else
            {
                LogCore.Error($"未加载到对象:{resourceName}");
            }
        }

        #endregion

        /// <summary>
        /// 销毁资源时调用，减少使用计数器
        /// </summary>
        /// <param name="bundleName"></param>
        public void ReleaseAsset(string bundleName)
        {
            bundleName = bundleName.ToLower();
            _assetBundleDic[bundleName].LoadCount--;
        }

        /// <summary>
        /// 如果未被使用则卸载ab包
        /// </summary>
        /// <param name="bundleName"></param>
        public void UnloadUnusedBundle(string bundleName)
        {
            bundleName = bundleName.ToLower();
            if (_assetBundleDic[bundleName].LoadCount == 0 && _assetBundleDic[bundleName].DependencyCount == 0)
            {
                UnloadBundle(bundleName);
            }
        }

        /// <summary>
        /// 强制卸载ab包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="checkDependencies"></param>
        public void UnloadBundle(string bundleName, bool checkDependencies = true)
        {
            bundleName = bundleName.ToLower();
            if (_assetBundleDic.ContainsKey(bundleName))
            {
                var bundle = _assetBundleDic[bundleName];
                if (bundle.AssetBundlePack != null)
                {
                    bundle.AssetBundlePack.Unload(true);
                    bundle.AssetBundlePack = null;
                    if (checkDependencies)
                    {
                        string[] dependencies = _assetBundleDic[bundleName].Dependencies;
                        foreach (var item in dependencies)
                        {
                            _assetBundleDic[item].DependencyCount--;
                            UnloadUnusedBundle(item);
                        }
                    }
                    else
                    {
                        bundle.DependencyCount = 0;
                    }
                }

                bundle.IsLoading = false;
                bundle.IsLoaded = false;
                bundle.IsLoadCompleted = false;
                bundle.LoadCount = 0;

                bundle.OnLoadCompleted = null;
                bundle.OnLoaded = null;
                bundle.OnLoading = null;
            }
        }

        /// <summary>
        /// 自动通过计数器卸载未被使用的ab包
        /// </summary>
        public void UnloadAllUnusedBundles()
        {
            foreach (var item in _assetBundleDic)
            {
                UnloadUnusedBundle(item.Key);
            }
        }

        /// <summary>
        /// 强制释放所有ab包资源
        /// </summary>
        public void UnloadAllBundles()
        {
            StopAllCoroutines();
            foreach (var item in _assetBundleDic)
            {
                UnloadBundle(item.Key, false);
            }
        }

        private class AssetBundleContext
        {
            public string BundlePath; //包的名称
            public string[] Dependencies; //直接依赖的包名称

            public AssetBundle AssetBundlePack; //ab包

            public bool IsLoading; //是否正在进行加载
            public bool IsLoaded; //是否已经加载完成，此时依赖包可能并未加载完成
            public bool IsLoadCompleted; //是否已经完全加载完成
            public int LoadCount; //包内对象加载的次数
            public int DependencyCount; //被其他包依赖加载的次数

            public Action<string, AssetBundle> OnLoadCompleted; //完全加载完成事件
            public Action<string> OnLoaded; //加载完成事件，此时依赖包可能尚未加载
            public Action<string, float> OnLoading; //加载中进度事件，会使用一个0到1的进度调用此事件代表下载进度

            public AssetBundleContext(string bundleName, string[] dependencies)
            {
                this.BundlePath = bundleName;
                this.Dependencies = dependencies;
            }
        }
    }
}
