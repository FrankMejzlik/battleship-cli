using Battleship.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship.Forms
{
    public partial class ServerForm : Form
    {
        public Server Server { get; private set; }

        private int size = 9;

        public ServerForm()
        {
            InitializeComponent();
        }

        public void SetServer(Server server)
        {
            if (Server == null)
            {
                Server = server;
                InitializeBattlefield();
            }
        }

        private void InitializeBattlefield()
        {
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    var coords = Utils.GetCoords(x, y);
                    var button = new Button()
                    {
                        Name = $"serverField{coords}",
                        Text = coords,
                        Location = new Point(10 + 30 * x, 10 + 30 * y),
                        Size = new Size(30, 30),
                        Enabled = false
                    };

                    Controls.Add(button);
                }
            }

            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    var coords = Utils.GetCoords(x, y);
                    var button = new Button()
                    {
                        Name = $"clientField{coords}",
                        Text = coords,
                        Location = new Point(350 + 30 * x, 10 + 30 * y),
                        Size = new Size(30, 30)
                    };

                    button.Click += new EventHandler(ServerFires);

                    Controls.Add(button);
                }
            }
        }

        private void ServerFires(object sender, EventArgs e)
        {
            var button = sender as Button;

            button.BackColor = Color.White;

            Server?.Fire(button.Text);
        }

        private void SendMessage(object sender, EventArgs e)
        {
            var message = sendMessageTextBox.Text;

            if (message == "")
            {
                return;
            }

            Server?.SendMessage(message);

            sendMessageTextBox.Text = "";
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            var confirmClosing = MessageBox.Show("Exit the game?", "Battleship", MessageBoxButtons.YesNo);

            if (confirmClosing != DialogResult.Yes)
            {
                e.Cancel = true;
            }

            // TODO: Server.Shutdown()
        }
    }
}
