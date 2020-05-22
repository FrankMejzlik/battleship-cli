using Battleship.Enums;
using Battleship.Models;
using Battleship.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Battleship
{
    public class Client
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public Form Form { get; private set; }

        private readonly TcpClient client;

        public Client(string host, int port, Form form)
        {
            Host = host;
            Port = port;
            Form = form;

            client = new TcpClient();
        }

        public void Connect()
        {
            try
            {
                WriteLog($"Connecting to the server...");
                client.Connect(Host, Port);
            }
            catch (SocketException ex)
            {
                WriteLog($"Cannot connect to the server: {ex.Message}");
            }

            if (client.Connected)
            {
                WriteLog($"Connected to the server at {client.Client.RemoteEndPoint}");
                PlaceShips(); // TODO: after ready button clicked
                Listen();
            }
            else
            {
                client.Close();
            }
        }

        public void Disconnect()
        {
            client.GetStream().Close();
            client.Close();
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
                        var data = packet.Data.Split('=');
                        var coordsFired = data[0];
                        var fireResponse = data[1];

                        WriteLog($"Enemy fired to field {coordsFired}");
                        WriteLog(fireResponse);

                        var button = Form.Controls.Find($"clientField{coordsFired}", false).First();
                        button.BackColor = (fireResponse != "WATER!") ? Color.Red : Color.LightBlue;
                    }
                    else if (packet.Type == PacketType.FireResponse)
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

            SetShips(clientShips);
        }

        private void PlaceShip(Ship ship)
        {
            foreach (var field in ship.Fields)
            {
                var button = Form.Controls.Find($"clientField{field.Coords}", false).First();

                button.BackColor = Color.Black;
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
            PacketService.SendPacket(new Packet(PacketType.Fire, coords), client);
            WriteLog($"Firing to field {coords}");
        }

        public void SendMessage(string message)
        {
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
