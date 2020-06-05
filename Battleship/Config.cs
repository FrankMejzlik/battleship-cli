
using Battleship.Common;
using System.Collections.Generic;


namespace Battleship
{
    /** Pure static class holding the app configuration. */
    static class Config
    {
        /*******************************************
         * vvvvvv  SETUP YOUR SHIPS HERE  vvvvvvv
         *******************************************/

        /** Definitions of the ships players will place when the game starts. 
         * 
         * Coordinates are relative to the origin point of the ship.
         * EXAMPLE: (1, 0), (-1, 1), (-1, 0)
         *        O
         *        O + O
         */
        public static List<Ship> ShipsToPlace { get; } = new List<Ship>()
        {
            // Aircraft ship #1
            new Ship() { Fields = new List<Field>() { new Field(0, 2),new Field(0, 1), new Field(0, 0), new Field(0, -1), new Field(0, -2) } },

            // Battleship #1
            new Ship() { Fields = new List<Field>() { new Field(0, 1), new Field(0, 0), new Field(0, -1), new Field(0, -2) } },

            // Cruiser #1
            new Ship() { Fields = new List<Field>() { new Field(-1, 0), new Field(0, 0), new Field(1, 0) } },

            // Patrol #1
            new Ship() { Fields = new List<Field>() { new Field(0, 0), new Field(1, 0) } },
            // Patrol #2
            new Ship() { Fields = new List<Field>() { new Field(0, 0), new Field(0, 1) } },

            // Submarine #1
            new Ship() { Fields = new List<Field>() { new Field(0, 0) } },
            // Submarine #2
            new Ship() { Fields = new List<Field>() { new Field(0, 0) } },
        };
          
        /*******************************************
         *  ^^^^^^^ SETUP YOUR SHIPS HERE ^^^^^^^
         *******************************************/

        /** Logging level. 
         * 
         * LEVELS:
         *      0 => None
         *      1 => Errors
         *      2 => And warnings
         *      3 => And info
         *      4 => And debug info
         */
        public static int LogLevel { get; } = 4;

        /** Time limit in seconds for the action before the game will be terminated. */
        public static int Timeout { get; } = 120;

        /** Minimal time in milisecond to wait between UI iterations. */
        public static int UpdateWait { get; } = 100;


        /** Where the client game log will be written. */
        public static string ClientGameLogFilepath { get; } = @"client_game_log.txt";

        /** Where the server game log will be written. */
        public static string ServerGameLogFilepath { get; } = @"server_game_log.txt";


        /** Default port that will be used to start server at. */
        public static int Port { get; } = 8888;

        /** Default IP address to connect to while acting like a client. */
        public static string Ip { get; } = "127.0.0.1";


        /******************************************
         *       Cmd UI specific config
         *       
         * This is not ideal place to be, but this 
         * way, all config values are at one place.
         *******************************************/

        /** Width of the game field. */
        public static int FieldWidth { get; } = 10;

        /** Height of the game field. */
        public static int FieldHeight { get; } = 10;

        /** Number of horizontal terminal fields that are occupied by ONE play field. */
        public static int TerminalFieldWidth { get; } = 4;

        /** Number of vertical terminal fields that are occupied by ONE play field. */
        public static int TerminalFieldHeight { get; } = 2;

        /** Field character markes. */
        public static char ShipChar { get; } = 'O';
        public static char UnknownChar { get; } = ' ';
        public static char MissedHimChar { get; } = '~';
        public static char HitHimChar { get; } = 'x';
        public static char MissedMe { get; } = '*';
        public static char HitMeChar { get; } = 'x';




        /** String constants to be used while coommunicating with the users. */
        public static class Strings
        {
            public static string Water { get; } = "WATER";
            public static string Sunk { get; } = "SUNK";
            public static string Hit { get; } = "HIT";

            /*
             * Non-error strings.
             */
            public static string Working { get; } = "Working...";
            public static string Timeout { get; } = "The game timed out!";
            public static string ForcedExit { get; } = "Forcefull game termination.";

            public static string MyFieldLabel { get; } = "=== My field ===";
            public static string EnemyFieldLabel { get; } = "=== Enemy field ===";

            public static string PlacingShips { get; } = "=>> PLACING SHIPS <<=";
            public static string PlacingShipsInstruction { get; } = "Move origin point of each ship and place it with SPACEBAR key.";

            public static string MyTurn { get; } = "=>> YOUR TURN <<=";
            public static string MyTurnInstruction { get; } = "Aim with ARROWS and shoot with the SPACEBAR key.";

            public static string OpponentsTurn { get; } = "=>> OPPONENT'S TURN <<=";
            public static string OpponentsTurnInstruction { get; } = "Take cover! Opponent is shooting at you!";

            public static string YouWin { get; } = ":) :) :) You WON! (: (: (: ";
            public static string YouLose { get; } = ":( :( :( You LOST! ): ): ): ";





            /*
             * User error strings.
             */
            public static string ErrConnectingFailed { get; } = "Connecting failed, sorry.";
            public static string ErrCannotConnectToTheServer { get; } = "Connecting failed, sorry.";
            public static string ErrServerCouldNotStart { get; } = "We're sorry but server couldn't start.";
        }

        /** Templates for the UI screens. */
        public static class ScreenStrings
        {
            public static string InitialScreen()
            {
                return
                    $"\n" +
                    $"  ----------------------------------------------------------------  \n" +
                    $"\tDo you want to launch a server or connect as a client?\n" +
                    $"  ----------------------------------------------------------------  \n" +
                    $"\n" +
                    $"\t  1) SERVER \n" +
                    $"\t  2) CLIENT \n";
            }

            public static string FinalScreen(string msg)
            {
                return
                    $"\n" +
                    $"  ----------------------------------------------------------  \n" +
                    $"                     END OF THE GAME\n" +
                    $"  ----------------------------------------------------------  \n" +
                    $"\n" +
                    $"\t ==  {msg} == \n" +
                    $"\n" +
                    $"\t\tq) EXIT\n";
            }

            public static string SelectPortScreen()
            {
                return
                    $"\n" +
                    $"  ----------------------------------------------------------  \n" +
                    $"     On what port? \n\t(for default {Config.Port} hit ENTER)\n" +
                    $"  ----------------------------------------------------------  \n" +
                    $"\n";
            }

            public static string SelectAddressScreen()
            {
                return
                    $"\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"     What's the address? \n\t(for default {Config.Ip}:{Config.Port} hit ENTER)\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"\n";
            }

            public static string ConnectingScreen()
            {
                return
                    $"\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"                     Connecting to the server...\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"\n";
            }

            public static string WaitingForConnectionScreen()
            {
                return
                    $"\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"                   Waiting for client to connect...\n" +
                    $"  -------------------------------------------------------------------------  \n" +
                    $"\n";
            }
        }
    }
}
