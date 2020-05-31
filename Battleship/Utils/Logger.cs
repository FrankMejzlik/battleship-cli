using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Battleship
{
    /**
     * Basic logging system.
     */
    public static class Logger
    {
        static Logger()
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            sw = new StreamWriter($"{ts}_log.txt");
            sw.AutoFlush = true;
        }

        public static void LogI(string msg)
        {
            sw.WriteLine(iPrefix + msg);
            sw.Flush();
        }

        public static void LogW(string msg)
        {
            sw.WriteLine(wPrefix + msg);
            sw.Flush();
        }

        public static void LogE(string msg)
        {
            sw.WriteLine(ePrefix + msg);
            sw.Flush();
        }

        private static readonly string iPrefix = "INFO: ";
        private static readonly string wPrefix = "WARNING: ";
        private static readonly string ePrefix = "ERROR: ";

        private static StreamWriter sw;
    }


}
