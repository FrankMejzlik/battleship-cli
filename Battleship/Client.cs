
using Battleship.Common;
using Battleship.Logic;
using Battleship.Services;
using Battleship.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    /** Class defining the client in the Battleships game.
     * 
     * Client is only a slave. It just obeys what server commands. 
     * Client can do almost nothing without server explicitly commanding
     * him to do it. 
     * 
     * It holds no data about state of the game EXCEPT where the client's 
     * ships are and his shot reponses (we need this for the UI rendering).
     */
    public class Client : ILogic
    {
        /** Client must be constructed with the target server it will connect to and IUi to use.  */
        public Client(string host, int port, IUi ui)
        {
            Host = host;
            Port = port;
            Ui = ui;

            // We are the client
            client = new TcpClient();
        }

        /** Properly terminates the client and lets the server know about it (if needed). */
        public void Shutdown()
        {
            // No multiple destrucitons
            if (IsClientDesstructed)
            {
                return;
            }

            // Set the running flag
            ShouldRun = false;

            // Send info about the end to the client
            PacketService.SendPacket(new Packet(PacketType.FIN), client, Shutdown);

            // Write client game log
            System.IO.File.WriteAllText(Config.ClientGameLogFilepath, GameLog.ToString());

            // Close the TCP client
            try
            {
                client.GetStream().Close();
                client.Close();
            }
            // If the client is disposed already
            catch (ObjectDisposedException) { }

            // ------------------------ UI -----------------------------
            // Shutdown the UI
            Ui.Shutdown();
            // ------------------------ UI -----------------------------

            // Set the dctor flag
            IsClientDesstructed = true;

            Logger.LogI($"Client destructed.");
        }

        public void Connect()
        {
            Logger.LogI($"Connecting to the server...");

            try
            {
                // Make sure that UI is interstate
                while (!Ui.IsInInterstate)
                {
                    Thread.Sleep(100);
                }

                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.CLIENT_CONNECTING);
                // ------------------------ UI -----------------------------

                // Do the connecting
                client.Connect(Host, Port);
            }
            catch (SocketException ex)
            {
                Logger.LogI($"Cannot connect to the server with the message '{ex.Message}'.");

                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.FINAL, Config.Strings.ErrCannotConnectToTheServer);
                // ------------------------ UI -----------------------------
            }

            // Check if everything went OK
            if (client.Connected)
            {
                Logger.LogI($"Connected to the server at {client.Client.RemoteEndPoint}");

                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.PLACING_SHIPS);
                // ------------------------ UI -----------------------------

                // Now listen to the server
                ListenToTheServer();
            }
            // There was some problem
            else
            {
                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.FINAL, Config.Strings.ErrConnectingFailed);
                // ------------------------ UI -----------------------------
            }
        }


        public void ListenToTheServer()
        {
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(client, Shutdown);

                // Act on recieving a valid packet
                if (packet != null)
                {
                    // Message 
                    if (packet.Type == PacketType.MESSAGE)
                    {
                        Logger.LogI($"Message received: {packet.Data}");
                    }
                    // End of the program
                    else if (packet.Type == PacketType.FIN)
                    {
                        Logger.LogI($"Game forcefully terminated.");

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.FINAL, Config.Strings.ForcedExit);
                        // ------------------------ UI -----------------------------
                    }
                    // Timeout
                    else if (packet.Type == PacketType.TIMED_OUT)
                    {
                        Logger.LogI(Config.Strings.Timeout);

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.FINAL, Config.Strings.Timeout);
                        // ------------------------ UI -----------------------------
                    }
                    // Your turn
                    else if (packet.Type == PacketType.YOUR_TURN)
                    {
                        Logger.LogI($"Command to YOUR TURN.");

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.YOUR_TURN);
                        // ------------------------ UI -----------------------------
                    }
                    // Opponent's turn
                    else if (packet.Type == PacketType.OPPONENTS_TURN)
                    {
                        Logger.LogI($"Command to MY TURN.");

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.OPPONENTS_TURN);
                        // ------------------------ UI -----------------------------
                    }
                    // Client won
                    else if (packet.Type == PacketType.YOU_WIN)
                    {
                        Logger.LogI($"Client won.");

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.FINAL, packet.Data);
                        // ------------------------ UI -----------------------------
                    }
                    // Client lost
                    else if (packet.Type == PacketType.YOU_LOSE)
                    {
                        Logger.LogI($"Server won.");

                        // ------------------------ UI -----------------------------
                        Ui.GotoState(UiState.FINAL, packet.Data);
                        // ------------------------ UI -----------------------------
                    }
                    // Server shot at me
                    else if (packet.Type == PacketType.FIRE)
                    {
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        Logger.LogI($"Enemy fired to field {coordsFired}");
                        Logger.LogI(fireResponse);


                        // ------------------------ UI -----------------------------
                        var coords = Utils.FromExcelCoords(coordsFired);
                        if (fireResponse == Config.Strings.Water)
                        {
                            Ui.HandleMissedMe(coords.Item1, coords.Item2);
                        }
                        else
                        {
                            Ui.HandleHitMe(coords.Item1, coords.Item2);
                        }
                        // ------------------------ UI -----------------------------

                    }
                    // I shot at server and he responded
                    else if (packet.Type == PacketType.FIRE_REPONSE)
                    {
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        Logger.LogI(fireResponse);

                        // ------------------------ UI -----------------------------
                        var coords = Utils.FromExcelCoords(coordsFired);
                        if (fireResponse == Config.Strings.Water)
                        {
                            Ui.HandleMissHimtAt(coords.Item1, coords.Item2);
                        }
                        else
                        {
                            Ui.HandleHitHimAt(coords.Item1, coords.Item2);
                        }
                        // ------------------------ UI -----------------------------
                    }
                }
            }
        }

        public void PlaceShips()
        {
            // Send those ships to the server
            var clientShipsSerialized = JsonConvert.SerializeObject(ClientShips);

            PacketService.SendPacket(new Packet(PacketType.SET_CLIENT_SHIPS, clientShipsSerialized), client, Shutdown);
        }

        public void SendMessage(string message)
        {
            PacketService.SendPacket(new Packet(PacketType.MESSAGE, message), client, Shutdown);
            Logger.LogI($"Send message '{message}'.");
        }

        public void FireAt(int x, int y)
        {
            // Convert to the Excel coordinates
            var strCoords = Utils.ToExcelCoords(x, y);

            PacketService.SendPacket(new Packet(PacketType.FIRE, strCoords), client, Shutdown);
            Logger.LogI($"Firing at the '{strCoords}' field.");
        }

        /**
         * Place the ship at provided coordinates.
         * 
         * This is only local action. Result is send to the server
         * once the placing is done.
         * 
         * \param x     X coordinate of the ship origin point.
         * \param y     Y coordinate of the ship origin point.
         * \param ship  Structure describing the ship (\ref Ship).
         */
        public void PlaceShip(int x, int y, Ship ship)
        {
            // Adjust coordinates
            Ship s = new Ship();
            foreach (var f in ship.Fields)
            {
                int newX = f.X + x;
                int newY = f.Y + y;

                Field newF = new Field(newX, newY);
                s.Fields.Add(newF);
            }
            ClientShips.Add(s);

            // Put into the UI
            foreach (var field in s.Fields)
            {
                var coords = Utils.FromExcelCoords(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);

                field.IsShip = true;
            }
        }

        /*
         * Member variables
         */


        private List<Ship> ClientShips { get; set; } = new List<Ship>();

        private StringBuilder GameLog { get; set; } = new StringBuilder();
        private string Host { get; set; }
        private int Port { get; set; }
        private IUi Ui { get; set; }

        private readonly TcpClient client;

        public bool ShouldRun { get; set; } = true;
        private bool IsClientDesstructed { get; set; } = false;
    }
}
