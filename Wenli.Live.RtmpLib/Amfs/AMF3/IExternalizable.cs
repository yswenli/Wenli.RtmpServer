
namespace Wenli.Live.RtmpLib.Amfs.AMF3
{
    public interface IExternalizable
    {
        void ReadExternal(IDataInput input);
        void WriteExternal(IDataOutput output);
    }
}
