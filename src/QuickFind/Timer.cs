using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace QuickFind
{
    class Timer:
        IDisposable
    {
        public static string output = "";

        private long _start;
        private string _msg;

        public Timer(string msg)
        {
            _msg = msg;

            _start = DateTime.Now.Ticks;
        }

        public void Dispose()
        {
            long ms = (DateTime.Now.Ticks - _start) / TimeSpan.TicksPerMillisecond;

            string str = _msg + " = " + ms + "ms.";

            Trace.WriteLine(str);

            output += str;
        }
    }
}
