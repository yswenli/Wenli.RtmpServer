using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Interfaces;

namespace Wenli.Live.RtmpLib.Libs
{
    public class AsObjectConverter : TypeConverter
    {
        public static SerializationContext DefaultSerializationContext = new SerializationContext();

        readonly SerializationContext serializationContext;

        public AsObjectConverter()
        {
            this.serializationContext = DefaultSerializationContext;
        }

        public AsObjectConverter(SerializationContext serializationContext)
        {
            this.serializationContext = serializationContext;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsValueType || destinationType.IsEnum || destinationType.IsArray || destinationType.IsAbstract || destinationType.IsInterface)
                return false;
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var instance = MethodFactory.CreateInstance(destinationType);
            var classDescription = serializationContext.GetClassDescription(destinationType, instance);
            var source = value as IDictionary<string, object>;
            if (source != null)
            {
                foreach (var kv in source)
                {
                    IMemberWrapper wrapper;
                    if (classDescription.TryGetMember(kv.Key, out wrapper))
                        wrapper.SetValue(instance, kv.Value);
                }
                return instance;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
