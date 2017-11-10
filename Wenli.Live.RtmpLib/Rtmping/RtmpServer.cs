using Fleck;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Wenli.Live.Common;
using Wenli.Live.RtmpLib.Events;
using Wenli.Live.RtmpLib.Interfaces;
using Wenli.Live.RtmpLib.Libs;
using Wenli.Live.RtmpLib.Models;
using Wenli.Live.RtmpLib.WebSockets;

namespace Wenli.Live.RtmpLib.Rtmping
{
    public class RtmpServer
    {

        #region  private fileds

        private readonly int PROTOCOL_MIN_CSID = 3;

        private readonly int PROTOCOL_MAX_CSID = 65599;

        Dictionary<Tuple<string, string>, ushort> _clientRouteTable = new Dictionary<Tuple<string, string>, ushort>();

        List<Tuple<ushort, ushort, ChannelType>> _routedClients = new List<Tuple<ushort, ushort, ChannelType>>();

        List<string> _liveChannelList = new List<string>();

        Random _random = new Random();

        Thread _clientWorkThread;

        Socket _listener;

        ManualResetEvent _manualResetEvent = new ManualResetEvent(false);


        private string _bindIp = "0.0.0.0";

        private int _bindRtmpPort = 1935;

        private int _bindWebsocketPort = -1;

        static object _locker = new object();

        //ping 的时间间隔

        private int _pingPeriod = 20;

        private int _pingTimeout = 20;


        /// <summary>
        /// 用户的流连接对象
        /// </summary>
        internal ClientSessionDictionary ClientSessions
        {
            get;
            set;
        } = new ClientSessionDictionary();

        #endregion


        #region public properties

        public SerializationContext Context { get; set; }

        public ObjectEncoding AmfEncoding { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public bool Started { get; private set; } = false;

        #endregion

        public delegate bool ParameterAuthCallback(string liveChannel, NameValueCollection collection);

        #region public funs
        public RtmpServer() : this(new SerializationContext())
        {
            this.RegisterLiveChannel("wenli");
        }

        public RtmpServer(
            SerializationContext context,
            X509Certificate2 cert = null,
            ObjectEncoding object_encoding = ObjectEncoding.Amf0,
            ParameterAuthCallback publishParameterAuth = null,
            ParameterAuthCallback playParameterAuth = null,
            string bindIp = "0.0.0.0",
            int bindRtmpPort = 1935,
            int bindWebsocketPort = 1936
            )
        {
            this.Context = context;

            this.AmfEncoding = object_encoding;

            this.Certificate = cert;

            if (publishParameterAuth != null) this.PublishParameterAuth = publishParameterAuth;

            if (playParameterAuth != null) this.PlayParameterAuth = playParameterAuth;

            this._bindIp = bindIp;

            this._bindRtmpPort = bindRtmpPort;

            this._bindWebsocketPort = bindWebsocketPort;

            this.ClientWorkHandler();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (!Started)
            {
                Started = true;
                try
                {

                    this.RtmpListener(this._bindIp, this._bindRtmpPort);

                    FleckLog.Info("rtmp server started");

                    if (this._bindWebsocketPort != -1)
                    {
                        FleckLog.Info("websocket server started");

                        this.WebSocketListener(this._bindIp, this._bindWebsocketPort);
                    }

                    while (true)
                    {
                        _manualResetEvent.Reset();
                        FleckLog.Info("waiting for connect...");
                        _listener.BeginAccept(new AsyncCallback(acceptCallback), _listener);
                        _manualResetEvent.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    FleckLog.Error("RtmpServer.Start error", e);
                }
            }
        }

        public Task StartAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                this.Start();
            });
        }


        public void RegisterLiveChannel(string liveChannel)
        {
            lock (_liveChannelList)
            {
                if (_liveChannelList.IndexOf(liveChannel) != -1) throw new InvalidOperationException("liveChannel is exists");
                _liveChannelList.Add(liveChannel);
            }
        }

