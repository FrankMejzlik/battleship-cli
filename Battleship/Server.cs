
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

namespace Battleship
{
    public interface Logic
    {
        public void Shutdown();
        void FireAt(int x, int y);
    }

    public class Server : Logic
    {
        public Server(int port, IUi ui)
        {
            ShouldRun = true;

            Port = port;
            Ui = ui;

            // We are the server
            listener = new TcpListener(IPAddress.Any, Port);
        }

        public void Shutdown()
        {
            // Set the running flag
            ShouldRun = false;

            // Write client game log
            System.IO.File.WriteAllText(Config.ClientGameLogFilepath, GameLog.ToString());

            try
            {
                client.GetStream().Close();
                client.Close();
            }
            catch (ObjectDisposedException)
            { }

            listener.Stop();
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

            // TODO: Simulated placing
            PlaceShips();

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

        private void PlaceShips()
        {
            var serverShips = new List<Ship>();

            serverShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(8, 8)
                }
            });

            serverShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(2, 6),
                    new Field(2, 7)
                }
            });

            serverShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(6, 2),
                    new Field(6, 3),
                    new Field(6, 4)
                }
            });

            serverShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(0, 1),
                    new Field(1, 1),
                    new Field(2, 1),
                    new Field(3, 1)
                }
            });


            foreach (var ship in serverShips)
            {
                PlaceShip(ship);
            }

            SetShips(serverShips);
        }

        private void PlaceShip(Ship ship)
        {
            foreach (var field in ship.Fields)
            {
                var coords = Utils.ToNumericCoordinates(field.Coords);
                Ui.HandlePlaceShipAt(coords.Item1, coords.Item2);

                field.IsShip = true;
            }
        }

        public void SetShips(List<Ship> serverShips)
        {
            ServerShips = serverShips;
            WriteLog("Server ships have been set.");
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
                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);

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
            MyTurn = !MyTurn;

            Packet p;
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

        /*
         * Member variables
         */
        private bool MyTurn { get; set; } = false;

        public bool ShouldRun { get; set; } = true;
        private StringBuilder GameLog { get; set; } = new StringBuilder();
        private int Port { get; set; }
        private IUi Ui { get; set; }
        private List<Ship> ServerShips { get; set; }
        private List<Ship> ClientShips { get; set; }

        private readonly TcpListener listener;
        private TcpClient client;
    }
}