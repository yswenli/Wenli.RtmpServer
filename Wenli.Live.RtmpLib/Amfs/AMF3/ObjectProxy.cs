using System;
using System.Collections.Generic;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Amfs.AMF3
{
    [Serializable]
    [SerializedName("flex.messaging.io.ObjectProxy")]
    class ObjectProxy : Dictionary<string, object>, IExternalizable
    {
        public void ReadExternal(IDataInput input)
        {
            var obj = input.ReadObject();
            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                    this[pair.Key] = pair.Value;
            }
        }

        public void WriteExternal(IDataOutput output)
        {
            output.WriteObject(new AsObject(this));
        }
    }
}
