using System;
using System.Net.Sockets;
using System.Threading;
using Wenli.Live.Common;

namespace Wenli.Live.WQueue.Net.Model
{
    internal class SocketInfo
    {

        public string ID
        {
            get; set;
        }

        public System.Net.Sockets.TcpClient Client
        {
            get; set;
        }

        public NetworkStream NStream
        {
            get; set;
        }

        public DateTime Actived
        {
            get; set;
        }

        public SocketInfo(string id, System.Net.Sockets.TcpClient client, NetworkStream ns)
        {
            this.ID = id;
            this.Client = client;
            this.NStream = ns;
            this.Actived = DateTimeHelper.Current;
        }
    }
}
