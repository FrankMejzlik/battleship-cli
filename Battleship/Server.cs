
using Battleship.Models;
using Battleship.Services;
using Battleship.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    public interface Logic
    {
        public void Shutdown();
        void FireAt(int x, int y);
        void PlaceShip(int item2, int item1, Ship ship);

        public void PlaceShips();

        public bool ShouldRun { get; set; }
    }

    public class Server : Logic
    {
        public Server(int port, IUi ui)
        {
            Destructed = false;
            ShouldRun = true;

            Port = port;
            Ui = ui;

            // We are the server
            listener = new TcpListener(IPAddress.Any, Port);
        }


        public void Shutdown()
        {
            // No multiple shutdowns
            if (Destructed)
            {
                return;
            }

            // Send info about the end to the client
            if (!PacketService.SendPacket(new Packet(ePacketType.FIN), client))
            {
                Shutdown();
            }

            // Flush the client game log
            System.IO.File.WriteAllText(Config.ClientGameLogFilepath, GameLog.ToString());

            // Shutdown the UI
            Ui.Shutdown();

            // Set the running flag
            ShouldRun = false;

            // Close the TCP client
            try
            {
                client.GetStream().Close();
                client.Close();
            }
            catch (ObjectDisposedException)
            { }

            // Stop the listener
            listener.Stop();

            Destructed = true;
        }

        internal void HandleTimeout()
        {
            // Send info about the end to the client
            PacketService.SendPacket(new Packet(ePacketType.TIMED_OUT), client);

            Ui.GotoState(eUiState.FINAL, Config.StrTimeout);
        }

        public void Start()
        {
            try
            {
                WriteLog($"Starting the server on port {Port}...");
                listener.Start();
            }
            catch (SocketException)
            {
                WriteLog($"Cannot start server on port {Port}, the port is already taken.");
                return;
            }
            catch (Exception ex)
            {
                WriteLog($"Cannot start server on port {Port}: " + ex.Message);
                return;
            }

            var msg = $"Server started. Waiting for incomming connections...";
            // UI => SERVER_WAITING
            Ui.GotoState(eUiState.SERVER_WAITING, msg);

            client = listener.AcceptTcpClient();

            msg = $"New connection from {client.Client.RemoteEndPoint}.";

            // UI => PLACING_SHIPS
            Ui.GotoState(eUiState.PLACING_SHIPS, msg);
            WriteLog(msg);

            // Start the timeout timer here
            Timer.Ping();

            // Wait until server places all the ships
            while (ServerShips.Count != Config.ShipsToPlace.Count && ShouldRun)
            {
                Thread.Sleep(100);
            }

            // Now listen to the client
            ListenToTheClient();
        }

        public void ListenToTheClient()
        {
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(client);

                if (packet != null)
                {
                    // Message
                    if (packet.Type == ePacketType.MESSAGE)
                    {
                        WriteLog($"Message received: {packet.Data}");
                    }
                    // End of the program
                    else if (packet.Type == ePacketType.FIN)
                    {
                        WriteLog($"Game forcefully terminated.");

                        Ui.GotoState(eUiState.FINAL, "Forcefull game termination.");
                    }
                    // Client fires at
                    else if (packet.Type == ePacketType.FIRE)
                    {
                        var coordsFired = packet.Data;

                        HandleClientFireAt(coordsFired);
                    }
                    // Set client ships
                    else if (packet.Type == ePacketType.SET_CLIENT_SHIPS)
                    {
                        var clientShips = JsonConvert.DeserializeObject<List<Ship>>(packet.Data);

                        HandleClientSetShips(clientShips);
                    }
                }
            }
        }

        private int CheckFinished()
        {
            // Find any unsunk ship
            bool clientWon = !(ServerShips.Any(ship =>
              ship.IsSunk == false
            ));
            if (clientWon)
            {
                return 1;
            }

            bool servertWon = !(ClientShips.Any(ship =>
              ship.IsSunk == false
            ));
            if (servertWon)
            {
                return -1;
            }
            return 0;
        }

        /**
         * Client provided locations of the ships - this is start of the game itself.
         */
        private void HandleClientSetShips(List<Ship> clientShips)
        {
            ClientShips = clientShips;

            WriteLog($"Client set ships.");

            // Game starts here <=

            if (MyTurn)
            {
                Ui.GotoState(eUiState.YOUR_TURN);
                if (!PacketService.SendPacket(new Packet(ePacketType.OPPONENTS_TURN), client))
                {
                    Shutdown();
                }
            }
            else
            {
                Ui.GotoState(eUiState.OPPONENTS_TURN);
                if (!PacketService.SendPacket(new Packet(ePacketType.YOUR_TURN), client))
                {
                    Shutdown();
                }
            }
        }

        /**
         * Handles event that client fired at the server.
         */
        private void HandleClientFireAt(string coordsFired)
        {
            var coords = Utils.ToNumericCoordinates(coordsFired);

            // If not client's turn
            if (MyTurn)
            {
                Logger.LogW("Client shooting while not it's turn.");

                // We ignore it
                return;
            }

            var fireResponse = FireField(coordsFired);

            WriteLog($"Enemy fired to field {coordsFired}");

            // Notify the client
            if (!PacketService.SendPacket(new Packet(ePacketType.FIRE_REPONSE, $"{coordsFired}={fireResponse.ToFriendlyString()}"), client))
            {
                Shutdown();
            }

            WriteLog(fireResponse.ToFriendlyString());


            // ---------------------------------------------------------
            // Handle UI
            if (fireResponse == eFireResponseType.WATER)
            {
                Ui.HandleMissedMe(coords.Item1, coords.Item2);
            }
            else
            {
                Ui.HandleHitMe(coords.Item1, coords.Item2);
            }
            // ---------------------------------------------------------

            // -----------------
            // Turn switch
            TurnSwitch();
            // -----------------
        }


        private void PutShipsToUi(Ship ship)
        {
            foreach (var field in ship.Fields)
            {
                var coords = Utils.ToNumericCoordinates(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);

                field.IsShip = true;
            }
        }



        /**
         * Server fires, result is send to the client.
         */
        public void Fire(string coordsFired)
        {
            var coords = Utils.ToNumericCoordinates(coordsFired);

            WriteLog($"Firing to field {coordsFired}");

            // Find what lies at these coordinates
            var shipOnCoords = ClientShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            eFireResponseType fireResponse;
            // If server missed the client's ships
            if (shipOnCoords == null)
            {
                fireResponse = eFireResponseType.WATER;
            }
            // It's a hit
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);
                fieldOnCoords.IsRevealed = true;

                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);
                shipOnCoords.IsSunk = isShipDestroyed;

                if (isShipDestroyed)
                {
                    fireResponse = eFireResponseType.SUNK;
                }
                else
                {
                    fireResponse = eFireResponseType.HIT;
                }
            }

            // Tell the client what the server fired at
            if (!PacketService.SendPacket(new Packet(ePacketType.FIRE, $"{coordsFired}={fireResponse.ToFriendlyString()}"), client))
            {
                Shutdown();
            }

            // ---------------------------------------------------------
            // Handle UI
            if (fireResponse == eFireResponseType.WATER)
            {
                Ui.HandleMissHimtAt(coords.Item1, coords.Item2);
            }
            else
            {
                Ui.HandleHitHimAt(coords.Item1, coords.Item2);
            }
            // ---------------------------------------------------------



            // -----------------
            // Turn switch
            TurnSwitch();
            // -----------------

            WriteLog(fireResponse.ToFriendlyString());
        }

        private eFireResponseType FireField(string coordsFired)
        {
            eFireResponseType fireResponse;

            var coords = Utils.ToNumericCoordinates(coordsFired);


            var shipOnCoords = ServerShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );



            if (shipOnCoords == null)
            {
                fireResponse = eFireResponseType.WATER;
            }
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);
                fieldOnCoords.IsRevealed = true;
                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);

                shipOnCoords.IsSunk = isShipDestroyed;

                if (isShipDestroyed)
                {
                    fireResponse = eFireResponseType.SUNK;
                }
                else
                {
                    fireResponse = eFireResponseType.HIT;
                }
            }

            return fireResponse;
        }

        public void SendMessage(string message)
        {
            if (client == null)
            {
                return;
            }

            if (!PacketService.SendPacket(new Packet(ePacketType.MESSAGE, message), client))
            {
                Shutdown();
            }
            WriteLog("Message sent: " + message);
        }

        private delegate void WriteLogCallback(string text);

        private void WriteLog(string text)
        {
            GameLog.Append(text);
        }

        private void TurnSwitch()
        {
            // Check end of the game
            var winner = CheckFinished();
            Packet p;

            // Server won
            if (winner == -1)
            {
                string msg = "Server won!";
                Ui.GotoState(eUiState.FINAL, msg);
                p = new Packet(ePacketType.YOU_LOSE, msg);

                // Stop the timer
                Timer.Stop();
            }
            // Client won
            else if (winner == 1)
            {
                string msg = "Client won!";
                Ui.GotoState(eUiState.FINAL, msg);
                p = new Packet(ePacketType.YOU_WIN, msg);

                // Stop the timer
                Timer.Stop();
            }
            // Game continues
            else
            {

                // Rest the timer
                Timer.Ping();

                MyTurn = !MyTurn;

                if (MyTurn)
                {
                    Ui.GotoState(eUiState.YOUR_TURN);
                    p = new Packet(ePacketType.OPPONENTS_TURN);
                }
                else
                {
                    Ui.GotoState(eUiState.OPPONENTS_TURN);
                    p = new Packet(ePacketType.YOUR_TURN);
                }
            }

            if (!PacketService.SendPacket(p, client))
            {
                Shutdown();
            }
        }

        public void FireAt(int x, int y)
        {
            // Convert to excel coordinates
            var strCoords = Utils.GetCoords(x, y);

            Fire(strCoords);
        }

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
            ServerShips.Add(s);

            foreach (var field in s.Fields)
            {
                var coords = Utils.ToNumericCoordinates(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);

                field.IsShip = true;
            }

        }


        public void PlaceShips()
        {
            WriteLog("Server ships have been set.");
        }

        /*
         * Member variables
         */
        private bool MyTurn { get; set; } = false;

        public bool ShouldRun { get; set; } = true;
        private StringBuilder GameLog { get; set; } = new StringBuilder();
        private int Port { get; set; }
        private IUi Ui { get; set; }
        private List<Ship> ServerShips { get; set; } = new List<Ship>();
        private List<Ship> ClientShips { get; set; } = new List<Ship>();

        private readonly TcpListener listener;
        private TcpClient client;
        private bool Destructed { get; set; } = false;
    }
}