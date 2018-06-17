using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using Newtonsoft.Json;

namespace SteamBot
{
    public class Inventory
    {
        private SteamWebClient SteamWebClient;

        public List<Tuple<asset, description>> Items = new List<Tuple<asset, description>>();
        public int Total_Inventory_Count;
        public bool Success;
        public string Error;

        public Inventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(int AppID, IEnumerable<int> ContextIDs, SteamID User, long StartAssetID = 0)
        {
            try
            {
                foreach (int ContextID in ContextIDs)
                {
                    inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                        String.Format("https://steamcommunity.com/inventory/{0}/{1}/{2}?l=english&count=5000{3}",
                        User.ConvertToUInt64(), AppID, ContextID, (StartAssetID != 0) ? "&start_assetid=" + StartAssetID : String.Empty), "GET"));

                    Success = inv.Success;

                    if (!inv.Success)
                    {
                        Error = inv.Error;
                        continue;
                    }

                    if (inv.Assets != null)
                    {
                        foreach (asset Asset in inv.Assets)
                            Items.Add(new Tuple<asset, description>(Asset, inv.Descriptions.First(x => x.ClassID == Asset.ClassID)));

                        if (inv.Total_Inventory_Count > 5000 && Items.Last().Item1.AssetID != StartAssetID)
                            LoadInventory(AppID, new int[] { ContextID }, User, Items.Last().Item1.AssetID);
                    }

                    Total_Inventory_Count = inv.Total_Inventory_Count;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public class inventory
        {
            [JsonProperty("assets")]
            public asset[] Assets { get; set; }

            [JsonProperty("descriptions")]
            public description[] Descriptions { get; set; }

            [JsonProperty("total_inventory_count")]
            public int Total_Inventory_Count { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            public string Error { get; set; }

            // Honestly have no idea what this is...
            private uint rwgrsn { get; set; }
        }

        public class asset
        {
            [JsonProperty("appid")]
            public int AppID { get; set; }

            [JsonProperty("contextid")]
            public int ContextID { get; set; }

            [JsonProperty("assetid")]
            public long AssetID { get; set; }

            [JsonProperty("classid")]
            public long ClassID { get; set; }

            [JsonProperty("instanceid")]
            public long InstanceID { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        public class description
        {
            [JsonProperty("appid")]
            public int AppID { get; set; }

            [JsonProperty("classid")]
            public long ClassID { get; set; }

            [JsonProperty("instanceid")]
            public long InstanceID { get; set; }

            [JsonProperty("currency")]
            public int Currency { get; set; }

            [JsonProperty("background_color")]
            public string Background_Color { get; set; }

            [JsonProperty("icon_url")]
            public string Icon_URL { get; set; }

            [JsonProperty("icon_url_large")]
            public string Icon_URL_Large { get; set; }

            [JsonProperty("descriptions")]
            public inner_description[] InnerDescription { get; set; }

            [JsonProperty("tradable")]
            public bool Tradable { get; set; }

            [JsonProperty("actions")]
            public action[] Actions { get; set; }

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
            public action[] Market_Actions { get; set; }

            [JsonProperty("commodity")]
            public bool Commodity { get; set; }

            [JsonProperty("market_tradable_restriction")]
            public int Market_Tradable_Restriction { get; set; }

            [JsonProperty("market_marketable_restriction")]
            public int Market_Marketable_Restriction { get; set; }

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }

            [JsonProperty("tags")]
            public tag[] Tags { get; set; }
        }

        public class inner_description
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }
        }

        public class action
        {
            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class tag
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
}
