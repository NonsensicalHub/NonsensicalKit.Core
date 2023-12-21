using NonsensicalKit.Editor;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace NonsensicalKit.Tools.NetworkTool
{
    /// <summary>
    /// 对HttpHelper进行封装，把协程操作转换成直接方法调用，以及对回调进行初步转换
    /// </summary>
    public class HttpManager : MonoSingleton<HttpManager>
    {
        public IHandleWebError HttpErrorProcess { get; set; }

        #region GET
        public void Get(string url, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            StartCoroutine(HttpUtility.Get(url, header, callback, HttpErrorProcess));
        }

        public void GetWithArgs(string url, Dictionary<string, string> bodys, Action<UnityWebRequest> callback)
        {
            StartCoroutine(HttpUtility.GetWithArgs(url, bodys, callback));
        }
        #endregion

        #region Post
        public void Post(string url, string json, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            StartCoroutine(HttpUtility.Post(url, json, header, callback, HttpErrorProcess));
        }

        public void Post(string url, Dictionary<string, string> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            StartCoroutine(HttpUtility.Post(url, formData, header, callback, HttpErrorProcess));
        }

        public void Post(string url, List<IMultipartFormSection> formData, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            StartCoroutine(HttpUtility.Post(url, formData, header, callback, HttpErrorProcess));
        }

        public void UploadPng(string url, byte[] imageByte, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

            formData.Add(new MultipartFormFileSection("preview", imageByte, "preview", "image/png"));

            StartCoroutine(HttpUtility.Post(url, formData, header, callback, HttpErrorProcess));
        }

        public void UploadPngWithDatas(string url, byte[] imageByte, Dictionary<string, string> datas, Dictionary<string, string> header, Action<UnityWebRequest> callback)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

            formData.Add(new MultipartFormFileSection("preview", imageByte, "preview", "image/png"));

            foreach (var item in datas)
            {
                formData.Add(new MultipartFormDataSection(item.Key, item.Value));
            }

            StartCoroutine(HttpUtility.Post(url, formData, header, callback, HttpErrorProcess));
        }
        #endregion
    }
}
