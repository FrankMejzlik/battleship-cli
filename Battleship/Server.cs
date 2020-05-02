using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
                WriteLog($"Cannot start server on port {Port}: " + ex);
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
            byte[] bytes = new byte[1024];

            while (true)
            {
                var bytesRead = stream.Read(bytes, 0, bytes.Length);
                var message = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                WriteLog("Message received: " + message);
            }
        }

        public void SendMessage(string message)
        {
            if (stream != null)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);
                WriteLog("Message sent: " + message);
            }
            else
            {
                WriteLog("Cannot send message, client not connected.");
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