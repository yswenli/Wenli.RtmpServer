using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Amfs;
using Wenli.Live.RtmpLib.Events;
using Wenli.Live.RtmpLib.Flexing;
using Wenli.Live.RtmpLib.Interfaces;
using Wenli.Live.RtmpLib.Libs;
using Wenli.Live.RtmpLib.Messages;
using Wenli.Live.RtmpLib.Models;

namespace Wenli.Live.RtmpLib.Rtmping
{
    internal class RtmpConnect : IDisposable, IStreamConnect
    {
        #region private fileds

        private const int CONTROL_CSID = 2;

        private int _invokeId = 0;

        private DateTime _connectTime;

        private volatile int _disconnectsFired = 0;

        private readonly TaskCallbackManager<int, object> _callbackManager;

        private ObjectEncoding _objectEncoding;

        private RtmpServer _server;

        private Socket _clientSocket;

        private string _liveChannel;

        private bool is_not_set_video_config = true;

        private bool is_not_set_auido_config = true;

        private Random _random = new Random();

        #endregion

        #region public properties

        public VideoData CurrentVideoData { get; private set; }
        public AudioData CurrentAudioData { get; private set; }
        public string ConnectedApp { get; private set; }
        public bool IsDisconnected => _disconnectsFired != 0;
        public bool HasConnected { get; private set; } = false;
        public ushort StreamID { get; private set; } = 0;
        public ushort ClientID { get; private set; } = 0;
        public NotifyAmf0 FlvMetaData { get; private set; }
        public bool IsPublishing { get; private set; } = false;
        public bool IsPlaying { get; private set; } = false;
        public bool AudioSended { get; internal set; }
        public VideoData AvCConfigureRecord { get; private set; } = null;
        public AudioData AACConfigureRecord { get; private set; } = null;

        public RtmpPacketWriter Writer { get; set; }

        public RtmpPacketReader Reader { get; set; }

        #endregion

        #region events

        public event EventHandler Disconnected;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public event EventHandler<Exception> CallbackException;

        public event ChannelDataReceivedEventHandler ChannelDataReceived;

        #endregion


        public RtmpConnect(Socket client_socket, Stream stream, RtmpServer server, ushort clientID, SerializationContext context, ObjectEncoding objectEncoding = ObjectEncoding.Amf0, bool asyncMode = false)
        {
            ClientID = clientID;

            this._clientSocket = client_socket;

            this._server = server;
            this._objectEncoding = objectEncoding;

            Writer = new RtmpPacketWriter(new AmfWriter(stream, context, ObjectEncoding.Amf0, asyncMode), ObjectEncoding.Amf0);
            Reader = new RtmpPacketReader(new AmfReader(stream, context, asyncMode));

            Reader.EventReceived += EventReceivedCallback;
            Reader.Disconnected += OnPacketProcessorDisconnected;
            Writer.Disconnected += OnPacketProcessorDisconnected;

            _callbackManager = new TaskCallbackManager<int, object>();

        }

        public void Close()
        {
            OnDisconnected(new ExceptionalEventArgs("disconnected"));
        }

        void OnPacketProcessorDisconnected(object sender, ExceptionalEventArgs args)
        {
            OnDisconnected(args);
        }

        public bool WriteOnce()
        {
            return Writer.WriteOnce();
        }

        public bool ReadOnce()
        {
            return Reader.ReadOnce();
        }

        public Task WriteOnceAsync()
        {
            return Writer.WriteOnceAsync();
        }

        public Task ReadOnceAsync()
        {
            return Reader.ReadOnceAsync();
        }


        Task<object> QueueCommandAsTask(Command command, int streamId, int messageStreamId, bool requireConnected = true)
        {
            if (requireConnected && IsDisconnected)
                return CreateExceptedTask(new ClientDisconnectedException("disconnected"));

            var task = _callbackManager.Create(command.InvokeId);
            Writer.Queue(command, streamId, _random.Next());
            return task;
        }

        public void OnDisconnected(ExceptionalEventArgs e)
        {
            if (Interlocked.Increment(ref _disconnectsFired) > 1)
                return;

            HasConnected = false;
            WrapCallback(() => Disconnected?.Invoke(this, e));
            WrapCallback(() => _callbackManager.SetExceptionForAll(new ClientDisconnectedException(e.Description, e.Exception)));
            Dispose(true);
        }

