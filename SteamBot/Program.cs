using System;
using System.IO;
using System.Threading;

namespace SteamBot
{
    class Program
    {
        public static Configuration Config { get; private set; }

        static void Main(string[] args)
        {
            Console.Title = "azzae99's SteamBot SteamKit 2.1.0 Base";

            if (!File.Exists("config.json"))
                Console.WriteLine("The 'config.json' file does not exist...");
            else
            {
                try
                {
                    Config = Configuration.LoadConfiguration("config.json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error attempting to deserialize config.json, this is most likely due to incorrect JSON syntax...");
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }

            if (Config != null)
            {
                foreach (Configuration.BotConfiguration config in Config.Bots)
                {
                    Bot bot = new Bot(config);
                    bot.Log.Info("Launching Bot...");
                    Thread thread = new Thread(bot.StartBot);
                    thread.Start();
                }
                // This is bad...
                while (true) Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
        }
    }
}
