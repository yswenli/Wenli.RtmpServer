using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Model
{
    [ProtoContract]
    public class TopicMessage
    {
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

        public TopicMessage() { }

        public TopicMessage(string topic, string content)
        {
            this.Content = content;
        }
    }
}
