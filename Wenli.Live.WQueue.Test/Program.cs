using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Live.Common;

namespace Wenli.Live.WQueue.Test
{

    static class Program
    {
        static bool b = false;

        static void Main()
        {
            Console.WriteLine("输入ip，默认127.0.0.1");

            var ip = Console.ReadLine();

            if (string.IsNullOrEmpty(ip))
            {
                ip = "127.0.0.1";
            }

            Console.WriteLine("输入b，进行大数据测试,默认小数");

            var bStr = Console.ReadLine();

            if (!string.IsNullOrEmpty(bStr) && bStr == "b")
            {
                b = true;
            }

            Console.WriteLine("输入a：全部测试 b：入队测试 c：出队测试 默认只启动服务器");

            Calc calc = new Calc();

            var c = Console.ReadLine();

            if (c == "a")
            {
                serverStart();
                Productor(calc, ip);
                Consumer(calc, ip);
            }
            else if (c == "p")
            {
                Console.Title = "队列生产者";
                Productor(calc, ip);
            }
            else if (c == "c")
            {
                Console.Title = "队列消费者";
                Consumer(calc, ip);
            }
            else
            {
                Console.Title = "队列服务器";
                serverStart();
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    StringBuilder ouputString = new StringBuilder();

                    long oldTicks = DateTimeHelper.Current.Ticks;
                    var oldTotal = 0L;
                    var oldOutNum = 0L;

                    var soldTotal = 0L;
                    var soldOutNum = 0L;

                    while (true)
                    {
                        Thread.Sleep(1000);

                        ouputString.Clear();
                        ouputString.Append(string.Format("{0}   客户端 已入队：{1} 已出队：{2}{3}", DateTimeHelper.GetCurrentString(), calc.Total, calc.OutNum, Environment.NewLine));

                        var span = new TimeSpan(DateTimeHelper.Current.Ticks - oldTicks).TotalSeconds;
                        var tSpeed = (calc.Total - oldTotal) / span;
                        var oSpeed = (calc.OutNum - oldOutNum) / span;
                        ouputString.Append(string.Format("{0}   客户端 入队速度：{1} 出队速度：{2}{3}{3}", DateTimeHelper.GetCurrentString(), tSpeed.ToString("N2"), oSpeed.ToString("N2"), Environment.NewLine));

                        oldTicks = DateTimeHelper.Current.Ticks;
                        oldTotal = calc.Total;
                        oldOutNum = calc.OutNum;



                        if (_server != null)
                        {
                            ouputString.Append(string.Format("{0}   服务器 已入队：{1} 已出队：{2}{3}", DateTimeHelper.GetCurrentString(), _server.In, _server.Out, Environment.NewLine));

                            var stSpeed = (_server.In - soldTotal) / span;
                            var soSpeed = (_server.Out - soldOutNum) / span;
                            ouputString.Append(string.Format("{0}   服务器 入队速度：{1} 出队速度：{2}{3}{3}{3}", DateTimeHelper.GetCurrentString(), stSpeed.ToString("N2"), soSpeed.ToString("N2"), Environment.NewLine));

                            soldTotal = _server.In;
                            soldOutNum = _server.Out;
                        }


                        Console.WriteLine(ouputString.ToString());

                    }
                }
                catch (Exception ex)
                {

                }
            });

            Console.ReadLine();
        }

        static Server _server;

        static void serverStart()
        {
            _server = new Server();
            _server.OnError += Server_OnError;
            _server.Start();
            Console.WriteLine("队列服务器已启动！");
        }

        private static void Server_OnError(string id, Exception ex)
        {
            Console.WriteLine("{0}  服务器端的客户端{1}发生了异常：{2}", DateTimeHelper.GetCurrentString(), id, ex.Message);
        }



        static void Productor(Calc calc, string ip)
        {
            var productor = new Client("productor" + new Random().Next(), ip);
            productor.Connect();

            if (b)
            {
                var data = new Data() { Description = "无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。无论何种语言，都对TCP连接提供基于setsockopt方法实现的SO_SNDBUF、SO_RCVBUF，怎么理解这两个属性的意义呢？SO_SNDBUF、SO_RCVBUF都是个体化的设置，即，只会影响到设置过的连接，而不会对其他连接生效。SO_SNDBUF表示这个连接上的内核写缓存上限。实际上，进程设置的SO_SNDBUF也并不是真的上限，在内核中会把这个值翻一倍再作为写缓存上限使用，我们不需要纠结这种细节，只需要知道，当设置了SO_SNDBUF时，就相当于划定了所操作的TCP连接上的写缓存能够使用的最大内存。然而，这个值也不是可以由着进程随意设置的，它会受制于系统级的上下限，当它大于上面的系统配置wmem_max（net.core.wmem_max）时，将会被wmem_max替代（同样翻一倍）；而当它特别小时，例如在2.6.18内核中设计的写缓存最小值为2K字节，此时也会被直接替代为2K。" };

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        productor.Enqueue<Data>("test", data);
                        calc.Total++;
                    }
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        productor.Enqueue("t", "0");
                        calc.Total++;
                    }
                });
            }

        }



        static void Consumer(Calc calc, string ip)
        {
            var consumer = new Client("consumer" + new Random().Next(), ip);
            consumer.Connect();

            if (b)
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        consumer.Dequeue<Data>("test");
                        calc.OutNum++;
                    }
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        consumer.Dequeue("t");
                        calc.OutNum++;
                    }
                });
            }
        }
    }


}
