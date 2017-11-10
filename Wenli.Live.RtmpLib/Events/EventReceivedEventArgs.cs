using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    public class EventReceivedEventArgs : EventArgs
    {
        public RtmpMessage Event { get; set; }

        public EventReceivedEventArgs(RtmpMessage @event)
        {
            this.Event = @event;
        }
    }
}
