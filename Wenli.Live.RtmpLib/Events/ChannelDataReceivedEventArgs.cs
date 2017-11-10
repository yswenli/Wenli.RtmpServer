using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{

    public delegate void ChannelDataReceivedEventHandler(object sender, ChannelDataReceivedEventArgs e);

    public class ChannelDataReceivedEventArgs : EventArgs
    {
        public ChannelType type;
        public RtmpMessage e;
        public ChannelDataReceivedEventArgs(ChannelType t, RtmpMessage e)
        {
            this.type = t;
            this.e = e;
        }
    }
}
