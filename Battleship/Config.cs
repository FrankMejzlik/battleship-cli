using System;
using System.Collections.Generic;
using System.Text;

namespace Battleship
{
    /**
     * Pure static class holding the app configuration.
     */
    static class Config
    {
        public static string ClientGameLogFilepath { get; set; } = @"client_game_log.txt";
        public static string ServerGameLogFilepath { get; set; } = @"server_game_log.txt";

        public static int Port { get; set; } = 8888;

        public static string Ip { get; set; } = "127.0.0.1";

        public static int FieldWidth { get; set; } = 10;
        public static int FieldHeight { get; set; } = 10;
    }
}
