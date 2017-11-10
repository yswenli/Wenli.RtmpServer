using System;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Amfs.AMF3.AMFWriters
{
    class Amf3IntWriter : IAmfItemWriter
    {
        public void WriteData(AmfWriter writer, object obj)
        {
            writer.WriteAmf3NumberSpecial(Convert.ToInt32(obj));
        }

        public void WriteDataAsync(AmfWriter writer, object obj)
        {
            writer.WriteAmf3NumberSpecialAsync(Convert.ToInt32(obj));
        }
    }
}