using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Battleship
{
    public class Client
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public Form Battlefield { get; private set; }

        private readonly TcpClient client;
        private NetworkStream stream;

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
                stream = client.GetStream();
                Listen();
            }
            else
            {
                client.Close();
            }
        }

        public void Listen()
        {
            var bytes = new byte[1024];

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
                WriteLog("Cannot send message, server not connected.");
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
