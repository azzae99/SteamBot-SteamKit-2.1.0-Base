using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SteamBot
{
    public class Configuration
    {
        public static Configuration LoadConfiguration(string FileName)
        {
            TextReader Reader = new StreamReader(FileName);
            string JSON = Reader.ReadToEnd();
            Reader.Close();

            return JsonConvert.DeserializeObject<Configuration>(JSON, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public class BotConfiguration
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string SharedSecret { get; set; }
        }

        public BotConfiguration[] Bots { get; set; }
    }
}
