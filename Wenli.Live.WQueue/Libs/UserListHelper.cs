using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Live.WQueue.Models;

namespace Wenli.Live.WQueue.Libs
{
    internal static class UserListHelper
    {
        static object _locker = new object();

        static List<Topic> _list = new List<Topic>();


        public static Topic GetOrAdd(string id, string topic)
        {
            lock (_locker)
            {
                var t = _list.Where(b => b.ID == id && b.Name == topic).FirstOrDefault();

                if (t == null)
                {
                    _list.Add(new Topic() { ID = id, Name = topic, Joined = DateTime.Now });
                }
                return t;
            }
        }

        public static void Remove(string id)
        {
            lock (_locker)
            {
                var tlts = _list.Where(b => b.ID == id).ToList();

                if (tlts != null)
                {
                    foreach (var item in tlts)
                    {
                        _list.Remove(item);
                    }
                }
                tlts.Clear();
                tlts = null;
            }
        }

        public static void RemoveTopic(string topic)
        {
            lock (_locker)
            {
                var tlts = _list.Where(b => b.Name == topic).ToList();

                if (tlts != null)
                {
                    foreach (var item in tlts)
                    {
                        _list.Remove(item);
                    }
                }
                tlts.Clear();
                tlts = null;
            }
        }

        public static List<Topic> List
        {
            get
            {
                lock (_locker)
                {
                    return _list.ToList();
                }
            }
        }
    }
}
