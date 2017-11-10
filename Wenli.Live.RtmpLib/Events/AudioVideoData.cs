
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    public abstract class ByteData : RtmpMessage
    {
        public byte[] Data { get; }

        protected ByteData(byte[] data, Common.MessageType messageType) : base(messageType)
        {
            Data = data;
        }
    }
}
