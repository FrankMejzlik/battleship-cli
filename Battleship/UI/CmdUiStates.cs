
using Battleship.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Battleship.UI
{
    /** Common interface for IUi states for the Battleships game. */
    public interface ICmdUiState
    {
        /** Updates the UI. 
         *
         * \param owner Ownning UI instance.
         */
        public abstract void Update(CmdUi owner);
    }


    /** State that needs to be rerendered based on user input. 
     * 
     * \see PlacingShipsState
     * \see YourTurnState
     * \see OpponetsTurnState
     */
    public abstract class RenderingCmdUiState : ICmdUiState
    {
        /** Updates the UI and RENDERS it out if needed. 
         *
         * \param owner Ownning UI instance.
         */
        public abstract void Update(CmdUi owner);

        protected void HandleInput(CmdUi owner, string keyStr)
        {
            switch (keyStr)
            {
            case "UpArrow":
                CurPos = (CurPos.Item1, Math.Max(CurPos.Item2 - 1, 0));
                break;
            case "DownArrow":
                CurPos = (CurPos.Item1, Math.Min(CurPos.Item2 + 1, Config.FieldHeight - 1));
                break;
            case "LeftArrow":
                CurPos = (Math.Max(CurPos.Item1 - 1, 0), CurPos.Item2);
                break;
            case "RightArrow":
                CurPos = (Math.Min(CurPos.Item1 + 1, Config.FieldWidth - 1), CurPos.Item2);
                break;

            // Shoot
            case "Spacebar":
                // If your turn, just shoot
                if (owner.State is YourTurnState)
                {
                    // Fire! Logic is branched depending on class of the Logic behind.
                    owner.Logic.FireAt(CurPos.Item1, CurPos.Item2);
                }
                break;
            }
        }

        protected void DrawGrid(CmdUi owner, int originX, int originY, string title, CellState[,] fields, bool withCursor)
        {
            // Draw the title
            DrawText(owner, originX, originY, title);

            // Go one line down before drawing the grids
            ++originY;

            // Draw vertical edges
            DrawVerticalGrid(owner, originX, originY);

            // Draw horizontal edges
            DrawHorizontalGrid(owner, originX, originY);

            // Draw contents of the fields
            DrawFieldContents(owner, originX, originY, fields);

            // Draw the cursor
            DrawCursor(owner, originX, originY, fields, withCursor);

        }

        private static void DrawHorizontalGrid(CmdUi owner, int originX, int originY)
        {
            for (int ix = 0; ix < owner.FieldH; ++ix)
            {
                var ixx = ix * 4;
                for (int iy = 0; iy <= owner.FieldW; ++iy)
                {
                    var iyy = iy * 2;
                    owner.frameBuffer[originX + ixx, originY + iyy] = '-';
                    owner.frameBuffer[originX + ixx + 1, originY + iyy] = '-';
                    owner.frameBuffer[originX + ixx + 2, originY + iyy] = '-';
                    owner.frameBuffer[originX + ixx + 3, originY + iyy] = '-';
                    owner.frameBuffer[originX + ixx + 4, originY + iyy] = '-';
                }

                // Draw column letter
                var coords = Utils.ToExcelCoords(ix, 0);
                owner.frameBuffer[originX + ixx + 2, originY] = coords[0];
            }
        }

        private static void DrawVerticalGrid(CmdUi owner, int originX, int originY)
        {
            for (int iy = 0; iy < owner.FieldW; ++iy)
            {
                var iyy = iy * 2;
                for (int ix = 0; ix <= owner.FieldH; ++ix)
                {
                    var ixx = ix * 4;
                    owner.frameBuffer[originX + ixx, originY + iyy] = '|';
                    owner.frameBuffer[originX + ixx, originY + iyy + 1] = '|';
                    owner.frameBuffer[originX + ixx, originY + iyy + 2] = '|';
                }

                // Draw row number
                {
                    string str = (iy + 1).ToString();
                    if (str.Length == 1)
                    {
                        owner.frameBuffer[originX, originY + iyy + 1] = str[0];
                    }
                    else if (str.Length == 2)
                    {
                        owner.frameBuffer[originX - 1, originY + iyy + 1] = str[0];
                        owner.frameBuffer[originX, originY + iyy + 1] = str[1];
                    }
                }
            }
        }

        private void DrawCursor(CmdUi owner, int originX, int originY, CellState[,] field, bool cursor)
        {
            for (int iy = 0; iy < owner.FieldW; ++iy)
            {
                var iyy = iy * 2;
                for (int ix = 0; ix < owner.FieldH; ++ix)
                {
                    var ixx = ix * 4 + 1;

                    var content = field[iy, ix];

                    // Draw cursor
                    if (CurPos.Item1 == ix && CurPos.Item2 == iy && cursor)
                    {
                        var xxx = originX + ixx;
                        var yyy = originY + iyy + 1;

                        // The primary cursor
                        owner.frameBuffer[xxx + 0, yyy] = '<';
                        owner.frameBuffer[xxx + 1, yyy] = '+';
                        owner.frameBuffer[xxx + 2, yyy] = '>';

                        // Ship around the cursor
                        DrawShipAroundCursor(owner, xxx, yyy);
                    }

                }
            }
        }

        /** Draws current ship shape around the cursor (used when placing the ship).
         * 
         * \param owner     Reference to the owning UI.
         * \param originX         X origin coord.
         * \param originY         Y origin coord.
         */
        private void DrawShipAroundCursor(CmdUi owner, int originX, int originY)
        {
            // If ship bound to the cursor
            if (CurShip != null)
            {
                foreach (var c in CurShip.Fields)
                {
                    // Get absolute terminal coordinates
                    var xxx = c.X * Config.TerminalFieldWidth + originX;
                    var yyy = c.Y * Config.TerminalFieldHeight + originY;

                    var maxX = owner.FieldH * Config.TerminalFieldHeight;
                    var maxY = owner.FieldW  * Config.TerminalFieldWidth;

                    var finY = Math.Clamp(yyy, 0, maxX);

                    // Draw 3 horizontal terminal fields (width of one field)
                    for (int off = 0; off < 3; ++off)
                    {
                        var finX = Math.Clamp(xxx + off, 0, maxY);
                        owner.frameBuffer[finX, finY] = Config.ShipChar;
                    }
                }
            }
        }

        private static void DrawFieldContents(CmdUi owner, int originX, int originY, CellState[,] field)
        {
            for (int iy = 0; iy < owner.FieldW; ++iy)
            {
                for (int ix = 0; ix < owner.FieldH; ++ix)
                {
                    var content = field[ix, iy];

                    // Compute the offsets in real terminal cells
                    var ixx = ix * Config.TerminalFieldWidth + 1;
                    var iyy = iy * Config.TerminalFieldHeight;

                    if (content == CellState.SHIP || content == CellState.HIT_HIM || content == CellState.HIT_ME)
                    {
                        owner.frameBuffer[originX + ixx + 0, originY + iyy + 1] = content.ToChar();
                        owner.frameBuffer[originX + ixx + 1, originY + iyy + 1] = content.ToChar();
                        owner.frameBuffer[originX + ixx + 2, originY + iyy + 1] = content.ToChar();
                    }
                    else
                    {
                        owner.frameBuffer[originX + ixx + 0, originY + iyy + 1] = ' ';
                        owner.frameBuffer[originX + ixx + 1, originY + iyy + 1] = content.ToChar();
                        owner.frameBuffer[originX + ixx + 2, originY + iyy + 1] = ' ';
                    }
                }
            }
        }

        /** Draws the provided status text into the frame buffer (terminal). 
         * 
         * \param owner     Reference to the owning UI.
         * \param x         X origin coord.
         * \param y         Y origin coord.
         * \param status    Text to be rendered.
         */
        protected void DrawText(CmdUi owner, int x, int y, string status)
        {
            // Draw the text
            for (int i = 0; i < status.Length; ++i)
            {
                owner.frameBuffer[x + i, y] = status[i];
            }
        }

        /*
         * Member variables
         */
        /** Computes absolute position in terminal window of my grid origin. */
        protected (int, int) MyGridOrigin(CmdUi owner) => (2, 0);

        /** Computes absolute position in terminal window of the opponent's grid origin. */
        protected (int, int) EnemyGridOrigin(CmdUi owner) => (owner.FieldW * Config.TerminalFieldWidth + (2 * Config.TerminalFieldWidth), 0);

        /** Computes absolute position in terminal window of the status text origin. */
        protected (int, int) StatusOrigin(CmdUi owner) => (10, MyGridOrigin(owner).Item2 + owner.FieldH * Config.TerminalFieldHeight + 2);

        /** Current cursor position. */
        protected ValueTuple<int, int> CurPos { get; set; } = (Config.FieldWidth / 2, Config.FieldHeight / 2);

        /** Represents ship that is "bound" to the cursor. Null if none. */
        protected Ship CurShip { get; set; } = null;
    }

    /** Intermediate state for UI to go to if it wants to switch states. 
     *
     * This state tels user to wait. This is state that UI jumps to every
     * time event that changes states happen. There must be explicit command
     * from the logic to switch state from here.
     * 
     * In other words, UI itself cannot go to any other state than intermediate.
     */
    class InterState : ICmdUiState
    {
        /** Show that user needs to wait.
         *
         * \param owner Ownning UI instance.
         */
        public void Update(CmdUi owner)
        {
            Console.Clear();

            Console.WriteLine(Config.Strings.Working);

            Thread.Sleep(Config.UpdateWait);
        }
    }

    /** State for decision between client/server version. */
    class InitialState : ICmdUiState
    {
        /** Shows initial state - chose between server/client.
         *
         * \param owner Ownning UI instance.
         */
        public void Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine(Config.ScreenStrings.InitialScreen());

            // Loop waiting for an input
            while (true)
            {
                // Wait for the input
                var key = owner.PollKey();

                if (key.KeyChar == '1')
                {
                    Console.Clear();
                    Console.WriteLine(Config.ScreenStrings.SelectPortScreen());

                    int port = Config.Port;
                    var line = Console.ReadLine();

                    // If default address is desired
                    if (line.Length != 0)
                    {
                        try
                        {
                            port = int.Parse(line);
                            Logger.LogI($"Using port: {port}");
                        }
                        catch (FormatException)
                        {
                            port = Config.Port;
                            Logger.LogE("Invalid address:" + line);
                            Logger.LogI($"Using default port: {port}");
                        }
                    }

                    // ----------------------
                    // Launch the SERVER
                    Program.LaunchServer(owner, port);
                    // ----------------------

                    break;
                }
                else if (key.KeyChar == '2')
                {
                    Console.Clear();
                    Console.WriteLine(Config.ScreenStrings.SelectAddressScreen());
    
                    // Get default values
                    string IP = Config.Ip;
                    int port = Config.Port;

                    var line = Console.ReadLine();

                    // If default address is desired
                    if (line.Length != 0)
                    {
                        var toks = line.Split(':');
                        IP = toks[0];
                        try
                        {
                            port = int.Parse(toks[1]);
                        }
                        catch (FormatException)
                        {
                            port = Config.Port;
                            Logger.LogE($"Invalid address '{line}'.");
                            Logger.LogI($"Using default '{IP}:{port}'.");
                        }
                    }

                    // ----------------------
                    // Launch the CLIENT
                    Program.LaunchClient(owner, IP, port);
                    // ----------------------
                    break;
                }

                else if (key.KeyChar == 'q')
                {
                    owner.Shutdown();
                }
            }

            // Go to interstate
            owner.GotoState(UiState.INTER);
        }
    }

    /** State for phase of connecting to the server. */
    class ClientConnectionState : ICmdUiState
    {
        /** Shows that client is connecting to the server.
         *
         * \param owner Ownning UI instance.
         */
        public void Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine(Config.ScreenStrings.ConnectingScreen());

            Thread.Sleep(Config.UpdateWait * 10);
        }
    }

    /** State for waiting for the client phase. */
    class ServerWaitingState : ICmdUiState
    {
        /** Shows that we're waiting for a client connection.
         *
         * \param owner Ownning UI instance.
         */
        public void Update(CmdUi owner)
        {

            Console.Clear();
            Console.WriteLine(Config.ScreenStrings.WaitingForConnectionScreen());

            Thread.Sleep(Config.UpdateWait * 10);
        }
    }

    /** State where I have to place all the ships. */
    class PlacingShipsState : RenderingCmdUiState
    {
        /*
         * Methods
         */
        /** Ctor initializes ships we will need to place. */
        public PlacingShipsState()
        {
            // Iterate over all ships that needs to be placed
            foreach (var s in Config.ShipsToPlace)
            {
                ShipsToPlace.Add(s);
            }
        }

        /** Handles update stete for placing the ships.
         *
         * \param owner Ownning UI instance.
         */
        public override void Update(CmdUi owner)
        {
            // Get absolute origin point coordinates
            var myGridOrigin = MyGridOrigin(owner);
            var enemyGridOrigin = EnemyGridOrigin(owner);
            var statusOrigin = StatusOrigin(owner);

            // Place the ship onto the cursor if stil placing
            CurShip = ShipsToPlace.Count > 0 ? ShipsToPlace[0] : null;

            // Draw grid for me
            DrawGrid(owner, myGridOrigin.Item1, myGridOrigin.Item2, Config.Strings.MyFieldLabel, owner.myField, true);

            // Draw grid for the enemy
            DrawGrid(owner, enemyGridOrigin.Item1, enemyGridOrigin.Item2, Config.Strings.EnemyFieldLabel, owner.enemyField, false);

            // Draw status to the UI
            string pluralSuffix = ShipsToPlace.Count != 1 ? "s" : "";
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2, Config.Strings.PlacingShips + $"\t {ShipsToPlace.Count} ship{pluralSuffix} to place left.");
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2 + 1, Config.Strings.PlacingShipsInstruction);

            // Render it
            owner.SwapBuffers();

            // Handle common input
            var input = owner.PollKey();
            HandleInput(owner, input.Key.ToString());
            // Handle input specific to placing ships
            HandlePlacingInput(owner, input.Key.ToString());

            // Are all ships placed?
            if (ShipsToPlace.Count == 0)
            {
                // Confirm it
                owner.Logic.PlaceShips();
            }
        }

        /** Handles input for placing the ships logic. 
         * 
         * \param owner         Owning UI.
         * \param keyString     String decribing the pressed key.
         */
        protected void HandlePlacingInput(CmdUi owner, string keyStr)
        {
            // Don't do anyhting if nothing to place.
            if (ShipsToPlace.Count == 0)
            {
                return;
            }

            switch (keyStr)
            {
            case "Spacebar":
                // Place this ship
                owner.Logic.PlaceShip(CurPos.Item1, CurPos.Item2, ShipsToPlace[0]);

                // Remove it fron the queue
                ShipsToPlace.RemoveAt(0);
                break;
            }
        }


        /*
         * Member variables
         */
        /** Remaining ships to place into the field. */
        public List<Ship> ShipsToPlace { get; set; } = new List<Ship>();
    }

    /** State where I choose field to shoot at. */
    class YourTurnState : RenderingCmdUiState
    {
        /** Handles update for state of my turn.
         *
         * I see both playfields, and I have cursor for shooting.
         * 
         * \param owner Ownning UI instance.
         */
        public override void Update(CmdUi owner)
        {
            var myGridOrigin = MyGridOrigin(owner);
            var enemyGridOrigin = EnemyGridOrigin(owner);
            var statusOrigin = StatusOrigin(owner);

            // Draw grid for me
            DrawGrid(owner, myGridOrigin.Item1, myGridOrigin.Item2, Config.Strings.MyFieldLabel, owner.myField, false);

            // Draw grid for the enemy
            DrawGrid(owner, enemyGridOrigin.Item1, enemyGridOrigin.Item2, Config.Strings.EnemyFieldLabel, owner.enemyField, true);

            // Draw status texts
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2, Config.Strings.MyTurn);
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2 + 1, Config.Strings.MyTurnInstruction);

            // Render it
            owner.SwapBuffers();

            // Handle input
            var input = owner.PollKey();
            HandleInput(owner, input.Key.ToString());
        }
    }

    /** State where I wait for opponent to strike. */
    class OpponetsTurnState : RenderingCmdUiState
    {
        /** Handles update for state of opponents turn.
         * 
         * I see both playfields, but I dont have any cursor.
         *
         * \param owner Ownning UI instance.
         */
        public override void Update(CmdUi owner)
        {
            // Got absolut coordinates for UI elements
            var myGridOrigin = MyGridOrigin(owner);
            var enemyGridOrigin = EnemyGridOrigin(owner);
            var statusOrigin = StatusOrigin(owner);

            // Draw grid for me
            DrawGrid(owner, myGridOrigin.Item1, myGridOrigin.Item2, Config.Strings.MyFieldLabel, owner.myField, false);

            // Draw grid for the enemy
            DrawGrid(owner, enemyGridOrigin.Item1, enemyGridOrigin.Item2, Config.Strings.EnemyFieldLabel, owner.enemyField, false);

            // Draw status texts
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2, Config.Strings.OpponentsTurn);
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2 + 1, Config.Strings.OpponentsTurnInstruction);

            // Render it
            owner.SwapBuffers();

            // We don't need the input, we just need to block the thread to avoid excessive rerendering
            owner.PollKey();
        }
    }

    /** State showing the result of the game or error that happened. */
    class FinalState : ICmdUiState
    {
        /**
         * Methods
         */
        /** Consturct this state with the message to show to the user. */
        public FinalState(string m)
        {
            Msg = m;
        }

        /** Updates the UI showing end of the game. 
         *
         * \param owner Ownning UI instance.
         */
        public void Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine(Config.ScreenStrings.FinalScreen(Msg));
            
            // Wait for an input
            while (true)
            {
                // Wait for the input
                var key = owner.PollKey();
                if (key.KeyChar == 'q')
                {
                    owner.Shutdown();
                    break;
                }
            }
        }

        /**
         * Member variables
         */
         /** Message to be displayed. */
        private string Msg { get; } = "";
    }
}
