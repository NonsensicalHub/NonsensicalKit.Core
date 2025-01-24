using System;
using NonsensicalKit.Core.Log;
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
            LogCore.Error("ProtocolError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
        }

        public void OnConnectionError(UnityWebRequest unityWebRequest)
        {
            if (Uri.IsWellFormedUriString(unityWebRequest.url, UriKind.Absolute) == false)
            {
                LogCore.Error($"url:\"{unityWebRequest.url}\"格式不正确");
            }
            else
            {
                LogCore.Error("ConnectionError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
            }
        }

        public void OnDataProcessingError(UnityWebRequest unityWebRequest)
        {
            LogCore.Error("DataProcessingError:" + unityWebRequest.downloadHandler.error + "\r\n" + unityWebRequest.downloadHandler.text);
        }

        public void OnUnknowError(UnityWebRequest unityWebRequest)
        {
            LogCore.Error("连接出现未知错误");
        }
    }
}
