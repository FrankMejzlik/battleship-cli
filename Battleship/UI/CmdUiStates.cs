
using Battleship.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Battleship.UI
{
    public interface ICmdUiState
    {
        public abstract bool Update(CmdUi owner);
    }


    public class RenderingCmdUiState : ICmdUiState
    {
        public virtual bool Update(CmdUi owner)
        {
            return true;
        }

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

                        owner.frameBuffer[xxx + 0, yyy] = '<';
                        owner.frameBuffer[xxx + 1, yyy] = '+';
                        owner.frameBuffer[xxx + 2, yyy] = '>';

                        if (CurShip != null)
                        {
                            foreach (var c in CurShip.Fields)
                            {
                                var cx = c.X * 4;
                                var cy = c.Y * 2;

                                try
                                {
                                    owner.frameBuffer[xxx + cx + 0, yyy + cy] = 'O';
                                    owner.frameBuffer[xxx + cx + 1, yyy + cy] = 'O';
                                    owner.frameBuffer[xxx + cx + 2, yyy + cy] = 'O';
                                }
                                catch (IndexOutOfRangeException) { }
                            }
                        }
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

        /** Computes absolute position in terminal window of my grid origin. */
        protected (int, int) MyGridOrigin(CmdUi owner) => (2, 0);

        /** Computes absolute position in terminal window of the opponent's grid origin. */
        protected (int, int) EnemyGridOrigin(CmdUi owner) =>  (owner.FieldW * Config.TerminalFieldWidth + (2 * Config.TerminalFieldWidth), 0);

        /** Computes absolute position in terminal window of the status text origin. */
        protected (int, int) StatusOrigin(CmdUi owner) =>  (10, MyGridOrigin(owner).Item2 + owner.FieldH * Config.TerminalFieldHeight + 2);
        
        /** Current cursor position. */
        protected ValueTuple<int, int> CurPos { get; set; } = (Config.FieldWidth / 2, Config.FieldHeight / 2);

        /** Represents ship that is "bound" to the cursor. Null if none. */
        protected Ship CurShip { get; set; } = null;
    }

    class InterState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine(Config.Strings.Working);
            Thread.Sleep(50);

            return true;
        }
    }

    class InitialState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine(Config.ScreenStrings.InitialScreen);

            // Loop waiting for an input
            while (true)
            {
                // Wait for the input
                var key = owner.PollKey();

                if (key.KeyChar == '1')
                {
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine($"\tOn what port? (for default {Config.Port} hit ENTER)");
                    Console.WriteLine("  ----------------------------------------------------  ");
                    Console.WriteLine();

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
                    Console.WriteLine();
                    Console.WriteLine($"\tWhat's the address? (for default {Config.Ip}:{Config.Port} hit ENTER)");
                    Console.WriteLine("  ------------------------------------------------------------------------  ");
                    Console.WriteLine();


                    string IP = "127.0.0.1";
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
                            Logger.LogE("Invalid address:" + line);
                            Logger.LogI($"Using default: {IP}:{port}");
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

            return true;
        }
    }

    class ClientConnectionState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Connecting to the server...");
            Thread.Sleep(50);

            return true;
        }
    }

    class ServerWaitingState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Waiting for the client to connect...");
            Thread.Sleep(50);

            return true;
        }
    }

    class PlacingShipsState : RenderingCmdUiState
    {
        public PlacingShipsState()
        {
            foreach (var s in Config.ShipsToPlace)
            {
                ShipsToPlace.Add(s);
            }
        }

        public override bool Update(CmdUi owner)
        {
            var myGridOrigin = MyGridOrigin(owner);
            var enemyGridOrigin = EnemyGridOrigin(owner);
            var statusOrigin = StatusOrigin(owner);


            // Place it to the cursor
            CurShip = ShipsToPlace.Count > 0 ? ShipsToPlace[0] : null;

            // Draw grid for me
            DrawGrid(owner, myGridOrigin.Item1, myGridOrigin.Item2, Config.Strings.MyFieldLabel, owner.myField, true);
                
            // Draw grid for the enemy
            DrawGrid(owner, enemyGridOrigin.Item1, enemyGridOrigin.Item2, Config.Strings.EnemyFieldLabel, owner.enemyField, false);

            // Draw status to the UI
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2, Config.Strings.PlacingShips + $"\t {ShipsToPlace.Count} ships to place left.");
            DrawText(owner, statusOrigin.Item1, statusOrigin.Item2 + 1, Config.Strings.PlacingShipsInstruction);

            owner.SwapBuffers();
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

            return true;
        }

        protected void HandlePlacingInput(CmdUi owner, string keyStr)
        {
            if (ShipsToPlace.Count == 0)
            {
                return;
            }

            switch (keyStr)
            {
            case "Spacebar":
                
                owner.Logic.PlaceShip(CurPos.Item1, CurPos.Item2, ShipsToPlace[0]);
                ShipsToPlace.RemoveAt(0);
                break;
            }
        }

        public List<Ship> ShipsToPlace { get; set; } = new List<Ship>();
    }

    class YourTurnState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
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

            owner.SwapBuffers();
            var input = owner.PollKey();
            HandleInput(owner, input.Key.ToString());

            return true;
        }
    }

    class OpponetsTurnState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
        {
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

            owner.SwapBuffers();
            var input = owner.PollKey();

            return true;
        }
    }

    class FinalState : ICmdUiState
    {
        public FinalState(string m)
        {
            msg = m;
        }

        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("                     END OF THE GAME");
            Console.WriteLine("  ----------------------------------------------------------  ");
            Console.WriteLine();
            Console.WriteLine("\t == " + msg + " == ");
            Console.WriteLine();

            Console.WriteLine("\t\tq) EXIT");

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

            return false;
        }

        string msg = "";
    }
}
