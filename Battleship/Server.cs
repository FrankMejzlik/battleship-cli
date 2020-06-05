
using Battleship.Common;
using Battleship.Logic;
using Battleship.Services;
using Battleship.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Battleship
{
    /** Class defining the SERVER in the Battleships game.
     * 
     * The server is a master. State of the game is stored in his isntace.
     * Even when the server shoots, it just consults it with it's own state
     * and produces correct results (it does not cheat!).
     * 
     * It holds all the data about the current game. Only one game can 
     * run in parallel per one Server instance.
     */
    public class Server : ILogic
    {
        /** Client must be constructed with the target port and IUi to use. */
        public Server(int port, IUi ui)
        {
            IsServerDestructed = false;
            ShouldRun = true;

            Port = port;
            Ui = ui;

            // We are the server
            listener = new TcpListener(IPAddress.Any, Port);
        }


        public void Shutdown()
        {
            // No multiple shutdowns
            if (IsServerDestructed)
            {
                return;
            }

            // Set the running flag
            ShouldRun = false;

            // Stop timer
            Timer.Stop();

            // Handle TCP client and the other side
            if (client != null)
            {
                // Let the client know
                PacketService.SendPacket(new Packet(PacketType.FIN), client, Shutdown);

                // Close the TCP client
                try
                {
                    client.GetStream().Close();
                    client.Close();
                }
                // If the client is disposed already
                catch (ObjectDisposedException) { }
            }

            // Stop the listener
            listener.Stop();

            // ------------------------ UI -----------------------------
            // Shutdown the UI
            Ui.Shutdown();
            // ------------------------ UI -----------------------------

            // Set sctor flag
            IsServerDestructed = true;

            Logger.LogI($"Server destructed.");
        }

        internal void HandleTimeout()
        {
            // Send info about the end to the client
            PacketService.SendPacket(new Packet(PacketType.TIMED_OUT), client, Shutdown);

            Ui.GotoState(UiState.FINAL, Config.Strings.Timeout);
        }

        public void Start()
        {
            Logger.LogI($"Starting the server...");

            // Make sure that UI is interstate
            while (!Ui.IsInInterstate)
            {
                Thread.Sleep(100);
            }

            try
            {
                Logger.LogI($"Starting the server on the port '{Port}' ...");
                listener.Start();
            }
            catch (Exception ex)
            {
                Logger.LogI($"Cannot start the server on the port {Port}. Error message is '{ex.Message}'.");

                // ------------------------ UI -----------------------------
                Ui.GotoState(UiState.FINAL, Config.Strings.ErrServerCouldNotStart);
                // ------------------------ UI -----------------------------

                return;
            }

            Logger.LogI($"Server started. Waiting for incomming connection...");

            // ------------------------ UI -----------------------------
            Ui.GotoState(UiState.SERVER_WAITING, Config.Strings.WaitingForOpponent);
            // ------------------------ UI -----------------------------

            // Accept any incomming connection
            client = listener.AcceptTcpClient();

            Logger.LogI($"New connection from {client.Client.RemoteEndPoint}.");

            // ------------------------ UI -----------------------------
            Ui.GotoState(UiState.PLACING_SHIPS);
            // ------------------------ UI -----------------------------

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
                var packet = PacketService.ReceivePacket(client, Shutdown);

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

                        Ui.GotoState(UiState.FINAL, "Forcefull game termination.");
                    }
                    // Client fires at
                    else if (packet.Type == PacketType.FIRE)
                    {
                        var coordsFired = packet.Data;

                        HandleClientFireAt(coordsFired);
                    }
                    // Set client ships
                    else if (packet.Type == PacketType.SET_CLIENT_SHIPS)
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

            Logger.LogI($"Client set ships.");

            // Game starts here <=

            if (MyTurn)
            {
                Ui.GotoState(UiState.YOUR_TURN);
                PacketService.SendPacket(new Packet(PacketType.OPPONENTS_TURN), client, Shutdown);
            }
            else
            {
                Ui.GotoState(UiState.OPPONENTS_TURN);
                PacketService.SendPacket(new Packet(PacketType.YOUR_TURN), client, Shutdown);

            }
        }

        /**
         * Handles event that client fired at the server.
         */
        private void HandleClientFireAt(string coordsFired)
        {
            var coords = Utils.FromExcelCoords(coordsFired);

            // If not client's turn
            if (MyTurn)
            {
                Logger.LogW("Client shooting while not it's turn.");

                // We ignore it
                return;
            }

            var fireResponse = FireField(coordsFired);

            Logger.LogI($"Enemy fired to field {coordsFired}");

            // Notify the client
            PacketService.SendPacket(new Packet(PacketType.FIRE_REPONSE, $"{coordsFired}={FireResponseTypeExt.ToString(fireResponse)}"), client, this.Shutdown);


            Logger.LogI(FireResponseTypeExt.ToString(fireResponse));


            // ---------------------------------------------------------
            // Handle UI
            if (fireResponse == FireResponseType.WATER)
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
                var coords = Utils.FromExcelCoords(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);

                field.IsShip = true;
            }
        }



        /**
         * Server fires, result is send to the client.
         */
        public void Fire(string coordsFired)
        {
            var coords = Utils.FromExcelCoords(coordsFired);

            Logger.LogI($"Firing to field {coordsFired}");

            // Find what lies at these coordinates
            var shipOnCoords = ClientShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            FireResponseType fireResponse;
            // If server missed the client's ships
            if (shipOnCoords == null)
            {
                fireResponse = FireResponseType.WATER;
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
                    fireResponse = FireResponseType.SUNK;
                }
                else
                {
                    fireResponse = FireResponseType.HIT;
                }
            }

            // Tell the client what the server fired at
            PacketService.SendPacket(new Packet(PacketType.FIRE, $"{coordsFired}={FireResponseTypeExt.ToString(fireResponse)}"), client, this.Shutdown);


            // ---------------------------------------------------------
            // Handle UI
            if (fireResponse == FireResponseType.WATER)
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

            Logger.LogI(FireResponseTypeExt.ToString(fireResponse));
        }

        private FireResponseType FireField(string coordsFired)
        {
            FireResponseType fireResponse;

            var coords = Utils.FromExcelCoords(coordsFired);


            var shipOnCoords = ServerShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );



            if (shipOnCoords == null)
            {
                fireResponse = FireResponseType.WATER;
            }
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);
                fieldOnCoords.IsRevealed = true;
                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);

                shipOnCoords.IsSunk = isShipDestroyed;

                if (isShipDestroyed)
                {
                    fireResponse = FireResponseType.SUNK;
                }
                else
                {
                    fireResponse = FireResponseType.HIT;
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

            PacketService.SendPacket(new Packet(PacketType.MESSAGE, message), client, Shutdown);

            Logger.LogI("Message sent: " + message);
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
                Ui.GotoState(UiState.FINAL, msg);
                p = new Packet(PacketType.YOU_LOSE, msg);

                // Stop the timer
                Timer.Stop();
            }
            // Client won
            else if (winner == 1)
            {
                string msg = "Client won!";
                Ui.GotoState(UiState.FINAL, msg);
                p = new Packet(PacketType.YOU_WIN, msg);

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
                    Ui.GotoState(UiState.YOUR_TURN);
                    p = new Packet(PacketType.OPPONENTS_TURN);
                }
                else
                {
                    Ui.GotoState(UiState.OPPONENTS_TURN);
                    p = new Packet(PacketType.YOUR_TURN);
                }
            }

            PacketService.SendPacket(p, client, Shutdown);
        }

        public void FireAt(int x, int y)
        {
            // Convert to excel coordinates
            var strCoords = Utils.ToExcelCoords(x, y);

            Fire(strCoords);
        }

        /**
         * Place the ship at provided coordinates.
         * 
         * Server writes directly into its ships.
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
            ServerShips.Add(s);

            foreach (var field in s.Fields)
            {
                // ------------------------ UI -----------------------------
                var coords = Utils.FromExcelCoords(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);
                // ------------------------ UI -----------------------------

                field.IsShip = true;
            }

        }

        public void PlaceShips()
        {
            // We literally do nothing
            Logger.LogI("Server set the ships.");
        }


        /*
         * Member variables
         */
        private bool MyTurn { get; set; } = false;

        public bool ShouldRun { get; set; } = true;
        private int Port { get; set; }
        private IUi Ui { get; set; }
        private List<Ship> ServerShips { get; set; } = new List<Ship>();
        private List<Ship> ClientShips { get; set; } = new List<Ship>();

        private readonly TcpListener listener;
        private TcpClient client;
        private bool IsServerDestructed { get; set; } = false;
    }
}