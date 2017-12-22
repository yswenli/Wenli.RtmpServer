using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.WQueue.Net.Model;

namespace Wenli.Live.WQueue.Net
{
    internal class SocketAsyncEventArgsPool
    {
        public ConcurrentQueue<SocketAsyncEventArgs> _pool = new ConcurrentQueue<SocketAsyncEventArgs>();


        public SocketAsyncEventArgsPool(int numConnections, EventHandler<SocketAsyncEventArgs> IO_Completed)
        {
            for (int i = 0; i < numConnections; i++)
            {
                var readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                _pool.Enqueue(readWriteEventArg);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs args;

            while (!_pool.TryDequeue(out args))
            {
                Thread.Sleep(1);
            }
            return args;
        }

        public void Push(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket != null) e.AcceptSocket = null;
            e.SetBuffer(null, 0, 0);
            _pool.Enqueue(e);
        }

    }
}
