using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    class Acknowledgement : RtmpMessage
    {
        public int BytesRead { get; }

        public Acknowledgement(int bytesRead) : base(Common.MessageType.Acknowledgement)
        {
            BytesRead = bytesRead;
        }
    }
}
