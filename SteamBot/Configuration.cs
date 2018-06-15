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

        //The items in here have the match the different items within the JSON Objects in the JSON Array
        //the order doesn't matter, as long as each item matches...
        //For example, if there's 7 items within each JSON Object, there must be 7 items in this class
        //with matching names (they are not case sensitive)
        //I like to match the order and character cases
        public class BotConfiguration
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string SharedSecret { get; set; }
            public string IdentitySecret { get; set; }
            public string DeviceID { get; set; }
            public string APIKey { get; set; }
            public List<Int32> Games { get; set; }
        }

        //The name of this array HAS to match the name / text of the JSON Array, in this case it is "Bots"
        public BotConfiguration[] Bots { get; set; }
    }
}
