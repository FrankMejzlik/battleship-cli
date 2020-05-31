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
                ListenToTheServer();
            }
            else
            {
                client.Close();
            }
        }


        public void ListenToTheServer()
        {
            while (ShouldRun)
            {
                var packet = PacketService.ReceivePacket(client);

                // Act on recieving a valid packet
                if (packet != null)
                {
                    // Message 
                    if (packet.Type == ePacketType.MESSAGE)
                    {
                        WriteLog($"Message received: {packet.Data}");
                    }
                    // Your turn
                    else if (packet.Type == ePacketType.YOUR_TURN)
                    {
                        WriteLog($"Command to YOUR TURN.");

                        // UI => Handle MyTurn
                        Ui.GotoState(eUiState.YOUR_TURN);
                    }
                    // Opponent's turn
                    else if (packet.Type == ePacketType.OPPONENTS_TURN)
                    {
                        WriteLog($"Command to MY TURN.");

                        // UI => Handle MyTurn
                        Ui.GotoState(eUiState.OPPONENTS_TURN);
                    }
                    // Client won
                    else if (packet.Type == ePacketType.YOU_WIN)
                    {
                        WriteLog($"Client won.");
                        Ui.GotoState(eUiState.FINAL, packet.Data);
                    }
                    // Client lost
                    else if (packet.Type == ePacketType.YOU_LOSE)
                    {
                        WriteLog($"Server won.");
                        Ui.GotoState(eUiState.FINAL, packet.Data);
                    }
                    // Server shot at me
                    else if (packet.Type == ePacketType.FIRE)
                    {
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        WriteLog($"Enemy fired to field {coordsFired}");
                        WriteLog(fireResponse);


                        var coords = Utils.ToNumericCoordinates(coordsFired);
                        // ---------------------------------------------------------
                        // Handle UI
                        if (fireResponse == Config.WaterString)
                        {
                            Ui.HandleMissedMe(coords.Item1, coords.Item2);
                        }
                        else
                        {
                            Ui.HandleHitMe(coords.Item1, coords.Item2);
                        }
                        // ---------------------------------------------------------

                    }
                    // I shot at server and he responded
                    else if (packet.Type == ePacketType.FIRE_REPONSE)
                    {
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        WriteLog(fireResponse);

                        // ---------------------------------------------------------
                        // Handle UI
                        var coords = Utils.ToNumericCoordinates(coordsFired);
                        if (fireResponse == Config.WaterString)
                        {
                            Ui.HandleMissHimtAt(coords.Item1, coords.Item2);
                        }
                        else
                        {
                            Ui.HandleHitHimAt(coords.Item1, coords.Item2);
                        }
                        // ---------------------------------------------------------
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

            //clientShips.Add(new Ship()
            //{
            //    Fields = new List<Field>()
            //    {
            //        new Field(0, 2),
            //        new Field(0, 3)
            //    }
            //});

            //clientShips.Add(new Ship()
            //{
            //    Fields = new List<Field>()
            //    {
            //        new Field(5, 4),
            //        new Field(5, 5),
            //        new Field(5, 6)
            //    }
            //});

            //clientShips.Add(new Ship()
            //{
            //    Fields = new List<Field>()
            //    {
            //        new Field(4, 1),
            //        new Field(5, 1),
            //        new Field(6, 1),
            //        new Field(7, 1)
            //    }
            //});

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
            if (!PacketService.SendPacket(new Packet(ePacketType.SET_CLIENT_SHIPS, clientShipsSerialized), client))
            {
                Shutdown();
            }
        }

        public void Fire(string coords)
        {
            if (!PacketService.SendPacket(new Packet(ePacketType.FIRE, coords), client))
            {
                Shutdown();
            }

            WriteLog($"Firing to field {coords}");
        }

        public void SendMessage(string message)
        {
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

        public void FireAt(int x, int y)
        {

            // Convert to excel coordinates
            var strCoords = Utils.GetCoords(x, y);

            Fire(strCoords);
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
