using System;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Flexing;

namespace Wenli.Live.RtmpLib.Messages
{
    [Serializable]
    [SerializedName("flex.messaging.messages.ErrorMessage")]
    public class ErrorMessage : FlexMessage
    {
        [SerializedName("faultCode")]
        public string FaultCode { get; set; }

        [SerializedName("faultString")]
        public string FaultString { get; set; }

        [SerializedName("faultDetail")]
        public string FaultDetail { get; set; }

        [SerializedName("rootCause")]
        public object RootCause { get; set; }

        [SerializedName("extendedData")]
        public object ExtendedData { get; set; }
    }
}
