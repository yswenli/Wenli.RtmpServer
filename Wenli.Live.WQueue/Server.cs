using System;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Libs;
using Wenli.Live.WQueue.Model;
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

        private SocketMessage Process(string id, SocketMessage msg)
        {
            try
            {
                TopicMessage b_msg;

                switch (msg.Type)
                {
                    case (byte)Net.Model.MessageType.Leave:

                        UserListHelper.Remove(id);
                        break;
                    case (byte)Net.Model.MessageType.EnqueueRequest:

                        b_msg = SerializeHelper.ProtolBufDeserialize<TopicMessage>(msg.Content);
                        UserListHelper.GetOrAdd(id, b_msg.Topic);
                        TopicQueueHelper.Enqueue(b_msg);
                        return new SocketMessage((byte)Net.Model.MessageType.EnqueueResponse, null);
                    case (byte)Net.Model.MessageType.DequeueRequest:

                        b_msg = SerializeHelper.ProtolBufDeserialize<TopicMessage>(msg.Content);
                        UserListHelper.GetOrAdd(id, b_msg.Topic);
                        var data = TopicQueueHelper.Dequque(b_msg.Topic);
                        if (data != null)
                        {
                            return new SocketMessage((byte)Net.Model.MessageType.DequeueResponse, SerializeHelper.ProtolBufSerialize(data));
                        }
                        else
                        {
                            var rmsg = new TopicMessage(b_msg.Topic, null);
                            return new SocketMessage((byte)Net.Model.MessageType.DequeueResponse, SerializeHelper.ProtolBufSerialize(rmsg));
                        }
                    case (byte)Net.Model.MessageType.Ping:
                        return new SocketMessage((byte)Net.Model.MessageType.Pong, null);
                    default:
                        _server.CloseClientSocket(id);
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
