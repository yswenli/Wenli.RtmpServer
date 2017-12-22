using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Net.Model
{
    internal class SocketMessage
    {
        public int Length
        {
            get;
            set;
        }

        public byte Type
        {
            get;
            set;
        }

        public byte[] Content
        {
            get;
            set;
        }

        public byte[] ToBytes()
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(this.Length));
            list.Add(this.Type);
            if (this.Content != null) list.AddRange(this.Content);
            return list.ToArray();
        }

        public SocketMessage() { }



        public SocketMessage(byte type, byte[] content)
        {
            this.Type = type;
            int len = 1;
            if (content != null)
            {
                len += content.Length;

                this.Content = content;
            }
            this.Length = len;
        }
    }
}
