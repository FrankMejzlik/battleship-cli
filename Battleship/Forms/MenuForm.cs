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

        private void startGameButton_Click(object sender, EventArgs e)
        {
            var battlefield = new BattlefieldForm();
            battlefield.Text += " - server";
            battlefield.Show();

            var server = new Server(6666, battlefield);
            var serverThread = new Thread(server.Start);
            serverThread.Start();

            battlefield.SetServer(server);
        }

        private void joinGameButton_Click(object sender, EventArgs e)
        {
            var battlefield = new BattlefieldForm();
            battlefield.Text += " - client";
            battlefield.Show();

            var client = new Client("localhost", 6666, battlefield);
            var clientThread = new Thread(client.Connect);
            clientThread.Start();

            battlefield.SetClient(client);            
        }
    }
}
