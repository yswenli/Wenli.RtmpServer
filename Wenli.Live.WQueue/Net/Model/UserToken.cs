using System;
using System.Net.Sockets;

namespace Wenli.Live.WQueue.Net.Model
{
    internal class UserToken
    {
        public object SyncLock = new object();

        public string ID
        {
            get; set;
        }

        Socket _socket;

        public Socket Socket
        {
            get
            {
                return _socket;
            }
            set
            {
                _socket = value;

                if(_socket!=null && _socket.Connected)
                {
                    this.ID = _socket.RemoteEndPoint.ToString();
                }

            }
        }

        public DateTime Actived
        {
            get; set;
        }

        public WLPackage WLPackage
        {
            get;set;
        }
    }
}
