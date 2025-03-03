using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Linq;
using UnityEditor;

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

        //public List<int> token_list;
        public bool not_for_sale;
        public string attack;
        public string health;
        public bool processed;
        public List<string> scripts;
    }
    public static class CDJsonUtils
    {
        static Dictionary<int, List<string>> scriptpaths;
        public static string jsonGUID;

        public static Dictionary<int, List<string>> Scriptpaths
        {
            get
            {
                if (scriptpaths == null)
                {
                    TextAsset json = Resources.Load<TextAsset>("CardData/Scripts/cardScriptsPaths");
                    Debug.Log(json);
                    Dictionary<string, object> decodedJSON = JSONToDict(json.text); 
                    scriptpaths = new();
                    foreach (KeyValuePair<string, object> item in decodedJSON) {
                        List<string> strings = new();
                        foreach (object item1 in (List<object>)item.Value) { strings.Add((string)item1); }
                        scriptpaths.Add(int.Parse(item.Key), strings);
                    }
                }
                return scriptpaths;
            }
        }
        public readonly static Dictionary<string, string> expansionMapping = new() {//Maps JSON name to folder name
            {"Základ", "V2"},
            {"PTMM","GULAG" }
        };
        public static List<object> JSONToList(string json)
        {
            object decodedJSON = MiniJson.JsonDecode(json);
            return (List<object>)decodedJSON;
        }
        public static Dictionary<string, object> JSONToDict(string json)
        {
            object decodedJSON = MiniJson.JsonDecode(json);
            var x = (Dictionary<string, object>)decodedJSON;
            return x;
        }

        static Dictionary<string, Dictionary<string, string>> ParseExtraJSON()
        {
            List<object> extraloadList = JSONToList(Resources.Load<TextAsset>("CardData/extraCardData").text);
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
            Dictionary<string, Dictionary<string, string>> extraData = ParseExtraJSON();
            Dictionary<int, CardData> CardDatabase = new();
            foreach (var (id, value) in from pair in JSONToDict(Resources.Load<TextAsset>("CardData/cards").text)
                                        select (int.Parse(pair.Key), (Dictionary<string, object>)pair.Value))
            {
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
                    health = extraData[cardname]["Health"],
                    scripts = Scriptpaths.ContainsKey(id) ? Scriptpaths[id] : new()

                };
                //if (Scriptpaths.ContainsKey(id)) { data.scripts = LoadScripts(Scriptpaths[id]); }
                /*if (((List<object>)value["token_list"]).Count != 0) // I doubt we'd be using tokenlists
                                {
                                    data.token_list = new();
                                    foreach (long item in (List<object>)value["token_list"])
                                    {
                                        data.token_list.Add((int)item);
                                    }
                                }*/
                CardDatabase.Add(id, data);
            }

            return CardDatabase;
        }

    }
}