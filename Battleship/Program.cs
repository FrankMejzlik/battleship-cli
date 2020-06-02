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
        /** 
         * The application entry point. 
         * 
         * \param args      Launch arguments.
         * \return  Return code. 
         *          - 0 means success 
         *          - other values indicate errors.
         */
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

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

        /** 
         * This kicks off the SERVER version. 
         * 
         * It is called from the IUi instance that is active.
         * 
         * \see IUi
         * \see CmdUi
         * 
         * \param ui    Reference to UI this client will be using.
         * \param ui    IP address of the server.
         * \param port  Port number of the server.
         */
        public static void LaunchServer(IUi ui, int port)
        {
            var server = new Server(port, ui);
            var serverThread = new Thread(server.Start);
            serverThread.Start();

            // Timer thread
            Timer.Start(server);
            var timerThread = new Thread(Timer.DoCheck);
            timerThread.Start();

            // Link this server back to the UI
            ui.SetLogic(server);
            AppInstance = server;
        }

        /** 
         * This kicks off the CLIENT version. 
         * 
         * It is called from the IUi instance that is active.
         * 
         * \see IUi
         * \see CmdUi
         * 
         * \param ui    Reference to UI this client will be using.
         * \param ui    IP address of the server.
         * \param port  Port number of the server.
         */
        public static void LaunchClient(IUi ui, string IP, int port)
        {
            var client = new Client(IP, port, ui);
            var clientThread = new Thread(client.Connect);
            clientThread.Start();

            // Link this client back to the UI
            ui.SetLogic(client);
            AppInstance = client;
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            AppInstance?.Shutdown();
        }

        private static Logic AppInstance { get; set; }
    }
}
