using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wenli.Live.Common;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    static class SessionManager
    {
        static List<SocketInfo> _list = new List<SocketInfo>();

        static object _locker = new object();

        static SessionManager()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        List<SocketInfo> nlist;
                        lock (_locker)
                        {
                            nlist = _list.Where(b => b.Actived.AddSeconds(20) < DateTimeHelper.Current || !b.Client.Connected || b.Client == null).ToList();
                        }
                        if (nlist != null && nlist.Count > 0)
                        {
                            foreach (var item in nlist)
                            {
                                if (item.Client == null || !item.Client.Connected)
                                {
                                    lock (_locker)
                                    {
                                        _list.Remove(item);
                                    }
                                }
                                else
                                {
                                    Remove(item.ID);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }


        static ActionBlock<SocketInfo> _addBlock = new ActionBlock<SocketInfo>((socketInfo) =>
        {
            lock (_locker)
            {
                _list.Add(socketInfo);
            }
        });

        public static void Add(string id, System.Net.Sockets.TcpClient client, NetworkStream ns)
        {
            _addBlock.Post(new SocketInfo(id, client, ns));
        }

        static ActionBlock<string> _activeBlock = new ActionBlock<string>((id) =>
        {
            lock (_locker)
            {
                var c = _list.Where(b => b.ID == id).FirstOrDefault();
                if (c != null)
                {
                    c.Actived = DateTimeHelper.Current;
                }
            }
        });

        public static void Active(string id)
        {
            _activeBlock.Post(id);
        }


        static ActionBlock<string> _remvoeBlock = new ActionBlock<string>((id) =>
        {
            lock (_locker)
            {
                try
                {
                    var c = _list.Where(b => b.ID == id).FirstOrDefault();
                    if (c != null)
                    {
                        _list.Remove(c);
                        c.Client.Close();
                    }
                }
                catch { }
            }
        });

        public static void Remove(string id)
        {
            _remvoeBlock.Post(id);
        }

        public static SocketInfo Get(string id)
        {
            lock (_locker)
            {
                var si = _list.Where(b => b.ID == id).FirstOrDefault();
                if (si != null) si.Actived = DateTimeHelper.Current;
                return si;
            }
        }

        public static List<System.Net.Sockets.TcpClient> ToList()
        {
            lock (_locker)
            {
                return _list.Select(b => b.Client).ToList();
            }
        }


    }


}
