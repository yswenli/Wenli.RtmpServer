
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Amfs.AMF3.AMFWriters
{
    class Amf3ByteArrayWriter : IAmfItemWriter
    {
        public void WriteData(AmfWriter writer, object obj)
        {
            writer.WriteMarker(Amf3TypeMarkers.ByteArray);
            writer.WriteAmf3ByteArray(obj as ByteArray);
        }

        public void WriteDataAsync(AmfWriter writer, object obj)
        {
            writer.WriteMarkerAsync(Amf3TypeMarkers.ByteArray);
            writer.WriteAmf3ByteArrayAsync(obj as ByteArray);
        }
    }
}