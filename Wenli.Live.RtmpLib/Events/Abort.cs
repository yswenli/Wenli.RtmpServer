using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    public class Abort : RtmpMessage
    {
        public int StreamId { get; }

        public Abort(int streamId) : base(Common.MessageType.AbortMessage)
        {
            StreamId = streamId;
        }
    }
}
