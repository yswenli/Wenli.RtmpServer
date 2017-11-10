using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Interfaces
{
    public interface IMemberWrapper
    {
        string Name { get; }
        string SerializedName { get; }
        object GetValue(object instance);
        void SetValue(object instance, object value);
    }
}
