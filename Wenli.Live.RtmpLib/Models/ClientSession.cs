using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Models
{
    internal class ClientSession
    {
        public IStreamConnect Connect;
        public DateTime LastPing;
        public Task ReaderTask;
        public Task WriterTask;
    }
}
