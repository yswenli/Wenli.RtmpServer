using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Libs
{
    class ExternalizableWrapper : IObjectWrapper
    {
        readonly SerializationContext context;

        public bool GetIsDynamic(object instance) => false;
        public bool GetIsExternalizable(object instance) => false;

        public ExternalizableWrapper(SerializationContext context)
        {
            this.context = context;
        }

        public ClassDescription GetClassDescription(object obj)
        {
            var type = obj.GetType();

            return new ExternalizableClassDescription(
                context.GetAlias(type.FullName),
                new IMemberWrapper[] { },
                externalizable: true,
                dynamic: false);
        }

        class ExternalizableClassDescription : ClassDescription
        {
            internal ExternalizableClassDescription(string name, IMemberWrapper[] members, bool externalizable, bool dynamic)
                : base(name, members, externalizable, dynamic)
            {
            }

            public override bool TryGetMember(string name, out IMemberWrapper member)
            {
                throw new NotSupportedException();
            }
        }
    }
}
