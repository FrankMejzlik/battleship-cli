using Battleship.Enums;
using Battleship.Models;
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
    public class Client : Logic
    {
        public Client(string host, int port, IUi ui)
        {
            ShouldRun = true;

            Host = host;
            Port = port;
            Ui = ui;

            // We are the client
            client = new TcpClient();
        }

        public void Shutdown()
        {
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
        }

        public void Connect()
        {
            try
            {
                var msg = $"Connecting to the server...";

                // UI => CLIENT_CONNECTING
                Ui.GotoState(eUiState.CLIENT_CONNECTING, msg);
                WriteLog(msg);

                client.Connect(Host, Port);
            }
            catch (SocketException ex)
            {
                var msg = $"Cannot connect to the server: {ex.Message}";

                // UI => FINAL
                Ui.GotoState(eUiState.FINAL, msg);
                WriteLog(msg);
            }

            if (client.Connected)
            {
                var msg = $"Connected to the server at {client.Client.RemoteEndPoint}";

                // UI => PLACING_SHIPS
                Ui.GotoState(eUiState.PLACING_SHIPS, msg);
                WriteLog(msg);

                // TODO: Simulated placing
                PlaceShips();

                // Now listen to the server
                Listen();
            }
            else
            {
                client.Close();
            }
        }


        public void Listen()
        {
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(client);

                // Act on recieving a valid packet
                if (packet != null)
                {
                    if (packet.Type == PacketType.MESSAGE)
                    {
                        WriteLog($"Message received: {packet.Data}");
                    }
                    else if (packet.Type == PacketType.FIRE)
                    {
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        WriteLog($"Enemy fired to field {coordsFired}");
                        WriteLog(fireResponse);

                        // UI => Handle FireAt
                        var coords = Utils.ToNumericCoordinates(coordsFired);
                        if (fireResponse == "WATER")
                        {
                            Ui.HandleHitAt(coords.Item1, coords.Item2);
                        }
                        else
                        {
                            Ui.HandleHitAt(coords.Item1, coords.Item2);
                        }
                    }
                    else if (packet.Type == PacketType.FIRE_REPONSE)
                    {
                        WriteLog(packet.Data);
                        // TODO: zjistit, který button je bílý (nebo inprogress) a podle response ho obarvit namodro nebo načerveno
                    }
                }
            }
        }

        public void PlaceShips()
        {
            var clientShips = new List<Ship>();

            clientShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(1, 7)
                }
            });


            

            clientShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(0, 2),
                    new Field(0, 3)
                }
            });
            
            clientShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(5, 4),
                    new Field(5, 5),
                    new Field(5, 6)
                }
            });
            
            clientShips.Add(new Ship()
            {
                Fields = new List<Field>()
                {
                    new Field(4, 1),
                    new Field(5, 1),
                    new Field(6, 1),
                    new Field(7, 1)
                }
            });

            foreach (var ship in clientShips)
            {
                PlaceShip(ship);
            }


            Thread.Sleep(3000);

            SetShips(clientShips);
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

        public void SetShips(List<Ship> clientShips)
        {
            var clientShipsSerialized = JsonConvert.SerializeObject(clientShips);
            PacketService.SendPacket(new Packet(PacketType.SetClientShips, clientShipsSerialized), client);
        }

        public void Fire(string coords)
        {
            PacketService.SendPacket(new Packet(PacketType.FIRE, coords), client);
            WriteLog($"Firing to field {coords}");
        }

        public void SendMessage(string message)
        {
            PacketService.SendPacket(new Packet(PacketType.MESSAGE, message), client);
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
        private bool ShouldRun { get; set; } = true;
        private StringBuilder GameLog { get; set; } = new StringBuilder();
        private string Host { get; set; }
        private int Port { get; set; }
        private IUi Ui { get; set; }

        private readonly TcpClient client;
    }
}
