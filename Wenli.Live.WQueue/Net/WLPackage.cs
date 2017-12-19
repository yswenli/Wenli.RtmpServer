using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal static class WLPackage
    {
        /// <summary>
        /// 从流中读取数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static MessageBase Read(Stream stream)
        {
            var len = BitConverter.ToInt32(StreamHelper.ReadBytes(stream, 4), 0);
            var content = StreamHelper.ReadBytes(stream, len);
            return SerializeHelper.ProtolBufDeserialize<MessageBase>(content);
        }

        private static void ReadSend(Stream stream)
        {
            var len = BitConverter.ToInt32(StreamHelper.ReadBytes(stream, 4), 0);
            StreamHelper.ReadBytes(stream, len);
        }

        /// <summary>
        /// 将内容写入到流
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="msg"></param>

        private static void Write(Stream stream, MessageBase msg)
        {
            var content = SerializeHelper.ProtolBufSerialize(msg);
            var len = BitConverter.GetBytes(content.Length);

            List<byte> dataList = new List<byte>();
            dataList.AddRange(len);
            dataList.AddRange(content);
            var data = dataList.ToArray();
            stream.Write(data, 0, data.Length);
        }

        private static void WriteAsync(Stream stream, MessageBase msg)
        {
            var content = SerializeHelper.ProtolBufSerialize(msg);
            var len = BitConverter.GetBytes(content.Length);
            List<byte> dataList = new List<byte>();
            dataList.AddRange(len);
            dataList.AddRange(content);
            var data = dataList.ToArray();
            stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 客户端请求
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static MessageBase Request(Stream stream, MessageBase msg)
        {
            Write(stream, msg);
            return Read(stream);
        }

        public static void Send(Stream stream, MessageBase msg)
        {
            Write(stream, msg);
            ReadSend(stream);
        }
        /// <summary>
        /// 服务器回复
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        public static void Response(Stream stream, string id, Func<string, MessageBase, MessageBase> handler)
        {
            var msg = Read(stream);

            SessionManager.Active(id);

            var rmsg = handler.Invoke(id, msg);

            if (rmsg != null)

                WriteAsync(stream, rmsg);

        }

    }
}
