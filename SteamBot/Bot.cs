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
        private readonly string Username;
        public readonly Logger Log;

        private System.Timers.Timer CallbackTimer;

        public readonly SteamClient SteamClient;
        public readonly CallbackManager CallbackManager;
        public readonly SteamUser SteamUser;
        public readonly SteamUser.LogOnDetails LogOnDetails;
        public readonly SteamFriends SteamFriends;
        public readonly PacketMsgHandler PacketMsgHandler;

        public readonly SteamGuardAccount SteamGuardAccount;

        public SteamWebClient SteamWebClient;
        private string WebAPIUserNonce;
        private string UniqueID;

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


            CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnUpdateMachineAuth);

            CallbackManager.Subscribe<SteamUser.WebAPIUserNonceCallback>(OnWebAPIUserNonce);
            CallbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);

            CallbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMsg);
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

        private void OnConnected(SteamClient.ConnectedCallback Callback)
        {
            // Normally you'd check the EResult of the Callback to verify if the bot actually
            // connected or not, but it seems to have been removed...
            if (Callback != null)
            {
                Log.Success("Connection to Steam established!");

                if (!String.IsNullOrEmpty(SteamGuardAccount.SharedSecret))
                    LogOnDetails.TwoFactorCode = SteamGuardAccount.GenerateSteamGuardCode();
                else if (File.Exists(Path.Combine("00_AuthFiles", String.Format("{0}.auth", Username))))
                {
                    SHA1 SHA = SHA1.Create();
                    byte[] SentryHash = SHA.ComputeHash(File.ReadAllBytes(Path.Combine("00_AuthFiles", String.Format("{0}.auth", Username))));
                    LogOnDetails.SentryFileHash = SentryHash;
                }

                Log.Info("Logging In to Steam...");
                SteamUser.LogOn(LogOnDetails);
            }
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback Callback)
        {
            if (!Callback.UserInitiated)
            {
                Log.Warn("Connection to Steam has been lost, retrying in 20 seconds...");
                CallbackTimer.Stop();
                Thread.Sleep(20000);
                SteamClient.Connect();
                CallbackTimer.Start();
            }
            else
            {
                Log.Info("Connection to Steam has been lost (user initiated), not retrying...");
                CallbackTimer.Stop();
            }
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback Callback)
        {
            Log.Debug("Received LoggedOn Callback, Result: {0}...", Callback.Result);

            if (Callback.Result == EResult.OK)
            {
                WebAPIUserNonce = Callback.WebAPIUserNonce;
                Log.Success("Successfully Logged On to Steam!");
            }
            else
            {
                // Stop the Timer so it doesn't handle any Callbacks (such as trying to Re-Connect and LogOn)
                CallbackTimer.Stop();

                if (Callback.Result == EResult.AccountLogonDenied)
                {
                    Log.Warn("This Account is protected by Steam Guard Email Authenticator...");
                    Log.Prompt("Please enter the Email Authentication Code for {0}, with an Email Address ending in {1}:", Username, Callback.EmailDomain);
                    while (String.IsNullOrEmpty(LogOnDetails.AuthCode))
                        LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.InvalidLoginAuthCode)
                {
                    string OldAuthCode = LogOnDetails.AuthCode;
                    Log.Error("The Email Authentication Code, {0}, provided for {1}, is incorrect...", OldAuthCode, Username);
                    Log.Prompt("Please enter the Email Authentication Code for {0}:", Username);
                    while (LogOnDetails.AuthCode == OldAuthCode)
                        LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.ExpiredLoginAuthCode)
                {
                    string OldAuthCode = LogOnDetails.AuthCode;
                    Log.Error("The Email Authentication Code, {0}, provided for {1}, has expired...", OldAuthCode, Username);
                    Log.Prompt("Please enter the Email Authentication Code for {0}:", Username);
                    while (LogOnDetails.AuthCode == OldAuthCode)
                        LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.AccountLoginDeniedNeedTwoFactor)
                {
                    Log.Warn("This Account is protected by Steam Guard Mobile Authenticator...");
                    Log.Info("You can allow this Bot to generate its own 2FA / Mobile Authenticator Codes by specifying a SharedSecret in the Config...");
                    Log.Prompt("Please enter the 2-Factor Authentication Code for {0}:", Username);
                    while (String.IsNullOrEmpty(LogOnDetails.TwoFactorCode))
                        LogOnDetails.TwoFactorCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.TwoFactorCodeMismatch)
                {
                    if (!String.IsNullOrEmpty(SteamGuardAccount.SharedSecret))
                        LogOnDetails.TwoFactorCode = SteamGuardAccount.GenerateSteamGuardCode();
                    else
                    {
                        string OldTwoFactorCode = LogOnDetails.TwoFactorCode;
                        Log.Error("The 2-Factor Authentication Code, {0}, provided for {1} is incorrect or has expired...", Username, OldTwoFactorCode);
                        Log.Prompt("Please enter the 2-Factor Authentication Code for {0}:", Username);
                        while (LogOnDetails.TwoFactorCode == OldTwoFactorCode)
                            LogOnDetails.TwoFactorCode = Console.ReadLine();
                    }
                }

                else if (Callback.Result == EResult.ServiceUnavailable || Callback.Result == EResult.TryAnotherCM)
                    Log.Warn("Failed to Log On to Steam, Steam may be down due to maintenance or high load...");
                else
                    Log.Warn("Failed to Log On to Steam, Result: {0}...", Callback.Result);

                CallbackTimer.Start();
            }
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback Callback)
        {
            Log.Warn("Logged Off of Steam, Result: {0}...", Callback.Result);
        }

        private void OnUpdateMachineAuth(SteamUser.UpdateMachineAuthCallback Callback)
        {
            if (String.IsNullOrEmpty(SteamGuardAccount.SharedSecret))
                Log.Info("Creating Machine Auth File...");

            SHA1 SHA = SHA1.Create();
            byte[] SentryHash = SHA.ComputeHash(Callback.Data);

            Directory.CreateDirectory("00_AuthFiles");
            File.WriteAllBytes(Path.Combine("00_AuthFiles", String.Format("{0}.auth", Username)), Callback.Data);

            SteamUser.MachineAuthDetails MachineAuthDetails = new SteamUser.MachineAuthDetails
            {
                BytesWritten = Callback.BytesToWrite,
                FileName = Callback.FileName,
                FileSize = Callback.BytesToWrite,
                Offset = Callback.Offset,
                // The SHA1 Hash we calculated earlier from the Callback Data
                SentryFileHash = SentryHash,
                OneTimePassword = Callback.OneTimePassword,
                LastError = 0,
                Result = EResult.OK,
                JobID = Callback.JobID,
            };

            if (String.IsNullOrEmpty(SteamGuardAccount.SharedSecret))
                Log.Success("Successfully created Machine Auth File!");

            SteamUser.SendMachineAuthResponse(MachineAuthDetails);
        }

        private void OnWebAPIUserNonce(SteamUser.WebAPIUserNonceCallback Callback)
        {
            if (Callback.Result == EResult.OK)
            {
                if (WebAPIUserNonce == Callback.Nonce)
                {
                    Log.Warn("Received duplicate WebAPIUserNonce, waiting 30 seconds before attempting to request a new one...");
                    Thread.Sleep(30000);
                    SteamUser.RequestWebAPIUserNonce();
                }
                else
                {
                    WebAPIUserNonce = Callback.Nonce;
                    AuthenticateSteamWebClient();
                }
            }
            else
                Log.Error("WebAPIUserNonceCallback Error, Result: {0}...", Callback.Result);
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback Callback)
        {
            UniqueID = Callback.UniqueID.ToString();
            AuthenticateSteamWebClient();

            SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback Callback)
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
                    XML.Load(String.Format("https://steamcommunity.com/gid/{0}/memberslistxml?xml=1&p=99999", friend.SteamID.ConvertToUInt64()));
                    Log.Info("Received Group Invite to {0} ({1}), ignoring...", XML.DocumentElement.SelectSingleNode("/memberList/groupDetails/groupName").InnerText, friend.SteamID.ConvertToUInt64());
                }
                else if (friend.SteamID.AccountType != EAccountType.Clan)
                {
                    if (friend.SteamID.AccountType == EAccountType.Individual)
                    {
                        if (friend.Relationship == EFriendRelationship.RequestRecipient)
                            Log.Info("{0} ({1}) added us to their friends list, ignoring...", SteamFriends.GetFriendPersonaName(friend.SteamID), friend.SteamID.ConvertToUInt64());
                        else if (friend.Relationship == EFriendRelationship.None)
                            Log.Info("{0} ({1}) removed us from their friends list...", SteamFriends.GetFriendPersonaName(friend.SteamID), friend.SteamID.ConvertToUInt64());
                    }
                }
            }
        }

        private void OnFriendMsg(SteamFriends.FriendMsgCallback Callback)
        {
            if (Callback.EntryType == EChatEntryType.ChatMsg)
            {
                Log.Info("Chat Message received from {0} ({1}): {2}", SteamFriends.GetFriendPersonaName(Callback.Sender), Callback.Sender.ConvertToUInt64(), Callback.Message);
            }
        }


        private void AuthenticateSteamWebClient()
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
