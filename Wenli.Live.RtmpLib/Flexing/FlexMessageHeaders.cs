using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Flexing
{
    public static class FlexMessageHeaders
    {
        // Messages pushed from the server may arrive in a batch, with messages in the batch 
        // potentially targeted to different Consumer instances.
        // Each message will contain this header identifying the Consumer instance that will 
        // receive the message.
        public const string DestinationClientId = "DSDstClientId";
        // Messages are tagged with the endpoint id for the Channel they are sent over.
        // Channels set this value automatically when they send a message.
        public const string Endpoint = "DSEndpoint";
        // Messages that need to set remote credentials for a destination carry the Base64 encoded 
        // credentials in this header.
        public const string RemoteCredentials = "DSRemoteCredentials";
        // Messages sent with a defined request timeout use this header.
        // The request timeout value is set on outbound messages by services or channels and the value 
        // controls how long the corresponding MessageResponder will wait for an acknowledgement, 
        // result or fault response for the message before timing out the request.
        public const string RequestTimeout = "DSRequestTimeout";
        // This header is used to transport the global FlexClient Id value in outbound messages 
        // once it has been assigned by the server.
        public const string FlexClientId = "DSId";
    }
}
