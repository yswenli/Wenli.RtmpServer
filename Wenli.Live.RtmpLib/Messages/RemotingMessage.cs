using System;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Flexing;

namespace Wenli.Live.RtmpLib.Messages
{
    [Serializable]
    [SerializedName("flex.messaging.messages.RemotingMessage")]
    public class RemotingMessage : FlexMessage
    {
        [SerializedName("source")]
        public string Source { get; set; }

        [SerializedName("operation")]
        public string Operation { get; set; }
    }
}
