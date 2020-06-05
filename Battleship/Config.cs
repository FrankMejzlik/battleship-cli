
using Battleship.Common;
using System.Collections.Generic;


namespace Battleship
{
    /** Pure static class holding the app configuration. */
    static class Config
    {
        /** Logging level. 
         * 
         * LEVELS:
         *      0 => None
         *      1 => Errors
         *      2 => And warnings
         *      3 => And info
         *      4 => And debug info
         */
        public static int LogLevel {get; } = 4;

        public static int TerminalFieldWidth {get; } = 4;
        public static int TerminalFieldHeight {get; } = 2;

        

        /** Time limit in seconds for the action before the game will be terminated. */
        public static int Timeout { get; } = 60;

        /** Where the client game log will be written. */
        public static string ClientGameLogFilepath { get; } = @"client_game_log.txt";

        /** Where the server game log will be written. */
        public static string ServerGameLogFilepath { get; } = @"server_game_log.txt";

        /** Default port that will be used to start server at. */
        public static int Port { get; } = 8888;

        /** Default IP address to connect to while acting like a client. */
        public static string Ip { get; } = "127.0.0.1";

        /** Width of the game field. */
        public static int FieldWidth { get; } = 10;

        /** Height of the game field. */
        public static int FieldHeight { get; } = 10;


        /** Definitions of the ships players will place when the game starts. 
         * 
         * Coordinates are relative to the origin point of the ship.
         * EXAMPLE: (1, 0), (-1, 1), (-1, 0)
         *        O
         *        O + O
         */
        public static List<Ship> ShipsToPlace { get; } = new List<Ship>()
        {
            new Ship() { Fields = new List<Field>() { new Field(0, 0) } },
            new Ship() { Fields = new List<Field>() { new Field(-1, 0), new Field(0, 0), new Field(1, 0) } },
            new Ship() { Fields = new List<Field>() { new Field(-1, 0), new Field(0, 0), new Field(1, 0), new Field(0, -1), new Field(0, 1) } }
        };

        /** String constants to be used while coommunicating with the users. */
        public static class Strings
        {
            public static string Water { get; } = "WATER";
            public static string Sunk { get; } = "SUNK";
            public static string Hit { get; } = "HIT";

            /*
             * Non-error strings.
             */
            public static string Timeout { get; } = "The game timed out!";
            public static string WaitingForOpponent { get; } = "Waiting for an opponent to join";

            /*
             * User error strings.
             */
            public static string ErrConnectingFailed { get; } = "Connecting failed, sorry.";
            public static string ErrCannotConnectToTheServer { get; } = "Connecting failed, sorry.";
            public static string ErrServerCouldNotStart { get; } = "We're sorry but server couldn't start.";
        }
    }
}
