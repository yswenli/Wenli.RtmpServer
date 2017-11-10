using System;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Flexing;

namespace Wenli.Live.RtmpLib.Messages
{
    [Serializable]
    [SerializedName("DSK", Canonical = false)]
    [SerializedName("flex.messaging.messages.AcknowledgeMessage")]
    public class AcknowledgeMessage : FlexMessage
    {
        public AcknowledgeMessage()
        {
            Timestamp = Environment.TickCount;
        }
    }
}
