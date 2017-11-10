using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Amfs
{
    public class AmfWriterMap : Dictionary<Type, IAmfItemWriter>
    {
        public IAmfItemWriter DefaultWriter { get; private set; }

        public AmfWriterMap(IAmfItemWriter defaultWriter)
        {
            DefaultWriter = defaultWriter;
        }
    }
}
