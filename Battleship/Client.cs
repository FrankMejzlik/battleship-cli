using Battleship.Models;
using Battleship.Services;
using Battleship.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    public class Client : Logic
    {
        public Client(string host, int port, IUi ui)
        {
            Host = host;
            Port = port;
            Ui = ui;

            // We are the client
            client = new TcpClient();
        }

        public void Shutdown()
        {
            // No multiple destrucitons
            if (Destructed)
            {
                return;
            }

            // Send info about the end to the client
            PacketService.SendPacket(new Packet(ePacketType.FIN), client, Shutdown);

            // Write client game log
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

            Destructed = true;
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
                var packet = PacketService.ReceivePacket(client, Shutdown);

                // Act on recieving a valid packet
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
                    // Timeout
                    else if (packet.Type == ePacketType.TIMED_OUT)
                    {
                        WriteLog(Config.StrTimeout);
                        Ui.GotoState(eUiState.FINAL, Config.StrTimeout);
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
            // Send those ships to the server
            var clientShipsSerialized = JsonConvert.SerializeObject(ClientShips);
            PacketService.SendPacket(new Packet(ePacketType.SET_CLIENT_SHIPS, clientShipsSerialized), client, Shutdown);
        }

        public void SendMessage(string message)
        {
            PacketService.SendPacket(new Packet(ePacketType.MESSAGE, message), client, Shutdown);
            WriteLog($"Send message '{message}'.");
        }

        private void WriteLog(string text)
        {
            GameLog.Append(text);
        }

        public void FireAt(int x, int y)
        {
            // Convert to the Excel coordinates
            var strCoords = Utils.GetCoords(x, y);

            PacketService.SendPacket(new Packet(ePacketType.FIRE, strCoords), client, Shutdown);
            WriteLog($"Firing at the '{strCoords}' field.");
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
            ClientShips.Add(s);

            // Put into the UI
            foreach (var field in s.Fields)
            {
                var coords = Utils.ToNumericCoordinates(field.Coords);
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
        private bool Destructed { get; set; } = false;
    }
}
