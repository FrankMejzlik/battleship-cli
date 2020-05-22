using System;
using System.Threading;
using System.Windows.Forms;

namespace Battleship.Forms
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }

        private void StartGame(object sender, EventArgs e)
        {
            var form = new ServerForm();
            form.Text += " - server";
            form.Show();

            var server = new Server(6666, form);
            var serverThread = new Thread(server.Start);
            serverThread.Start();

            form.SetServer(server);
        }

        private void JoinGame(object sender, EventArgs e)
        {
            var form = new ClientForm();
            form.Text += " - client";
            form.Show();

            var client = new Client("localhost", 6666, form);
            var clientThread = new Thread(client.Connect);
            clientThread.Start();

            form.SetClient(client);            
        }
    }
}
