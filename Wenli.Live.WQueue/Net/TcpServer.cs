using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal class TcpServer
    {
        Socket _server;

        int _port = 1937;

        const int opsToPreAlloc = 2;

        int m_numConnections;

        int m_receiveBufferSize;

        SocketAsyncEventArgsPool m_socketAsyncEventArgsPool;

        public event Action<string, Exception> OnError;

        Func<string, SocketMessage, SocketMessage> _handler;

        public TcpServer(int port = 1937, int numConnections = 200000, int receiveBufferSize = 100 * 1024)
        {
            this._port = port;

            m_numConnections = numConnections;

            m_receiveBufferSize = receiveBufferSize;

            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _server.Blocking = false;

            //_server.NoDelay = true;

            _server.SendTimeout = _server.ReceiveTimeout = 120 * 1000;

            _server.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start(Func<string, SocketMessage, SocketMessage> handler)
        {
            _handler = handler;

            m_socketAsyncEventArgsPool = new SocketAsyncEventArgsPool(m_numConnections, IO_Completed);

            _server.Listen(1000);

            StartAccept();
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg = null)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += AcceptEventArg_Completed;
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }
            var willRaiseEvent = _server.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
                ProcessAccepted(acceptEventArg);
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccepted(e);
        }

        private void ProcessAccepted(SocketAsyncEventArgs acceptArgs)
        {
            if (acceptArgs.SocketError == SocketError.OperationAborted) return;

            var remoteSocket = acceptArgs.AcceptSocket;

            Task.Factory.StartNew(() =>
            {
                var userToken = new UserToken
                {
                    Socket = remoteSocket,
                    WLPackage = new WLPackage(),
                    Actived = DateTimeHelper.Current
                };

                SessionManager.Add(userToken);

                var receiveArgs = m_socketAsyncEventArgsPool.Pop();

                receiveArgs.UserToken = userToken;

                receiveArgs.SetBuffer(new byte[m_receiveBufferSize], 0, m_receiveBufferSize);

                if (!userToken.Socket.ReceiveAsync(receiveArgs))
                {
                    ProcessReceived(receiveArgs);
                }
            });

            StartAccept(acceptArgs);
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceived(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        private void ProcessReceived(SocketAsyncEventArgs receiveArgs)
        {
            UserToken userToken = (UserToken)receiveArgs.UserToken;

            userToken.Actived = DateTimeHelper.Current;

            try
            {
                if (receiveArgs.BytesTransferred > 0 && receiveArgs.SocketError == SocketError.Success)
                {
                    var data = new byte[receiveArgs.BytesTransferred];

                    Buffer.BlockCopy(receiveArgs.Buffer, receiveArgs.Offset, data, 0, receiveArgs.BytesTransferred);

                    userToken.WLPackage.ProcessReceived(data, (d) =>
                    {
                        var s = _handler.Invoke(userToken.ID, d);

                        this.SendBase(userToken, s.ToBytes());
                    });

                    if (!userToken.Socket.ReceiveAsync(receiveArgs))
                    {
                        ProcessReceived(receiveArgs);
                    }
                }
                else
                {
                    CloseClientSocket(receiveArgs);
                }
            }
            catch (Exception ex)
            {
                CloseClientSocket(receiveArgs);
                OnError?.Invoke(userToken.ID, ex);
            }
        }



        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="data"></param>
        public void SendBase(UserToken userToken, byte[] data)
        {
            if (userToken.Socket != null && userToken.Socket.Connected)
            {
                try
                {
                    var sendArgs = m_socketAsyncEventArgsPool.Pop();

                    sendArgs.AcceptSocket = userToken.Socket;

                    sendArgs.UserToken = userToken;

                    sendArgs.SetBuffer(data, 0, data.Length);

                    if (!userToken.Socket.SendAsync(sendArgs))
                    {
                        ProcessSend(sendArgs);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(userToken.ID, ex);
                }
            }
            else
            {
                OnError?.Invoke(userToken.ID, new Exception("发送数据失败，当前连接已断开"));
            }
        }


        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                CloseClientSocket(e);
            }
            else
            {
                m_socketAsyncEventArgsPool.Push(e);
            }
        }

        public void CloseClientSocket(string id)
        {
            var userToken = SessionManager.Get(id);
            try
            {
                SessionManager.Remove(id);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(userToken != null ? userToken.ID : "", ex);
            }
        }


        public void CloseClientSocket(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;
            try
            {

                if (token.Socket != null)
                {
                    token.Socket.Shutdown(SocketShutdown.Both);
                }
                token.Socket = null;

                m_socketAsyncEventArgsPool.Push(e);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(token.ID, ex);
            }
        }

    }
}
