using System;
using Wenli.Live.RtmpLib.Rtmping;

namespace Wenli.Live.LiveServer
{
    class Program
    {
        /// <summary>
        /// ffmpeg -re -i test.mp4 -f flv -vcodec h264 -acodec aac "rtmp://127.0.0.1/wenli/live"
        /// ffplay "rtmp://127.0.0.1/wenli/live"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "Wenli.Live.LiveServer";

            RtmpServer rtmpServer = new RtmpServer();

            rtmpServer.StartAsync();

            Console.Read();
        }
    }
}
