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

        protected void HandleInput(char c)
        {
            switch (c)
            {
            case 'w':
                CurPos = (CurPos.Item1, Math.Min(CurPos.Item2, Config.FieldHeight - 1));
                break;
            case 's':
                CurPos = (CurPos.Item1, Math.Max(CurPos.Item2, 0));
                break;
            case 'a':
                CurPos = (Math.Max(CurPos.Item1, 0), CurPos.Item2);
                break;
            case 'd':
                CurPos = (Math.Min(CurPos.Item1, Config.FieldWidth - 1), CurPos.Item2);
                break;
            }
        }

        protected void drawGrid(ref char[,] frameBuffer, int originX, int originY, string title)
        {
            // Draw text
            for (int i = 0; i < title.Length; ++i)
            {
                frameBuffer[originX + i, originY] = title[i];
            }

            // Draw edges
            ++originY;
            for (int iy = 0; iy < FieldW; ++iy)
            {
                var iyy = iy * 2;
                for (int ix = 0; ix <= FieldH; ++ix)
                {
                    var ixx = ix * 4;
                    frameBuffer[originX + ixx, originY + iyy] = '|';
                    frameBuffer[originX + ixx, originY + iyy + 1] = '|';
                }
            }
            for (int ix = 0; ix < FieldH; ++ix)
            {
                var ixx = ix * 4;
                for (int iy = 0; iy <= FieldW; ++iy)
                {
                    var iyy = iy * 2;
                    frameBuffer[originX + ixx, originY + iyy] = '-';
                    frameBuffer[originX + ixx + 1, originY + iyy] = '-';
                    frameBuffer[originX + ixx + 2, originY + iyy] = '-';
                    frameBuffer[originX + ixx + 3, originY + iyy] = '-';
                }
            }

        }

        protected void drawStatus(ref char[,] frameBuffer, string status)
        {
            // Draw text
            for (int i = 0; i < status.Length; ++i)
            {
                var x = 10;
                var y = FieldH * 2 + 2;
                frameBuffer[x + i, y] = status[i];
            }

        }
        protected eCellState[,] myField = new eCellState[Config.FieldHeight, Config.FieldWidth];
        protected eCellState[,] enemyField = new eCellState[Config.FieldHeight, Config.FieldWidth];
        protected int FieldW { get; set; } = Config.FieldWidth;
        protected int FieldH { get; set; } = Config.FieldHeight;
        protected ValueTuple<int, int> CurPos { get; set; } = (Config.FieldWidth / 2, Config.FieldHeight / 2);
    }

    class InterState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine("Working...");
            Thread.Sleep(50);

            // No rendering needed
            return false;
        }
    }

    class InitialState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\tDo you want to launch server or connect as a client?");
            Console.WriteLine("  ----------------------------------------------------------  ");
            Console.WriteLine();
            Console.WriteLine("\t\t1) SERVER");
            Console.WriteLine("\t\t2) CLIENT");

            while (true)
            {
                // Wait for the input
                var key = owner.pollKey();

                if (key.KeyChar == '1')
                {
                    break;
                }
                else if (key.KeyChar == '2')
                {
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine($"\tWhat's the address? ({Config.Ip}:{Config.Port})");
                    Console.WriteLine("  ----------------------------------------------------------  ");
                    Console.WriteLine();


                    string IP = "127.0.0.1";
                    int port = Config.Port;

                    var line = Console.ReadLine();

                    // If default address is desired
                    if (line.Length != 0)
                    {
                        var toks = line.Split(':');
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

            // No buffer swap needed
            return false;
        }
    }


    class ClientConnectionState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Connecting to the server...");
            Thread.Sleep(50);

            // No buffer swap needed
            return false;
        }
    }

    class ServerWaitingState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Waiting for the client to connect...");
            Thread.Sleep(50);

            // No buffer swap needed
            return false;
        }
    }

    class PlacingShipsState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
        {
            var input = owner.pollKey();

            // Draw grid for me
            drawGrid(ref owner.frameBuffer, 1, 0, "ME:");

            // Draw grid for the enemy
            drawGrid(ref owner.frameBuffer, FieldW * 4 + 4, 0, "ENEMY: ");

            drawStatus(ref owner.frameBuffer,"Placing ships");

            // Buffer swap needed
            return true;
        }
    }

    class YourTurnState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
        {
            var input = owner.pollKey();

            // Draw grid for me
            drawGrid(ref owner.frameBuffer, 1, 0, "ME:");

            // Draw grid for the enemy
            drawGrid(ref owner.frameBuffer, FieldW * 4 + 4, 0, "ENEMY: ");

            drawStatus(ref owner.frameBuffer,"YOUR TURN! Shoot!");

            // Buffer swap needed
            return true;
        }
    }

    class OpponetsTurnState : RenderingCmdUiState
    {
        public override bool Update(CmdUi owner)
        {
            var input = owner.pollKey();

            // Draw grid for me
            drawGrid(ref owner.frameBuffer, 1, 0, "ME:");

            // Draw grid for the enemy
            drawGrid(ref owner.frameBuffer, FieldW * 4 + 4, 0, "ENEMY: ");

            drawStatus(ref owner.frameBuffer,"OPPONENT'S TURN.");

            // Buffer swap needed
            return true;
        }
    }

    class FinalState : ICmdUiState
    {
        public bool Update(CmdUi owner)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\tEND OF THE GAME");
            Console.WriteLine("  ----------------------------------------------------------  ");
            Console.WriteLine();
            Console.WriteLine("\t\tq) EXIT");

            while (true)
            {
                // Wait for the input
                var key = owner.pollKey();
                if (key.KeyChar == 'q')
                {
                    owner.Shutdown();
                    break;
                }
            }

            // No buffer swap needed
            return false;
        }
    }
}
