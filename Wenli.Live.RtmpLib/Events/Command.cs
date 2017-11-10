using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.RtmpLib.Events
{
    public enum CallStatus
    {
        Request,
        Result,
    }

    public class Method
    {
        public CallStatus CallStatus { get; internal set; }
        public string Name { get; internal set; }
        public bool IsSuccess { get; internal set; }
        public object[] Parameters { get; internal set; }

        internal Method(string methodName, object[] parameters)
        {
            Name = methodName;
            Parameters = parameters;
            CallStatus = CallStatus.Request;
        }
    }

    public class Command : RtmpMessage
    {
        public Method MethodCall { get; internal set; }
        public byte[] Buffer { get; internal set; }
        public int InvokeId { get; internal set; }
        public object ConnectionParameters { get; internal set; }

        public Command(Common.MessageType messageType) : base(messageType) { }
    }

    public abstract class Invoke : Command
    {
        protected Invoke(Common.MessageType messageType) : base(messageType) { }
    }

    public abstract class Notify : Command
    {
        protected Notify(Common.MessageType messageType) : base(messageType) { }
    }

    public class InvokeAmf3 : Invoke
    {
        public InvokeAmf3() : base(Common.MessageType.CommandAmf3) { }
    }

    public class NotifyAmf3 : Notify
    {
        public NotifyAmf3() : base(Common.MessageType.DataAmf3) { }
    }

    public class InvokeAmf0 : Invoke
    {
        public InvokeAmf0() : base(Common.MessageType.CommandAmf0) { }
    }

    public class NotifyAmf0 : Notify
    {
        public NotifyAmf0() : base(Common.MessageType.DataAmf0) { }
    }
}
