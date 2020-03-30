using System;
using System.IO;
using System.Security.Cryptography;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnConnected(Bot Sender, SteamClient.ConnectedCallback Callback)
        {
            // Normally you'd check the EResult of the Callback to verify if the bot actually
            // connected or not, but it seems to have been removed...
            if (Callback != null)
            {
                Sender.Log.Success("Connection to Steam established!");

                if (!String.IsNullOrEmpty(Sender.SteamGuardAccount.SharedSecret))
                    Sender.LogOnDetails.TwoFactorCode = Sender.SteamGuardAccount.GenerateSteamGuardCode();
                else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "00_AuthFiles", $"{Sender.Username}.auth")))
                {
                    SHA1 SHA = SHA1.Create();
                    byte[] SentryHash = SHA.ComputeHash(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "00_AuthFiles", $"{Sender.Username}.auth")));
                    Sender.LogOnDetails.SentryFileHash = SentryHash;
                }

                Sender.Log.Info("Logging In to Steam...");
                Sender.SteamUser.LogOn(Sender.LogOnDetails);
            }
        }
    }
}
