using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Interfaces
{
    public interface IObjectWrapper
    {
        bool GetIsExternalizable(object instance);
        bool GetIsDynamic(object instance);
        // Gets the class definition for an object `obj`, applying transformations like type name mappings
        ClassDescription GetClassDescription(object obj);
    }
}
