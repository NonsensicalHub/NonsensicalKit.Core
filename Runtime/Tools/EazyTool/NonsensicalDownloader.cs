using NonsensicalKit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace NonsensicalKit.Tools.EazyTool
{
    public class NonsensicalDownloader : NonsensicalMono
    {
        [SerializeField][Tooltip("最大同时下载数")] private int m_downloadSameTime = 10;

        private int _count;

        private Queue<IEnumerator> _buffer = new Queue<IEnumerator>();

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

        public void Get(IEnumerator enumerator)
        {
            _buffer.Enqueue(enumerator);
            CheckOut();
        }

        private IEnumerator GetTMD(IEnumerator enumerator)
        {
            _count++;
            yield return enumerator;
            _count--;
            CheckOut();
        }

        private void CheckOut()
        {
            if (_count < m_downloadSameTime && _buffer.Count > 0)
            {
                var enumerator = _buffer.Dequeue();
                StartCoroutine(GetTMD(enumerator));
            }
        }
    }
    public class Texture2DDownloader : DownloaderBase<Texture2DDownloader, Texture2D>
    {
        protected override string FolderPath => _folderPath;

        private string _folderPath = "Sprites";

        protected override UnityWebRequest CreateUnityWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
            return unityWebRequest;
        }

        protected override IEnumerator ParsingData(UnityWebRequest request, Context<Texture2D> context)
        {
            yield return null;
            var texture2D = DownloadHandlerTexture.GetContent(request);

            if (texture2D != null)
            {
                Texture2D newTex = new Texture2D(texture2D.width, texture2D.height);
                newTex.SetPixels(texture2D.GetPixels());
                newTex.Apply(true);
                GameObject.Destroy(texture2D);
                context.Resource = newTex;
            }
            else
            {
                context.Resource = null;
            }
        }

        public override void Clear()
        {
            foreach (var item in Resources)
            {
                UnityEngine.Object.Destroy(item.Value.Resource);
            }
            Resources.Clear();
        }
    }

    public class SpriteDownloader : DownloaderBase<SpriteDownloader, Sprite>
    {
        protected override string FolderPath => _folderPath;

        private string _folderPath = "Sprites";

        protected override UnityWebRequest CreateUnityWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
            return unityWebRequest;
        }

        protected override IEnumerator ParsingData(UnityWebRequest request, Context<Sprite> context)
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

        public override void Clear()
        {
            foreach (var item in Resources)
            {
                UnityEngine.Object.Destroy(item.Value.Resource);
            }
            Resources.Clear();
        }
    }

    public class AudioDownloader : DownloaderBase<AudioDownloader, AudioClip>
    {
        protected override string FolderPath => _folderPath;

        private string _folderPath = "Audio";

        protected override UnityWebRequest CreateUnityWebRequest(string url)
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

        protected override IEnumerator ParsingData(UnityWebRequest request, Context<AudioClip> context)
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

        public override void Clear()
        {
            foreach (var item in Resources)
            {
                UnityEngine.Object.Destroy(item.Value.Resource);
            }
            Resources.Clear();
        }
    }

    public abstract class DownloaderBase<Class, ResourceType> where Class : class, new()
    {
        protected Dictionary<string, Context<ResourceType>> Resources = new Dictionary<string, Context<ResourceType>>();
        protected abstract string FolderPath { get; }

        private string _basePath;
        private NonsensicalDownloader _downloader;

        public static Class Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Class();
                }
                return _instance;
            }
        }

        private static Class _instance;

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

        public void Get(string url, Action<ResourceType> callback)
        {
            if (Resources.ContainsKey(url))
            {
                if (Resources[url].Resource != null)
                {
                    callback(Resources[url].Resource);
                }
                else
                {
                    Resources[url].Callback += callback;
                }
            }
            else
            {
                var newContext = new Context<ResourceType>() { Url = url, Callback = callback };
                Resources.Add(url, newContext);
                _downloader.Get(Get(newContext));
            }
        }

        private IEnumerator Get(Context<ResourceType> context)
        {
            string path = Path.Combine(_basePath, MD5Tool.GetMd5Str(context.Url, false)+Path.GetExtension(context.Url));

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
            Debug.LogWarning("加载资源失败，" + context.Url);
        }

        private void ApplyCallback(Context<ResourceType> context)
        {
            context.Callback?.Invoke(context.Resource);
            context.Callback = null;
        }

        protected abstract UnityWebRequest CreateUnityWebRequest(string url);
        protected abstract IEnumerator ParsingData(UnityWebRequest request, Context<ResourceType> context);

        public abstract void Clear();
    }

    public class Context<ResourceType>
    {
        public string Url;
        public ResourceType Resource;
        public Action<ResourceType> Callback;
    }
}
