using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Battleship.UI
{
    public class WinFormsUi : IUi
    {
        WinFormsUi(Form form)
        {
            Form = form;
        }
        void IUi.HandleHitAt(int x, int y)
        {
            string coordsFired = $"{x}, {y}";
            var button = Form.Controls.Find($"clientField{coordsFired}", false).First();
            button.BackColor = Color.Red;
        }

        void IUi.HandleMisstAt(int x, int y)
        {
            string coordsFired = $"{x}, {y}";
            var button = Form.Controls.Find($"clientField{coordsFired}", false).First();
            button.BackColor = Color.LightBlue;
        }

        void IUi.Launch()
        {
            throw new NotImplementedException();
        }

        public void HandlePlaceShipAt(int x, int y)
        {
            string coords = Utils.GetCoords(x, y);

            var button = Form.Controls.Find($"serverField{coords}", false).First();

            button.BackColor = Color.Black;
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void SetLogic(Logic s)
        {
            throw new NotImplementedException();
        }

        public void GotoState(eUiState state, string msg = "")
        {
            throw new NotImplementedException();
        }

        public Form Form { get; set; }
    }
}
