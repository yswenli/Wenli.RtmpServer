using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public class ByteUtils
    {
        public static uint ByteToUInt(byte[] bs, int length)
        {
            if (bs == null || bs.Length < length)
                return 0;
            uint rtn = 0;
            for (int i = 0; i < length; i++)
            {
                rtn <<= 8;
                rtn |= bs[i];
            }
            return rtn;
        }
        public static double ByteToDouble(byte[] bs)
        {
            if (bs == null || bs.Length < 8)
                return 0;
            byte[] b2 = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                b2[i] = bs[7 - i];
            }
            return BitConverter.ToDouble(b2, 0);
        }
        public static short ReadUI16(Stream src)
        {
            byte[] bs = new byte[2];
            if (src.Read(bs, 0, 2) <= 0)
                return 0;
            return (short)((bs[0] << 8) | bs[1]);
        }
        public static uint ReadUI24(Stream src)
        {
            byte[] bs = new byte[3];
            if (src.Read(bs, 0, 3) <= 0)
                throw new IOException("Stream end.");
            return ByteToUInt(bs, 3);
        }
        public static uint ReadUI32(Stream src)
        {
            byte[] bs = new byte[4];
            if (src.Read(bs, 0, 4) <= 0)
                throw new IOException("Stream end.");
            return ByteToUInt(bs, 4);
        }
        public static string GetTime(uint time)
        {
            return (time / 60000).ToString() + ":"
                + (time / 1000 % 60).ToString("D2") + "."
                + (time % 1000).ToString("D3");
        }



    }
}
