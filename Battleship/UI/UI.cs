using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Battleship.UI
{
    public enum eUiState
    {
        /** Interstate, in which UI waits for next commands from the logic. 
         * \see Battleship.UI.InterState
         * OUT: CLIENT_CONNECTING (on select `client`), SERVER_WAITING (on select `server`)
         */
        INTER,

        /** Initial state after the launch. 
         * \see Battleship.UI.InitialState
         * OUT: CLIENT_CONNECTING (on select `client`), SERVER_WAITING (on select `server`)
         */
        INITIAL,

        /** Client is connecting to the server. 
         * \see Battleship.UI.ClientConnectionState
         * OUT: PLACING_SHIPS (on connected), FINAL (on error)
         */
        CLIENT_CONNECTING,

        /** Server is waiting for the client to join.
         * \see Battleship.UI.ServerWaitingState
         * OUT: PLACING_SHIPS (on connected), FINAL (on error)
         */
        SERVER_WAITING,

        /** You're placing your ships. 
         * \see Battleship.UI.PlacingShipsState
         * OUT: OPPONENTS_TURN (on random number == 0), YOUR_TURN (on random number == 1)
         */
        PLACING_SHIPS,

        /** You're picking the field to fire at.
         * \see Battleship.UI.YourTurnState
         * OUT: OPPONENTS_TURN, FINAL (on finish/timeout)
         */
        YOUR_TURN,

        /** You just wait for opponent to strike.
         * \see Battleship.UI.OpponetsTurnState
         * OUT: OPPONENTS_TURN, FINAL (on finish/timeout)
         */
        OPPONENTS_TURN,

        /** Displaying end of the game (finished/timed out) with a message. 
         * \see Battleship.UI.FinalState
         * OUT: INITIAL (on restart)
         */
        FINAL
    }
    public interface IUi
    {
        public bool IsInter { get; }

        public void Launch();
        public void Shutdown();
        public void GotoState(eUiState state, string msg = "");
        /**
         *  Sets reference to the server this UI is working with.
         */
        public void SetLogic(Logic s);


        public void HandleHitHimAt(int x, int y);

        public void HandleMissHimtAt(int x, int y);

        public void HandlePlaceShipAt(int x, int y);
        public void HandleMissedMe(int x, int y);
        public void HandleHitMe(int x, int y);
    }
}