        public void Stop()
        {
            try
            {
                Started = false;

                ClientSessionDictionary nClientSessions = null;

                lock (_locker)
                {
                    nClientSessions = ClientSessions.Clone() as ClientSessionDictionary;
                }

                foreach (var current in nClientSessions)
                {
                    ClientSession state = current.Value;
                    ushort client_id = current.Key;
                    IStreamConnect connect = state.Connect;
                    if (connect.IsDisconnected)
                    {
                        continue;
                    }
                    try
                    {
                        DisconnectSession(client_id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
                lock (_locker)
                {
                    ClientSessions.Clear();
                }
                _listener.Close();
            }
            catch { }
        }

        #endregion


        #region internal funs


        internal void SendDataHandler(object sender, ChannelDataReceivedEventArgs e)
        {
            var server = (RtmpConnect)sender;

            var server_clients = _routedClients.FindAll((t) => t.Item2 == server.ClientID);
            foreach (var i in server_clients)
            {
                IStreamConnect client;
                ClientSession client_state = null;
                if (e.type != i.Item3)
                {
                    continue;
                }

                ClientSessions.TryGetValue(i.Item1, out client_state);

                switch (i.Item3)
                {
                    case ChannelType.Audio:
                        if (client_state == null) continue;
                        client = client_state.Connect;
                        client.SendAmf0Data(e.e);
                        break;
                    case ChannelType.Video:
                        if (client_state == null) continue;
                        client = client_state.Connect;
                        client.SendAmf0Data(e.e);
                        break;
                    case ChannelType.Message:
                        throw new NotImplementedException();
                }

            }
        }

        internal void ConnectToClient(string liveChannel, string path, ushort clientID, ChannelType channel_type)
        {
            ClientSession clientSession;

            ushort cachedClientID;

            var uri = new Uri("http://127.0.0.1/" + path);

            var key = new Tuple<string, string>(liveChannel, uri.AbsolutePath);

            if (!_clientRouteTable.TryGetValue(key, out cachedClientID)) throw new KeyNotFoundException("请求地址不存在~");

            if (!ClientSessions.TryGetValue(cachedClientID, out clientSession))
            {
                IStreamConnect connect = clientSession.Connect;
                _clientRouteTable.Remove(key);
                throw new KeyNotFoundException("请求客户端不存在~");
            }

            _routedClients.Add(new Tuple<ushort, ushort, ChannelType>(clientID, cachedClientID, channel_type));

        }

        /// <summary>
        /// 将发布者的媒体的meta发给播放者
        /// </summary>
        /// <param name="liveChannel"></param>
        /// <param name="path"></param>
        /// <param name="self"></param>
        /// <param name="flvHeader"></param>
        internal void SendMetadataToPlayer(string liveChannel, string path, IStreamConnect self, bool flvHeader = false)
        {
            ushort publisherID;

            IStreamConnect publisher;

            ClientSession publisherState;

            var uri = new Uri("http://127.0.0.1/" + path);
            var key = new Tuple<string, string>(liveChannel, uri.AbsolutePath);
            if (!_clientRouteTable.TryGetValue(key, out publisherID)) throw new KeyNotFoundException("请求地址不存在~");
            if (!ClientSessions.TryGetValue(publisherID, out publisherState))
            {
                _clientRouteTable.Remove(key);
                throw new KeyNotFoundException("请求客户端不存在~");
            }
            publisher = publisherState.Connect;
            if (publisher.IsPublishing)
            {
                var flv_metadata = (Dictionary<string, object>)publisher.FlvMetaData.MethodCall.Parameters[0];
                var has_audio = flv_metadata.ContainsKey("audiocodecid");
                var has_video = flv_metadata.ContainsKey("videocodecid");
                if (flvHeader)
                {
                    var header_buffer = Enumerable.Repeat<byte>(0x00, 13).ToArray<byte>();
                    header_buffer[0] = 0x46;
                    header_buffer[1] = 0x4C;
                    header_buffer[2] = 0x56;
                    header_buffer[3] = 0x01;
                    byte has_audio_flag = 0x01 << 2;
                    byte has_video_flag = 0x01;
                    byte type_flag = 0x00;
                    if (has_audio) type_flag |= has_audio_flag;
                    if (has_video) type_flag |= has_video_flag;
                    header_buffer[4] = type_flag;
                    var data_offset = BitConverter.GetBytes((uint)9);
                    header_buffer[5] = data_offset[3];
                    header_buffer[6] = data_offset[2];
                    header_buffer[7] = data_offset[1];
                    header_buffer[8] = data_offset[0];
                    self.SendRawData(header_buffer);
                }
                self.SendAmf0Data(publisher.FlvMetaData);
                if (has_audio) self.SendAmf0Data(publisher.AACConfigureRecord);
                if (has_video) self.SendAmf0Data(publisher.AvCConfigureRecord);
            }
        }

        internal bool CheckUpChannel(string liveChannel, ushort client_id)
        {
            if (_liveChannelList.IndexOf(liveChannel) == -1) return false;
            return true;
        }

        internal ushort RequestStreamId()
        {
            var streamIDs = this.ClientSessions.Values.Select(b => b.Connect.StreamID).ToList();

            return GetUniqueIdOfList(streamIDs, PROTOCOL_MIN_CSID, PROTOCOL_MAX_CSID);
        }

        internal bool RegisterPublish(string liveChannel, string path, ushort clientId)
        {
            var uri = new Uri("http://127.0.0.1/" + path);
            var key = new Tuple<string, string>(liveChannel, uri.AbsolutePath);
            var ret = PublishParameterAuth(liveChannel, HttpUtility.ParseQueryString(uri.Query));
            if (ret && !_clientRouteTable.ContainsKey(key)) _clientRouteTable.Add(key, clientId);
            return ret;
        }

        internal bool UnRegisterPublish(ushort clientId)
        {
            var key = _clientRouteTable.First(x => x.Value == clientId).Key;
            ClientSession state;
            if (_clientRouteTable.ContainsKey(key))
            {
                if (ClientSessions.TryGetValue(clientId, out state))
                {
                    IStreamConnect connect = state.Connect;
                    connect.ChannelDataReceived -= SendDataHandler;

                    var clients = _routedClients.FindAll(t => t.Item2 == clientId);
                    foreach (var i in clients)
                    {
                        DisconnectSession(i.Item1);
                    }
                    _routedClients.RemoveAll(t => t.Item2 == clientId);

                }
                _clientRouteTable.Remove(key);
                return true;
            }
            return false;
        }

        internal bool RegisterPlay(string liveChannel, string path, int clientId)
        {
            var uri = new Uri("http://127.0.0.1/" + path);
            return PlayParameterAuth(liveChannel, HttpUtility.ParseQueryString(uri.Query));
        }



        #endregion


        #region private funs

        ParameterAuthCallback PublishParameterAuth = (a, n) => true;

        ParameterAuthCallback PlayParameterAuth = (a, n) => true;

        /// <summary>
        /// 启动rtmp
        /// </summary>
        /// <param name="bindIp"></param>
        /// <param name="bindRtmpPort"></param>
        void RtmpListener(string bindIp, int bindRtmpPort)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.NoDelay = true;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(bindIp), bindRtmpPort);
            _listener.Bind(localEndPoint);
            _listener.Listen(10);
        }

        /// <summary>
        /// 启动ws
        /// </summary>
        /// <param name="bindIp"></param>
        /// <param name="bindWebsocketPort"></param>
        void WebSocketListener(string bindIp, int bindWebsocketPort)
        {
            var websocketServer = new WebSocketServer("ws://" + bindIp.ToString() + ":" + bindWebsocketPort.ToString());

            if (this.Certificate != null)
            {
                websocketServer.Certificate = Certificate;
                websocketServer.EnabledSslProtocols = SslProtocols.Default;
            }
            websocketServer.ListenerSocket.NoDelay = true;

            websocketServer.Start(wsSocket =>
            {
                wsSocket.OnOpen = () =>
                {
                    var path = wsSocket.ConnectionInfo.Path.Split('/');

                    if (path.Length != 3) wsSocket.Close();

                    ushort clientID = GetNewClientId();

                    RegisterPlay(path[1], path[2], clientID);

                    IStreamConnect wsConnect = new WebsocketConnect(this, wsSocket, clientID, this.Context, this.AmfEncoding);

                    ClientSessions.Add(clientID, new ClientSession()
                    {
                        Connect = wsConnect,
                        LastPing = DateTime.UtcNow,
                        ReaderTask = null,
                        WriterTask = null
                    });

                    try
                    {
                        SendMetadataToPlayer(path[1], path[2], wsConnect, flvHeader: true);
                        ConnectToClient(path[1], path[2], clientID, ChannelType.Audio);
                        ConnectToClient(path[1], path[2], clientID, ChannelType.Video);
                    }
                    catch (Exception ex)
                    {
                        FleckLog.Error("ws请求了一个不存在的直播，请检查直播地址是否有误或检查直播状态，error:", ex);
                        DisconnectSession(clientID);
                    }
                };
                wsSocket.OnPing = b => wsSocket.SendPong(b);
            });
        }

        /// <summary>
        /// 启动连接状态检查线程
        /// 执行rtmp的读写异步任务
        /// </summary>
        void ClientWorkHandler()
        {
            _clientWorkThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (ClientSessions.Count() < 1)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        ClientSessionDictionary nclientSessions = null;

                        lock (_locker)
                        {
                            nclientSessions = ClientSessions.Clone() as ClientSessionDictionary;
                        }

                        Parallel.ForEach(nclientSessions, current =>
                        {
                            ClientSession state = current.Value;
                            ushort client_id = current.Key;
                            IStreamConnect connect = state.Connect;

                            try
                            {
                                
                                if (connect.IsDisconnected)
                                {
                                    DisconnectSession(client_id);
                                }
                                else
                                {

                                    if (state.WriterTask == null)
                                    {
                                        state.WriterTask = connect.WriteOnceAsync();
                                    }
                                    else
                                    {
                                        if (state.WriterTask.IsCompleted)
                                        {
                                            state.WriterTask = connect.WriteOnceAsync();
                                        }
                                        if (state.WriterTask.IsCanceled || state.WriterTask.IsFaulted)
                                        {
                                            this.DisconnectSession(current.Key);
                                            //throw state.WriterTask.Exception;
                                        }
                                        if (state.LastPing == null || DateTime.UtcNow - state.LastPing >= new TimeSpan(0, 0, _pingPeriod))
                                        {
                                            connect.PingAsync(_pingTimeout);
                                            state.LastPing = DateTime.UtcNow;
                                        }

                                        if (state.ReaderTask == null || state.ReaderTask.IsCompleted)
                                        {
                                            state.ReaderTask = connect.ReadOnceAsync();
                                        }

                                        if (state.ReaderTask.IsCanceled || state.ReaderTask.IsFaulted)
                                        {
                                            this.DisconnectSession(current.Key);
                                            //throw state.ReaderTask.Exception;
                                        }
                                    }

                                }
                            }
                            catch
                            {
                                DisconnectSession(client_id);
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        FleckLog.Error("ConnectStateCheckUp.Thread.Error", ex);
                    }
                }
            })
            {
                IsBackground = true
            };
            _clientWorkThread.Start();
        }


