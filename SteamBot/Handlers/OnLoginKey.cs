using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnLoginKey(Bot Sender, SteamUser.LoginKeyCallback Callback)
        {
            Sender.UniqueID = Callback.UniqueID.ToString();
            Sender.AuthenticateSteamWebClient();

            Sender.SteamFriends.SetPersonaState(EPersonaState.Online);
        }
    }
}
