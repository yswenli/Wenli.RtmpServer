
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    // user control message
   public class UserControlMessage : RtmpMessage
    {
        public UserControlMessageType EventType { get; private set; }
        public int[] Values { get; private set; }

        public UserControlMessage(UserControlMessageType eventType, int[] values) : base(Common.MessageType.UserControlMessage)
        {
            EventType = eventType;
            Values = values;
        }
    }

  
}