        async void acceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            handler.NoDelay = true;
            // Signal the main thread to continue.
            _manualResetEvent.Set();
            try
            {
                await RtmpHandshake.HandshakeAsync(this, handler, this.GetNewClientId(), this.Certificate);
            }
            catch (TimeoutException)
            {
                handler.Close();
            }
            catch (AuthenticationException)
            {
                handler.Close();
                throw;
            }

        }

        ushort GetUniqueIdOfList(IList<ushort> list, int min_value, int max_value)
        {
            ushort id;
            do
            {
                id = (ushort)_random.Next(min_value, max_value);
            } while (list.IndexOf(id) != -1);
            list.Add(id);
            return id;
        }

        ushort GetUniqueIdOfList(IList<ushort> list)
        {
            ushort id;
            do
            {
                id = (ushort)_random.Next();
            } while (list.IndexOf(id) != -1);
            list.Add(id);
            return id;
        }

        ushort GetNewClientId()
        {
            var clientIDs = this.ClientSessions.Values.Select(b => b.Connect.ClientID).ToList();
            return GetUniqueIdOfList(clientIDs);
        }

        void DisconnectSession(ushort client_id)
        {
            ClientSession clientSession;

            if (ClientSessions.TryGetValue(client_id, out clientSession))
            {
                if (clientSession != null)
                {
                    IStreamConnect client = clientSession.Connect;

                    if (client.IsPublishing) UnRegisterPublish(client_id);

                    if (client.IsPlaying)
                    {
                        var client_channels = _routedClients.FindAll((t) => (t.Item1 == client_id || t.Item2 == client_id));
                        _routedClients.RemoveAll((t) => (t.Item1 == client_id));
                        foreach (var i in client_channels)
                        {
                            _routedClients.Remove(i);
                        }

                    }
                    client.OnDisconnected(new ExceptionalEventArgs("disconnected"));
                }
            }

            ClientSessions.Remove(client_id);
        }

        #endregion


    }
}
