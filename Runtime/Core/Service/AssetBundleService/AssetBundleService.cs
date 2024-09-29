using NonsensicalKit.Core.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace NonsensicalKit.Core.Service.Asset
{
    [ServicePrefab("Services/AssetBundleService")]
    public class AssetBundleService : NonsensicalMono, IMonoService
    {
        [SerializeField][FormerlySerializedAs("m_baseUrl")] private string m_runtimeBaseUrl;  //基础url
        [SerializeField] private string m_editorBasePath = "..\\AssetBundles";
        [SerializeField] private int m_version = 10000;
        [SerializeField] private string[] m_preDownloadBundles;   //需要提前下载的ab包

        public bool IsReady { get; set; }

        public Action InitCompleted { get; set; }

        public Action<string, float> OnBundleLoading { get; set; }
        public Action<string> OnBundleCompleted { get; set; }
        public Action<string, string, float> OnResourceLoading { get; set; }
        public Action<string, string> OnResourceCompleted { get; set; }

        public bool IsLoading => LoadBundleCount > 0;       //当前是否正在加载包

        public int LoadBundleCount { get; private set; }    //当前加载的包的数量

        private string _rootUrl;    //基础url拼接平台字符串得出的根路径

        private Dictionary<string, AssetBundleContext> _assstBundleDic = new Dictionary<string, AssetBundleContext>();      //key是包名，value是ab包上下文信息

        protected void Awake()
        {
#if UNITY_EDITOR
            //读取当前版本并赋值到预制体中
            int version = UnityEditor.EditorPrefs.GetInt("assetBundle_Version", 10000);
            if (m_version < version)
            {
                m_version = version;
                var service = Resources.Load<AssetBundleService>("Services/AssetBundleService");
                service.m_version = version;
                UnityEditor.EditorUtility.SetDirty(service);
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(service.gameObject);
                UnityEditor.PrefabUtility.SavePrefabAsset(service.gameObject);
            }
            else if (m_version > version)
            {
                UnityEditor.EditorPrefs.SetInt("assetBundle_Version", m_version);
            }
#endif

            StartCoroutine(InitCor());
        }

        /// <summary>
        /// 初始化管理类
        /// </summary>
        private IEnumerator InitCor()
        {
            if (PlatformInfo.IsEditor)
            {
                _rootUrl = Path.Combine( Application.dataPath, m_editorBasePath, PlatformInfo.GetPlaformFolderName());
            }
            else
            {
                _rootUrl = Path.Combine(m_runtimeBaseUrl, PlatformInfo.GetPlaformFolderName());
            }

            var assetBundleCreateRequest = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(_rootUrl, PlatformInfo.GetPlaformFolderName()));
            yield return assetBundleCreateRequest.SendWebRequest();
            AssetBundle assetBundle = (assetBundleCreateRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle;

            if (assetBundle == null)
            {
                LogCore.Warning("未找到AssetBundles:" + _rootUrl);
                yield break;
            }

            var manifestRequest = assetBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
            yield return manifestRequest;
            AssetBundleManifest assetBundleManifest = manifestRequest.asset as AssetBundleManifest;

            string[] bundles = assetBundleManifest.GetAllAssetBundles();

            foreach (var item in bundles)
            {
                _assstBundleDic.Add(item, new AssetBundleContext(item, assetBundleManifest.GetDirectDependencies(item)));
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
            for (int i = 0; i < m_preDownloadBundles.Length; i++)
            {
                //当上一次加载还未完成或者有其他加载正在进行则延迟下一次加载
                while (LoadBundleCount > 0)
                {
                    yield return null;
                }
                var _crtPreDownloadBundleName = m_preDownloadBundles[i];
                LogCore.Info("开始提前下载包：" + _crtPreDownloadBundleName);
                LoadAssetBundle(_crtPreDownloadBundleName, null, null);
                while (LoadBundleCount > 0)
                {
                    yield return null;
                }
                UnloadUnusedBundle(_crtPreDownloadBundleName);
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
            if (_assstBundleDic.ContainsKey(bundlePath) == false)
            {
                LogCore.Warning($"错误的包名：{bundlePath}");
                return false;
            }

            return _assstBundleDic[bundlePath].IsLoadCompleted;
        }

        /// <summary>
        /// 加载AB包
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="onLoading"></param>
        public void LoadAssetBundle(string bundlePath, Action<string> onComplete = null, Action<string, float> onLoading = null, Action<string> onLoaded = null)
        {
            bundlePath=bundlePath.ToLower();
            LogCore.Info("加载ab包：" + bundlePath);
            if (_assstBundleDic.ContainsKey(bundlePath) == false)
            {
                LogCore.Warning($"错误的包名：{bundlePath}");
                return;
            }

            if (_assstBundleDic[bundlePath].IsLoadCompleted)
            {
                try
                {
                    onLoading?.Invoke(bundlePath, 1);
                    onLoaded?.Invoke(bundlePath);
                    onComplete?.Invoke(bundlePath);
                }
                catch (Exception)
                {
                }
                return;
            }

            if (_assstBundleDic[bundlePath].IsLoading == false)
            {
                _assstBundleDic[bundlePath].OnLoadCompleted = onComplete;
                _assstBundleDic[bundlePath].OnLoading = onLoading;
                _assstBundleDic[bundlePath].OnLoaded = onLoaded;
                StartCoroutine(LoadAssetBundleCoroutine(bundlePath));
            }
            else
            {
                _assstBundleDic[bundlePath].OnLoadCompleted += onComplete;
                _assstBundleDic[bundlePath].OnLoading += onLoading;
                _assstBundleDic[bundlePath].OnLoaded += onLoaded;
            }
        }

        /// <summary>
        /// 加载Ab包协程
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="onLoading"></param>
        /// <returns></returns>
        private IEnumerator LoadAssetBundleCoroutine(string bundlePath)
        {
            LoadBundleCount++;
            _assstBundleDic[bundlePath].IsLoading = true;

            string bundleUrl = Path.Combine(_rootUrl, bundlePath);

            var request = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, (uint)m_version, 0);

            request.SendWebRequest();
            float crtProcess;
            do
            {
                yield return null;
                crtProcess = request.downloadProgress;
                _assstBundleDic[bundlePath].OnLoading?.Invoke(bundlePath, crtProcess);
                OnBundleLoading?.Invoke(bundlePath, crtProcess);
            }
            while (!request.isDone);

            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);

            if (assetBundle == null)
            {
                LogCore.Error($"AB包加载失败，路径：{bundleUrl}");
                //Todo:错误处理
                yield break;
            }

            try
            {
                _assstBundleDic[bundlePath].OnLoaded?.Invoke(bundlePath);
            }
            catch (Exception) { }

            _assstBundleDic[bundlePath].OnLoaded = null;
            _assstBundleDic[bundlePath].IsLoaded = true;
            _assstBundleDic[bundlePath].AssetBundlePack = assetBundle;

            //加载依赖包
            //应当先加载自己再加载依赖包，防止出现互相依赖时互相等待的情况
            //但应当等待加载完成后再回调_onCompleted，保证使用时不出现引用丢失
            string[] dependencies = _assstBundleDic[bundlePath].Dependencies;
            int completeCount = 0;
            foreach (var item in dependencies)
            {
                _assstBundleDic[item].DependencieCount++;
                LoadAssetBundle(item, null, null, (p) => { completeCount++; });
            }
            while (completeCount < dependencies.Length)
            {
                yield return null;
            }

            _assstBundleDic[bundlePath].IsLoading = false;
            _assstBundleDic[bundlePath].OnLoading = null;

            _assstBundleDic[bundlePath].IsLoadCompleted = true;

            try
            {
                _assstBundleDic[bundlePath].OnLoadCompleted?.Invoke(bundlePath);
                OnBundleCompleted?.Invoke(bundlePath);
            }
            catch (Exception) { }

            _assstBundleDic[bundlePath].OnLoadCompleted = null;

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
        public void LoadResource<T>(string resourceName, string bundlePath, Action<string, string, T> onCompleted, Action<string, string, float> onLoading = null) where T : UnityEngine.Object
        {
            bundlePath = bundlePath.ToLower();

            if (_assstBundleDic.ContainsKey(bundlePath) == false)
            {
                LogCore.Warning($"错误的包名{bundlePath}");
                return;
            }

            if (_assstBundleDic[bundlePath].IsLoadCompleted)
            {
                _assstBundleDic[bundlePath].LoadCount++;
                StartCoroutine(LoadResourceCoroutine<T>(resourceName, bundlePath, _assstBundleDic[bundlePath].AssetBundlePack, onCompleted, onLoading));
            }
            else
            {
                StartCoroutine(WaitAssetBundleLoad<T>(resourceName, bundlePath, onCompleted, onLoading));
                LoadAssetBundle(bundlePath, null);
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
        private IEnumerator WaitAssetBundleLoad<T>(string resourceName, string bundlePath, Action<string, string, T> onCompleted, Action<string, string, float> onLoading) where T : UnityEngine.Object
        {
            while (_assstBundleDic[bundlePath].IsLoadCompleted == false)
            {
                yield return null;
            }
            _assstBundleDic[bundlePath].LoadCount++;
            StartCoroutine(LoadResourceCoroutine<T>(resourceName, bundlePath, _assstBundleDic[bundlePath].AssetBundlePack, onCompleted, onLoading));
        }

        /// <summary>
        /// 加载资源协程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        private IEnumerator LoadResourceCoroutine<T>(string resourceName, string bundlePath, AssetBundle assetBundle, Action<string, string, T> onCompleted, Action<string, string, float> onLoading) where T : UnityEngine.Object
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

                } while (!assetBundleRequest.isDone);
            }

            if (assetBundleRequest.asset != null)
            {
                T Object = assetBundleRequest.asset as T;
                onCompleted(bundlePath, resourceName, Object);
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
            _assstBundleDic[bundleName].LoadCount--;
        }

        /// <summary>
        /// 如果未被使用则卸载ab包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="unloadAllObjects"></param>
        public void UnloadUnusedBundle(string bundleName)
        {
            bundleName = bundleName.ToLower();
            if (_assstBundleDic[bundleName].LoadCount == 0 && _assstBundleDic[bundleName].DependencieCount == 0)
            {
                UnloadBundle(bundleName);
            }
        }

        /// <summary>
        /// 强制卸载ab包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="unloadAllObjects"></param>
        public void UnloadBundle(string bundleName, bool checkDependencie = true)
        {
            bundleName = bundleName.ToLower();
            if (_assstBundleDic.ContainsKey(bundleName))
            {
                var bundle = _assstBundleDic[bundleName];
                if (bundle.AssetBundlePack != null)
                {
                    bundle.AssetBundlePack.Unload(true);
                    bundle.AssetBundlePack = null;
                    if (checkDependencie)
                    {
                        string[] dependencies = _assstBundleDic[bundleName].Dependencies;
                        foreach (var item in dependencies)
                        {
                            _assstBundleDic[item].DependencieCount--;
                            UnloadUnusedBundle(item);
                        }
                    }
                    else
                    {
                        bundle.DependencieCount = 0;
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
            foreach (var item in _assstBundleDic)
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
            foreach (var item in _assstBundleDic)
            {
                UnloadBundle(item.Key, false);
            }
        }

        private class AssetBundleContext
        {
            public string BundlePath;           //包的名称
            public string[] Dependencies;       //直接依赖的包名称

            public AssetBundle AssetBundlePack; //ab包

            public bool IsLoading;              //是否正在进行加载
            public bool IsLoaded;               //是否已经加载完成
            public bool IsLoadCompleted;        //是否已经完全加载完成
            public int LoadCount = 0;           //包内对象加载的次数
            public int DependencieCount = 0;    //被其他包依赖加载的次数

            public Action<string> OnLoadCompleted;       //完全加载完成事件
            public Action<string> OnLoaded;             //加载完成事件，此时依赖包可能尚未加载
            public Action<string, float> OnLoading;     //加载中进度事件，会使用一个0到1的进度调用此事件代表下载进度

            public AssetBundleContext(string bundleName, string[] dependencies)
            {
                this.BundlePath = bundleName;
                this.Dependencies = dependencies;
            }
        }
    }
}
