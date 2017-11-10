using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Amfs;
using Wenli.Live.RtmpLib.Interfaces;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Rtmping
{
    public class RtmpHandshake
    {
        public const int ReceiveTimeout = 10000;

        public const int SendTimeout = 10000;

        public const int HandshakeRandomSize = 1528;

        public const int HandshakeSize = HandshakeRandomSize + 4 + 4;

        public byte Version;

        public uint Time;

        public uint Time2;

        public byte[] Random;

        public RtmpHandshake Clone()
        {
            return new RtmpHandshake()
            {
                Version = Version,
                Time = Time,
                Time2 = Time2,
                Random = Random
            };
        }

        public static async Task<RtmpHandshake> ReadAsync(Stream stream, bool readVersion, CancellationToken cancellationToken)
        {
            var size = HandshakeSize + (readVersion ? 1 : 0);
            var buffer = await StreamHelper.ReadBytesAsync(stream, size, cancellationToken);

            using (var reader = new AmfReader(new MemoryStream(buffer), null))
            {
                return new RtmpHandshake()
                {
                    Version = readVersion ? reader.ReadByte() : default(byte),
                    Time = reader.ReadUInt32(),
                    Time2 = reader.ReadUInt32(),
                    Random = reader.ReadBytes(HandshakeRandomSize)
                };
            }
        }

        public static Task WriteAsync(Stream stream, RtmpHandshake h, bool writeVersion, CancellationToken ct)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new AmfWriter(memoryStream, null))
            {
                if (writeVersion)
                    writer.WriteByte(h.Version);

                writer.WriteUInt32(h.Time);
                writer.WriteUInt32(h.Time2);
                writer.WriteBytes(h.Random);

                var buffer = memoryStream.ToArray();
                return stream.WriteAsync(buffer, 0, buffer.Length, ct);
            }
        }

        /// <summary>
        /// rtmp握手逻辑
        /// HandshakeAsync2
        /// c01->s01->s2->c2
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client_socket"></param>
        /// <param name="client_id"></param>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static async Task<int> HandshakeAsync(RtmpServer server, Socket client_socket, ushort client_id, X509Certificate2 cert = null)
        {
            Stream stream;
            if (cert != null)
            {
                var temp_stream = new SslStream(new NetworkStream(client_socket));
                try
                {
                    await temp_stream.AuthenticateAsServerAsync(cert);
                }
                catch (AuthenticationException)
                {
                    temp_stream.Close();
                    throw;
                }
                stream = temp_stream;
            }
            else
            {
                stream = new NetworkStream(client_socket);
            }

            var random = new Random(Environment.TickCount);

            var randomBytes = new byte[1528];
            random.NextBytes(randomBytes);
            client_socket.NoDelay = true;

            CancellationTokenSource cts = new CancellationTokenSource();

            //over time cancel task
            Timer timer = new Timer((s) =>
            {
                //cts.Cancel();
                //throw new TimeoutException();
            }, null, ReceiveTimeout, Timeout.Infinite);


            //read c0 c1
            var c01 = await RtmpHandshake.ReadAsync(stream, true, cts.Token);
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            //write s0 s1
            var s01 = new RtmpHandshake()
            {
                Version = 3,
                Time = (uint)Environment.TickCount,
                Time2 = 0,
                Random = randomBytes
            };
            timer.Change(ReceiveTimeout, Timeout.Infinite);
            await RtmpHandshake.WriteAsync(stream, s01, true, cts.Token);

            //write s2
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            random.NextBytes(randomBytes);
            var s2 = new RtmpHandshake()
            {
                Time = (uint)Environment.TickCount,
                Time2 = 0,
                Random = randomBytes
            };
            timer.Change(ReceiveTimeout, Timeout.Infinite);
            await RtmpHandshake.WriteAsync(stream, s2, false, cts.Token);

            // read c2
            timer.Change(SendTimeout, Timeout.Infinite);
            var c2 = await RtmpHandshake.ReadAsync(stream, false, cts.Token);
            timer.Change(Timeout.Infinite, Timeout.Infinite);


            // handshake check
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            //if (!c0.Random.SequenceEqual(s2.Random))
            //throw new ProtocolViolationException();

            var connect = new RtmpConnect(client_socket, stream, server, client_id, server.Context, server.AmfEncoding, true);

            connect.ChannelDataReceived += server.SendDataHandler;

            server.ClientSessions.Add(client_id, new ClientSession()
            {
                Connect = connect,
                LastPing = DateTime.UtcNow,
                ReaderTask = null,
                WriterTask = null
            });

            return client_id;
        }



    }
}
