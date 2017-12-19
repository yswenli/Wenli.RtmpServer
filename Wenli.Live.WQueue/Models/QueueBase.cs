using System.Collections.Generic;
using System.Linq;

namespace Wenli.Live.WQueue.Models
{
    internal class QueueBase
    {
        public const int MAX_PAGE_SIZE = 130000000;

        private List<Queue<string>> _list = new List<Queue<string>>();

        private object _locker = new object();
        /// <summary>
        /// 队列长度
        /// </summary>
        public long Count
        {
            get;
            set;
        }        

        public void Enqueue(string t)
        {
            lock (_locker)
            {
                Queue<string> queue = null;
                if (_list.Count > 0)
                    queue = _list.Last();
                if (queue != null)
                {
                    if ((queue.Count + 1) <= MAX_PAGE_SIZE)
                    {
                        queue.Enqueue(t);
                        this.Count++;
                        return;
                    }
                }
                queue = new Queue<string>();
                queue.Enqueue(t);
                this.Count++;
                _list.Add(queue);
            }
        }

        public string Dequeue()
        {
            lock (_locker)
            {
                var t = string.Empty;

                var queue = _list.FirstOrDefault();

                if (queue == null)
                {
                    t = string.Empty;
                }
                else if (queue.Count == 0)
                {
                    _list.Remove(queue);

                    t = Dequeue();
                }
                else
                {
                    t = queue.Dequeue();
                }
                if (t != null)
                    this.Count--;
                return t;
            }
        }
    }
}
