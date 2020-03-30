using System.Threading;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnDisconnected(Bot Sender, SteamClient.DisconnectedCallback Callback)
        {
            if (!Callback.UserInitiated)
            {
                Sender.Log.Warn("Connection to Steam has been lost, retrying in 20 seconds...");
                Sender.CallbackTimer.Stop();
                Thread.Sleep(20000);
                Sender.SteamClient.Connect();
                Sender.CallbackTimer.Start();
            }
            else
            {
                Sender.Log.Info("Connection to Steam has been lost (user initiated), not retrying...");
                Sender.CallbackTimer.Stop();
            }
        }
    }
}