        void EventReceivedCallback(object sender, EventReceivedEventArgs e)
        {
            switch (e.Event.MessageType)
            {
                case MessageType.UserControlMessage:
                    var m = (UserControlMessage)e.Event;
                    if (m.EventType == UserControlMessageType.PingRequest)
                    {
                        Console.WriteLine("Client Ping Request");
                        WriteProtocolControlMessage(new UserControlMessage(UserControlMessageType.PingResponse, m.Values));
                    }
                    else if (m.EventType == UserControlMessageType.SetBufferLength)
                    {
                        Console.WriteLine("Set Buffer Length");
                        // TODO
                    }
                    else if (m.EventType == UserControlMessageType.PingResponse)
                    {
                        Console.WriteLine("Ping Response");
                        var message = m as UserControlMessage;
                        _callbackManager.SetResult(message.Values[0], null);
                    }
                    break;

                case MessageType.DataAmf3:
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    break;

                case MessageType.CommandAmf3:
                case MessageType.DataAmf0:
                case MessageType.CommandAmf0:
                    var command = (Command)e.Event;
                    var call = command.MethodCall;
                    var param = call.Parameters.Length == 1 ? call.Parameters[0] : call.Parameters;
                    switch (call.Name)
                    {
                        case "connect":
                            StreamID = _server.RequestStreamId();
                            HandleConnectInvokeAsync(command);
                            HasConnected = true;
                            break;
                        case "_result":
                            var ack = param as AcknowledgeMessage;
                            _callbackManager.SetResult(command.InvokeId, ack != null ? ack.Body : param);
                            break;

                        case "_error":
                            var error = param as ErrorMessage;
                            _callbackManager.SetException(command.InvokeId, error != null ? new InvocationException(error) : new InvocationException());
                            break;
                        case "receiveAudio":
                            // TODO
                            break;
                        case "releaseStream":
                            Console.WriteLine("ReleaseStream");
                            // TODO
                            break;
                        case "publish":
                            HandlePublish(command);
                            break;
                        case "unpublish":
                            HandleUnpublish(command);
                            break;
                        case "FCpublish":
                        case "FCPublish":
                            // TODO
                            break;
                        case "FCUnpublish":
                        case "FCunPublish":
                            HandleUnpublish(command);
                            break;
                        case "createStream":
                            SetResultValInvoke(StreamID, command.InvokeId);
                            break;
                        case "play":
                            HandlePlay(command);
                            break;
                        case "deleteStream":
                            Console.WriteLine("deleteStream");
                            // TODO
                            break;
                        case "@setDataFrame":
                            SetDataFrame(command);
                            break;
                        case "getStreamLength":
                            // TODO
                            break;
                        default:
#if DEBUG
                            System.Diagnostics.Debug.Print($"unknown rtmp command: {call.Name}");

                            System.Diagnostics.Debugger.Break();
#endif
                            break;
                    }
                    break;
                case MessageType.WindowAcknowledgementSize:
                    var msg = (WindowAcknowledgementSize)e.Event;
                    break;
                case MessageType.Video:
                    var video_data = e.Event as VideoData;
                    if (is_not_set_video_config && video_data.Data.Length >= 2 && video_data.Data[1] == 0)
                    {
                        is_not_set_video_config = false;
                        AvCConfigureRecord = video_data;
                    }
                    ChannelDataReceived?.Invoke(this, new ChannelDataReceivedEventArgs(ChannelType.Video, e.Event));
                    break;
                case MessageType.Audio:
                    var audio_data = e.Event as AudioData;
                    if (is_not_set_auido_config && audio_data.Data.Length >= 2 && audio_data.Data[1] == 0)
                    {
                        is_not_set_auido_config = false;
                        AACConfigureRecord = audio_data;
                    }
                    ChannelDataReceived?.Invoke(this, new ChannelDataReceivedEventArgs(ChannelType.Audio, e.Event));
                    break;
                case MessageType.Acknowledgement:
                    break;
                default:
                    Console.WriteLine(string.Format("Unknown message type {0}", e.Event.MessageType));
                    break;
            }
        }

