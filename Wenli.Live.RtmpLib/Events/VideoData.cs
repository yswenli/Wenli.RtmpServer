using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Events
{
    public class VideoData : ByteData
    {
        public VideoData(byte[] data) : base(data, Common.MessageType.Video)
        {
        }
    }
}
