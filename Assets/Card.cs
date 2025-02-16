using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame
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
    }
    public class Card : MonoBehaviour
    {
        public enum CardType
        {
            Minion,
            Spell
        }
        public int mana;
        public string cardname;
        public CardType cardType;
        Sprite face;
        Sprite cardBack;
        bool hidden;
        readonly Dictionary<string,string> expansionMapping = new () {
            {"Základ", "V2"},
            {"PTMM","GULAG" }
        };
        public bool Hidden { get => hidden; set {  hidden = value;
                if (hidden) GetComponent<SpriteRenderer>().sprite = cardBack;
                else GetComponent<SpriteRenderer>().sprite = face;
            } }
        // Start is called before the first frame update

        public void initialize(CardData data)
        {
            mana = data.cost;
            cardname = data.name;
            switch (data.type)
            {
                case "Token":
                case "Jednotka":
                    cardType = CardType.Minion;
                    break;
                case "Spelltoken":
                case "Spell":
                    cardType = CardType.Spell;
                    break;
            }
            string expansion = "Tokeny";
            if(expansionMapping.ContainsKey(data.expansion))expansion = expansionMapping[data.expansion];
            Sprite sprite = Resources.Load<Sprite>("CardData/" + expansion + "/" + cardname);
            if (sprite != null) {
                face = sprite;
                GetComponent<SpriteRenderer>().sprite = face;
            }
            cardBack = Resources.Load<Sprite>("CardData/card-back");
        }
        public void OnDiscard()
        {

        }
    }
}