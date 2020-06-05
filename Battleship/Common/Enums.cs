using System;
using System.Collections.Generic;
using System.Text;

namespace Battleship.Common
{
    /** Types of packets client and server communicate with. */
    public enum PacketType
    {
        /** There was an error, you should terminate. */
        ERROR,

        /** Message to the user - not used yet. */
        MESSAGE,

        /** Fire at the position. */
        FIRE,

        /** Result of the previous fire. */
        FIRE_REPONSE,

        /** You shoot now. */
        YOUR_TURN,

        /** You wait. */
        OPPONENTS_TURN,

        /** Client send us his ships. */
        SET_CLIENT_SHIPS,

        /** Indicates that the addressee won. */
        YOU_WIN,

        /** Indicates that the addressee lost. */
        YOU_LOSE,

        /** Indicates that game ended due to long inactivity */
        TIMED_OUT,

        /** End of communication. */
        FIN

    }

    /** State of single cell in the IUi representation. */
    public enum CellState
    {
        /** Ship that is not hit. */
        SHIP,

        /** We don't know yet. */
        UNKNOWN,

        /** We shot there and it was a miss. */
        MISSED_HIM,

        /** We shot there and it was s ship. */
        HIT_HIM,

        /** He shot at me and it was a miss. */
        MISSED_ME,

        /** He shot at me and it was a ship. */
        HIT_ME
    };

    /** Extends CellState enumeration. */
    public static class CellStateExt
    {
        /** Converts enum to the representative character */
        public static char ToChar(this CellState state)
        {
            switch (state)
            {
            case CellState.SHIP:
                return Config.ShipChar;

            case CellState.MISSED_HIM:
                return Config.MissedHimChar;

            case CellState.HIT_HIM:
                return Config.HitHimChar;

            case CellState.MISSED_ME:
                return Config.MissedMe;

            case CellState.HIT_ME:
                return Config.HitMeChar;

            case CellState.UNKNOWN:
            default:
                return Config.UnknownChar;
            }
        }
    }



    /** Types of results for one shot. */
    public enum FireResponseType
    {
        /** It is a miss. */
        WATER = 0,

        /** It is a hit. */
        HIT = 1,

        /** It was a hit and the ship went down. */
        SUNK = 2
    }

    /** Extension for the FireReponseType enum. */
    public static class FireResponseTypeExt
    {
        /** 
         * Converts the provided type into nice readable string.
         * 
         * \param   type    Type to be converted into nice representation.
         * \return  Human readable representation of the provided type.
         */
        public static string ToString(this FireResponseType type)
        {
            switch (type)
            {
            case FireResponseType.WATER:
                return Config.Strings.Water;

            case FireResponseType.HIT:
                return Config.Strings.Hit;

            case FireResponseType.SUNK:
                return Config.Strings.Sunk;

            default:
                return string.Empty;
            }
        }
    }

    /** Represent ONE state UI is currently in. */
    public enum UiState
    {
        /** Interstate to which the IUi goes after it triggers action that will (probably)
         *  lead to change of the IUi state.
         *  
         *  IUi itself cannot change it's state to different state then Interstate, only logic can!
         *  
         *  This state takes no input from the user and is here to guarantee valid state even 
         *  if things dont go according to plan. For whatever reason the logic can decide to 
         *  do something different. Normally, this state shouldn't be visible to the user.
         * 
         * \see Battleship.UI.InterState
         * 
         * VALID OUTS: CLIENT_CONNECTING (on select `client`), SERVER_WAITING (on select `server`)
         */
        INTER,

        /** Initial state after the launch. 
         * 
         * \see Battleship.UI.InitialState
         * 
         * VALID OUTS: CLIENT_CONNECTING (on select `client`), SERVER_WAITING (on select `server`)
         */
        INITIAL,

        /** Client is connecting to the server. 
         * 
         * \see Battleship.UI.ClientConnectionState
         * 
         * VALID OUTS: PLACING_SHIPS (on connected), FINAL (on error)
         */
        CLIENT_CONNECTING,

        /** Server is waiting for the client to join.
         * 
         * \see Battleship.UI.ServerWaitingState
         * 
         * VALID OUTS: PLACING_SHIPS (on connected), FINAL (on error)
         */
        SERVER_WAITING,

        /** You're placing your ships. 
         * 
         * \see Battleship.UI.PlacingShipsState
         * 
         * VALID OUTS: OPPONENTS_TURN (on random number == 0), YOUR_TURN (on random number == 1)
         */
        PLACING_SHIPS,

        /** You're picking the field to fire at.
         * 
         * \see Battleship.UI.YourTurnState
         * 
         * VALID OUTS: OPPONENTS_TURN, FINAL (on finish/timeout)
         */
        YOUR_TURN,

        /** You just wait for opponent to strike.
         * 
         * \see Battleship.UI.OpponetsTurnState
         * 
         * VALID OUTS: OPPONENTS_TURN, FINAL (on finish/timeout)
         */
        OPPONENTS_TURN,

        /** Displaying end of the game (finished/timed out) with a message. 
         * 
         * \see Battleship.UI.FinalState
         * 
         * VALID OUTS: INITIAL (on restart)
         */
        FINAL
    }
}
