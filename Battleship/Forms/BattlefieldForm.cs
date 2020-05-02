using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Forms
{
    public partial class BattlefieldForm : Form
    {
        public Server Server { get; private set; }
        public Client Client { get; private set; }

        public BattlefieldForm()
        {
            InitializeComponent();
        }

        public void SetServer(Server server)
        {
            Server = server;
        }

        public void SetClient(Client client)
        {
            Client = client;
        }

        private void sendMessageButton_Click(object sender, EventArgs e)
        {
            var message = sendMessageTextBox.Text;

            if (message == "")
            {
                return;
            }

            if (Server != null)
            {
                Server.SendMessage(message);
            }
            else if (Client != null)
            {
                Client.SendMessage(message);
            }

            sendMessageTextBox.Text = "";
        }
    }
}
