using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public class ConcurrentList<T> : IEnumerable<T>, IList<T>
    {
        object _locker = new object();

        List<T> _list = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            lock (_locker)
            {
                return _list.GetEnumerator();
            }
        }

        public IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator1();
        }

        public void Add(T t)
        {
            lock (_locker)
            {
                if (!_list.Contains(t))
                    _list.Add(t);
            }
        }
        public bool Contains(T t)
        {
            lock (_locker)
            {
                return _list.Contains(t);
            }
        }

        public void Remove(T t)
        {
            lock (_locker)
            {
                if (_list.Contains(t))
                    _list.Remove(t);
            }
        }

        public void Set(T t)
        {
            lock (_locker)
            {
                if (_list.Contains(t))
                    _list.Remove(t);
                _list.Add(t);
            }
        }

        public void AddRang(List<T> list)
        {
            lock (_locker)
            {
                _list.AddRange(list);
            }

        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _list.Count;
                }
            }
        }


        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_locker)
                {
                    return _list[index];
                }
            }

            set
            {
                lock (_locker)
                {
                    _list[index] = value;
                }
            }
        }
        public int IndexOf(T item)
        {
            lock (_locker)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_locker)
            {
                _list.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_locker)
            {
                _list.RemoveAt(index);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _list.Clear();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            lock (_locker)
            {
                if (_list.Contains(item))
                    return _list.Remove(item);
                return false;
            }
        }
    }
}
