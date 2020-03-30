using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using SteamKit2;
using SteamAuth;

namespace SteamBot
{
    public class Bot
    {
        public readonly string Username;
        public readonly Logger Log;

        public System.Timers.Timer CallbackTimer;

        public readonly SteamClient SteamClient;
        public readonly CallbackManager CallbackManager;
        public readonly SteamUser SteamUser;
        public readonly SteamUser.LogOnDetails LogOnDetails;
        public readonly SteamFriends SteamFriends;
        public readonly PacketMsgHandler PacketMsgHandler;

        public readonly SteamGuardAccount SteamGuardAccount;

        public SteamWebClient SteamWebClient;
        public string WebAPIUserNonce;
        public string UniqueID;

        public Bot(Configuration.BotConfiguration Config)
        {
            Username = Config.Username;
            Log = new Logger(Username);

            CallbackTimer = new System.Timers.Timer();
            CallbackTimer.Elapsed += new System.Timers.ElapsedEventHandler(CallbackTimer_Elapsed);
            CallbackTimer.Interval = 500;

            SteamClient = new SteamClient();
            CallbackManager = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            LogOnDetails = new SteamUser.LogOnDetails
            {
                Username = Username,
                Password = Config.Password,
            };
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            SteamClient.AddHandler(new PacketMsgHandler());
            PacketMsgHandler = SteamClient.GetHandler<PacketMsgHandler>();

            SteamGuardAccount = new SteamGuardAccount();
            if (!String.IsNullOrEmpty(Config.SharedSecret))
                SteamGuardAccount.SharedSecret = Config.SharedSecret;

            SteamWebClient = new SteamWebClient();


            CallbackManager.Subscribe<SteamClient.ConnectedCallback>((Callback) => Handlers.OnConnected(this, Callback));
            CallbackManager.Subscribe<SteamClient.DisconnectedCallback>((Callback) => Handlers.OnDisconnected(this, Callback));

            CallbackManager.Subscribe<SteamUser.LoggedOnCallback>((Callback) => Handlers.OnLoggedOn(this, Callback));
            CallbackManager.Subscribe<SteamUser.LoggedOffCallback>((Callback) => Handlers.OnLoggedOff(this, Callback));

            CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>((Callback) => Handlers.OnUpdateMachineAuth(this, Callback));

            CallbackManager.Subscribe<SteamUser.WebAPIUserNonceCallback>((Callback) => Handlers.OnWebAPIUserNonce(this, Callback));
            CallbackManager.Subscribe<SteamUser.LoginKeyCallback>((Callback) => Handlers.OnLoginKey(this, Callback));

            CallbackManager.Subscribe<SteamFriends.FriendsListCallback>((Callback) => Handlers.OnFriendsList(this, Callback));
            CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>((Callback) => Handlers.OnFriendMsg(this, Callback));
        }

        private void CallbackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CallbackManager.RunWaitAllCallbacks(TimeSpan.Zero);
        }

        public void StartBot()
        {
            Log.Info("Connecting to Steam...");
            if (!SteamClient.IsConnected)
                SteamClient.Connect();
            else
                Log.Warn("Already Connected to Steam...");
            CallbackTimer.Start();
        }

        public void StopBot()
        {
            Log.Info("Disconnecting from Steam...");
            if (SteamClient.IsConnected)
            {
                SteamClient.Disconnect();
                CallbackManager.RunWaitAllCallbacks(TimeSpan.Zero);
            }
            else
            {
                Log.Warn("Already Disconnected from Steam...");
                CallbackTimer.Stop();
            }
            
        }

        public void AuthenticateSteamWebClient()
        {
            bool IsAuthenticated = false;
            do
            {
                IsAuthenticated = SteamWebClient.Authenticate(WebAPIUserNonce, UniqueID, SteamClient.SteamID.ConvertToUInt64(), SteamClient.Universe);
                if (!IsAuthenticated)
                {
                    Log.Warn("Failed to Authenticate SteamWebClient, retrying in 10 seconds...");
                    Thread.Sleep(10000);
                }
            }
            while (!IsAuthenticated);
            Log.Success("SteamWebClient Authenticated!");
        }
    }
}
