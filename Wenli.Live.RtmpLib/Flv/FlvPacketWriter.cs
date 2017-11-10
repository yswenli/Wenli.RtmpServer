using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Amfs;
using Wenli.Live.RtmpLib.Events;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Flv
{
    class FlvPacketWriter
    {
        bool _started = false;
        internal readonly AmfWriter _writer;
        readonly ConcurrentQueue<RtmpPacket> _packetQueue;
        readonly AutoResetEvent _packetAvailableEvent;
        readonly ObjectEncoding _objectEncoding;

        public event EventHandler<ExceptionalEventArgs> Disconnected;

        public FlvPacketWriter(AmfWriter writer, ObjectEncoding objectEncoding)
        {
            this._objectEncoding = objectEncoding;

            this._writer = writer;

            _packetQueue = new ConcurrentQueue<RtmpPacket>();

            _packetAvailableEvent = new AutoResetEvent(false);

            _started = true;
        }

        void OnDisconnected(ExceptionalEventArgs e)
        {
            _started = false;

            if (Disconnected != null)
                Disconnected(this, e);
        }

        public bool WriteOnce()
        {
            if (_packetAvailableEvent.WaitOne(1))
            {
                RtmpPacket packet;
                while (_packetQueue.TryDequeue(out packet))
                    WritePacket(packet);
                return true;
            }
            return false;
        }

        public void WriteLoop()
        {
            try
            {
                while (_started)
                {
                    WriteOnce();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.Print("Exception: {0} at {1}", ex, ex.StackTrace);
                if (ex.InnerException != null)
                {
                    var inner = ex.InnerException;
                    System.Diagnostics.Debug.Print("InnerException: {0} at {1}", inner, inner.StackTrace);
                }
#endif

                OnDisconnected(new ExceptionalEventArgs("rtmp-packet-writer", ex));
                throw;
            }
        }

        public void WriteLoopAsync()
        {
            Task.Factory.StartNew(() =>
            {
                this.WriteLoop();
            });
        }

        static ChunkMessageHeaderType GetMessageHeaderType(RtmpHeader header, RtmpHeader previousHeader)
        {
            if (previousHeader == null || header.MessageStreamId != previousHeader.MessageStreamId || !header.IsTimerRelative)
                return ChunkMessageHeaderType.New;

            if (header.PacketLength != previousHeader.PacketLength || header.MessageType != previousHeader.MessageType)
                return ChunkMessageHeaderType.SameSource;

            if (header.Timestamp != previousHeader.Timestamp)
                return ChunkMessageHeaderType.TimestampAdjustment;

            return ChunkMessageHeaderType.Continuation;
        }

        public void Queue(RtmpMessage message, int streamId, int messageStreamId)
        {
            var header = new RtmpHeader();
            var packet = new RtmpPacket(header, message);

            header.StreamId = streamId;
            header.Timestamp = message.Timestamp;
            header.MessageStreamId = messageStreamId;
            header.MessageType = message.MessageType;
            _packetQueue.Enqueue(packet);
            _packetAvailableEvent.Set();
        }

        static int GetBasicHeaderLength(int streamId)
        {
            if (streamId >= 320)
                return 3;
            if (streamId >= 64)
                return 2;
            return 1;
        }

        FlvPacket RtmpPacketToFlvPacket(RtmpPacket rtmp_packet)
        {
            var rtmp_header = rtmp_packet.Header;
            var header = new FlvTagHeader();
            header.StreamId = 0;
            header.TagType = rtmp_header.MessageType;
            header.Timestamp = rtmp_header.Timestamp;
            var packet = new FlvPacket(header);
            packet.Body = rtmp_packet.Body;
            return packet;
        }

        void WritePacket(RtmpPacket packet)
        {
            var flv_packet = RtmpPacketToFlvPacket(packet);
            var header = flv_packet.Header;
            var streamId = header.StreamId;
            var message = flv_packet.Body;

            var buffer = GetMessageBytes(header, message);
            header.DataSize = buffer.Length;
            WriteTagHeader(header);
            _writer.Write(buffer);
            WritePrevTagLength(buffer);
        }

        private void WritePrevTagLength(byte[] buffer)
        {
            _writer.WriteUInt32((uint)buffer.Length + 11);
        }

        void WriteTagHeader(FlvTagHeader header)
        {
            _writer.WriteByte((byte)header.TagType);
            _writer.WriteUInt24(header.DataSize);
            var timestamp = BitConverter.GetBytes(header.Timestamp);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timestamp);
            _writer.Write(timestamp, 1, 3);
            _writer.WriteByte(timestamp[0]);
            _writer.WriteUInt24(header.StreamId);

        }

        byte[] GetMessageBytes(RtmpMessage message, Action<AmfWriter, RtmpMessage> handler)
        {
            using (var stream = new MemoryStream())
            using (var messageWriter = new AmfWriter(stream, _writer.SerializationContext, _objectEncoding))
            {
                handler(messageWriter, message);
                return stream.ToArray();
            }
        }

        byte[] GetMessageBytes(FlvTagHeader header, RtmpMessage message)
        {
            switch (header.TagType)
            {
                case MessageType.Audio:
                case MessageType.Video:
                    return GetMessageBytes(message, (w, o) => w.WriteBytes(((ByteData)o).Data));
                case MessageType.DataAmf0:
                    return GetMessageBytes(message, (w, o) => WriteCommandOrData(w, o, ObjectEncoding.Amf0));
                default:
                    throw new ArgumentOutOfRangeException("Unknown RTMP message type: " + (int)header.TagType);
            }
        }

        void WriteData(AmfWriter writer, RtmpMessage o, ObjectEncoding encoding)
        {
            var command = o as Command;
            if (command.MethodCall == null)
                WriteCommandOrData(writer, o, encoding);
            else
                writer.WriteBytes(command.Buffer);
        }

        void WriteCommandOrData(AmfWriter writer, RtmpMessage o, ObjectEncoding encoding)
        {
            var command = o as Command;
            var methodCall = command.MethodCall;
            var isInvoke = command is Invoke;

            // write the method name or result type (first section)
            if (methodCall.Name == "@setDataFrame")
            {
                writer.WriteAmfItem(encoding, command.ConnectionParameters);
            }

            // write arguments
            foreach (var arg in methodCall.Parameters)
                writer.WriteAmfItem(encoding, arg);
        }
    }
}
