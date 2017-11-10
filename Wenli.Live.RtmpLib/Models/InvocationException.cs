using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Messages;

namespace Wenli.Live.RtmpLib.Models
{
    public class InvocationException : Exception
    {
        public string FaultCode { get; set; }
        public string FaultString { get; set; }
        public string FaultDetail { get; set; }
        public object RootCause { get; set; }
        public object ExtendedData { get; set; }
        public object SourceException { get; set; }

        internal InvocationException(ErrorMessage errorMessage)
        {
            SourceException = errorMessage;

            FaultCode = errorMessage.FaultCode;
            FaultString = errorMessage.FaultString;
            FaultDetail = errorMessage.FaultDetail;
            RootCause = errorMessage.RootCause;
            ExtendedData = errorMessage.ExtendedData;
        }

        public InvocationException()
        {
        }

        public override string Message => FaultString;
        public override string StackTrace => FaultDetail;
    }
}
