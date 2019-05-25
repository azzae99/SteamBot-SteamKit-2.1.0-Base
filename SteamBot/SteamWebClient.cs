using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SteamKit2;

namespace SteamBot
{
    public class SteamWebClient
    {
        public const string SteamCommunityDomain = "steamcommunity.com";
        public const string SteamPoweredDomain = "steampowered.com";

        private HttpClient Client;
        private CookieContainer Cookies;

        public string SessionID { get; private set; }
        public string Token { get; private set; }
        public string TokenSecure { get; private set; }

        public class Response
        {
            public Dictionary<string, string> Headers;
            public string Data;
            public string Error;
        }

        public bool Authenticate(string WebAPIUserNonce, string UniqueID, ulong SteamID64, EUniverse Universe)
        {
            // Fix "The request was aborted: Could not create SSL/TLS secure channel" error
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            SessionID = Convert.ToBase64String(Encoding.UTF8.GetBytes(UniqueID));
            Cookies = new CookieContainer();

            using (dynamic SteamUserAuth = WebAPI.GetInterface("ISteamUserAuth"))
            {
                // Generate AES Session Key
                byte[] SessionKey = CryptoHelper.GenerateRandomBlock(32);

                // RSA Encrypt the SessionKey with Steam's Public RSA Key for our Account's Universe
                byte[] EncryptedSessionKey = null;
                using (RSACrypto Crypto = new RSACrypto(KeyDictionary.GetPublicKey(Universe)))
                {
                    EncryptedSessionKey = Crypto.Encrypt(SessionKey);
                }

                byte[] LoginKey = new byte[20];
                Array.Copy(Encoding.ASCII.GetBytes(WebAPIUserNonce), LoginKey, WebAPIUserNonce.Length);

                // AES Encrypt the LoginKey with our SessionKey
                byte[] EncryptedLoginKey = CryptoHelper.SymmetricEncrypt(LoginKey, SessionKey);

                KeyValue AuthResult;
                try
                {
                    AuthResult = SteamUserAuth.AuthenticateUser(
                        steamid: SteamID64,
                        sessionkey: HttpUtility.UrlEncode(EncryptedSessionKey),
                        encrypted_loginkey: HttpUtility.UrlEncode(EncryptedLoginKey),
                        method: "POST",
                        secure: true
                    );
                }
                catch (Exception)
                {
                    return false;
                }

                Token = AuthResult["token"].AsString();
                TokenSecure = AuthResult["tokensecure"].AsString();
            }

            // Add Cookies for SteamCommunity
            Cookies.Add(new Cookie("sessionid", SessionID, String.Empty, SteamCommunityDomain));
            Cookies.Add(new Cookie("steamLogin", Token, String.Empty, SteamCommunityDomain));
            Cookies.Add(new Cookie("steamLoginSecure", TokenSecure, String.Empty, SteamCommunityDomain));

            // Add Cookies for SteamPowered
            Cookies.Add(new Cookie("sessionid", SessionID, String.Empty, SteamPoweredDomain));
            Cookies.Add(new Cookie("steamLogin", Token, String.Empty, SteamPoweredDomain));
            Cookies.Add(new Cookie("steamLoginSecure", TokenSecure, String.Empty, SteamPoweredDomain));
            Cookies.Add(new Cookie("birthtime", "-729000000", String.Empty, SteamPoweredDomain));
            Cookies.Add(new Cookie("lastagecheckage", "1-January-1900", String.Empty, SteamPoweredDomain));
            Cookies.Add(new Cookie("mature_content", "1", String.Empty, SteamPoweredDomain));

            Client = new HttpClient(new WebRequestHandler
            {
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate),
                CookieContainer = Cookies,
            });

            Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-AU,en-GB,en-US,en");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36");

            return true;
        }

        public Response Request(string URL, HttpMethod Method, Dictionary<string, string> Data = null, string Referer = null)
        {
            try
            {
                Task<HttpResponseMessage> ResponseMessage;
                HttpRequestMessage RequestMessage = new HttpRequestMessage()
                {
                    Method = Method,
                };

                if (Method == HttpMethod.Get)
                {
                    if (Data != null)
                    {
                        NameValueCollection Collection = HttpUtility.ParseQueryString(String.Empty);
                        foreach (KeyValuePair<string, string> Pair in Data)
                        {
                            Collection[Pair.Key] = Pair.Value;
                        }

                        URL = $"{URL}?{Collection.ToString()}";
                    }
                }
                else if (Method == HttpMethod.Post)
                {
                    if (Data != null)
                    {
                        RequestMessage.Content = new FormUrlEncodedContent(Data);
                    }
                }

                RequestMessage.RequestUri = new Uri(URL, UriKind.Absolute);
                if (!String.IsNullOrEmpty(Referer))
                {
                    RequestMessage.Headers.Referrer = new Uri(Referer, UriKind.Absolute);
                }

                ResponseMessage = Client.SendAsync(RequestMessage);

                return new Response
                {
                    Headers = ResponseMessage.Result.Headers.ToDictionary(x => x.Key, x => x.Value.First()),
                    Data = ResponseMessage.Result.Content.ReadAsStringAsync().Result,
                    Error = String.Empty
                };
            }
            catch (Exception Ex)
            {
                return new Response { Error = Ex.Message };
            }
        }

        public bool CheckCookies()
        {
            string SteamCommunity = Request("https://steamcommunity.com/", HttpMethod.Get).Data;
            string SteamPowered = Request("https://store.steampowered.com/", HttpMethod.Get).Data;
            return (SteamCommunity.Contains("g_steamID") && !SteamPowered.Contains("var g_AccountID = 0;"));
        }
    }
}
