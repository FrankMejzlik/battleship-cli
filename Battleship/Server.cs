using Battleship.Enums;
using Battleship.Models;
using Battleship.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Battleship
{
    public class Server
    {
        public int Port { get; private set; }
        public Form Form { get; private set; }
        public List<Ship> ServerShips { get; set; }
        public List<Ship> ClientShips { get; set; }

        private readonly TcpListener listener;
        private TcpClient client;

        public Server(int port, Form form)
        {
            Port = port;
            Form = form;

            listener = new TcpListener(IPAddress.Any, Port);
        }

        public void Shutdown()
        {
            client?.GetStream()?.Close();
            client?.Close();
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

            PlaceShips(); // TODO: after ready button clicked
            Listen();
        }

        public void Listen()
        {
            while (true) // TODO: while (isRunning)
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
                var button = Form.Controls.Find($"serverField{field.Coords}", false).First();

                button.BackColor = Color.Black;
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
                var button = Form.Controls.Find($"clientField{coordsFired}", false).First();

                fieldOnCoords.IsRevealed = true;
                button.BackColor = Color.Red;

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

            var button = Form.Controls.Find($"serverField{coordsFired}", false).First();

            var shipOnCoords = ServerShips.FirstOrDefault(ship =>
               ship.Fields.Any(field =>
                   field.Coords.Equals(coordsFired)
               )
            );

            if (shipOnCoords == null)
            {
                fireResponse = FireResponseType.Water;
                button.BackColor = Color.LightBlue;
            }
            else
            {
                var fieldOnCoords = shipOnCoords.Fields.First(field => field.Coords == coordsFired);

                fieldOnCoords.IsRevealed = true;
                button.BackColor = Color.Red;

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
            var log = Form.Controls["log"];

            if (log.InvokeRequired)
            {
                Form.Invoke(new WriteLogCallback(WriteLog), new object[] { text });
            }
            else
            {
                log.Text += (text + Environment.NewLine);
            }
        }
    }
}