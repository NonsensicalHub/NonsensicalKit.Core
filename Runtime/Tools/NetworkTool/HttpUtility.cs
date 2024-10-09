using NonsensicalKit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace NonsensicalKit.Tools.NetworkTool
{
    /// <summary>
    /// http请求工具类
    /// </summary>
    public static class HttpUtility
    {
        #region Extensions
        public static IEnumerator Get(this UnityWebRequest unityWebRequest, string url)
        {
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWebRequest.SendWebRequest();
        }

        public static IEnumerator GetWithArgs(this UnityWebRequest unityWebRequest, string url, Dictionary<string, string> fields)
        {
            unityWebRequest.method = "GET";
            unityWebRequest.url = GetArgsStr(url, fields);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWebRequest.SendWebRequest();
        }

        public static IEnumerator GetTexture(this UnityWebRequest unityWebRequest, string url)
        {
            unityWebRequest.method = "GET";
            unityWebRequest.url = url;
            unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
            yield return unityWebRequest.SendWebRequest();
        }

        public static IEnumerator Post(this UnityWebRequest unityWebRequest, string url, Dictionary<string, string> formData, Dictionary<string, string> header)
        {
            unityWebRequest.method = "Post";
            unityWebRequest.url = url;
            IncreaseHeader(unityWebRequest, header);
            var form = CreateForm(formData);
            byte[] array = null;
            array = form.data;
            if (array.Length == 0)
            {
                array = null;
            }

            if (array != null)
            {
                unityWebRequest.uploadHandler = new UploadHandlerRaw(array);
            }
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWebRequest.SendWebRequest();
        }
        public static IEnumerator Post(this UnityWebRequest unityWebRequest, string url, string json)
        {
            unityWebRequest.method = "Post";
            unityWebRequest.url = url;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWebRequest.SendWebRequest();
        }
        #endregion

        #region GET 
        public static IEnumerator Get(string url, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator Get(string url, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetNoSSL(string url, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                unityWebRequest.certificateHandler = new BypassCertificate();
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetWithArgs(string url, Dictionary<string, string> fields, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            string uri = GetArgsStr(url, fields);
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(uri))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetWithArgs(string url, Dictionary<string, string> fields, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            string uri = GetArgsStr(url, fields);
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(uri))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetPicture(string url, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerTexture(true);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetWav(string url, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetAudio(string url, AudioType audioType, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerAudioClip(url, audioType);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator GetAudio(string url, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            AudioType audioType;
            switch (StringTool.GetFileExtensionFromUrl(url))
            {
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                default:
                    audioType = AudioType.MPEG;
                    break;
            }
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url))
            {
                unityWebRequest.downloadHandler = new DownloadHandlerAudioClip(url, audioType);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }
        #endregion

        #region Post
        public static IEnumerator Post(string url, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST"))
            {
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator Post(string url, Dictionary<string, string> header, Action<UnityWebRequest> callback, Action<float> uploadProcessCallback, Action<float> downloadProcessCallback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST"))
            {
                IncreaseHeader(unityWebRequest, header);
                if (uploadProcessCallback == null && downloadProcessCallback == null)
                {
                    yield return SendRequest(unityWebRequest, callback, iHandleWebError);
                }
                else
                {
                    NonsensicalInstance.Instance.StartCoroutine(SendRequest(unityWebRequest, callback, iHandleWebError));
                    while (!unityWebRequest.isDone)
                    {
                        uploadProcessCallback?.Invoke(unityWebRequest.uploadProgress);
                        downloadProcessCallback?.Invoke(unityWebRequest.downloadProgress);
                        yield return null;
                    }
                    yield return null;
                }
            }
        }

        public static IEnumerator Post(string url, Dictionary<string, string> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            yield return Post(url, CreateForm(formData), header, callback, iHandleWebError);
        }

        public static IEnumerator Post(string url, string json, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                unityWebRequest.SetRequestHeader("Content-Type", "application/json");
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator Post(string url, WWWForm form, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, form))
            {
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator Post(string url, WWWForm form, Dictionary<string, string> header, Action<UnityWebRequest> callback, Action<float> uploadProcessCallback, Action<float> downloadProcessCallback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, form))
            {
                unityWebRequest.useHttpContinue = false;
                IncreaseHeader(unityWebRequest, header);
                if (uploadProcessCallback == null && downloadProcessCallback == null)
                {
                    yield return SendRequest(unityWebRequest, callback, iHandleWebError);
                }
                else
                {
                    NonsensicalInstance.Instance.StartCoroutine(SendRequest(unityWebRequest, callback, iHandleWebError));
                    while (!unityWebRequest.isDone)
                    {
                        uploadProcessCallback?.Invoke(unityWebRequest.uploadProgress);
                        downloadProcessCallback?.Invoke(unityWebRequest.downloadProgress);
                        yield return null;
                    }
                    yield return null;
                }
            }
        }

        public static IEnumerator Post(string url, List<IMultipartFormSection> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback, Action<float> uploadProcessCallback, Action<float> downloadProcessCallback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, formData))
            {
                unityWebRequest.useHttpContinue = false;
                IncreaseHeader(unityWebRequest, header);
                if (uploadProcessCallback == null && downloadProcessCallback == null)
                {
                    yield return SendRequest(unityWebRequest, callback, iHandleWebError);
                }
                else
                {
                    NonsensicalInstance.Instance.StartCoroutine(SendRequest(unityWebRequest, callback, iHandleWebError));
                    while (!unityWebRequest.isDone)
                    {
                        uploadProcessCallback?.Invoke(unityWebRequest.uploadProgress);
                        downloadProcessCallback?.Invoke(unityWebRequest.downloadProgress);
                        yield return null;
                    }
                    yield return null;
                }
            }
        }

        public static IEnumerator Post(string url, List<IMultipartFormSection> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, formData))
            {
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator PostWithArgs(string url, Dictionary<string, string> fields, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            string uri = GetArgsStr(url, fields);
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(uri, new WWWForm()))
            {
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator PostWithArgs(string url, Dictionary<string, string> fields, Action<UnityWebRequest> callback, Action<float> uploadProcessCallback, Action<float> downloadProcessCallback, IHandleWebError iHandleWebError = null)
        {
            string uri = GetArgsStr(url, fields);
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(uri, new WWWForm()))
            {
                if (uploadProcessCallback == null && downloadProcessCallback == null)
                {
                    yield return SendRequest(unityWebRequest, callback, iHandleWebError);
                }
                else
                {
                    NonsensicalInstance.Instance.StartCoroutine(SendRequest(unityWebRequest, callback, iHandleWebError));
                    while (!unityWebRequest.isDone)
                    {
                        uploadProcessCallback?.Invoke(unityWebRequest.uploadProgress);
                        downloadProcessCallback?.Invoke(unityWebRequest.downloadProgress);
                        yield return null;
                    }
                    yield return null;
                }
            }
        }

        public static IEnumerator PostTexture(string url, Dictionary<string, string> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, CreateForm(formData)))
            {
                DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(true);
                unityWebRequest.downloadHandler = downloadTexture;
                IncreaseHeader(unityWebRequest, header);
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static IEnumerator UploadFiles(string url, List<string> names, List<string> urls, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            for (int i = 0; i < urls.Count; i++)
            {
                using (UnityWebRequest crtFileRequest = UnityWebRequest.Get(urls[i]))
                {
                    yield return crtFileRequest.SendWebRequest();

                    formData.Add(new MultipartFormFileSection(Path.GetFileName(names[i]), crtFileRequest.downloadHandler.data));
                }
            }

            yield return Post(url, formData, header, callback, iHandleWebError);
        }

        public static IEnumerator UploadLocalFiles(string url, List<string> fileFullNames, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

            foreach (var item in fileFullNames)
            {
                using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        byte[] buffur = new byte[fs.Length];
                        fs.Read(buffur, 0, (int)fs.Length);
                        formData.Add(new MultipartFormFileSection(Path.GetFileName(item), buffur));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            yield return Post(url, formData, header, callback, iHandleWebError);
        }

        public static IEnumerator UploadBinaryFile(string url, string fieldName, byte[] fileByte, string fileName, string contentType, Dictionary<string, string> header, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData(fieldName, fileByte, fileName, contentType);

            using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, form))
            {
                unityWebRequest.useHttpContinue = false;
                IncreaseHeader(unityWebRequest, header);

                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }
        #endregion

        #region Special
        public static IEnumerator LoadAssetbundle(string uri, uint version, uint crc, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri, version, crc))
            {
                yield return SendRequest(unityWebRequest, callback, iHandleWebError);
            }
        }

        public static Dictionary<string, string> GetJsonHeader()
        {
            Dictionary<string, string> jsonHeader = new Dictionary<string, string>();
            jsonHeader.Add("Content-Type", "application/json;charset=utf-8");
            return jsonHeader;
        }
        #endregion

        #region PrivateMethod
        private static WWWForm CreateForm(Dictionary<string, string> formData)
        {
            WWWForm form = new WWWForm();
            if (formData != null)
            {
                foreach (var item in formData)
                {
                    form.AddField(item.Key, item.Value);
                }
            }
            return form;
        }

        private static string GetArgsStr(string baseUrl, Dictionary<string, string> fields)
        {
            if (fields != null && fields.Count > 0)
            {
                StringBuilder sb = new StringBuilder(baseUrl);
                sb.Append("?");
                foreach (var item in fields)
                {
                    sb.Append(item.Key);
                    sb.Append("=");
                    sb.Append(item.Value);
                    sb.Append("&");
                }
                sb.Remove(sb.Length - 1, 1);   //去掉结尾的&
                return sb.ToString();
            }
            else
            {
                return baseUrl;
            }
        }

        private static void IncreaseHeader(UnityWebRequest unityWebRequest, Dictionary<string, string> headerData)
        {
            if (headerData != null)
            {
                foreach (var tmp in headerData)
                {
                    unityWebRequest.SetRequestHeader(tmp.Key, tmp.Value);
                }
            }
        }

        private static IEnumerator SendRequest(UnityWebRequest unityWebRequest, Action<UnityWebRequest> callback, IHandleWebError iHandleWebError = null)
        {
            yield return unityWebRequest.SendWebRequest();

            if (iHandleWebError == null)
            {
                callback?.Invoke(unityWebRequest);
            }
            else
            {
                switch (unityWebRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                        callback?.Invoke(unityWebRequest);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        iHandleWebError?.OnProtocolError(unityWebRequest);
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                        iHandleWebError?.OnConnectionError(unityWebRequest);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        iHandleWebError?.OnDataProcessingError(unityWebRequest);
                        break;
                    default:
                        iHandleWebError?.OnUnknowError(unityWebRequest);
                        break;
                }
            }
        }
        #endregion
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
