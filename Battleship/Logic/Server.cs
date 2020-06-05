
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

        /** Terminates the server and notifies the client. */
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
                PacketService.SendPacket(new Packet(PacketType.FIN), client, () => { });

                // Close the TCP client
                try
                {
                    client.Dispose();
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

        /** Handles timeouted game session. */
        internal void HandleTimeout()
        {
            // Send info about the end to the client
            PacketService.SendPacket(new Packet(PacketType.TIMED_OUT), client, Shutdown);

            // ------------------------ UI -----------------------------
            Ui.GotoState(UiState.FINAL, Config.Strings.Timeout);
            // ------------------------ UI -----------------------------
        }

        public void Start()
        {
            Logger.LogI($"Starting the server...");

            // Make sure that UI is interstate
            while (!Ui.IsInInterstate)
            {
                Thread.Sleep(Config.UpdateWait);
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
            Ui.GotoState(UiState.SERVER_WAITING);
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

        /** Starts network listenning loop. */
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

        /** Check winning condition. */
        private int CheckFinished()
        {
            // Find any unsunk ship among server ships
            bool clientWon = !(ServerShips.Any(ship => ship.IsSunk == false));
            if (clientWon)
            {
                return 1;
            }

            // Fin any unsung ships among client ships
            bool servertWon = !(ClientShips.Any(ship => ship.IsSunk == false));
            if (servertWon)
            {
                return -1;
            }
            return 0;
        }

        /**Client provided locations of the ships - this is start of the game itself.
         * 
         * \param clientShips   List of the client ships.
         */
        private void HandleClientSetShips(List<Ship> clientShips)
        {
            ClientShips = clientShips;

            Logger.LogI($"Client set ships.");

            // Game starts here
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

        /** Handles event that client fired at the server.
         *
         * \pram coordsFired    Target excel coordinates.
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

            // Switch turn
            SwitchTurn();
        }


        /** Server fires, result is send to the client.
         * 
         * \pram coordsFired    Target excel coordinates.
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
            SwitchTurn();
            // -----------------

            Logger.LogI(FireResponseTypeExt.ToString(fireResponse));
        }

        /** Simulates fire at the given coordinates.
         * 
         * \param coordsFired   Target excel coordinates.
         */
        private FireResponseType FireField(string coordsFired)
        {
            FireResponseType fireResponse;

            // Convert to numeric values
            var coords = Utils.FromExcelCoords(coordsFired);

            // Find shit that is placed at this cooedinates (if any)
            var shipOnCoords = ServerShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            // If no ship there
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

                // Check if it was the last piece of the ship
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

        /** Checks win condition and swaps turns. */
        private void SwitchTurn()
        {
            // Check end of the game
            var winner = CheckFinished();
            Packet p;

            // Server won
            if (winner == -1)
            {
                Ui.GotoState(UiState.FINAL, Config.Strings.YouWin);
                p = new Packet(PacketType.YOU_LOSE, Config.Strings.YouLose);

                // Stop the timer
                Timer.Stop();
            }
            // Client won
            else if (winner == 1)
            {
                Ui.GotoState(UiState.FINAL, Config.Strings.YouLose);
                p = new Packet(PacketType.YOU_WIN, Config.Strings.YouWin);

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

        /** Handles fire at the provided coordinates. 
         * 
         * \param x     X coord.
         * \param x     Y coord.
         */
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
                var isShipAlready = ServerShips.Any(ship =>
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

        /** Confirms that server ships are placed. */
        public void PlaceShips()
        {
            // We literally do nothing
            Logger.LogI("Server set the ships.");
        }


        /*
         * Member variables
         */
        /** True if it's my turn. */
        private bool MyTurn { get; set; } = false;

        /** Indicates that app is running. */
        public bool ShouldRun { get; set; } = true;

        /** Port this server listens at. */
        private int Port { get; set; }

        /** UI this server uses. */
        private IUi Ui { get; set; }

        /** Ships of the server. */
        private List<Ship> ServerShips { get; set; } = new List<Ship>();

        /** Ships of the client. */
        private List<Ship> ClientShips { get; set; } = new List<Ship>();

        /** TCP listener used for listening to the client messages. */
        private readonly TcpListener listener;

        /** TCP client used for listening to the client messages. */
        private TcpClient client;

        /** Indicates that this instance has been already destructed. */
        private bool IsServerDestructed { get; set; } = false;
    }
}