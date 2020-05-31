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


        public static string WaterString { get; set; } = "WATER";
        public static string SunkString { get; set; } = "SUNK";
        public static string HitString { get; set; } = "HIT";
    }
    public enum ePacketType
    {
        ERROR = 0,
        MESSAGE = 1,
        SET_CLEINT_SHIPS = 2,
        FIRE = 3,
        FIRE_REPONSE = 4,
        YOUR_TURN = 5,
        OPPONENTS_TURN = 6,
        SET_CLIENT_SHIPS = 7,

        /** Indicates that the addressee won. */
        YOU_WIN = 8,

        /** Indicates that the addressee lost. */
        YOU_LOSE = 9,

        /** Indicates that game ended due to long inactivity */
        TIMED_OUT = 10

    }
    public enum eFireResponseType
    {

        WATER = 0,

        HIT = 1,

        SUNK = 2
    }

    public static class FireResponseExtensions
    {
        public static string ToFriendlyString(this eFireResponseType type)
        {
            switch (type)
            {
            case eFireResponseType.WATER:
                return Config.WaterString;
            case eFireResponseType.HIT:
                return Config.HitString;
            case eFireResponseType.SUNK:
                return Config.SunkString;
            default:
                return string.Empty;
            }
        }
    }
}
