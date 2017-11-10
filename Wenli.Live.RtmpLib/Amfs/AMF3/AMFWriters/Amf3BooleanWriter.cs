
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Amfs.AMF3.AMFWriters
{
    class Amf3BooleanWriter : IAmfItemWriter
    {
        public void WriteData(AmfWriter writer, object obj)
        {
            writer.WriteAmf3BoolSpecial((bool)obj);
        }

        public void WriteDataAsync(AmfWriter writer, object obj)
        {
            writer.WriteAmf3BoolSpecialAsync((bool)obj);
        }
    }
}