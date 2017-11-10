using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Events
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public readonly object Body;
        public readonly string ClientId;
        public readonly string Subtopic;

        internal MessageReceivedEventArgs(string clientId, string subtopic, object body)
        {
            ClientId = clientId;
            Subtopic = subtopic;
            Body = body;
        }
    }
}
