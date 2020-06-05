
using Battleship.Common;
using Battleship.Logic;
using Battleship.Services;
using Battleship.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
            TcpClient = new TcpClient();
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
            PacketService.SendPacket(new Packet(PacketType.FIN), TcpClient, () => { });


            // Close the TCP client
            try
            {
                TcpClient.Dispose();
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

        /** Connects to the server or fails if something happens. */
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
                TcpClient.Connect(Host, Port);
            }
            catch (SocketException ex)
            {
                Logger.LogI($"Cannot connect to the server with the message '{ex.Message}'.");

                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.FINAL, Config.Strings.ErrCannotConnectToTheServer);
                // ------------------------ UI -----------------------------
            }

            // Check if everything went OK
            if (TcpClient.Connected)
            {
                Logger.LogI($"Connected to the server at {TcpClient.Client.RemoteEndPoint}");

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

        /** Starts the network listening loop where we accept packets from the server. */
        public void ListenToTheServer()
        {
            // Main loop
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(TcpClient, Shutdown);

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

        /** Places the selected ships and sends then to the server. */
        public void PlaceShips()
        {
            // Send those ships to the server
            var clientShipsSerialized = JsonConvert.SerializeObject(ClientShips);

            PacketService.SendPacket(new Packet(PacketType.SET_CLIENT_SHIPS, clientShipsSerialized), TcpClient, Shutdown);
        }


        /** Fires at the provided coordinates. 
         * 
         * \param x     X coordinate of the ship origin point.
         * \param y     Y coordinate of the ship origin point.
         */
        public void FireAt(int x, int y)
        {
            // Convert to the Excel coordinates
            var strCoords = Utils.ToExcelCoords(x, y);

            Logger.LogI($"Firing at the '{strCoords}' field.");

            // Let server know
            PacketService.SendPacket(new Packet(PacketType.FIRE, strCoords), TcpClient, Shutdown);
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
                if (newX < 0 || newX >= Config.FieldWidth - 1)
                {
                    continue;
                }
                int newY = f.Y + y;
                if (newY < 0 || newY >= Config.FieldHeight - 1)
                {
                    continue;
                }

                // Check uniqeness
                var isShipAlready = ClientShips.Any(ship =>
                   ship.Fields.Any(field =>
                       field.Coords.Equals(Utils.ToExcelCoords(newX, newY))
                   )
                );

                if (!isShipAlready)
                {
                    Field newF = new Field(newX, newY);
                    s.Fields.Add(newF);
                }
            }
            // Handle empty ships
            if (s.Fields.Count == 0)
            {
                s.IsSunk = true;
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
        /** Ships of the client. */
        private List<Ship> ClientShips { get; set; } = new List<Ship>();

        /** Address of the server. */
        private string Host { get; set; }

        /** Port of the server. */
        private int Port { get; set; }

        /** Instance of the UI. */
        private IUi Ui { get; set; }

        /** TCP client used for communication with the server. */
        private TcpClient TcpClient { get; set; }

        /** Indicates that app should run. */
        public bool ShouldRun { get; set; } = true;

        /** Indicates if the client has been already destructed. */
        private bool IsClientDesstructed { get; set; } = false;
    }
}
