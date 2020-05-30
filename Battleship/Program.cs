using Battleship.Forms;
using Battleship.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship
{
    static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Contains("gui"))
            {
                Logger.LogI("Launching GUI version.");
                //Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new MenuForm());
            }
            else
            {
                Logger.LogI("Launching CLI version.");
                CmdUi ui = new CmdUi();
                ui.Launch();
            }
            return 0;
        }


        public static void LaunchServer(IUi ui)
        {
            var server = new Server(6666, ui);
            var serverThread = new Thread(server.Start);
            serverThread.Start();

            // Link this server back to the UI
            ui.SetLogic(server);
        }

        public static void LaunchClient(IUi ui, string IP, int port)
        {
            var client = new Client(IP, port, ui);
            var clientThread = new Thread(client.Connect);
            clientThread.Start();

            // Link this client back to the UI
            ui.SetLogic(client);
        }
    }
}
