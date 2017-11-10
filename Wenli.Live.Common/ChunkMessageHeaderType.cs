using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public enum ChunkMessageHeaderType : byte
    {
        New = 0,
        SameSource = 1,
        TimestampAdjustment = 2,
        Continuation = 3
    }
}
