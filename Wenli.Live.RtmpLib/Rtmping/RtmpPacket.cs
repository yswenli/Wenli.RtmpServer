using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Rtmping
{
    class RtmpPacket
    {
        public RtmpHeader Header { get; set; }
        public RtmpMessage Body { get; set; }
        public byte[] Buffer { get; private set; }
        public int Length { get; private set; }
        public int CurrentLength { get; private set; }
        public bool IsComplete => Length == CurrentLength;

        public RtmpPacket(RtmpHeader header)
        {
            Header = header;
            Length = header.PacketLength;
            Buffer = new byte[Length];
        }

        public RtmpPacket(RtmpMessage body)
        {
            Body = body;
        }

        public RtmpPacket(RtmpHeader header, RtmpMessage body) : this(header)
        {
            Body = body;
            Length = header.PacketLength;
        }

        internal void AddBytes(byte[] bytes)
        {
            Array.Copy(bytes, 0, Buffer, CurrentLength, bytes.Length);
            CurrentLength += bytes.Length;
        }
    }
}
