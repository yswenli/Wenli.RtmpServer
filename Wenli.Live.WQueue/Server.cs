using System;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Libs;
using Wenli.Live.WQueue.Models;
using Wenli.Live.WQueue.Net;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue
{
    public class Server
    {
        TcpServer _server;

        public event Action<string, Exception> OnError;


        public long In
        {
            get
            {
                return TopicQueueHelper.In;
            }
        }

        public long Out
        {
            get
            {
                return TopicQueueHelper.Out;
            }
        }

        public Server(int port = 1937)
        {
            _server = new TcpServer(port);
            _server.OnError += _server_OnError;

        }

        private void _server_OnError(string id, Exception ex)
        {
            OnError.Invoke(id, ex);
        }

        public void Start()
        {
            _server.Start(Process);
        }

        private MessageBase Process(string id, MessageBase msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case (byte)Net.Model.MessageType.Leave:
                        UserListHelper.Remove(id);
                        break;
                    case (byte)Net.Model.MessageType.EnqueueRequest:
                        UserListHelper.GetOrAdd(id, msg.Topic);
                        TopicQueueHelper.Enqueue(new QueueMessage<string>() { Topic = msg.Topic, Content = msg.Content });
                        return new MessageBase((byte)Net.Model.MessageType.EnqueueResponse, msg.Topic, null);
                    case (byte)Net.Model.MessageType.DequeueRequest:
                        UserListHelper.GetOrAdd(id, msg.Topic);
                        var data = TopicQueueHelper.Dequque(msg.Topic);
                        if (data != null)
                        {
                            return new MessageBase((byte)Net.Model.MessageType.DequeueResponse, msg.Topic, data.Content);
                        }
                        else
                        {
                            return new MessageBase((byte)Net.Model.MessageType.DequeueResponse, msg.Topic, null);
                        }
                    case (byte)Net.Model.MessageType.Ping:
                        return new MessageBase((byte)Net.Model.MessageType.Pong, null, null);
                    default:
                        _server.Disconnected(id);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.BeginInvoke(id, ex, null, null);
            }

            return null;
        }
    }
}
