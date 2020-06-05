
using Battleship.Common;
using Battleship.Logic;
using System;
using System.Text;
using System.Threading;

namespace Battleship.UI
{
    
    /**
     * Command line UI for the Battleships game.
     * 
     * It is interactively rendered into the terminal and acts on each input user provides.
     */
    public class CmdUi : IUi
    {
        /*
         * Methods
         */
        public CmdUi()
        {
            Logger.LogI("Creating command line UI...");

            // Prepre the terminal
            HandleWindowChange();

            // My field
            for (int x = 0; x < FieldW; ++x)
            {
                for (int y = 0; y < FieldH; ++y)
                {
                    myField[x, y] = CellState.UNKNOWN;
                }
            }

            // Enemy field
            for (int x = 0; x < FieldW; ++x)
            {
                for (int y = 0; y < FieldH; ++y)
                {
                    enemyField[x, y] = CellState.UNKNOWN;
                }
            }

            Logger.LogI("The UI created.");
        }
        public void Shutdown()
        {
            Logger.LogI("Shutting down the UI...");

            Logic.ShouldRun = false;
            ShouldRun = false;

            Logger.LogI("UI is shut down.");
        }

        public void Launch()
        {
            Logger.LogI("Starting the UI game loop...");

            // The main UI loop
            while (ShouldRun)
            {
                // Let the active state handle it
                State.Update(this);
            }

            Logger.LogI("The UI game loop ended.");
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

                // Sleep long enough but short enough so user won't notice
                Thread.Sleep(100);
            }
            input = Console.ReadKey();

            Logger.LogD($"Input key: {input.Key}");

            return input;
        }

        public void GotoState(UiState newState, string msg = "")
        {
            Logger.LogD($"Changing UI state to '{newState}'...");

            Console.Clear();

            switch (newState)
            {
            case UiState.INTER:
                State = new InterState();
                break;

            case UiState.INITIAL:
                State = new InitialState();
                break;

            case UiState.CLIENT_CONNECTING:
                State = new ClientConnectionState();
                break;

            case UiState.SERVER_WAITING:
                State = new ServerWaitingState();
                break;

            case UiState.PLACING_SHIPS:
                State = new PlacingShipsState();
                break;

            case UiState.YOUR_TURN:
                State = new YourTurnState();
                break;

            case UiState.OPPONENTS_TURN:
                State = new OpponetsTurnState();
                break;

            case UiState.FINAL:
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
            enemyField[x, y] = CellState.HIT_HIM;
        }

        public void HandleMissHimtAt(int x, int y)
        {
            enemyField[x, y] = CellState.MISSED_HIM;
        }

        public void HandlePlaceShipAt(int x, int y)
        {
            myField[x, y] = CellState.SHIP;
        }

        public void HandleMissedMe(int x, int y)
        {
            myField[x, y] = CellState.MISSED_ME;
        }

        public void HandleHitMe(int x, int y)
        {
            myField[x, y] = CellState.HIT_ME;
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

        

        public void SwapBuffers()
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
            // Check status
            if (!ShouldRun)
            {
                Shutdown();
            }
        }

        public void SetLogic(ILogic logic)
        {
            Logic = logic;

            if (logic is Server)
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
        public bool IsInInterstate
        {
            get => State is InterState;
        }

        public bool ShouldUnblock { get; set; } = false;

        public CellState[,] myField = new CellState[Config.FieldHeight, Config.FieldWidth];
        public CellState[,] enemyField = new CellState[Config.FieldHeight, Config.FieldWidth];
        public int FieldW { get; set; } = Config.FieldWidth;
        public int FieldH { get; set; } = Config.FieldHeight;

        public ILogic Logic { get; set; }

        public bool ShouldRun { get; set; } = true;


        private int WindowWidth { get; set; }
        private int WindowHeight { get; set; }


        public char[,] frameBuffer;
    }
}
