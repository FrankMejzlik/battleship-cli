
using System;
using System.IO;


namespace Battleship
{
    /** Basic logging system. */
    public static class Logger
    {
        static Logger()
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Writer = new StreamWriter($"{ts}_log.txt")
            {
                AutoFlush = true
            };
        }

        /** Debug log. */
        public static void LogD(string msg)
        {
            if (Config.LogLevel < 4)
            {
                return;
            }
            Writer.WriteLine(dPrefix + msg);
            Writer.Flush();
        }

        /** Info log. */
        public static void LogI(string msg)
        {
            if (Config.LogLevel < 3)
            {
                return;
            }
            Writer.WriteLine(iPrefix + msg);
            Writer.Flush();
        }

        /** Warning log. */
        public static void LogW(string msg)
        {
            if (Config.LogLevel < 2)
            {
                return;
            }
            Writer.WriteLine(wPrefix + msg);
            Writer.Flush();
        }

        /** Error log. */
        public static void LogE(string msg)
        {
            if (Config.LogLevel < 1)
            {
                return;
            }
            Writer.WriteLine(ePrefix + msg);
            Writer.Flush();
        }

        private static readonly string dPrefix = "DEBUG: ";
        private static readonly string iPrefix = "INFO: ";
        private static readonly string wPrefix = "WARNING: ";
        private static readonly string ePrefix = "ERROR: ";

        /** Writer used for logs. */
        private static StreamWriter Writer { get; }
    }


}
