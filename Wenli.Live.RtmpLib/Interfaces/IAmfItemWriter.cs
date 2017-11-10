using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Amfs;

namespace Wenli.Live.RtmpLib.Interfaces
{
    public interface IAmfItemWriter
    {
        void WriteData(AmfWriter writer, Object obj);
        void WriteDataAsync(AmfWriter amfWriter, object data);
    }
}
