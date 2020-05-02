using Battleship.Models;
using Battleship.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Battleship
{
    public class Server
    {
        public int Port { get; private set; }
        public Form Battlefield { get; private set; }

        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;

        public Server(int port, Form battlefield)
        {
            Port = port;
            Battlefield = battlefield;

            listener = new TcpListener(IPAddress.Any, Port);
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

            stream = client.GetStream();

            Listen();
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