
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    class ChunkSize : RtmpMessage
    {
        public int Size { get; }

        public ChunkSize(int size) : base(Common.MessageType.SetChunkSize)
        {
            if (size > 0xFFFFFF)
                size = 0xFFFFFF;
            Size = size;
        }
    }
}
