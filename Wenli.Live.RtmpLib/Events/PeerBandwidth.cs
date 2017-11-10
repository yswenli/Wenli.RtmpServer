
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    class PeerBandwidth : RtmpMessage
    {
        public enum BandwithLimitType : byte
        {
            Hard = 0,
            Soft = 1,
            Dynamic = 2
        }

        public int AcknowledgementWindowSize { get; }
        public BandwithLimitType LimitType { get; }

        private PeerBandwidth() : base(Common.MessageType.SetPeerBandwith) { }

        public PeerBandwidth(int acknowledgementWindowSize, BandwithLimitType limitType) : this()
        {
            AcknowledgementWindowSize = acknowledgementWindowSize;
            LimitType = limitType;
        }

        public PeerBandwidth(int acknowledgementWindowSize, byte limitType) : this()
        {
            AcknowledgementWindowSize = acknowledgementWindowSize;
            LimitType = (BandwithLimitType)limitType;
        }
    }
}
