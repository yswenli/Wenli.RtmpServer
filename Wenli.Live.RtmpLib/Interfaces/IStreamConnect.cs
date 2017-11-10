using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.RtmpLib.Events;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Interfaces
{
    public interface IStreamConnect
    {
        ushort ClientID { get; }

        bool IsDisconnected { get; }
        ushort StreamID { get; }
        bool IsPublishing { get; }
        bool IsPlaying { get; }
        NotifyAmf0 FlvMetaData { get; }

        event ChannelDataReceivedEventHandler ChannelDataReceived;

        VideoData AvCConfigureRecord { get; }
        AudioData AACConfigureRecord { get; }

        void SendAmf0Data(RtmpMessage e);

        bool WriteOnce();
        bool ReadOnce();
        Task PingAsync(int pingTimeout);
        void OnDisconnected(ExceptionalEventArgs exceptionalEventArgs);
        void SendRawData(byte[] data);
        Task ReadOnceAsync();
        Task WriteOnceAsync();
    }
}
