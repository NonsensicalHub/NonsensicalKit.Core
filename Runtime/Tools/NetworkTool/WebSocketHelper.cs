using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.NetworkTool
{
    public class WebSocketHelper : MonoSingleton<WebSocketHelper>
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

        private Thread _heartbeatThread; //心跳线程

        private float _reconnectionInterval; //重连间隔
        private float _heartbeatInterval; //心跳间隔

        private string _uri;

        protected override void Awake()
        {
            base.Awake();
            _reconnectionInterval = 2f;
            _heartbeatInterval = 29f;
        }

        protected void Update()
        {
            if (_reConnect)
            {
                StartCoroutine(ReConnect());
                _reConnect = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _isQuitting = true;

            if (_heartbeatThread != null)
            {
                _heartbeatThread.Abort();
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
            if (_heartbeatThread != null)
            {
                _heartbeatThread.Abort();
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
        public void WebsocketConnect(string uri)
        {
            this._uri = uri;
            _ws = new WebSocketWrap();

            _ws.OnConnectError += () => { Debug.LogError($"Websocket发生错误"); };

            _ws.OnMessage += OnMessage;

            _ws.OnWebSocketState += OnWebSocketState;
            _ws.Connect(uri);
            if (_heartbeatThread != null)
            {
                _heartbeatThread.Abort();
            }

            StartCoroutine(StartHeartbeat());
        }

        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessageToServer(string msg)
        {
            _ws.Send(msg);
        }

        /// <summary>
        /// 重连协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReConnect()
        {
            yield return new WaitForSeconds(_reconnectionInterval);
            WebsocketConnect(_uri);
        }

        /// <summary>
        /// 开启心跳协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartHeartbeat()
        {
            while (_ws.IsConnected == false)
            {
                yield return new WaitForSeconds(0.5f);
            }

            _heartbeatThread = new Thread(Heartbeat);
            _heartbeatThread.Start();
        }

        /// <summary>
        /// 心跳线程方法
        /// </summary>
        private void Heartbeat()
        {
            while (_ws != null)
            {
                SendMessageToServer("");
                Thread.Sleep((int)(_heartbeatInterval * 1000));
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

        //数据返回
        public Action<string> OnMessage { get; set; }

        public bool IsConnected => _ws.State == WebSocketState.Open;

        private ClientWebSocket _ws;

        private CancellationToken _ct;

        public async void Connect(string uri)
        {
            try
            {
                Debug.LogFormat("[WebSocket] 开始连接 {0}", uri);
                _ws = new ClientWebSocket();
                _ct = new CancellationToken();
                Uri url = new Uri(uri);
                await _ws.ConnectAsync(url, _ct);

                if (OnWebSocketState != null)
                    OnWebSocketState(_ws.State);

                while (true)
                {
                    var result = new byte[1024];
                    await _ws.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken()); //接受数据
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

        // 发送心跳
        public void SendHeartBeat()
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
                return;

            _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{\"id\":101}")), WebSocketMessageType.Text, true, _ct);
        }

        // 发送数据
        public void Send(string data)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                Debug.LogErrorFormat("[WebSocket] 发送数据失败，未连接上服务器。");
                return;
            }

            _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, _ct);
        }

        //中止WebSocket连接并取消任何挂起的IO操作
        public void Abort()
        {
            if (_ws == null)
                return;
            _ws.Abort();
        }
    }
}
