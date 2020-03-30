using System;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnLoggedOn(Bot Sender, SteamUser.LoggedOnCallback Callback)
        {
            Sender.Log.Debug("Received LoggedOn Callback, Result: {0}...", Callback.Result);

            if (Callback.Result == EResult.OK)
            {
                Sender.WebAPIUserNonce = Callback.WebAPIUserNonce;
                Sender.Log.Success("Successfully Logged On to Steam!");
            }
            else
            {
                // Stop the Timer so it doesn't handle any Callbacks (such as trying to Re-Connect and LogOn)
                Sender.CallbackTimer.Stop();

                if (Callback.Result == EResult.AccountLogonDenied)
                {
                    Sender.Log.Warn("This Account is protected by Steam Guard Email Authenticator...");
                    Sender.Log.Prompt("Please enter the Email Authentication Code for {0}, with an Email Address ending in {1}:", Sender.Username, Callback.EmailDomain);
                    while (String.IsNullOrEmpty(Sender.LogOnDetails.AuthCode))
                        Sender.LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.InvalidLoginAuthCode)
                {
                    string OldAuthCode = Sender.LogOnDetails.AuthCode;
                    Sender.Log.Error("The Email Authentication Code, {0}, provided for {1}, is incorrect...", OldAuthCode, Sender.Username);
                    Sender.Log.Prompt("Please enter the Email Authentication Code for {0}:", Sender.Username);
                    while (Sender.LogOnDetails.AuthCode == OldAuthCode)
                        Sender.LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.ExpiredLoginAuthCode)
                {
                    string OldAuthCode = Sender.LogOnDetails.AuthCode;
                    Sender.Log.Error("The Email Authentication Code, {0}, provided for {1}, has expired...", OldAuthCode, Sender.Username);
                    Sender.Log.Prompt("Please enter the Email Authentication Code for {0}:", Sender.Username);
                    while (Sender.LogOnDetails.AuthCode == OldAuthCode)
                        Sender.LogOnDetails.AuthCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.AccountLoginDeniedNeedTwoFactor)
                {
                    Sender.Log.Warn("This Account is protected by Steam Guard Mobile Authenticator...");
                    Sender.Log.Info("You can allow this Bot to generate its own 2FA / Mobile Authenticator Codes by specifying a SharedSecret in the Config...");
                    Sender.Log.Prompt("Please enter the 2-Factor Authentication Code for {0}:", Sender.Username);
                    while (String.IsNullOrEmpty(Sender.LogOnDetails.TwoFactorCode))
                        Sender.LogOnDetails.TwoFactorCode = Console.ReadLine();
                }
                else if (Callback.Result == EResult.TwoFactorCodeMismatch)
                {
                    if (!String.IsNullOrEmpty(Sender.SteamGuardAccount.SharedSecret))
                        Sender.LogOnDetails.TwoFactorCode = Sender.SteamGuardAccount.GenerateSteamGuardCode();
                    else
                    {
                        string OldTwoFactorCode = Sender.LogOnDetails.TwoFactorCode;
                        Sender.Log.Error("The 2-Factor Authentication Code, {0}, provided for {1} is incorrect or has expired...", Sender.Username, OldTwoFactorCode);
                        Sender.Log.Prompt("Please enter the 2-Factor Authentication Code for {0}:", Sender.Username);
                        while (Sender.LogOnDetails.TwoFactorCode == OldTwoFactorCode)
                            Sender.LogOnDetails.TwoFactorCode = Console.ReadLine();
                    }
                }

                else if (Callback.Result == EResult.ServiceUnavailable || Callback.Result == EResult.TryAnotherCM)
                    Sender.Log.Warn("Failed to Log On to Steam, Steam may be down due to maintenance or high load...");
                else
                    Sender.Log.Warn("Failed to Log On to Steam, Result: {0}...", Callback.Result);

                Sender.CallbackTimer.Start();
            }
        }
    }
}
