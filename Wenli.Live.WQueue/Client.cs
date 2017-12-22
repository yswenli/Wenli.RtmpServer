
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Model;
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
        

        TcpClient _client;

        public Client(string userID, string ip = "127.0.0.1", int port = 1937, int maxReConnectCount = 10, int maxClients = 4)
        {
            _userID = userID;
            _ip = ip;
            _port = port;
            _client = new TcpClient(userID, ip, port);
        }


        public void Connect()
        {
            _client.Connect();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(5 * 1000);

                    if (_actived.AddSeconds(20) < DateTimeHelper.Current)
                    {
                        Ping();
                    }
                }
            });
        }


        public void Leave()
        {
            _client.SendBase((byte)Net.Model.MessageType.Leave, null);
        }

        public void Ping()
        {
            _client.SendBase((byte)Net.Model.MessageType.Ping, null);

            _actived = DateTimeHelper.Current;
        }


        public void Enqueue<T>(string queue, T t)
        {
            this.Enqueue(queue, SerializeHelper.Serialize(t));
        }

        public void Enqueue(string queue, string value)
        {
            var msg = new TopicMessage()
            {
                Topic = queue,
                Content = value
            };
            _client.SendBase((byte)Net.Model.MessageType.EnqueueRequest, msg);
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
            var msg = new TopicMessage()
            {
                Topic = queue
            };

            var rmsg = _client.RequestBase((byte)Net.Model.MessageType.DequeueRequest, msg);

            _actived = DateTimeHelper.Current;

            if (rmsg == null) return string.Empty;

            return rmsg.Content;
        }

    }
}
