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
                    inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                        String.Format("https://steamcommunity.com/inventory/{0}/{1}/{2}?l=english&count=5000{3}",
                        User.ConvertToUInt64(), AppID, ContextID, (StartAssetID != 0) ? "&start_assetid=" + StartAssetID : String.Empty), "GET"));

                    Success = inv.Success;

                    if (!Success)
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
            public uint Total_Inventory_Count { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            public string Error { get; set; }

            // Honestly have no idea what this is...
            private int rwgrsn { get; set; }
        }

        public class asset
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

        public class description
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

    public class TF2Inventory
    {
        private SteamWebClient SteamWebClient;

        public List<item> Items = new List<item>();
        public int Num_Backpack_Slots;
        public bool Status;
        public string Error;

        public TF2Inventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(SteamID User, string APIKey)
        {
            try
            {
                inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconItems_440/GetPlayerItems/v1/?key={0}&steamid={1}",
                    APIKey, User.ConvertToUInt64()), "GET"));

                Status = inv.Result.Status;

                if (!Status)
                {
                    Error = inv.Result.StatusDetail;
                    return;
                }

                if (inv.Result.Items != null)
                    Items = inv.Result.Items.ToList();

                Num_Backpack_Slots = inv.Result.Num_Backpack_Slots;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public class inventory
        {
            [JsonProperty("result")]
            public result Result { get; set; }
        }

        public class result
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("statusDetail")]
            public string StatusDetail { get; set; }

            [JsonProperty("num_backpack_slots")]
            public int Num_Backpack_Slots { get; set; }

            [JsonProperty("items")]
            public item[] Items { get; set; }
        }

        public class item
        {
            [JsonProperty("id")]
            public ulong ID { get; set; }

            [JsonProperty("original_id")]
            public ulong Original_ID { get; set; }

            [JsonProperty("defindex")]
            public int DefIndex { get; set; }

            [JsonProperty("level")]
            public int Level { get; set; }

            [JsonProperty("quality")]
            public int Quality { get; set; }

            [JsonProperty("inventory")]
            public ulong Inventory { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("origin")]
            public int Origin { get; set; }

            [JsonProperty("style")]
            public int Style { get; set; }

            [JsonProperty("equipped")]
            public equipped[] Equipped { get; set; }

            [JsonProperty("flag_cannot_trade")]
            public bool Flag_Cannot_Trade { get; set; }

            [JsonProperty("attributes")]
            public attribute[] Attributes { get; set; }
        }

        public class equipped
        {
            [JsonProperty("class")]
            public int Class { get; set; }
            
            [JsonProperty("slot")]
            public int Slot { get; set; }
        }

        public class attribute
        {
            [JsonProperty("defindex")]
            public int DefIndex { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("float_value")]
            public float Float_Value { get; set; }
        }
    }

    public class Dota2Inventory
    {
        private SteamWebClient SteamWebClient;

        public List<item> Items = new List<item>();
        public int Num_Backpack_Slots;
        public bool Status;
        public string Error;

        public Dota2Inventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(SteamID User, string APIKey)
        {
            try
            {
                inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconItems_570/GetPlayerItems/v1/?key={0}&steamid={1}",
                    APIKey, User.ConvertToUInt64()), "GET"));

                Status = inv.Result.Status;

                if (!Status)
                {
                    Error = inv.Result.StatusDetail;
                    return;
                }

                if (inv.Result.Items != null)
                    Items = inv.Result.Items.ToList();

                Num_Backpack_Slots = inv.Result.Num_Backpack_Slots;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public class inventory
        {
            [JsonProperty("result")]
            public result Result { get; set; }
        }

        public class result
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("statusDetail")]
            public string StatusDetail { get; set; }

            [JsonProperty("num_backpack_slots")]
            public int Num_Backpack_Slots { get; set; }

            [JsonProperty("items")]
            public item[] Items { get; set; }
        }

        public class item
        {
            [JsonProperty("id")]
            public ulong ID { get; set; }

            [JsonProperty("original_id")]
            public ulong Original_ID { get; set; }

            [JsonProperty("defindex")]
            public int DefIndex { get; set; }

            [JsonProperty("level")]
            public int Level { get; set; }

            [JsonProperty("quality")]
            public int Quality { get; set; }

            [JsonProperty("inventory")]
            public ulong Inventory { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("style")]
            public int Style { get; set; }

            [JsonProperty("equipped")]
            public equipped[] Equipped { get; set; }

            [JsonProperty("flag_cannot_trade")]
            public bool Flag_Cannot_Trade { get; set; }

            [JsonProperty("flag_cannot_craft")]
            public bool Flag_Cannot_Craft { get; set; }

            [JsonProperty("attributes")]
            public attribute[] Attributes { get; set; }
        }

        public class equipped
        {
            [JsonProperty("class")]
            public int Class { get; set; }

            [JsonProperty("slot")]
            public int Slot { get; set; }
        }

        public class attribute
        {
            [JsonProperty("defindex")]
            public int DefIndex { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("float_value")]
            public float Float_Value { get; set; }
        }
    }

    public class Portal2Inventory
    {
        private SteamWebClient SteamWebClient;

        public List<item> Items = new List<item>();
        public int Num_Backpack_Slots;
        public bool Status;
        public string Error;

        public Portal2Inventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(SteamID User, string APIKey)
        {
            try
            {
                inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconItems_620/GetPlayerItems/v1/?key={0}&steamid={1}",
                    APIKey, User.ConvertToUInt64()), "GET"));

                Status = inv.Result.Status;

                if (!Status)
                {
                    Error = inv.Result.StatusDetail;
                    return;
                }

                if (inv.Result.Items != null)
                    Items = inv.Result.Items.ToList();

                Num_Backpack_Slots = inv.Result.Num_Backpack_Slots;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public class inventory
        {
            [JsonProperty("result")]
            public result Result { get; set; }
        }

        public class result
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("statusDetail")]
            public string StatusDetail { get; set; }

            [JsonProperty("num_backpack_slots")]
            public int Num_Backpack_Slots { get; set; }

            [JsonProperty("items")]
            public item[] Items { get; set; }
        }

        public class item
        {
            [JsonProperty("id")]
            public ulong ID { get; set; }

            [JsonProperty("original_id")]
            public ulong Original_ID { get; set; }

            [JsonProperty("defindex")]
            public int DefIndex { get; set; }

            [JsonProperty("level")]
            public int Level { get; set; }

            [JsonProperty("quality")]
            public int Quality { get; set; }

            [JsonProperty("inventory")]
            public ulong Inventory { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("origin")]
            public int Origin { get; set; }

            [JsonProperty("equipped")]
            public equipped[] Equipped { get; set; }

            [JsonProperty("flag_cannot_trade")]
            public bool Flag_Cannot_Trade { get; set; }

            [JsonProperty("flag_cannot_craft")]
            public bool Flag_Cannot_Craft { get; set; }
        }

        public class equipped
        {
            [JsonProperty("class")]
            public int Class { get; set; }

            [JsonProperty("slot")]
            public int Slot { get; set; }
        }
    }

    public class BattleBlockTheaterInventory
    {
        private SteamWebClient SteamWebClient;

        public List<item> Items = new List<item>();
        public int Num_Backpack_Slots;
        public bool Status;
        public string Error;

        public BattleBlockTheaterInventory(ref SteamWebClient Client)
        {
            SteamWebClient = Client;
        }

        public void LoadInventory(SteamID User, string APIKey)
        {
            try
            {
                inventory inv = JsonConvert.DeserializeObject<inventory>(SteamWebClient.Request(
                    String.Format("https://api.steampowered.com/IEconItems_238460/GetPlayerItems/v1/?key={0}&steamid={1}",
                    APIKey, User.ConvertToUInt64()), "GET"));

                Status = inv.Result.Status;

                if (!Status)
                {
                    Error = inv.Result.StatusDetail;
                    return;
                }

                if (inv.Result.Items != null)
                    Items = inv.Result.Items.ToList();

                Num_Backpack_Slots = inv.Result.Num_Backpack_Slots;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public class inventory
        {
            [JsonProperty("result")]
            public result Result { get; set; }
        }

        public class result
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("statusDetail")]
            public string StatusDetail { get; set; }

            [JsonProperty("num_backpack_slots")]
            public int Num_Backpack_Slots { get; set; }

            [JsonProperty("items")]
            public item[] Items { get; set; }
        }

        public class item
        {
            [JsonProperty("id")]
            public ulong ID { get; set; }

            [JsonProperty("original_id")]
            public ulong Original_ID { get; set; }

            [JsonProperty("icon_url")]
            public string Icon_URL { get; set; }

            [JsonProperty("background_color")]
            public string Background_Color { get; set; }

            [JsonProperty("name_color")]
            public string Name_Color { get; set; }

            [JsonProperty("tradable")]
            public bool Tradable { get; set; }

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }

            [JsonProperty("market_tradable_restriction")]
            public int Market_Tradable_Restriction { get; set; }

            [JsonProperty("stacksize")]
            public int StackSize { get; set; }

            [JsonProperty("commodity")]
            public bool Commodity { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("descriptions")]
            public description[] Descriptions { get; set; }
        }

        public class description
        {
            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}
