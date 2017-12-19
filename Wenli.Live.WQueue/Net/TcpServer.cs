using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal class TcpServer
    {
        TcpListener _server;

        int _port = 1937;

        public event Action<string, Exception> OnError;





        public TcpServer(int port = 1937)
        {
            this._port = port;

            _server = new TcpListener(new IPEndPoint(IPAddress.Any, port));

            _server.ExclusiveAddressUse = false;

            _server.Server.NoDelay = false;

            _server.Server.SendTimeout = _server.Server.ReceiveTimeout = 120 * 1000;

            _server.Server.SendBufferSize = _server.Server.ReceiveBufferSize = 10 * 1024 * 1024;

        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start(Func<string, MessageBase, MessageBase> handler)
        {
            _server.Start(1000);

            AcceptAsync(handler);
        }


        private void AcceptAsync(Func<string, MessageBase, MessageBase> handler)
        {
            new Thread(() =>
            {
                while (true)
                {
                    var remote = _server.AcceptTcpClient();
                    ProcessAcceptAsync(remote, handler);
                }
            })
            { IsBackground = true }.Start();
        }

        private void ProcessAcceptAsync(System.Net.Sockets.TcpClient remote, Func<string, MessageBase, MessageBase> handler)
        {
            new Thread(() =>
            {
                var id = remote.Client.RemoteEndPoint.ToString();

                var ns = remote.GetStream();

                SessionManager.Add(id, remote, ns);

                while (true)
                {
                    try
                    {
                        WLPackage.Response(ns, id, handler);
                    }
                    catch (Exception ex)
                    {
                        OnError.Invoke(id, ex);
                        SessionManager.Remove(id);
                        break;
                    }
                }

            })
            { IsBackground = true }.Start();
        }


        public void Disconnected(string id)
        {
            try
            {
                SessionManager.Remove(id);
            }
            catch { }
        }

    }
}
