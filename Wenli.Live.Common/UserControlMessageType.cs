using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public enum UserControlMessageType : ushort
    {
        StreamBegin = 0,
        StreamEof = 1,
        StreamDry = 2,
        SetBufferLength = 3,
        StreamIsRecorded = 4,
        PingRequest = 6,
        PingResponse = 7
    }
}
