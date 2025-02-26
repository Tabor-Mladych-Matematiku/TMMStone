using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CardData
{
    public struct CardData
    {
        public string name;

        public string rarity;

        public string type;

        public int cost;

        public string tag;

        public string Class;

        public string expansion;

        public List<int> token_list;
        public bool not_for_sale;
        public string attack;
        public string health;
    }
    public static class CDJsonUtils
    {
        public readonly static Dictionary<string, string> expansionMapping = new() {//Maps JSON name to folder name
            {"Základ", "V2"},
            {"PTMM","GULAG" }
        };
        static Dictionary<string, Dictionary<string, string>> ParseExtraJSON()
        {
            var extracardsJSONfile = Resources.Load<TextAsset>("CardData/extraCardData");
            object decodedJSON = MiniJson.JsonDecode(extracardsJSONfile.text);
            List<object> extraloadList = (List<object>)decodedJSON;
            Dictionary<string, Dictionary<string, string>> extraData = new();
            foreach (Dictionary<string, object> item in extraloadList)
            {
                string[] stats = ((string)item["Staty"]).Split("/");

                extraData.Add((string)item["Název"], new() { { "Attack", stats[0] }, { "Health", stats[1] } });
            }
            return extraData;
        }
        public static Dictionary<int, CardData> LoadCardDatabase()
        {
            var cardsJSONfile = Resources.Load<TextAsset>("CardData/cards");
            object decodedJSON = MiniJson.JsonDecode(cardsJSONfile.text);
            var loadDict = (Dictionary<string, object>)decodedJSON;

            Dictionary<string, Dictionary<string, string>> extraData = ParseExtraJSON();

            Dictionary<int, CardData> CardDatabase = new();
            foreach (var pair in loadDict)
            {
                var value = (Dictionary<string, object>)pair.Value;
                if (value["cost"] is String) continue;//We'll deal with you later (These are the ? costs)
                string cardname = (string)value["name"];
                if (cardname.EndsWith(" (token)")) { cardname = cardname[..^" (token)".Length]; }//Truly the most elegant solution. SarcasmError: maximum sarcasm value exceeded
                CardData data = new()
                {
                    name = (string)value["name"],
                    rarity = (string)value["rarity"],
                    type = (string)value["type"],
                    cost = (int)(System.Int64)value["cost"],
                    tag = (string)value["tag"],
                    Class = (string)value["Class"],
                    expansion = (string)value["expansion"],
                    not_for_sale = (bool)value["not_for_sale"],
                    attack = extraData[cardname]["Attack"],
                    health = extraData[cardname]["Health"]

                };
                if (((List<object>)value["token_list"]).Count != 0)
                {
                    data.token_list = new();
                    foreach (long item in (List<object>)value["token_list"])
                    {
                        data.token_list.Add((int)item);
                    }
                }
                CardDatabase.Add(int.Parse(pair.Key), data);
            }
            return CardDatabase;
        }
    }
}