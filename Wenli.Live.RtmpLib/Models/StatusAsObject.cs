using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.Common;

namespace Wenli.Live.RtmpLib.Models
{
    class StatusAsObject : AsObject
    {
        private Exception exception;

        public StatusAsObject(Exception exception)
        {
            // todo: complete AsObject member initialization
            this.exception = exception;
        }

        public StatusAsObject(string code, string level, string description, object application, ObjectEncoding objectEncoding)
        {
            this["code"] = code;
            this["level"] = level;
            this["description"] = description;
            this["application"] = application;
            this["objectEncoding"] = (double)objectEncoding;
        }

        public StatusAsObject(string code, string level, string description)
        {
            this["code"] = code;
            this["level"] = level;
            this["description"] = description;
        }
    }
}
