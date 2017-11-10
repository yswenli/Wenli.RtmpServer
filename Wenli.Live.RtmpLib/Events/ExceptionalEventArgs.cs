using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Events
{
    public class ExceptionalEventArgs : EventArgs
    {
        public string Description;
        public Exception Exception;

        public ExceptionalEventArgs(string description)
        {
            Description = description;
        }

        public ExceptionalEventArgs(string description, Exception exception)
        {
            Description = description;
            Exception = exception;
        }
    }
}
