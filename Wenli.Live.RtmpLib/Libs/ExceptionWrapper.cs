using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Libs
{
    class ExceptionWrapper : BasicObjectWrapper
    {
        static readonly HashSet<string> ExcludedMembers = new HashSet<string>()
        {
            "HelpLink",
            "HResult",
            "Source",
            "StackTrace",
            "TargetSite"
        };

        public ExceptionWrapper(SerializationContext context) : base(context)
        {
        }

        public override ClassDescription GetClassDescription(object obj)
        {
            var klass = base.GetClassDescription(obj);

            return new ClassDescription(
                klass.Name,
                klass.Members.Where(x => !ExcludedMembers.Contains(x.Name)).ToArray(),
                klass.IsExternalizable,
                klass.IsDynamic);
        }
    }
}
