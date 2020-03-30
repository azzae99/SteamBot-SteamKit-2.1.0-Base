using System.Threading;
using SteamKit2;

namespace SteamBot
{
    public partial class Handlers
    {
        public static void OnWebAPIUserNonce(Bot Sender, SteamUser.WebAPIUserNonceCallback Callback)
        {
            if (Callback.Result == EResult.OK)
            {
                if (Sender.WebAPIUserNonce == Callback.Nonce)
                {
                    Sender.Log.Warn("Received duplicate WebAPIUserNonce, waiting 30 seconds before attempting to request a new one...");
                    Thread.Sleep(30000);
                    Sender.SteamUser.RequestWebAPIUserNonce();
                }
                else
                {
                    Sender.WebAPIUserNonce = Callback.Nonce;
                    Sender.AuthenticateSteamWebClient();
                }
            }
            else
                Sender.Log.Error("WebAPIUserNonceCallback Error, Result: {0}...", Callback.Result);
        }
    }
}
