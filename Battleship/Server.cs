using Battleship.Enums;
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

            WriteLog("Server started. Waiting for incomming connections...");
            client = listener.AcceptTcpClient();
            WriteLog($"New connection from {client.Client.RemoteEndPoint}.");

            // TODO: Simulated placing
            PlaceShips();

            // Now listen to the client
            Listen();
        }

        public void Listen()
        {
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(client);

                if (packet != null)
                {
                    if (packet.Type == PacketType.Message)
                    {
                        WriteLog($"Message received: {packet.Data}");
                    }
                    else if (packet.Type == PacketType.Fire)
                    {
                        var coordsFired = packet.Data;
                        var fireResponse = FireField(coordsFired);

                        WriteLog($"Enemy fired to field {coordsFired}");
                        RespondFire(fireResponse);
                    }
                    else if (packet.Type == PacketType.SetClientShips)
                    {
                        ClientShips = JsonConvert.DeserializeObject<List<Ship>>(packet.Data);
                        WriteLog($"Client set ships.");
                    }
                }
            }
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

        public void Fire(string coordsFired)
        {
            WriteLog($"Firing to field {coordsFired}");

            FireResponseType fireResponse;

            var shipOnCoords = ClientShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            if (shipOnCoords == null)
            {
                fireResponse = FireResponseType.Water;
            }
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);

                // UI
                var coords = Utils.ToNumericCoordinates(coordsFired);
                Ui.HandleHitAt(coords.Item1, coords.Item2);

                fieldOnCoords.IsRevealed = true;

                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);

                if (isShipDestroyed)
                {
                    fireResponse = FireResponseType.HitAndSunk;
                }
                else
                {
                    fireResponse = FireResponseType.Hit;
                }
            }

            PacketService.SendPacket(new Packet(PacketType.Fire, $"{coordsFired}={fireResponse.ToFriendlyString()}"), client);
            WriteLog(fireResponse.ToFriendlyString());
        }

        public void RespondFire(FireResponseType fireResponse)
        {
            PacketService.SendPacket(new Packet(PacketType.FireResponse, fireResponse.ToFriendlyString()), client);
            WriteLog(fireResponse.ToFriendlyString());
        }

        private FireResponseType FireField(string coordsFired)
        {
            FireResponseType fireResponse;

            var coords = Utils.ToNumericCoordinates(coordsFired);


            var shipOnCoords = ServerShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            if (shipOnCoords == null)
            {
                Ui.HandleMisstAt(coords.Item1, coords.Item2);

                fireResponse = FireResponseType.Water;
            }
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);

                Ui.HandleHitAt(coords.Item1, coords.Item2);

                var isShipDestroyed = !shipOnCoords.Fields.Any(field => field.IsRevealed == false);

                if (isShipDestroyed)
                {
                    fireResponse = FireResponseType.HitAndSunk;
                }
                else
                {
                    fireResponse = FireResponseType.Hit;
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

            PacketService.SendPacket(new Packet(PacketType.Message, message), client);
            WriteLog("Message sent: " + message);
        }

        private delegate void WriteLogCallback(string text);

        private void WriteLog(string text)
        {
            GameLog.Append(text);
        }

        /*
         * Member variables
         */
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