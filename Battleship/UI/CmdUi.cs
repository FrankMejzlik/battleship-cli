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

        MISSED_HIM,
        HIT_HIM,

        UNKNOWN,

        MISSED_ME,
        HIT_ME
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

            // My field
            for (int x = 0; x < FieldW; ++x)
            {
                for (int y = 0; y < FieldH; ++y)
                {
                    myField[x, y] = eCellState.UNKNOWN;
                }
            }

            // Enemy field
            for (int x = 0; x < FieldW; ++x)
            {
                for (int y = 0; y < FieldH; ++y)
                {
                    enemyField[x, y] = eCellState.UNKNOWN;
                }
            }
        }
        public void Shutdown()
        {
            Logic.ShouldRun = false;
            ShouldRun = false;
        }

        public void Launch()
        {
            // The main UI loop
            while (ShouldRun)
            {
                // Let the active state handle it
                State.Update(this);
            }
        }

        public ConsoleKeyInfo PollKey()
        {
            ConsoleKeyInfo input;
            while (!Console.KeyAvailable)
            {
                DoCheck();

                if (ShouldUnblock || !ShouldRun)
                {
                    ShouldUnblock = false;
                    return new ConsoleKeyInfo();
                }

                Thread.Sleep(50);
            }
            input = Console.ReadKey();
            return input;
        }

        public void GotoState(eUiState newState, string msg = "")
        {
            Console.Clear();

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
                State = new FinalState(msg);
                break;

            default:
                Logger.LogE($"Error getting unknown state: {newState}");
                break;
            }

            ShouldUnblock = true;
        }


        public void HandleHitHimAt(int x, int y)
        {
            enemyField[x, y] = eCellState.HIT_HIM;
        }

        public void HandleMissHimtAt(int x, int y)
        {
            enemyField[x, y] = eCellState.MISSED_HIM;
        }

        public void HandlePlaceShipAt(int x, int y)
        {
            myField[x, y] = eCellState.SHIP;
        }

        public void HandleMissedMe(int x, int y)
        {
            myField[x, y] = eCellState.MISSED_ME;
        }

        public void HandleHitMe(int x, int y)
        {
            myField[x, y] = eCellState.HIT_ME;
        }

        private void HandleWindowChange()
        {
            Console.SetWindowSize(FieldW * 4 * 2 + 20, FieldH * 2 + 5);

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
            case eCellState.MISSED_HIM:
                return '~';

            case eCellState.UNKNOWN:
                return ' ';
            case eCellState.SHIP:
                return 'O';
            case eCellState.HIT_HIM:
                return 'X';
            case eCellState.MISSED_ME:
                return '*';
                case eCellState.HIT_ME:
                return 'X';

                
            }
            return ' ';
        }

        public void SwapBuffers()
        {
            Console.Clear();
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

            if (l is Server)
            {
                Console.Title = "Battleships: -- SERVER -- ";
            }
            else
            {
                Console.Title = "Battleships: -- CLIENT -- ";
            }

        }
        /**
         * Member variables
         */
        public ICmdUiState State { get; set; } = new InitialState();

        public bool ShouldUnblock {get;set;} = false;

        public eCellState[,] myField = new eCellState[Config.FieldHeight, Config.FieldWidth];
        public eCellState[,] enemyField = new eCellState[Config.FieldHeight, Config.FieldWidth];
        public int FieldW { get; set; } = Config.FieldWidth;
        public int FieldH { get; set; } = Config.FieldHeight;

        public Logic Logic { get; set; }

        public bool ShouldRun { get; set; } = true;


        private int WindowWidth { get; set; }
        private int WindowHeight { get; set; }


        public char[,] frameBuffer;
    }
}
