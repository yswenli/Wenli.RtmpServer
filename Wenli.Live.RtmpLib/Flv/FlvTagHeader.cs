using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Flv
{
    public class FlvTagHeader
    {
        // size of the chunk, including the header and payload
        public int DataSize { get; set; }
        public int StreamId { get; set; }
        public MessageType TagType { get; set; }
        public int MessageStreamId { get; set; }
        public int Timestamp { get; set; }
        public bool IsTimerRelative { get; set; }

        public static int GetHeaderLength(ChunkMessageHeaderType chunkMessageHeaderType)
        {
            switch (chunkMessageHeaderType)
            {
                case ChunkMessageHeaderType.New:
                    return 11;
                case ChunkMessageHeaderType.SameSource:
                    return 7;
                case ChunkMessageHeaderType.TimestampAdjustment:
                    return 3;
                case ChunkMessageHeaderType.Continuation:
                    return 0;
                default:
                    return -1;
            }
        }

        public RtmpHeader Clone()
        {
            return (RtmpHeader)this.MemberwiseClone();
        }

        // static string[] headerTypeNames = { "unknown", "chunk_size", "unknown2", "bytes_read", "ping", "server_bw", "client_bw", "unknown7", "audio", "video", "unknown10", "unknown11", "unknown12", "unknown13", "unknown14", "flex_stream", "flex_shared_object", "flex_message", "notify", "shared_object", "invoke" };
    }
}