        /// <summary>
        /// 播放命令处理
        /// </summary>
        /// <param name="command"></param>
        private void HandlePlay(Command command)
        {
            string path = (string)command.MethodCall.Parameters[0];
            if (!_server.RegisterPlay(_liveChannel, path, ClientID))
            {
                OnDisconnected(new ExceptionalEventArgs("play parameter auth failed"));
                return;
            }
            WriteProtocolControlMessage(new UserControlMessage(UserControlMessageType.StreamIsRecorded, new int[] { StreamID }));
            WriteProtocolControlMessage(new UserControlMessage(UserControlMessageType.StreamBegin, new int[] { StreamID }));

            var status_reset = new AsObject
            {
                {"level", "status" },
                {"code", "NetStream.Play.Reset" },
                {"description", "Resetting and playing stream." },
                {"details", path }
            };

            var call_on_status_reset = new InvokeAmf0
            {
                MethodCall = new Method("onStatus", new object[] { status_reset }),
                InvokeId = 0,
                ConnectionParameters = null,
            };
            call_on_status_reset.MethodCall.CallStatus = CallStatus.Request;
            call_on_status_reset.MethodCall.IsSuccess = true;

            var status_start = new AsObject
            {
                {"level", "status" },
                {"code", "NetStream.Play.Start" },
                {"description", "Started playing." },
                {"details", path }
            };

            var call_on_status_start = new InvokeAmf0
            {
                MethodCall = new Method("onStatus", new object[] { status_start }),
                InvokeId = 0,
                ConnectionParameters = null,
            };
            call_on_status_start.MethodCall.CallStatus = CallStatus.Request;
            call_on_status_start.MethodCall.IsSuccess = true;
            Writer.Queue(call_on_status_reset, StreamID, _random.Next());
            Writer.Queue(call_on_status_start, StreamID, _random.Next());
            try
            {
                _server.SendMetadataToPlayer(_liveChannel, path, this);
                _server.ConnectToClient(_liveChannel, path, ClientID, ChannelType.Video);
                _server.ConnectToClient(_liveChannel, path, ClientID, ChannelType.Audio);
            }
            catch (Exception e)
            {
                OnDisconnected(new ExceptionalEventArgs("Not Found", e));
            }
            IsPlaying = true;
        }


        public async Task<T> InvokeAsync<T>(string method, object[] arguments)
        {
            var result = await QueueCommandAsTask(new InvokeAmf0
            {
                MethodCall = new Method(method, arguments),
                InvokeId = GetNextInvokeId()
            }, 3, 0);
            return (T)MiniTypeConverter.ConvertTo(result, typeof(T));
        }

        public void SendAmf0Data(RtmpMessage e)
        {
            //var timestamp = (int)(DateTime.UtcNow - connectTime).TotalMilliseconds;
            //e.Timestamp = timestamp;
            Writer.Queue(e, StreamID, _random.Next());
        }


        public async Task<T> InvokeAsync<T>(string endpoint, string destination, string method, object[] arguments)
        {
            if (_objectEncoding != ObjectEncoding.Amf3)
                throw new NotSupportedException("Flex RPC requires AMF3 encoding.");
            var client_id = Guid.NewGuid().ToString("D");
            var remotingMessage = new RemotingMessage
            {
                ClientId = client_id,
                Destination = destination,
                Operation = method,
                Body = arguments,
                Headers = new Dictionary<string, object>()
                {
                    { FlexMessageHeaders.Endpoint, endpoint },
                    { FlexMessageHeaders.FlexClientId, client_id ?? "nil" }
                }
            };

            var result = await QueueCommandAsTask(new InvokeAmf3()
            {
                InvokeId = GetNextInvokeId(),
                MethodCall = new Method(null, new object[] { remotingMessage })
            }, 3, 0);
            return (T)MiniTypeConverter.ConvertTo(result, typeof(T));
        }

        void SetDataFrame(Command command)
        {
            if ((string)command.ConnectionParameters != "onMetaData")
            {
                Console.WriteLine("Can only set metadata");
                throw new InvalidOperationException("Can only set metadata");
            }
            FlvMetaData = (NotifyAmf0)command;
        }

