using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Model;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal class TcpClient
    {
        System.Net.Sockets.TcpClient _client;

        string _name = string.Empty;

        string _ip = string.Empty;

        int _port = 1937;

        NetworkStream _NStream;

        private object _locker = new object();

        public TcpClient(string name, string ip = "127.0.0.1", int port = 1937)
        {
            _name = name;
            _ip = ip;
            _port = port;

            _client = new System.Net.Sockets.TcpClient
            {
                NoDelay = true,
                SendTimeout = 120 * 1000,
                ReceiveTimeout = 120 * 1000,
                SendBufferSize = 0,
                ReceiveBufferSize = 0
            };
        }

        public void Connect()
        {
            _client.Connect(_ip, _port);
            _NStream = _client.GetStream();
        }


        public TopicMessage RequestBase(byte type, TopicMessage msg)
        {
            lock (_locker)
            {
                return WLPackage.Request(_NStream, type, msg);
            }

        }

        public void SendBase(byte type, TopicMessage msg)
        {
            lock (_locker)
            {
                WLPackage.Send(_NStream, type, msg);
            }
        }


        #region MyRegion

        #endregion

    }
}
