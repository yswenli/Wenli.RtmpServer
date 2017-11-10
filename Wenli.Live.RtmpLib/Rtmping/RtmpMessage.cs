using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.Common;

namespace Wenli.Live.RtmpLib.Rtmping
{
    public abstract class RtmpMessage
    {
        public RtmpHeader Header { get; set; }
        public int Timestamp { get; set; }
        public MessageType MessageType { get; set; }

        protected RtmpMessage(MessageType messageType)
        {
            MessageType = messageType;
        }
    }
}
