using System;
using Wenli.Live.Common;

namespace Wenli.Live.RtmpLib.Messages
{
    [Serializable]
    [SerializedName("DSC", Canonical = false)]
    [SerializedName("flex.messaging.messages.CommandMessage")]
    public class CommandMessage : AsyncMessage
    {
        [SerializedName("messageRefType")]
        public string MessageRefType { get; set; }

        [SerializedName("operation")]
        public CommandOperation Operation { get; set; }

        public CommandMessage()
        {
        }
    }
}
