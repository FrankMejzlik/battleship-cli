using Battleship.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Battleship.UI
{
    public enum eCellState
    {
        SHIP,
        WATER,
        HIT,
        UNKNOWN
    };
    /**
     * Command line UI for the game.
     */
    public class CmdUi : IUi
    {
        /*
         * Methods
         */
        public CmdUi()
        {
            // Prepre terminal
            HandleWindowChange();
        }
        public void Shutdown()
        {
            Logic.Shutdown();
            ShouldRun = false;
        }

        public void Launch()
        {
            // The main UI loop
            while (ShouldRun)
            {
                // Let the active state handle it
                bool swapNeeded = State.Update(this);
                if (swapNeeded)
                {
                    // "Draw" frame buffer into the terminal
                    SwapBuffers();
                }
            }
        }

        public ConsoleKeyInfo pollKey()
        {
            ConsoleKeyInfo input;
            while (!Console.KeyAvailable)
            {
                DoCheck();

                Thread.Sleep(50);
            }
            input = Console.ReadKey();
            return input;
        }

        public void GotoState(eUiState newState, string msg = "")
        {
            switch (newState)
            {
            case eUiState.INTER:
                State = new InterState();
                break;

            case eUiState.INITIAL:
                State = new InitialState();
                break;

            case eUiState.CLIENT_CONNECTING:
                State = new ClientConnectionState();
                break;

            case eUiState.SERVER_WAITING:
                State = new ServerWaitingState();
                break;

            case eUiState.PLACING_SHIPS:
                State = new PlacingShipsState();
                break;

            case eUiState.YOUR_TURN:
                State = new YourTurnState();
                break;

            case eUiState.OPPONENTS_TURN:
                State = new OpponetsTurnState();
                break;

            case eUiState.FINAL:
                State = new FinalState();
                break;

            default:
                Logger.LogE($"Error getting unknown state: {newState}");
                break;
            }
        }


        public void HandleHitAt(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void HandleMisstAt(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void HandlePlaceShipAt(int x, int y)
        {
            throw new NotImplementedException();
        }



        private void HandleWindowChange()
        {
            //Console.SetWindowSize(FieldW * 4 * 2 + 20, FieldH * 2 + 5);

            // Update console window dimensions
            WindowWidth = Console.WindowWidth;
            WindowHeight = Console.WindowHeight;

            // Reinitialize frame buffer
            frameBuffer = new char[WindowWidth, WindowHeight];

            // Initialize frame buffer
            for (int x = 0; x < WindowWidth; ++x)
            {
                for (int y = 0; y < WindowHeight; ++y)
                {
                    frameBuffer[x, y] = ' ';
                }
            }
        }

        public char CellStateToString(eCellState state)
        {
            switch (state)
            {
            case eCellState.WATER:
                return '~';

            case eCellState.UNKNOWN:
                return ' ';
            case eCellState.SHIP:
                return '#';
            case eCellState.HIT:
                return 'X';
            }
            return ' ';
        }

        private void SwapBuffers()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            // Initialize frame buffer
            for (int y = 0; y < WindowHeight; ++y)
            {
                for (int x = 0; x < WindowWidth; ++x)
                {
                    sb.Append(frameBuffer[x, y]);
                }
                sb.Append('\n');
            }
            Console.WriteLine(sb.ToString());
            Console.SetCursorPosition(0, 0);
        }


        private void DoCheck()
        {
            // Check timer

            // Check status
            if (!ShouldRun)
            {
                Shutdown();
            }
        }

        public void SetLogic(Logic l)
        {
            Logic = l;
        }

        /**
         * Member variables
         */
        public ICmdUiState State { get; set; } = new InitialState();

        private Logic Logic { get; set; }

        public bool ShouldRun { get; set; } = true;


        private int WindowWidth { get; set; }
        private int WindowHeight { get; set; }


        public char[,] frameBuffer;
    }
}
