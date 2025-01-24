using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NonsensicalKit.Tools.NetworkTool
{
    /// <summary>
    /// 连接本机socket服务端
    /// </summary>
    public class SocketHelper : MonoBehaviour
    {
        public Action<string> OnReceived { get; set; }
        private SocketClientInstance _sci;
        private Queue<string> _datas = new();

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
            _sci = new SocketClientInstance();
            _ = _sci.SocketConnectAsync(port);
            _sci.OnConnectSuccess += () => { Debug.Log("连接成功"); };
            _sci.OnConnectFail += Debug.LogWarning;
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
        public byte[] Buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder Sb = new StringBuilder();

        // Client socket.
        public Socket WorkSocket;
    }

    public class SocketClientInstance
    {
        public Action OnConnectSuccess { get; set; }
        public Action<string> OnConnectFail { get; set; }
        public Action<string> OnReceived { get; set; }

        public bool Connected => _socket.Connected;

        private Socket _socket;

        public async Task SocketConnectAsync(int post)
        {
            string host = Dns.GetHostName();
            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(host);
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
                }
                catch (Exception e)
                {
                    OnConnectFail?.Invoke(address + "连接失败\n错误原因: " + e);
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
            state.WorkSocket = _socket;
            _socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallBack, state);
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.Sb.Append(Encoding.UTF8.GetString(
                    state.Buffer, 0, bytesRead));
                var content = state.Sb.ToString();

                string[] contents = content.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                int i;
                for (i = 0; i < contents.Length - 1; i++)
                {
                    OnReceived?.Invoke(contents[i]);
                }

                state.Sb.Clear();
                state.Sb.Append(contents[i]);
                state.Sb.Append("\0");
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallBack, state);
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
