using System.Xml;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnFriendsList(Bot Sender, SteamFriends.FriendsListCallback Callback)
        {
            foreach (SteamFriends.FriendsListCallback.Friend friend in Callback.FriendList)
            {
                // Groups are included in the FriendsListCallback, so we also check if it's pending,
                // meaning we have been invited to the group
                if (friend.SteamID.AccountType == EAccountType.Clan && friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    // Because we're working with a group rather than a user, it's quite difficult to actually get the name of it
                    // since there's no WebAPI Endpoints that we can use to get it, nor any such implementation in SteamKit2
                    XmlDocument XML = new XmlDocument();
                    XML.Load($"https://steamcommunity.com/gid/{friend.SteamID.ConvertToUInt64()}/memberslistxml?xml=1&p=99999");
                    Sender.Log.Info("Received Group Invite to {0} ({1}), ignoring...", XML.DocumentElement.SelectSingleNode("/memberList/groupDetails/groupName").InnerText, friend.SteamID.ConvertToUInt64());
                }
                else if (friend.SteamID.AccountType != EAccountType.Clan)
                {
                    if (friend.SteamID.AccountType == EAccountType.Individual)
                    {
                        if (friend.Relationship == EFriendRelationship.RequestRecipient)
                            Sender.Log.Info("{0} ({1}) added us to their friends list, ignoring...", Sender.SteamFriends.GetFriendPersonaName(friend.SteamID), friend.SteamID.ConvertToUInt64());
                        else if (friend.Relationship == EFriendRelationship.None)
                            Sender.Log.Info("{0} ({1}) removed us from their friends list...", Sender.SteamFriends.GetFriendPersonaName(friend.SteamID), friend.SteamID.ConvertToUInt64());
                    }
                }
            }
        }
    }
}
