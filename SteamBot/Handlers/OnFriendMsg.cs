using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnFriendMsg(Bot Sender, SteamFriends.FriendMsgCallback Callback)
        {
            if (Callback.EntryType == EChatEntryType.ChatMsg)
            {
                Sender.Log.Info("Chat Message received from {0} ({1}): {2}", Sender.SteamFriends.GetFriendPersonaName(Callback.Sender), Callback.Sender.ConvertToUInt64(), Callback.Message);
            }
        }
    }
}
