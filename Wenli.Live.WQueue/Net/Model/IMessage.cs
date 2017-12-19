using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Net.Model
{
    public interface IMessage
    {
        byte Type { get; set; }

        string Topic { get; set; }
        
        byte[] Content { get; set; }
    }
}
