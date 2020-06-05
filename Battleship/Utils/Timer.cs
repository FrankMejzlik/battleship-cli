
using System.Diagnostics;
using System.Threading;

namespace Battleship
{
    /** Class responsible for timer that prevents the game from infinity. */
    public static class Timer
    {
        /** Assigns reference to the logic instance this wimer will guard. */
        public static void Start(Server l)
        {
            // Assign logic we will time guard
            Server = l;
        }

        /** Refresh timer (or initialize it if null). 
         * 
         *  This is called every action happens. */
        public static void Ping()
        {
            // Reset this stopwatch
            Sw = Stopwatch.StartNew();
        }

        /** Stops the timer. */
        public static void Stop()
        {
            Sw = null;
        }

        /** Runs endless loop that periodically check whether it's a timeout. */
        public static void DoCheck()
        {
            // Run endlessly
            while (Server.ShouldRun)
            {
                // If we detect timeout
                if (Sw != null && Sw.Elapsed.TotalSeconds > Config.Timeout)
                {
                    // We kill the game
                    Server.HandleTimeout();
                    break;
                }
                // 1s period is enoughs
                Thread.Sleep(1000);
            }
        }

        /** Reference to the logic this class guards. */
        private static Server Server { get; set; }

        /** Stowpatch reset at the last action. */
        private static Stopwatch Sw { get; set; } = null;
    }
}
