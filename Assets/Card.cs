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
        public Vector3 standardScale;
        readonly static Dictionary<string, string> expansionMapping = new() {//Maps JSON name to folder name
            {"Základ", "V2"},
            {"PTMM","GULAG" }
        };
        public bool Hidden
        {
            get => hidden; set
            {
                hidden = value;
                if (hidden) GetComponent<SpriteRenderer>().sprite = cardBack;
                else GetComponent<SpriteRenderer>().sprite = face;
            }
        }
        // Start is called before the first frame update
        private void Start()
        {
            standardScale = transform.localScale;
        }
        public void Initialize(CardData data)
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
            if (expansionMapping.ContainsKey(data.expansion)) expansion = expansionMapping[data.expansion];
            Sprite sprite = Resources.Load<Sprite>("CardData/" + expansion + "/" + cardname);
            if (sprite != null)
            {
                face = sprite;
                GetComponent<SpriteRenderer>().sprite = face;
            }
            cardBack = Resources.Load<Sprite>("CardData/card-back");
        }
        public void OnDiscard()
        {

        }
        private void OnMouseUp()
        {
            if (GameManager.Instance.cursor == this)
            {
                if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Minion)
                {//We have source and target
                    GameManager.Instance.OnUIPlayMinion(this);
                }
                else
                {//Reset
                    transform.localPosition = new Vector3(0, 0, -1);
                    GameManager.Instance.cursor = null;
                }
            }
        }
        public void OnMouseDown()
        {
            Debug.Log("Click Detected!");
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<HandSlot>() == null) return;//Without visuals of failure
            if (!GameManager.Instance.IsCardPlayable(this)) return;//Possibly with visual indication
            GameManager.Instance.cursor = this;
            Debug.Log("Attached to cursor");
        }
        public void OnMouseEnter()
        {
            if (Hidden || GameManager.Instance.cursor != null) return;
            transform.localScale = standardScale * 1.2f;
            Debug.Log("Mice entered");
            //TODO better highlight for reading
        }
        public void OnMouseExit()
        {
            transform.localScale = standardScale;//we cleanup regardless just in case
        }
    }
}