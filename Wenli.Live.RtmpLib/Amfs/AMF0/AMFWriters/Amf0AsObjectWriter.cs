
using System;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Amfs.AMF0.AMFWriters
{
    class Amf0AsObjectWriter : IAmfItemWriter
    {
        public void WriteData(AmfWriter writer, object obj)
        {
            writer.WriteAmf0AsObject(obj as AsObject);
        }

        public void WriteDataAsync(AmfWriter writer, object obj)
        {
            writer.WriteAmf0AsObjectAsync(obj as AsObject);
        }
    }
}
