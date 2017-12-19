using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Net.Model
{
    [ProtoContract]
    public class MessageBase
    {
        [ProtoMember(1, IsRequired = true)]
        public byte Type
        {
            get; set;
        }

        [ProtoMember(2, IsRequired = false)]
        public string Topic
        {
            get;
            set;
        }

        [ProtoMember(3, IsRequired = false)]
        public string Content
        {
            get; set;
        }

        public MessageBase() { }

        public MessageBase(byte type, string topic, string content)
        {
            this.Type = type;
            this.Content = content;
        }
    }
}
