using Battleship.Models;
using Battleship.Services;
using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Battleship
{
    public class Client
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public Form Battlefield { get; private set; }

        private readonly TcpClient client;        

        public Client(string host, int port, Form battlefield)
        {
            Host = host;
            Port = port;
            Battlefield = battlefield;

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
                Listen();
            }
            else
            {
                client.Close();
            }
        }

        public void Listen()
        {            
            while (true)
            {
                var packet = PacketService.ReceivePacket(client);

                if (packet != null && packet.Type == "message")
                {
                    WriteLog("Message received: " + packet.Data);
                }
            }
        }

        public void SendMessage(string message)
        {
            if (client != null && PacketService.SendPacket(new Packet("message", message), client))
            {                
                WriteLog("Message sent: " + message);                
            }
            else
            {
                WriteLog("Cannot send message.");
            }
        }

        private delegate void WriteLogCallback(string text);

        private void WriteLog(string text)
        {
            var log = Battlefield.Controls["log"];

            if (log.InvokeRequired)
            {
                Battlefield.Invoke(new WriteLogCallback(WriteLog), new object[] { text });
            }
            else
            {
                log.Text += (text + Environment.NewLine);
            }
        }
    }
}
