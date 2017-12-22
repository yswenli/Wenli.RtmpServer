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
        static List<UserToken> _list = new List<UserToken>();

        static object _locker = new object();

        static SessionManager()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        List<UserToken> nlist;
                        lock (_locker)
                        {
                            nlist = _list.Where(b => b.Actived.AddSeconds(12000) < DateTimeHelper.Current || b.Socket == null || (b.Socket != null && !b.Socket.Connected)).ToList();
                        }
                        if (nlist != null && nlist.Count > 0)
                        {
                            foreach (var item in nlist)
                            {
                                if (item.Socket == null || !item.Socket.Connected)
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


        static ActionBlock<UserToken> _addBlock = new ActionBlock<UserToken>((socketInfo) =>
        {
            lock (_locker)
            {
                _list.Add(socketInfo);
            }
        });

        public static void Add(UserToken userToken)
        {
            _addBlock.Post(userToken);
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
                        c.Socket.Close();
                        c.Socket = null;
                    }
                }
                catch { }
            }
        });

        public static void Remove(string id)
        {
            _remvoeBlock.Post(id);
        }

        public static UserToken Get(string id)
        {
            lock (_locker)
            {
                var si = _list.Where(b => b.ID == id).FirstOrDefault();
                if (si != null) si.Actived = DateTimeHelper.Current;
                return si;
            }
        }

        public static List<UserToken> ToList()
        {
            lock (_locker)
            {
                return _list.ToList();
            }
        }


    }


}
