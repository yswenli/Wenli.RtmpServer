using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.Common;

namespace Wenli.Live.RtmpLib.Flexing
{
    public class FlexMessage
    {
        [SerializedName("clientId")]
        public string ClientId { get; set; }

        [SerializedName("destination")]
        public string Destination { get; set; }

        [SerializedName("messageId")]
        public string MessageId { get; set; }

        [SerializedName("timestamp")]
        public long Timestamp { get; set; }

        // TTL (in milliseconds) that message is valid for after `Timestamp`
        [SerializedName("timeToLive")]
        public long TimeToLive { get; set; }

        [SerializedName("body")]
        public object Body { get; set; }

        [SerializedName("headers")]
        public Dictionary<string, object> Headers
        {
            get { return headers ?? (headers = new Dictionary<string, object>()); }
            set { headers = value; }
        }
        Dictionary<string, object> headers;

        public FlexMessage()
        {
            MessageId = Guid.NewGuid().ToString("D");
        }
    }
}
