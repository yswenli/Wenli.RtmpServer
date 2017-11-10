using System;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Flexing;

namespace Wenli.Live.RtmpLib.Messages
{
    [Serializable]
    [SerializedName("DSA", Canonical = false)]
    [SerializedName("flex.messaging.messages.AsyncMessage")]
    public class AsyncMessage : FlexMessage
    {
        [SerializedName("correlationId")]
        public string CorrelationId { get; set; }
    }

    static class AsyncMessageHeaders
    {
        public const string Subtopic = "DSSubtopic";
    }
}
