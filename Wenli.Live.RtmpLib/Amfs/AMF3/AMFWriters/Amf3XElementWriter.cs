using System.Threading.Tasks;
using System.Xml.Linq;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Amfs.AMF3.AMFWriters
{
    class Amf3XElementWriter : IAmfItemWriter
    {
        public void WriteData(AmfWriter writer, object obj)
        {
            writer.WriteMarker(Amf3TypeMarkers.Xml);
            writer.WriteAmf3XElement(obj as XElement);
        }

        public void WriteDataAsync(AmfWriter writer, object obj)
        {
            writer.WriteMarkerAsync(Amf3TypeMarkers.Xml);
            writer.WriteAmf3XElementAsync(obj as XElement);
        }
    }
}