        void HandlePublish(Command command)
        {
            string path = (string)command.MethodCall.Parameters[0];
            if (!_server.RegisterPublish(_liveChannel, path, ClientID))
            {
                OnDisconnected(new ExceptionalEventArgs("Server publish error"));
                return;
            }
            var status = new AsObject
            {
                {"level", "status" },
                {"code", "NetStream.Publish.Start" },
                {"description", "Stream is now published." },
                {"details", path }
            };

            var call_on_status = new InvokeAmf0
            {
                MethodCall = new Method("onStatus", new object[] { status }),
                InvokeId = 0,
                ConnectionParameters = null,
            };
            call_on_status.MethodCall.CallStatus = CallStatus.Request;
            call_on_status.MethodCall.IsSuccess = true;

            // result.MessageType = MessageType.UserControlMessage;
            var stream_begin = new UserControlMessage(UserControlMessageType.StreamBegin, new int[] { StreamID });
            WriteProtocolControlMessage(stream_begin);
            Writer.Queue(call_on_status, StreamID, _random.Next());
            SetResultValInvoke(new object(), command.InvokeId);
            IsPublishing = true;
        }

        void SetResultValInvoke(object param, int transcationId)
        {
            ReturnResultInvoke(null, transcationId, param);
        }

        void ReturnResultInvoke(object connectParameters, int transcationId, object param, bool requiredConnected = true, bool success = true)
        {
            var result = new InvokeAmf0
            {
                MethodCall = new Method("_result", new object[] { param }),
                InvokeId = transcationId,
                ConnectionParameters = connectParameters
            };
            result.MethodCall.CallStatus = CallStatus.Result;
            result.MethodCall.IsSuccess = success;
            Writer.Queue(result, StreamID, _random.Next());
            //Console.WriteLine("_result");
        }

        void HandleUnpublish(Command command)
        {
            IsPublishing = false;
            _server.UnRegisterPublish(ClientID);
        }

        void HandleConnectInvokeAsync(Command command)
        {
            string code;
            string description;
            bool connect_result = false;
            var app = ((AsObject)command.ConnectionParameters)["app"];
            if (app == null)
            {
                OnDisconnected(new ExceptionalEventArgs("app value cannot be null"));
                return;
            }
            this._liveChannel = app.ToString();
            if (!_server.CheckUpChannel(app.ToString(), ClientID))
            {
                code = "NetConnection.Connect.Error";
                description = "Connection Failure.";
            }
            else
            {
                code = "NetConnection.Connect.Success";
                description = "Connection succeeded.";
                connect_result = true;
            }
            _connectTime = DateTime.UtcNow;
            AsObject param = new AsObject
            {
                { "code", code },
                { "description", description },
                { "level", "status" },
            };
            ReturnResultInvoke(new AsObject {
                    { "capabilities", 255.00 },
                    { "fmsVer", "FMS/4,5,1,484" },
                    { "mode", 1.0 }
                }, command.InvokeId, param, false, connect_result);
            if (!connect_result)
            {
                OnDisconnected(new ExceptionalEventArgs("Auth Failure"));
                return;
            }
        }

        public Task PingAsync(int PingTimeout)
        {
            Console.WriteLine("Server Ping Request");
            var timestamp = (int)((DateTime.UtcNow - _connectTime).TotalSeconds);
            var ping = new UserControlMessage(UserControlMessageType.PingRequest, new int[] { timestamp });
            //WriteProtocolControlMessage(ping);
            var ret = _callbackManager.Create(timestamp);
            return ret;
        }

        public void SendRawData(byte[] data)
        {
            Writer.writer.WriteBytes(data);
        }

        void WriteProtocolControlMessage(RtmpMessage @event)
        {
            Writer.Queue(@event, CONTROL_CSID, 0);
        }

        int GetNextInvokeId()
        {
            return Interlocked.Increment(ref _invokeId);
        }

        void WrapCallback(Action action)
        {
            try
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    CallbackException?.Invoke(this, ex);
                    throw ex;
                }
            }
#if DEBUG 
            catch (Exception unhandled)
            {
                System.Diagnostics.Debug.Print("UNHANDLED EXCEPTION IN CALLBACK: {0}: {1} @ {2}", unhandled.GetType(), unhandled.Message, unhandled.StackTrace);
                System.Diagnostics.Debugger.Break();
            }
#else
            catch { }
#endif
        }

        static Task<object> CreateExceptedTask(Exception exception)
        {
            var source = new TaskCompletionSource<object>();
            source.SetException(exception);
            return source.Task;
        }

        #region IDisposable Support

        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _clientSocket.Close();
                    Reader.reader.Dispose();
                }

                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            Dispose(true);
        }


        #endregion
    }
}
