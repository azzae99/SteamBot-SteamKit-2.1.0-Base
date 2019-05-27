using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SteamKit2;
using Newtonsoft.Json;

namespace SteamBot
{
    public class Inventory
    {
        private SteamWebClient SteamWebClient;

        public List<Item> Items = new List<Item>();
        public uint Total_Inventory_Count;
        public bool Success;
        public string Error;

        public Inventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(int AppID, IEnumerable<int> ContextIDs, SteamID User, ulong StartAssetID = 0)
        {
            try
            {
                foreach (int ContextID in ContextIDs)
                {
                    LoadInventoryResponse inventory = JsonConvert.DeserializeObject<LoadInventoryResponse>(SteamWebClient.Request(
                        String.Format("https://steamcommunity.com/inventory/{0}/{1}/{2}?l=english&count=5000{3}",
                        User.ConvertToUInt64(), AppID, ContextID, (StartAssetID != 0) ? "&start_assetid=" + StartAssetID : String.Empty), HttpMethod.Get).Data);

                    Success = inventory.Success;

                    if (!Success)
                    {
                        Error = inventory.Error;
                        continue;
                    }

                    if (inventory.Assets != null)
                    {
                        foreach (Asset asset in inventory.Assets)
                        {
                            Item item = new Item(asset, inventory.Descriptions.First(x => x.ClassID == asset.ClassID));
                            if (!Items.Contains(item))
                                Items.Add(item);
                        }

                        if (inventory.Total_Inventory_Count > 5000 && Items.Last().AssetID != StartAssetID)
                            LoadInventory(AppID, new int[] { ContextID }, User, Items.Last().AssetID);
                    }

                    Total_Inventory_Count = inventory.Total_Inventory_Count;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private class LoadInventoryResponse
        {
            [JsonProperty("assets")]
            public Asset[] Assets { get; set; }

            [JsonProperty("descriptions")]
            public Description[] Descriptions { get; set; }

            [JsonProperty("total_inventory_count")]
            public uint Total_Inventory_Count { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            public string Error { get; set; }

            // Honestly have no idea what this is...
            private int rwgrsn { get; set; }
        }

        public class Asset
        {
            [JsonProperty("appid")]
            public int AppID { get; set; }

            [JsonProperty("contextid")]
            public int ContextID { get; set; }

            [JsonProperty("assetid")]
            public ulong AssetID { get; set; }

            [JsonProperty("classid")]
            public ulong ClassID { get; set; }

            [JsonProperty("instanceid")]
            public ulong InstanceID { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        public class Description
        {
            [JsonProperty("appid")]
            public int AppID { get; set; }

            [JsonProperty("classid")]
            public ulong ClassID { get; set; }

            [JsonProperty("instanceid")]
            public ulong InstanceID { get; set; }

            [JsonProperty("currency")]
            public int Currency { get; set; }

            [JsonProperty("background_color")]
            public string Background_Color { get; set; }

            [JsonProperty("icon_url")]
            public string Icon_URL { get; set; }

            [JsonProperty("icon_url_large")]
            public string Icon_URL_Large { get; set; }

            [JsonProperty("descriptions")]
            public Inner_Description[] InnerDescriptions { get; set; }

            [JsonProperty("tradable")]
            public bool Tradable { get; set; }

            [JsonProperty("actions")]
            public Action[] Actions { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("name_color")]
            public string Name_Color { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("market_name")]
            public string Market_Name { get; set; }

            [JsonProperty("market_hash_name")]
            public string Market_Hash_Name { get; set; }

            [JsonProperty("market_fee_app")]
            public int Market_Fee_App { get; set; }

            [JsonProperty("market_actions")]
            public Action[] Market_Actions { get; set; }

            [JsonProperty("commodity")]
            public bool Commodity { get; set; }

            [JsonProperty("market_tradable_restriction")]
            public int Market_Tradable_Restriction { get; set; }

            [JsonProperty("market_marketable_restriction")]
            public int Market_Marketable_Restriction { get; set; }

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }

            [JsonProperty("tags")]
            public Tag[] Tags { get; set; }


            public class Inner_Description
            {
                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("value")]
                public string Value { get; set; }

                [JsonProperty("color")]
                public string Color { get; set; }
            }

            public class Action
            {
                [JsonProperty("link")]
                public string Link { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }
            }

            public class Tag
            {
                [JsonProperty("category")]
                public string Category { get; set; }

                [JsonProperty("internal_name")]
                public string Internal_Name { get; set; }

                [JsonProperty("localized_category_name")]
                public string Localized_Category_Name { get; set; }

                [JsonProperty("localized_tag_name")]
                public string Localized_Tag_Name { get; set; }

                [JsonProperty("color")]
                public string Color { get; set; }
            }
        }

        public class Item
        {
            public Item(Asset asset, Description description)
            {
                AppID = asset.AppID;
                ContextID = asset.ContextID;
                AssetID = asset.AssetID;
                ClassID = asset.ClassID;
                InstanceID = asset.InstanceID;
                Amount = asset.Amount;

                Currency = description.Currency;
                Background_Color = description.Background_Color;
                Icon_URL = description.Icon_URL;
                Icon_URL_Large = description.Icon_URL_Large;
                InnerDescriptions = description.InnerDescriptions;
                Tradable = description.Tradable;
                Actions = description.Actions;
                Name = description.Name;
                Name_Color = description.Name_Color;
                Type = description.Type;
                Market_Name = description.Market_Name;
                Market_Hash_Name = description.Market_Hash_Name;
                Market_Fee_App = description.Market_Fee_App;
                Market_Actions = description.Market_Actions;
                Commodity = description.Commodity;
                Market_Tradable_Restriction = description.Market_Tradable_Restriction;
                Market_Marketable_Restriction = description.Market_Marketable_Restriction;
                Marketable = description.Marketable;
                Tags = description.Tags;
            }

            public int AppID;
            public int ContextID;
            public ulong AssetID;
            public ulong ClassID;
            public ulong InstanceID;
            public int Amount;

            public int Currency;
            public string Background_Color;
            public string Icon_URL;
            public string Icon_URL_Large;
            public Description.Inner_Description[] InnerDescriptions;
            public bool Tradable;
            public Description.Action[] Actions;
            public string Name;
            public string Name_Color;
            public string Type;
            public string Market_Name;
            public string Market_Hash_Name;
            public int Market_Fee_App;
            public Description.Action[] Market_Actions;
            public bool Commodity;
            public int Market_Tradable_Restriction;
            public int Market_Marketable_Restriction;
            public bool Marketable;
            public Description.Tag[] Tags;
        }
    }
}
