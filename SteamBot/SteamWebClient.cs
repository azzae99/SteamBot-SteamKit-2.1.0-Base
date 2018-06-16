using System;
using System.Net;
using SteamKit2;
using System.Text;
// System.Web must be added as a reference from the framework assemblies
using System.Web;
using System.Collections.Specialized;
using System.Net.Cache;
using System.IO;

namespace SteamBot
{
    public class SteamWebClient
    {
        public const string SteamCommunityDomain = "steamcommunity.com";
        public const string SteamPoweredDomain = "steampowered.com";
        private CookieContainer Cookies;

        public string SessionID { get; private set; }
        public string Token { get; private set; }
        public string TokenSecure { get; private set; }

        public bool Authenticate(string UniqueID, SteamClient Client, string WebAPIUserNonce)
        {
            Token = TokenSecure = String.Empty;
            SessionID = Convert.ToBase64String(Encoding.UTF8.GetBytes(UniqueID));
            Cookies = new CookieContainer();

            using (dynamic SteamUserAuth = WebAPI.GetInterface("ISteamUserAuth"))
            {
                // Generate AES Session Key
                byte[] SessionKey = CryptoHelper.GenerateRandomBlock(32);

                // RSA Encrypt the SessionKey with Steam's Public RSA Key for our Account's Universe
                byte[] EncryptedSessionKey = null;
                using (RSACrypto Crypto = new RSACrypto(KeyDictionary.GetPublicKey(Client.Universe)))
                    EncryptedSessionKey = Crypto.Encrypt(SessionKey);

                byte[] LoginKey = new byte[20];
                Array.Copy(Encoding.ASCII.GetBytes(WebAPIUserNonce), LoginKey, WebAPIUserNonce.Length);

                // AES Encrypt the LoginKey with our SessionKey
                byte[] EncryptedLoginKey = CryptoHelper.SymmetricEncrypt(LoginKey, SessionKey);

                KeyValue AuthResult;
                try
                {
                    AuthResult = SteamUserAuth.AuthenticateUser(
                        steamid: Client.SteamID.ConvertToUInt64(),
                        sessionkey: HttpUtility.UrlEncode(EncryptedSessionKey),
                        encrypted_loginkey: HttpUtility.UrlEncode(EncryptedLoginKey),
                        method: "POST",
                        secure: true
                    );
                }
                catch (Exception)
                {
                    Token = TokenSecure = null;
                    return false;
                }

                Token = AuthResult["token"].AsString();
                TokenSecure = AuthResult["tokensecure"].AsString();

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

                return true;
            }
        }

        public string Request(string URL, string Method, NameValueCollection Data = null, string Referer = null)
        {
            try
            {
                // Convert Data to a Query String with URL Parameters
                string QueryString = (Data == null ? null : String.Join("&", Array.ConvertAll(Data.AllKeys, Key =>
                    String.Format("{0}={1}", HttpUtility.UrlEncode(Key), HttpUtility.UrlEncode(Data[Key]))
                )));

                // Add QueryString to URL if it exists
                if (Method == "GET" && !String.IsNullOrEmpty(QueryString))
                    URL += (URL.Contains("?") ? "&" : "?") + QueryString;

                // Create the Request
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
                Request.Method = Method;
                Request.Accept = "application/json, application/xml, text/html, application/xhtml+xml, text/javascript;q=0.9, */*;q=0.5";
                Request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
                Request.Referer = String.IsNullOrEmpty(Referer) ? "https://steamcommunity.com/trade/1" : Referer;
                Request.Timeout = 15000;
                Request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
                Request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                // These are normally set for AJAX, but there's no real reason to disable it
                Request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                Request.Headers.Add("X-Prototype-Version", "1.7");
                // Add our Cookies
                Request.CookieContainer = Cookies;

                if (Method == "POST" && !String.IsNullOrEmpty(QueryString))
                {
                    byte[] QueryBytes = Encoding.UTF8.GetBytes(QueryString);
                    Request.ContentLength = QueryBytes.Length;

                    using (Stream RequestStream = Request.GetRequestStream())
                        RequestStream.Write(QueryBytes, 0, QueryBytes.Length);
                }

                using (StreamReader Reader = new StreamReader(Request.GetResponse().GetResponseStream()))
                    return Reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
