using UnityEngine.Networking;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// UnityWebRequest错误处理接口
    /// </summary>
    public interface IHandleWebError
    {
        public void OnProtocolError(UnityWebRequest unityWebRequest);
        public void OnConnectionError(UnityWebRequest unityWebRequest);
        public void OnUnknowError(UnityWebRequest unityWebRequest);
        public void OnDataProcessingError(UnityWebRequest unityWebRequest);
    }
}
