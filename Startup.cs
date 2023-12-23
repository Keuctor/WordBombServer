using System.Globalization;
using WordBombServer.Server;

namespace WordBombServer
{
    public class Startup
    {
        static int saveTimer = 0;
        const int SAVE_TIME_SECONDS = 30 * 60;
        public static WordBomb Server;
        public static RequestTimeoutList RequestTimer;

        static void Main()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                RequestTimer = new RequestTimeoutList();
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(ConsoleExit);
                Server = new WordBomb(60000, 9050);
                Server.StartServer();
                var timer = new System.Timers.Timer();
                timer.Interval = 250;
                timer.Elapsed += Timer;
                timer.Start();

                while (true)
                {
                    Server.ServerTick();
                    Thread.Sleep(2);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Loop Error:" + e.ToString());
                try
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + "error.txt";
                    File.AppendAllText(path, e.ToString());
                }
                catch (Exception x) {
                    Console.WriteLine("File Error: " + x);
                }
            }
        }

        private static void ConsoleExit(object? sender, EventArgs e)
        {
            Console.WriteLine("Saving Users..");
            Server.UserContext.SaveChanges();
        }

        private static void Timer(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Server.lobbyRequestHandler.TickLobbies();
            RequestTimer.Tick();
            saveTimer++;
            if (saveTimer > 4 * SAVE_TIME_SECONDS)
            {
                Server.UserContext.SaveChanges();
                Console.WriteLine("Saving Users..");
                saveTimer = 0;
            }
        }
    }
}
