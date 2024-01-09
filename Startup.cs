using System.Globalization;
using WordBombServer.Server;

namespace WordBombServer
{
    public class Startup
    {
        static int saveTimer = 0;
        const int SAVE_TIME_SECONDS = 30 * 60;
        private static WordBomb server;
        public static RequestTimeoutList RequestTimer;
        static void Main()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                RequestTimer = new RequestTimeoutList();
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(ConsoleExit);
                server = new WordBomb(60000, 9050);
                server.StartServer();
                var timer = new System.Timers.Timer();
                timer.Interval = 250;
                timer.Elapsed += Timer;
                timer.Start();
                while (true)
                {
                    server.ServerTick();
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
            server.UserContext.SaveChanges();
        }

        private static void Timer(object? sender, System.Timers.ElapsedEventArgs e)
        {
            server.lobbyRequestHandler.TickLobbies();
            RequestTimer.Tick();
            saveTimer++;
            if (saveTimer > 4 * SAVE_TIME_SECONDS)
            {
                server.UserContext.SaveChanges();
                Console.WriteLine("Saving Users..");
                saveTimer = 0;
            }
        }
    }
}
