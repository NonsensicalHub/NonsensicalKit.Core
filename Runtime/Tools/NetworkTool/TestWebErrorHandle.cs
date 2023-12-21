using System;
using UnityEngine;
using UnityEngine.Networking;

namespace NonsensicalKit.Tools.NetworkTool
{
    /// <summary>
    /// 简单的网络错误处理
    /// </summary>
    public class TestWebErrorHandle : IHandleWebError
    {
        public void OnProtocolError(UnityWebRequest unityWebRequest)
        {
            Debug.Log("ProtocolError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
        }

        public void OnConnectionError(UnityWebRequest unityWebRequest)
        {
            if (Uri.IsWellFormedUriString(unityWebRequest.url, UriKind.Absolute) == false)
            {
                Debug.Log($"url:\"{unityWebRequest.url}\"格式不正确");
            }
            else
            {
                Debug.Log("ConnectionError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
            }
        }

        public void OnDataProcessingError(UnityWebRequest unityWebRequest)
        {
            Debug.Log("DataProcessingError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
        }

        public void OnUnknowError(UnityWebRequest unityWebRequest)
        {
            Debug.Log("连接出现未知错误");
        }
    }
}
