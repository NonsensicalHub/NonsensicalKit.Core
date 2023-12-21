using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace NonsensicalKit.Tools.NetworkTool
{
    /// <summary>
    /// 连接本机socket服务端
    /// </summary>
    public class SocketHelper : MonoBehaviour
    {
        public Action<string> OnReceived { get; set; }
        private ScoektClientInstace _sci;
        private Queue<string> _datas = new Queue<string>();

        private void Update()
        {
            while (_datas.Count > 0)
            {
                string str = _datas.Dequeue();
                OnReceived?.Invoke(str);
            }
        }

        private void OnDestroy()
        {
            Abort();
        }

        public void Init(int port)
        {
            _sci = new ScoektClientInstace();
            _sci.SocketConnectAsync(port);
            _sci.OnConnectSuccess += () => { Debug.Log("连接成功"); };
            _sci.OnConnectFail += (msg) => { Debug.LogWarning(msg); };
            _sci.OnReceived += OnReceivedMessage;
        }

        private void OnReceivedMessage(string msg)
        {
            _datas.Enqueue(msg);
        }

        public void Send(string msg)
        {

            _sci.Send(msg);
        }

        public void Abort()
        {
            _sci?.Abort();
        }
    }

    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 2048;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public class ScoektClientInstace
    {
        public Action OnConnectSuccess { get; set; }
        public Action<string> OnConnectFail { get; set; }
        public Action<string> OnReceived { get; set; }

        public bool Connected
        {
            get
            {
                return _socket.Connected;
            }
        }

        private Socket _socket;

        public async void SocketConnectAsync(int post)
        {
            string host = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, post);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    await tempSocket.ConnectAsync(ipe);

                    if (tempSocket.Connected)
                    {
                        OnConnectSuccess?.Invoke();
                        _socket = tempSocket;
                        ReceiveMsg();
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    OnConnectFail?.Invoke(address.ToString() + "连接失败\n错误原因: " + e.ToString());
                }

            }
            if (_socket == null)
            {
                OnConnectFail?.Invoke("无可用连接");
            }
        }

        public void Send(string msg)
        {
            if (_socket != null && _socket.Connected)
            {
                msg += "\0";
                _socket.Send(Encoding.UTF8.GetBytes(msg));
            }
        }

        public void Abort()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        private void ReceiveMsg()
        {
            StateObject state = new StateObject();
            state.workSocket = _socket;
            _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallBack), state);

        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.UTF8.GetString(
                    state.buffer, 0, bytesRead));
                content = state.sb.ToString();

                string[] contents = content.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                int i;
                for (i = 0; i < contents.Length - 1; i++)
                {
                    OnReceived?.Invoke(contents[i]);
                }

                state.sb.Clear();
                state.sb.Append(contents[i]);
                state.sb.Append("\0");
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallBack), state);
            }
            else
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                Debug.LogError("收到数据为空");
            }
        }
    }
}
