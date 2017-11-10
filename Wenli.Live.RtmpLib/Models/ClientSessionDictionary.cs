using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.RtmpLib.Models
{
    /// <summary>
    /// 用户的流连接对象
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class ClientSessionDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        static object locker = new object();

        Dictionary<ushort, ClientSession> _connects = new Dictionary<ushort, ClientSession>();

        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _connects.GetEnumerator() as IEnumerator<KeyValuePair<TKey, TValue>>;
        }

        public void Add(ushort key, ClientSession value)
        {
            lock (locker)
            {
                _connects.Add(key, value);
            }
        }

        public void Remove(ushort key)
        {
            lock (locker)
            {
                if (_connects.ContainsKey(key))
                    _connects.Remove(key);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                _connects.Clear();
            }
        }

        public bool TryGetValue(ushort key, out ClientSession value)
        {
            bool result = false;
            value = null;
            lock (locker)
            {
                if (_connects.ContainsKey(key))
                {
                    value = _connects[key];
                    result = true;
                }
            }
            return result;
        }

        public int Count()
        {
            lock (locker)
            {
                return _connects.Count();
            }
        }

        public List<ushort> Keys
        {
            get
            {
                lock (locker)
                {
                    return _connects.Keys.ToList();
                }
            }
        }

        public List<ClientSession> Values
        {
            get
            {
                lock (locker)
                {
                    return _connects.Values.ToList();
                }
            }
        }

    }

    internal class ClientSessionDictionary : ClientSessionDictionary<ushort, ClientSession>, ICloneable
    {
        /// <summary>
        /// 复制一个新实例
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var newObj = new ClientSessionDictionary();

            if (this.Count() > 0)
            {
                foreach (var item in this)
                {
                    newObj.Add(item.Key, item.Value);
                }
            }

            return newObj;
        }

    }


}