using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class TransientAttribute : Attribute
    {
    }
}
