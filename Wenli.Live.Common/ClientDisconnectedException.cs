using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public class ClientDisconnectedException : Exception
    {
        // a disconnection may be accompanied with a string describing the nature or source of the
        // problem. along with the exception, you should be able to figure out the cause. More
        // often than not caused by one of the following reasons (during development):
        // 
        //     - incompatible IExternalizable implementation: data may be read or written in a way
        //           that the remote server does not expect.
        // 
        //     - parameterless constructor unavailable: in order to deserialize to concrete
        //           classes, that class must be instantiatable using a parameterless constructor.
        //           only applies when deserializing to objects.
        // 
        public string Description;

        // exception that caused the disconnect (if any)
        public Exception Exception;

        public ClientDisconnectedException()
        {
        }

        public ClientDisconnectedException(string description)
        {
            Description = description;
        }

        public ClientDisconnectedException(string description, Exception exception)
        {
            Description = description;
            Exception = exception;
        }
    }
}
