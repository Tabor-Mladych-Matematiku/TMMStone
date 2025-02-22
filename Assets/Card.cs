using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardGame.Card;
using static UnityEngine.GraphicsBuffer;

namespace CardGame
{
    public abstract class GameActor : MonoBehaviour
    {
        public event EventHandler<TurnEventArgs> OnStartTurn;
        public event EventHandler<TurnEventArgs> OnEndTurn;
        public string expansion;
        public virtual void StartTurn(bool onTurn)
        {
            OnStartTurn?.Invoke(this, new(onTurn));
        }
        public virtual void EndTurn(bool onTurn)
        {
            OnEndTurn?.Invoke(this, new(onTurn));
        }
    }


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
    public class Card : GameActor
    {
        public enum CardType
        {
            Minion,
            Spell,
            Field
        }
        public int mana;
        public string cardname;
        public CardType cardType;
        Sprite face;
        Sprite cardBack;
        bool hidden;
        public Vector3 standardScale;
        public int[] stats;

        [SerializeField] GameObject MinionPrefab;
        [SerializeField] GameObject FieldPrefab;
        [SerializeField] GameObject EffectPrefab;
        bool Targetted;
        public readonly static Dictionary<string, string> expansionMapping = new() {//Maps JSON name to folder name
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

        public event EventHandler OnDiscardEvent;
        public class TurnEventArgs
        {
            public TurnEventArgs(bool onTurn)
            {
                this.onTurn = onTurn;
            }
            public bool onTurn;
        }

        public event EventHandler<CardPlayedEventArgs> OnPlayed;
        public class CardPlayedEventArgs
        {
            public int Target;
        }

        private void Awake()
        {
            face = GetComponent<SpriteRenderer>().sprite;
            standardScale = transform.localScale;
            Targetted = false;//TODO: load from cardScripts.
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
                    stats = new int[2] { int.Parse(data.attack), int.Parse(data.health) };
                    break;
                case "Spelltoken":
                case "Spell":
                    cardType = CardType.Spell;
                    break;
                case "Pole":
                    cardType = CardType.Field;
                    break;
                default: throw new Exception("Unknown cardtype: " + data.type);
            }
            expansion = "Tokeny";
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
            OnDiscardEvent?.Invoke(this, new());
        }
        private void OnMouseUp()
        {
            if (GameManager.Instance.cursor == this)
            {
                if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Minion)//TODO figure out targetted battlecries
                {//We have source and target
                    if (!Targetted)
                        GameManager.Instance.OnUIPlayMinion(this, GameManager.Instance.HighlightedSlotIndex);
                    else
                    {
                        //HEre we need to make the secondary targetting system. (MAMA mia!)
                        throw new NotImplementedException("We did not make minions with targetted battlecries yet");
                    }
                }
                else if (cardType == CardType.Spell && !Targetted)
                {
                    GameManager.Instance.OnUICastSpell(this);
                }
                else if (cardType == CardType.Spell)
                {
                    throw new NotImplementedException("check valid targetting. Either a func here or let ONUICastSpell return bool.");
                    GameManager.Instance.OnUICastSpell(this);
                }
                else if (cardType == CardType.Field)
                {
                    GameManager.Instance.OnUIPlayField(this);
                }
                else
                {//Reset
                    transform.localPosition = new Vector3(0, 0, -1);
                }
                GameManager.Instance.cursor = null;
            }
        }
        public void OnMouseDown()
        {
            //Debug.Log("Click Detected!");
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<HandSlot>() == null) return;//Without visuals of failure
            if (!GameManager.Instance.IsCardPlayable(this)) return;//Possibly with visual indication
            GameManager.Instance.cursor = this;
            //Debug.Log("Attached to cursor");
        }
        public void OnMouseEnter()
        {
            if (Hidden || GameManager.Instance.cursor != null) return;
            transform.localScale = standardScale * 1.2f;
            //Debug.Log("Mice entered");
            //TODO better highlight for reading
        }
        public void OnMouseExit()
        {
            transform.localScale = standardScale;//we cleanup regardless just in case
        }

        internal void PlayMinion(CardSlot slot, int target)
        {
            OnPlayed?.Invoke(this, new() { Target = target });
            GameObject g = Instantiate(MinionPrefab, slot.transform);
            g.transform.SetLocalPositionAndRotation(new(0, 0, -1.1f), Quaternion.AngleAxis(-90, new(0, 0, 1)));
            Minion m = g.GetComponent<Minion>();
            m.Battlecry(target);//(Battlecries get procked after occupying the slot but before getting affected) therefore probably before his INIT
            m.Initialize(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">-1 if no target given</param>
        internal void CastSpell(int target)
        {
            OnPlayed?.Invoke(this, new() { Target = target });
        }

        internal void PlayField()
        {
            OnPlayed?.Invoke(this, new());
            GameObject g = Instantiate(FieldPrefab, GameManager.Instance.FieldSlot.transform);
            g.transform.SetLocalPositionAndRotation(new(0, 0, -1.1f), Quaternion.AngleAxis(-90, new(0, 0, 1)));
            Field f = g.GetComponent<Field>();
            f.Initialize(this);
        }
    }
}