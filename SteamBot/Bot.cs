using System;
using System.Collections.Generic;
using System.Timers;
using SteamKit2;
using SteamAuth;

namespace SteamBot
{
    public class Bot
    {
        private readonly string Username;
        private readonly string Password;
        private readonly string SharedSecret;
        private readonly string IdentitySecret;
        private readonly string DeviceID;
        private readonly string APIKey;
        private readonly List<Int32> Games;

        private readonly Logger Log;

        private Timer CallbackTimer;

        public readonly SteamClient SteamClient;
        public readonly CallbackManager CallbackManager;
        public readonly SteamUser SteamUser;
        public readonly SteamFriends SteamFriends;
        public readonly SteamTrading SteamTrading;
        public readonly SteamUser.LogOnDetails LogOnDetails;

        public SteamWebClient SteamWebClient;
        //Used to Authenticate the SteamWebClient
        private string WebAPIUserNonce;
        private string UniqueID;

        //SteamAuth SteamGuardAccount from the SteamAuth library by geel9 and Jessecar96
        public readonly SteamGuardAccount SteamGuardAccount;

        public Bot(Configuration.BotConfiguration Config)
        {
            CallbackTimer = new Timer();
            CallbackTimer.Elapsed += new ElapsedEventHandler(CallbackTimer_Elapsed);
            CallbackTimer.Interval = 500;//Half a second

            Username = Config.Username;
            Password = Config.Password;
            SharedSecret = Config.SharedSecret;
            IdentitySecret = Config.IdentitySecret;
            DeviceID = Config.DeviceID;
            APIKey = Config.APIKey;
            Games = Config.Games;

            Log = new Logger(Username);

            SteamClient = new SteamClient();
            SteamWebClient = new SteamWebClient();
            CallbackManager = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            SteamTrading = SteamClient.GetHandler<SteamTrading>();
            SteamGuardAccount = new SteamGuardAccount();

            if (!String.IsNullOrEmpty(SharedSecret))
                SteamGuardAccount.SharedSecret = SharedSecret;
            if (!String.IsNullOrEmpty(IdentitySecret))
                SteamGuardAccount.IdentitySecret = IdentitySecret;
            if (!String.IsNullOrEmpty(DeviceID))
                SteamGuardAccount.DeviceID = DeviceID;

            LogOnDetails = new SteamUser.LogOnDetails
            {
                Username = Username,
                Password = Password,
            };

            // Subscribe to callbacks
            CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            // Handle this for when our SteamWebClient cookies expire and we need
            // to request a new WebAPIUserNonce using SteamUser.RequestWebAPIUserNonce();
            CallbackManager.Subscribe<SteamUser.WebAPIUserNonceCallback>(OnWebAPIUserNonce);
            //Handle this to get the UniqueID for the SteamWebClient
            CallbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);

            CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);

            CallbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMsg);

            //Live Trading isn't supported, but we'll handle the Callbacks anyway
            CallbackManager.Subscribe<SteamTrading.SessionStartCallback>(OnSessionStart);
            CallbackManager.Subscribe<SteamTrading.TradeProposedCallback>(OnTradeProposed);
            //Don't really need to handle TradeResultCallback since we don't support it, whomever manually starts the Live Trading Session
            //should know the result
        }

        private void OnTradeProposed(SteamTrading.TradeProposedCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnSessionStart(SteamTrading.SessionStartCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnFriendMsg(SteamFriends.FriendMsgCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnUpdateMachineAuth(SteamUser.UpdateMachineAuthCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnWebAPIUserNonce(SteamUser.WebAPIUserNonceCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback obj)
        {
            throw new NotImplementedException();
        }

        private void OnConnected(SteamClient.ConnectedCallback obj)
        {
            throw new NotImplementedException();
        }

        private void CallbackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
