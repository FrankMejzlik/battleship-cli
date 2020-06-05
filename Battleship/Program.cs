
using Battleship.Logic;
using Battleship.UI;
using System;
using System.Threading;

namespace Battleship
{
    /**
     * The main singleton class representing one party of the Battleships game.
     * 
     * The game first initializes the UI where the user provides wheter he/she wants
     * to launch the server or the client version. Based on that, specified instance 
     * of Logic is instantiated and launched.
     */
    static class Program
    {
        /*
         * Methods.
         */
        /** The application entry point. */
        public static void Main()
        {
            Logger.LogI("Starting the application...");

            /* We need to let the other side know when the app is forcefully closed.
               Therefore we set exit process handler. */
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(HandleProcessExit);

            // We launch the UI
            CmdUi ui = new CmdUi();
            ui.Launch();
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

        /** Handles the process termination. */
        static void HandleProcessExit(object sender, EventArgs e)
        {
            Logger.LogI("Process close required...");

            Console.Clear();

            // Shut down the appliaction correctly
            AppInstance?.Shutdown();
        }

        /*
         * Member variables 
         */
        /** Application instance. */
        private static ILogic AppInstance { get; set; }
    }
}
