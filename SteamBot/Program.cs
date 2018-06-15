using System;
using System.Collections.Generic;
using System.IO;

namespace SteamBot
{
    class Program
    {
        public static Configuration Config { get; private set; }

        static void Main(string[] args)
        {
            Console.Title = "azzae99's SteamBot SteamKit 2.1.0 Base";

            if (!File.Exists("settings.json"))
                Console.WriteLine("The 'settings.json' file does not exist...");
            else
            {
                try
                {
                    Config = Configuration.LoadConfiguration("settings.json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error attempting to deserialize settings.json, this is most likely due to incorrect JSON syntax...");
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }

            if (Config != null)
            {
                foreach (Configuration.BotConfiguration config in Config.Bots)
                {
                    Bot bot = new Bot(config);
                }
            }
        }
    }
}
