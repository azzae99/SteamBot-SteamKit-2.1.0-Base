using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnLoggedOff(Bot Sender, SteamUser.LoggedOffCallback Callback)
        {
            Sender.Log.Warn("Logged Off of Steam, Result: {0}...", Callback.Result);
        }
    }
}
