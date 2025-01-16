using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace NonsensicalKit.Tools.EasyTool
{
    /// <summary>
    /// 缓存和同时下载管理
    /// </summary>
    internal class NonsensicalDownloader : NonsensicalMono
    {
        [SerializeField] [Tooltip("最大同时下载数,小于等于0时代表无限制")]
        private int m_downloadSameTime = 10;

        private int _count; //数量

        private readonly Queue<IEnumerator> _waitBuffer = new Queue<IEnumerator>(); //等待队列

        private void Awake()
        {
            if (IOCC.Get<NonsensicalDownloader>() != null)
            {
                Destroy(this);
                return;
            }

            Register<NonsensicalDownloader>(GetDownloader);
        }

        private NonsensicalDownloader GetDownloader()
        {
            return this;
        }

        internal void TryGet(IEnumerator enumerator)
        {
            _waitBuffer.Enqueue(enumerator);
            CheckOut();
        }

        private IEnumerator GetNow(IEnumerator enumerator)
        {
            _count++;
            yield return enumerator;
            _count--;
            CheckOut();
        }

        private void CheckOut()
        {
            if (m_downloadSameTime < 0
                || (_count < m_downloadSameTime && _waitBuffer.Count > 0))
            {
                var enumerator = _waitBuffer.Dequeue();
                StartCoroutine(GetNow(enumerator));
            }
        }
    }

    public class DownloadContext<TResourceType>
    {
        public TResourceType Resource;
    }

    internal class Context<TResourceType>
    {
        public string Url;
        public bool Downloading;
        public TResourceType Resource;
        public Action<TResourceType> Callback;
    }

    /// <summary>
    /// 如果想要自定义文件类型的加载，继承此类并实现抽象属性和方法即可
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <typeparam name="TResourceType"></typeparam>
    public abstract class DownloaderBase<TClass, TResourceType> where TClass : class, new()
    {
        protected abstract string FolderPath { get; }

        internal readonly Dictionary<string, Context<TResourceType>> Resources = new Dictionary<string, Context<TResourceType>>();
        private readonly string _basePath;
        private readonly NonsensicalDownloader _downloader;

        public static TClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TClass();
                }

                return _instance;
            }
        }

        private static TClass _instance;

        public DownloaderBase()
        {
            _basePath = Path.Combine(Application.persistentDataPath, FolderPath);
            Directory.CreateDirectory(_basePath);
            if (IOCC.TryGet<NonsensicalDownloader>(out _downloader) == false)
            {
                GameObject go = new GameObject("NonsensicalDownloader");
                _downloader = go.AddComponent<NonsensicalDownloader>();
            }
        }

        public void Get(string url, Action<TResourceType> callback = null)
        {
            if (Resources.ContainsKey(url))
            {
                if (Resources[url].Resource != null)
                {
                    callback?.Invoke(Resources[url].Resource);
                }
                else
                {
                    Resources[url].Callback += callback;
                }
            }
            else
            {
                var newContext = new Context<TResourceType>() { Url = url, Callback = callback };
                Resources.Add(url, newContext);
                _downloader.TryGet(GetCor(newContext));
            }
        }

        public IEnumerator Get(string url, DownloadContext<TResourceType> context)
        {
            if (Resources.ContainsKey(url))
            {
                if (Resources[url].Resource != null)
                {
                    context.Resource = (Resources[url].Resource);
                    yield return Resources[url].Resource;
                }
            }
            else
            {
                var newContext = new Context<TResourceType>() { Url = url };
                Resources.Add(url, newContext);
                _downloader.TryGet(GetCor(newContext));
            }

            while (Resources[url].Downloading)
            {
                yield return null;
            }

            context.Resource = (Resources[url].Resource);
        }

        public void Clear()
        {
            DoClear();
            Resources.Clear();
        }

        private IEnumerator GetCor(Context<TResourceType> context)
        {
            context.Downloading = true;

            string path = Path.Combine(_basePath, MD5Tool.GetMd5Str(context.Url, false) + Path.GetExtension(context.Url));

            //WebGL构建中应勾选Data Catching,则unity会自行缓存数据
            if (!PlatformInfo.IsWebGL)
            {
                UnityWebRequest localRequest = CreateUnityWebRequest(path);

                yield return localRequest.SendWebRequest();
                if (localRequest.result == UnityWebRequest.Result.Success)
                {
                    yield return ParsingData(localRequest, context);
                    if (context.Resource != null)
                    {
                        ApplyCallback(context);
                        yield break;
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }
            }

            UnityWebRequest remoteRequest = CreateUnityWebRequest(context.Url);

            yield return remoteRequest.SendWebRequest();

            if (remoteRequest.result == UnityWebRequest.Result.Success)
            {
                yield return ParsingData(remoteRequest, context);
                if (context.Resource != null)
                {
                    if (!PlatformInfo.IsWebGL)
                    {
                        File.WriteAllBytes(path, remoteRequest.downloadHandler.data);
                    }

                    ApplyCallback(context);
                    yield break;
                }
            }

            context.Downloading = false;
            Debug.LogWarning("加载资源失败，" + context.Url);
        }

        private void ApplyCallback(Context<TResourceType> context)
        {
            context.Downloading = false;
            context.Callback?.Invoke(context.Resource);
            context.Callback = null;
        }

        /// <summary>
        /// 创建获取对应资源所需的UnityWebRequest
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal abstract UnityWebRequest CreateUnityWebRequest(string url);

        /// <summary>
        /// 读取UnityWebRequest中的有效内容，写入Resource字段
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal abstract IEnumerator ParsingData(UnityWebRequest request, Context<TResourceType> context);

        /// <summary>
        /// 清除缓存的
        /// </summary>
        internal abstract void DoClear();
    }

    public class Texture2DDownloader : DownloaderBase<Texture2DDownloader, Texture2D>
    {
        protected override string FolderPath => "Sprites";

        internal override UnityWebRequest CreateUnityWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
            return unityWebRequest;
        }

        internal override IEnumerator ParsingData(UnityWebRequest request, Context<Texture2D> context)
        {
            yield return null;
            var texture2D = DownloadHandlerTexture.GetContent(request);

            if (texture2D != null)
            {
                Texture2D newTex = new Texture2D(texture2D.width, texture2D.height);
                newTex.SetPixels(texture2D.GetPixels());
                newTex.Apply(true);
                Object.Destroy(texture2D);
                context.Resource = newTex;
            }
            else
            {
                context.Resource = null;
            }
        }

        internal override void DoClear()
        {
            foreach (var item in Resources)
            {
                Object.Destroy(item.Value.Resource);
            }
        }
    }

    public class SpriteDownloader : DownloaderBase<SpriteDownloader, Sprite>
    {
        protected override string FolderPath => "Sprites";

        internal override UnityWebRequest CreateUnityWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
            return unityWebRequest;
        }

        internal override IEnumerator ParsingData(UnityWebRequest request, Context<Sprite> context)
        {
            yield return null;
            var texture2D = DownloadHandlerTexture.GetContent(request);
            if (texture2D != null)
            {
                context.Resource = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
            }
            else
            {
                context.Resource = null;
            }
        }

        internal override void DoClear()
        {
            foreach (var item in Resources)
            {
                Object.Destroy(item.Value.Resource);
            }
        }
    }

    public class AudioDownloader : DownloaderBase<AudioDownloader, AudioClip>
    {
        protected override string FolderPath => "Audio";

        internal override UnityWebRequest CreateUnityWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            DownloadHandler dh;
            var extension = Path.GetExtension(url).ToLower();
            switch (extension)
            {
                case ".ogg":
                {
                    var dhau = new DownloadHandlerAudioClip(url, AudioType.OGGVORBIS);
                    dhau.compressed = true;
                    dh = dhau;
                }
                    break;
                case ".wav":
                {
                    var dhau = new DownloadHandlerAudioClip(url, AudioType.WAV);
                    dhau.compressed = true;
                    dh = dhau;
                }
                    break;
                case ".mp2":
                case ".mp3":
                default:
                {
                    var dhau = new DownloadHandlerAudioClip(url, AudioType.MPEG);
                    dhau.compressed = true;
                    dh = dhau;
                }
                    break;
            }

            unityWebRequest.downloadHandler = dh;
            return unityWebRequest;
        }

        internal override IEnumerator ParsingData(UnityWebRequest request, Context<AudioClip> context)
        {
            yield return null;

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null)
            {
                context.Resource = clip;
            }
            else
            {
                context.Resource = null;
            }
        }

        internal override void DoClear()
        {
            foreach (var item in Resources)
            {
                Object.Destroy(item.Value.Resource);
            }
        }
    }
}
