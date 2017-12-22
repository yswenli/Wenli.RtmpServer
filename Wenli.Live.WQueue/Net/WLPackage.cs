using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Model;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal class WLPackage
    {
        /// <summary>
        /// 从流中读取数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static TopicMessage Read(Stream stream)
        {
            var len = BitConverter.ToInt32(StreamHelper.ReadBytes(stream, 4), 0);
            var type = StreamHelper.ReadBytes(stream, 1);
            var content = StreamHelper.ReadBytes(stream, len - 1);
            return SerializeHelper.ProtolBufDeserialize<TopicMessage>(content);
        }

        private static void ReadSend(Stream stream)
        {
            var len = BitConverter.ToInt32(StreamHelper.ReadBytes(stream, 4), 0);
            StreamHelper.ReadBytes(stream, len);
        }

        private static void Write(Stream stream, byte type, TopicMessage msg)
        {
            var content = SerializeHelper.ProtolBufSerialize(msg);
            var len = BitConverter.GetBytes(content.Length + 1);

            List<byte> dataList = new List<byte>();
            dataList.AddRange(len);
            dataList.Add(type);
            dataList.AddRange(content);

            var data = dataList.ToArray();
            stream.Write(data, 0, data.Length);
        }



        public static TopicMessage Request(Stream stream, byte type, TopicMessage msg)
        {
            Write(stream, type, msg);
            return Read(stream);
        }


        public static void Send(Stream stream, byte type, TopicMessage msg)
        {
            Write(stream, type, msg);
            ReadSend(stream);
        }



        #region server

        object _locker = new object();

        List<byte> _sessionData = new List<byte>();

        const int LenSize = 4;

        const int HeadSize = 5;


        public void ProcessReceived(byte[] data, Action<SocketMessage> callBack)
        {
            lock (_locker)
            {
                try
                {
                    List<byte> list = new List<byte>();

                    list.AddRange(data);

                    _sessionData.AddRange(list);

                    if (_sessionData.Count > 0)
                    {
                        do
                        {
                            var bsd = _sessionData.ToArray();

                            if (bsd.Length >= HeadSize)
                            {
                                var len = BitConverter.ToInt32(bsd, 0);

                                var bodyLen = len - 1;

                                var total = len + LenSize;

                                if (total <= bsd.Length)
                                {
                                    var type = bsd[4];

                                    var msg = new SocketMessage()
                                    {
                                        Length = len,
                                        Type = type
                                    };
                                    if (bodyLen > 0)
                                    {
                                        msg.Content = bsd.Skip(HeadSize).Take(bodyLen).ToArray();
                                    }
                                    else
                                    {
                                        msg.Content = null;
                                    }

                                    _sessionData.RemoveRange(0, total);

                                    callBack?.Invoke(msg);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        while (true);
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }




        #endregion

    }
}
