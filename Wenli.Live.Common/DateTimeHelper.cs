using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public static class DateTimeHelper
    {
        static DateTime _dateTime;

        static DateTimeHelper()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _dateTime = DateTime.Now;
                    Thread.Sleep(1);
                }
            });
        }

        public static DateTime Current
        {
            get
            {
                if (_dateTime.Year == 1)
                {
                    _dateTime = DateTime.Now;
                }
                return _dateTime;
            }
        }

        public static string GetCurrentString(string format="yyyy-MM-dd HH:mm:ss.fff")
        {
            return _dateTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
