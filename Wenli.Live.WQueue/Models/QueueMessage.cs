using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Models
{
    public class QueueMessage<T>
    {
        public string Topic
        {
            get; set;
        }
        public T Content
        {
            get; set;
        }
    }
}
