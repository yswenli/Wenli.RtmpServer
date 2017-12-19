
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Net;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class Client
    {
        List<TcpClient> _pool = new List<TcpClient>();

        DateTime _actived = DateTimeHelper.Current;

        Random _random = new Random((int)DateTimeHelper.Current.Ticks);

        public bool IsConnected
        {
            get; private set;
        } = false;


        string _userID;

        string _ip;

        int _port;

        int _maxClients = 4;

        public Client(string userID, string ip = "127.0.0.1", int port = 1937, int maxReConnectCount = 10, int maxClients = 4)
        {
            _userID = userID;
            _ip = ip;
            _port = port;
            _maxClients = maxClients;

            for (int i = 1; i <= 4; i++)
            {
                _pool.Add(new TcpClient(_userID + i, _ip, _port));
            }
        }

        private TcpClient GetClient()
        {
            var rnd = _random.Next();

            var index = rnd % _maxClients;

            return _pool[index];
        }


        public void Connect()
        {
            foreach (var item in _pool)
            {
                item.Connect();
            }

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_actived.AddSeconds(20) < DateTimeHelper.Current)
                    {
                        Ping();
                    }
                    Thread.Sleep(5 * 1000);
                }
            });
        }


        public void Leave()
        {
            var msg = new MessageBase()
            {
                Type = (byte)Net.Model.MessageType.Leave
            };
            GetClient().SendBase(msg);
        }

        public void Ping()
        {
            var msg = new MessageBase()
            {
                Type = (byte)Net.Model.MessageType.Ping
            };

            GetClient().SendBase(msg);

            _actived = DateTimeHelper.Current;
        }


        public void Enqueue<T>(string queue, T t)
        {
            this.Enqueue(queue, SerializeHelper.Serialize(t));
        }

        public void Enqueue(string queue, string value)
        {
            var msg = new MessageBase()
            {
                Type = (byte)Net.Model.MessageType.EnqueueRequest,
                Topic = queue,
                Content = value
            };
            GetClient().SendBase(msg);
            _actived = DateTimeHelper.Current;
        }

        public T Dequeue<T>(string queue)
        {
            var json = Dequeue(queue);

            if (!string.IsNullOrEmpty(json))
            {
                return SerializeHelper.Deserialize<T>(json);
            }
            return default(T);
        }


        public string Dequeue(string queue)
        {
            var msg = new MessageBase()
            {
                Type = (byte)Net.Model.MessageType.DequeueRequest,
                Topic = queue
            };

            var rmsg = GetClient().RequestBase(msg);

            _actived = DateTimeHelper.Current;

            if (rmsg == null) return string.Empty;

            return rmsg.Content;
        }

    }
}
