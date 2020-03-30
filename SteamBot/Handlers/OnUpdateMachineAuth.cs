using System;
using System.IO;
using System.Security.Cryptography;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnUpdateMachineAuth(Bot Sender, SteamUser.UpdateMachineAuthCallback Callback)
        {
            if (String.IsNullOrEmpty(Sender.SteamGuardAccount.SharedSecret))
                Sender.Log.Info("Creating Machine Auth File...");

            SHA1 SHA = SHA1.Create();
            byte[] SentryHash = SHA.ComputeHash(Callback.Data);

            Directory.CreateDirectory("00_AuthFiles");
            File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "00_AuthFiles", $"{Sender.Username}.auth"), Callback.Data);

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

            if (String.IsNullOrEmpty(Sender.SteamGuardAccount.SharedSecret))
                Sender.Log.Success("Successfully created Machine Auth File!");

            Sender.SteamUser.SendMachineAuthResponse(MachineAuthDetails);
        }
    }
}
