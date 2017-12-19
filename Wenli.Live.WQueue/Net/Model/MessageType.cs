using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Net.Model
{
    public enum MessageType
    {
        Leave = 1,
        EnqueueRequest = 2,
        EnqueueResponse = 3,
        DequeueRequest = 4,
        DequeueResponse = 5,
        Ping = 6,
        Pong = 7
    }
}
