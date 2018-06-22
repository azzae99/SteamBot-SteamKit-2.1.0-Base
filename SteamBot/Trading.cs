using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SteamKit2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SteamBot
{
    public class TradeOffer
    {
        public class Offer
        {
            [JsonProperty("newversion")]
            public bool NewVersion { get; set; }

            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("me")]
            public User MyItems { get; set; }

            [JsonProperty("them")]
            public User TheirItems { get; set; }

            public Offer()
            {
                Version = 1;
                MyItems = new User();
                TheirItems = new User();
            }
        }

        public class User
        {
            [JsonProperty("assets")]
            public List<Asset> Assets { get; private set; }

            [JsonProperty("currency")]
            public List<Asset> Currency { get; private set; }

            [JsonProperty("ready")]
            public bool Ready { get; set; }

            public User()
            {
                Assets = new List<Asset>();
                Currency = new List<Asset>();
            }

            public void AddItem(Asset asset)
            {
                if (!Assets.Contains(asset))
                    Assets.Add(asset);
            }

            public void AddItem(int appid, int contextid, int amount, ulong assetid)
            {
                AddItem(new Asset
                {
                    AppID = appid,
                    ContextID = contextid.ToString(),
                    Amount = amount,
                    AssetID = assetid.ToString()
                });
            }

            public void RemoveItem(Asset asset)
            {
                Assets.Remove(asset);
            }

            public void RemoveItem(int appid, int contextid, int amount, ulong assetid)
            {
                RemoveItem(new Asset
                {
                    AppID = appid,
                    ContextID = contextid.ToString(),
                    Amount = amount,
                    AssetID = assetid.ToString()
                });
            }

            public void AddCurrencyItem(Asset asset)
            {
                if (!Currency.Contains(asset))
                    Currency.Add(asset);
            }

            public void AddCurrencyItem(int appid, int contextid, int amount, ulong assetid)
            {
                AddCurrencyItem(new Asset
                {
                    AppID = appid,
                    ContextID = contextid.ToString(),
                    Amount = amount,
                    AssetID = assetid.ToString()
                });
            }

            public void RemoveCurrencyItem(Asset asset)
            {
                Currency.Remove(asset);
            }

            public void RemoveCurrencyItem(int appid, int contextid, int amount, ulong assetid)
            {
                RemoveCurrencyItem(new Asset
                {
                    AppID = appid,
                    ContextID = contextid.ToString(),
                    Amount = amount,
                    AssetID = assetid.ToString()
                });
            }
        }

        // Steam likes to have ContextIDs and AssetIDs as strings,
        // we can deserialize them as different integer data types,
        // but if it's needed in a request to Steam (such as sending a TradeOffer),
        // you will end up with a 500 Internal Server Error if they're not strings
        public class Asset
        {
            [JsonProperty("appid")]
            public int AppID { get; set; }

            [JsonProperty("contextid")]
            public string ContextID { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }

            [JsonProperty("assetid")]
            public string AssetID { get; set; }
        }

        public class SendResponse
        {
            [JsonProperty("tradeofferid")]
            public ulong TradeOfferID { get; set; }

            [JsonProperty("needs_mobile_confirmation")]
            public bool Needs_Mobile_Confirmation { get; set; }

            [JsonProperty("needs_email_confirmation")]
            public bool Needs_Email_Confirmation { get; set; }

            [JsonProperty("email_domain")]
            public string Email_Domain { get; set; }

            [JsonProperty("strError")]
            public string Error { get; set; }
        }

        public class AcceptResponse
        {
            //https://steamcommunity.com/trade/{tradeid}/receipt
            [JsonProperty("tradeid")]
            public ulong TradeID { get; set; }
        }

        public class DeclineResponse
        {
            [JsonProperty("tradeofferid")]
            public ulong TradeOfferID { get; set; }
        }
    }

    public class Trading
    {
        private SteamWebClient SteamWebClient;
        private string Key;

        public string Error;
        public string Response;

        public Trading(ref SteamWebClient Client, string APIKey)
        {
            SteamWebClient = Client;
            Key = APIKey;
        }

        public SteamID GetPartnerSteamID(ulong AccountID_Other)
        {
            return new SteamID(String.Format("STEAM_0:{0}:{1}", AccountID_Other & 1, AccountID_Other >> 1));
        }

        public List<CEcon_TradeOffer> GetReceivedTradeOffers(bool ActiveOnly = true)
        {
            try
            {
                tradeoffers offers = JsonConvert.DeserializeObject<tradeoffers>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconService/GetTradeOffers/v1/?key={0}&get_received_offers=1&active_only={1}",
                    Key, (ActiveOnly) ? "1" : "0"), "GET"));

                if (offers.Response.Trade_Offers_Received != null)
                    return offers.Response.Trade_Offers_Received.ToList();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return null;
        }

        public List<CEcon_TradeOffer> GetSentTradeOffers(bool ActiveOnly = true)
        {
            try
            {
                tradeoffers offers = JsonConvert.DeserializeObject<tradeoffers>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconService/GetTradeOffers/v1/?key={0}&get_sent_offers=1&active_only={1}",
                    Key, (ActiveOnly) ? "1" : "0"), "GET"));

                if (offers.Response.Trade_Offers_Sent != null)
                    return offers.Response.Trade_Offers_Sent.ToList();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return null;
        }

        public TradeOffer.Offer CreateTradeOffer()
        {
            return new TradeOffer.Offer();
        }

        public TradeOffer.SendResponse SendTradeOffer(TradeOffer.Offer Offer, SteamID Partner, string Token = null, string TradeMessage = null)
        {
            try
            {
                NameValueCollection Data = new NameValueCollection();
                Data.Add("sessionid", SteamWebClient.SessionID);
                Data.Add("serverid", "1");
                Data.Add("partner", Partner.ConvertToUInt64().ToString());
                Data.Add("tradeoffermessage", (String.IsNullOrEmpty(TradeMessage)) ? "Automatic TradeOffer" : TradeMessage);
                Data.Add("json_tradeoffer", JsonConvert.SerializeObject(Offer));
                Data.Add("trade_offer_create_params", (String.IsNullOrEmpty(Token)) ? "{}" : new JObject { "trade_offer_access_token", Token }.ToString());

                string Referer = String.Format("https://steamcommunity.com/tradeoffer/new/?partner={0}{1}", Partner.AccountID, (String.IsNullOrEmpty(Token)) ? String.Empty : "&token=" + Token);

                return JsonConvert.DeserializeObject<TradeOffer.SendResponse>(SteamWebClient.Request("https://steamcommunity.com/tradeoffer/new/send", "POST", Data, Referer));
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public TradeOffer.AcceptResponse AcceptTradeOffer(CEcon_TradeOffer Offer)
        {
            try
            {
                NameValueCollection Data = new NameValueCollection();
                Data.Add("sessionid", SteamWebClient.SessionID);
                Data.Add("serverid", "1");
                Data.Add("tradeofferid", Offer.TradeOfferID.ToString());
                Data.Add("partner", GetPartnerSteamID(Offer.AccountID_Other).ConvertToUInt64().ToString());

                string URL = String.Format("https://steamcommunity.com/tradeoffer/{0}/", Offer.TradeOfferID);

                return JsonConvert.DeserializeObject<TradeOffer.AcceptResponse>(SteamWebClient.Request(URL + "accept", "POST", Data, URL));
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public TradeOffer.DeclineResponse DeclineTradeOffer(CEcon_TradeOffer Offer)
        {
            try
            {
                NameValueCollection Data = new NameValueCollection();
                Data.Add("sessionid", SteamWebClient.SessionID);

                return JsonConvert.DeserializeObject<TradeOffer.DeclineResponse>(SteamWebClient.Request(String.Format("https://steamcommunity.com/tradeoffer/{0}/decline", Offer.TradeOfferID), "POST", Data));
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        private class tradeoffers
        {
            [JsonProperty("response")]
            public response Response { get; set; }
        }

        private class response
        {
            [JsonProperty("trade_offers_sent")]
            public CEcon_TradeOffer[] Trade_Offers_Sent { get; set; }

            [JsonProperty("trade_offers_received")]
            public CEcon_TradeOffer[] Trade_Offers_Received { get; set; }
        }

        public class CEcon_TradeOffer
        {
            [JsonProperty("tradeofferid")]
            public ulong TradeOfferID { get; set; }

            [JsonProperty("accountid_other")]
            public ulong AccountID_Other { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("expiration_time")]
            public ulong Expiration_Time { get; set; }

            [JsonProperty("trade_offer_state")]
            public ETradeOfferState Trade_Offer_State { get; set; }

            [JsonProperty("items_to_give")]
            public CEcon_Asset[] Items_To_Give { get; set; }

            [JsonProperty("items_to_receive")]
            public CEcon_Asset[] Items_To_Receive { get; set; }

            [JsonProperty("is_our_offer")]
            public bool Is_Our_Offer { get; set; }

            [JsonProperty("time_created")]
            public ulong Time_Created { get; set; }

            [JsonProperty("time_updated")]
            public ulong Time_Updated { get; set; }

            [JsonProperty("tradeid")]
            public ulong TradeID { get; set; }

            [JsonProperty("from_real_time_trade")]
            public bool From_Real_Time_Trade { get; set; }

            [JsonProperty("escrow_end_date")]
            public ulong Escrow_End_Date { get; set; }

            [JsonProperty("confirmation_method")]
            public ETradeOfferConfirmationMethod Confirmation_Method { get; set; }
        }

        public class CEcon_Asset : Inventory.asset
        {
            [JsonProperty("missing")]
            public bool Missing { get; set; }

            [JsonProperty("est_usd")]
            public ulong Est_USD { get; set; }
        }

        public enum ETradeOfferState
        {
            TradeOfferStateInvalid = 1,
            TradeOfferStateActive = 2,
            TradeOfferStateAccepted = 3,
            TradeOfferStateCountered = 4,
            TradeOfferStateExpired = 5,
            TradeOfferStateCanceled = 6,
            TradeOfferStateDeclined = 7,
            TradeOfferStateInvalidItems = 8,
            TradeOfferStateCreatedNeedsConfirmation = 9,
            TradeOfferStateCanceledBySecondFactor = 10,
            TradeOfferStateInEscrow = 11
        }

        public enum ETradeOfferConfirmationMethod
        {
            TradeOfferConfirmationMethod_Invalid = 0,
            TradeOfferConfirmationMethod_Email = 1,
            TradeOfferConfirmationMethod_MobileApp = 2
        }
    }
}
