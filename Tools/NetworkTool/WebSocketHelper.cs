using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using NonsensicalKit.Core.Log;
using UnityEngine;

namespace NonsensicalKit.Tools.NetworkTool
{
    public class WebSocketHelper : MonoBehaviour
    {
        /// <summary>
        /// 发送消息回调Action
        /// </summary>
        public Action<bool> SendMessageCallback { get; set; }

        /// <summary>
        /// 接受消息Action
        /// </summary>
        public Action<string> ReceiveMessage { get; set; }

        private WebSocketWrap _ws; //websocket实例
        private bool _reConnect; //线程重连用
        private bool _isQuitting; //是否正在退出应用，用于防止在退出时尝试实例化新物体

        public float ReconnectionInterval { get; set; } = 2f; //重连间隔

        public float HeartbeatInterval { get; set; } = -1; //心跳间隔

        public string HeartbeatMessage { get; set; } = string.Empty; //心跳消息

        private string _uri;

        private   Coroutine _heartbeatCoroutine;

        protected void Update()
        {
            if (_reConnect)
            {
                StartCoroutine(ReConnect());
                _reConnect = false;
            }
        }

        private void OnDestroy()
        {
            _isQuitting = true;

            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
            }

            if (_ws != null)
            {
                _ws.Abort();
                _reConnect = false;
            }
        }

        /// <summary>
        /// 登出时执行
        /// </summary>
        public void Abort()
        {
            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
            }

            if (_ws != null)
            {
                _ws.Abort();
                _reConnect = false;
            }
        }

        /// <summary>
        /// 初始化websocket并连接
        /// </summary>
        public void Connect(string uri, Dictionary<string, string> headers = null)
        {
            this._uri = uri;
            _ws = new WebSocketWrap();

            _ws.OnConnectError += () => { Debug.LogError($"Websocket发生错误"); };

            _ws.OnMessage += OnMessage;

            _ws.OnWebSocketState += OnWebSocketState;
            _ws.Connect(uri, headers);
            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
            }

            _heartbeatCoroutine = StartCoroutine(StartHeartbeat());
        }

        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            _ws.SendMessage(msg);
        }

        /// <summary>
        /// 重连协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReConnect()
        {
            yield return new WaitForSeconds(ReconnectionInterval);
            Connect(_uri);
        }

        /// <summary>
        /// 开启心跳协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartHeartbeat()
        {
            if (HeartbeatInterval<=0)
            {
                yield break;
            }
            
            while (_ws.IsConnected == false)
            {
                yield return new WaitForSeconds(0.5f);
            }

            while (true)
            {
                yield return new WaitForSeconds(HeartbeatInterval);
                _ws.SendMessage(string.Empty);
            }
        }


        private void OnMessage(string msg)
        {
            Debug.Log("收到消息：" + msg);
            ReceiveMessage?.Invoke(msg);
        }

        private void OnWebSocketState(WebSocketState state)
        {
            Debug.Log("Websocket状态改变：" + state.ToString());
            switch (state)
            {
                case WebSocketState.Aborted:
                    break;
                case WebSocketState.Closed:
                    if (!_isQuitting)
                    {
                        _reConnect = true;
                    }
                    break;
                case WebSocketState.CloseReceived:
                    break;
                case WebSocketState.CloseSent:
                    break;
                case WebSocketState.Connecting:
                    break;
                case WebSocketState.None:
                    break;
                case WebSocketState.Open:
                    break;
            }
        }
    }

    /// <summary>
    /// 封装WebSocket
    /// </summary>
    public class WebSocketWrap
    {
        //连接状态改变
        public Action<WebSocketState> OnWebSocketState { get; set; }

        //连接失败
        public Action OnConnectError { get; set; }

        //收到消息
        public Action<string> OnMessage { get; set; }

        public bool IsConnected => _ws.State == WebSocketState.Open;

        private ClientWebSocket _ws;

        private CancellationTokenSource _ct;

        public async void Connect(string uri, Dictionary<string, string> headers = null)
        {
            try
            {
                _ws = new ClientWebSocket();
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        _ws.Options.SetRequestHeader(header.Key, header.Value);
                    }
                }

                _ct = new CancellationTokenSource();
                Uri url = new Uri(uri);
                await _ws.ConnectAsync(url, _ct.Token);

                if (OnWebSocketState != null)
                    OnWebSocketState(_ws.State);

                while (true)
                {
                    var result = new byte[1024];
                    await _ws.ReceiveAsync(new ArraySegment<byte>(result), _ct.Token); //接受数据
                    var str = Encoding.UTF8.GetString(result, 0, result.Length);
                    str = str.Replace("\0", ""); //去掉尾部空字符

                    OnMessage?.Invoke(str);
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[WebSocket] {0}\nCloseStatus: {1}\nCloseStatusDescription: {2}", ex.Message, _ws.CloseStatus,
                    _ws.CloseStatusDescription);
                if (OnConnectError != null)
                    OnConnectError();
            }
            finally
            {
                if (_ws != null)
                    _ws.Dispose();
                _ws = null;
            }
        }

        // 发送数据
        public void SendMessage(string msg)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                LogCore.Warning("[WebSocket] 发送数据失败，未连接上websocket服务端。");
                return;
            }

            _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, _ct.Token);
        }

        //中止WebSocket连接并取消任何挂起的IO操作
        public void Abort()
        {
            if (_ws == null)
                return;
            _ct.Cancel();
            _ws.Abort();
        }
    }
}
