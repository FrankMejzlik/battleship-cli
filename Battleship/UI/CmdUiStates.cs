using Battleship.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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

        protected void drawGrid(CmdUi owner, int originX, int originY, string title, eCellState[,] field, bool cursor)
        {
            // Draw text
            for (int i = 0; i < title.Length; ++i)
            {
                owner.frameBuffer[originX + i, originY] = title[i];
            }

            // Draw grid
            ++originY;
            for (int iy = 0; iy < owner.FieldW; ++iy)
            {
                var iyy = iy * 2;
                for (int ix = 0; ix <= owner.FieldH; ++ix)
                {
                    var ixx = ix * 4;
                    owner.frameBuffer[originX + ixx, originY + iyy] = '|';
                    owner.frameBuffer[originX + ixx, originY + iyy + 1] = '|';
                }
            }
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
                }
            }

            // Draw contents
            for (int iy = 0; iy < owner.FieldW; ++iy)
            {
                var iyy = iy * 2;
                for (int ix = 0; ix < owner.FieldH; ++ix)
                {
                    var ixx = ix * 4 + 1;

                    var content = field[iy, ix];

                    if (content == eCellState.SHIP || content == eCellState.HIT_HIM || content == eCellState.HIT_ME)
                    {
                        owner.frameBuffer[originX + ixx + 0, originY + iyy + 1] = owner.CellStateToString(content);
                        owner.frameBuffer[originX + ixx + 1, originY + iyy + 1] = owner.CellStateToString(content);
                        owner.frameBuffer[originX + ixx + 2, originY + iyy + 1] = owner.CellStateToString(content);
                    }
                    else
                    {
                        owner.frameBuffer[originX + ixx + 0, originY + iyy + 1] = ' ';
                        owner.frameBuffer[originX + ixx + 1, originY + iyy + 1] = owner.CellStateToString(content);
                        owner.frameBuffer[originX + ixx + 2, originY + iyy + 1] = ' ';
                    }
                }
            }


            // Draw cursor
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

        protected void drawStatus(CmdUi owner, string status)
        {
            // Draw text
            for (int i = 0; i < status.Length; ++i)
            {
                var x = 10;
                var y = owner.FieldH * 2 + 2;
                owner.frameBuffer[x + i, y] = status[i];
            }

        }

        protected ValueTuple<int, int> CurPos { get; set; } = (Config.FieldWidth / 2, Config.FieldHeight / 2);


        protected Ship CurShip { get; set; } = null;
    }

    class InterState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine("Working...");
            Thread.Sleep(50);

            return true;
        }
    }

    class InitialState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\tDo you want to launch a server or connect as a client?");
            Console.WriteLine("  ----------------------------------------------------------------  ");
            Console.WriteLine();
            Console.WriteLine("\t  1) SERVER");
            Console.WriteLine("\t  2) CLIENT");

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
            owner.GotoState(eUiState.INTER, "From INITIAL.");

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
            // Draw grid for me
            // Active cursor here -> you're placing
            drawGrid(owner, 1, 0, "ME:", owner.myField, true);

            // Draw grid for the enemy
            drawGrid(owner, owner.FieldW * 4 + 4, 0, "ENEMY: ", owner.enemyField, false);

            drawStatus(owner, "Placing ships");

            if (ShipsToPlace.Count > 0)
            {
                CurShip = ShipsToPlace[0];
            }

            owner.SwapBuffers();
            var input = owner.PollKey();
            if (ShipsToPlace.Count > 0)
            {
                HandleInput(owner, input.Key.ToString());
                HandlePlacingInput(owner, input.Key.ToString());
            }
            else
            {
                owner.Logic.PlaceShips();
            }





            return true;
        }

        protected void HandlePlacingInput(CmdUi owner, string keyStr)
        {
            switch (keyStr)
            {
            case "Spacebar":
                // If placing, place the ship
                if (owner.State is PlacingShipsState)
                {
                    owner.Logic.PlaceShip(CurPos.Item1, CurPos.Item2, ShipsToPlace[0]);
                    ShipsToPlace.RemoveAt(0);
                }
                break;
            }
        }

        public List<Ship> ShipsToPlace { get; set; } = new List<Ship>();
    }

    class YourTurnState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
        {
            // Draw grid for me
            drawGrid(owner, 1, 0, "ME:", owner.myField, false);

            // Draw grid for the enemy
            // Active cursor here -> you're shooting
            drawGrid(owner, owner.FieldW * 4 + 4, 0, "ENEMY: ", owner.enemyField, true);

            drawStatus(owner, "YOUR TURN! Aim with arrows and SHOOT with the SPACE key!");

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
            // Draw grid for me
            drawGrid(owner, 1, 0, "ME:", owner.myField, false);

            // Draw grid for the enemy
            drawGrid(owner, owner.FieldW * 4 + 4, 0, "ENEMY: ", owner.enemyField, false);

            drawStatus(owner, "OPPONENT'S TURN.");

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
            Console.WriteLine("\tEND OF THE GAME");
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
