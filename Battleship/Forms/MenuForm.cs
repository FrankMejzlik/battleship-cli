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
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            var battlefieldForm = new BattlefieldForm();
            battlefieldForm.Show();            
        }

        private void joinGameButton_Click(object sender, EventArgs e)
        {
            var battlefieldForm = new BattlefieldForm();
            battlefieldForm.Show();
        }
    }
}
