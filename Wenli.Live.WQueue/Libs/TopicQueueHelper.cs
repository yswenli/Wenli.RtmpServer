using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.WQueue.Models;

namespace Wenli.Live.WQueue.Libs
{
    internal static class TopicQueueHelper
    {
        static ConcurrentDictionary<string, QueueBase> _dic = new ConcurrentDictionary<string, QueueBase>();        

        static long _in = 0;

        static long _out = 0;


        public static void Enqueue(QueueMessage<string> msg)
        {            
            QueueBase queue = new QueueBase();
            queue = _dic.GetOrAdd(msg.Topic, queue);
            queue.Enqueue(msg.Content);
            Interlocked.Increment(ref _in);
        }

        public static QueueMessage<string> Dequque(string topic)
        {
            string msg = null;

            QueueBase queue = null;

            if (_dic.TryGetValue(topic, out queue))
            {
                msg = queue.Dequeue();

                Interlocked.Increment(ref _out);

                if (msg != null)
                    return new QueueMessage<string>() { Topic = topic, Content = msg };
            }
            return null;
        }

        public static long In
        {
            get
            {
                return _in;
            }
        }

        public static long Out
        {
            get
            {
                return _out;
            }
        }

    }
}
