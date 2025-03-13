using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace CardGame
{
    public abstract class GameActor : MonoBehaviour
    {
        public class TurnEventArgs
        {
            public TurnEventArgs(bool onTurn) => this.OnTurn = onTurn;
            public bool OnTurn { get; private set; }
        }
        public event EventHandler<TurnEventArgs> OnStartTurn;
        public event EventHandler<TurnEventArgs> OnEndTurn;
        public string expansion;
        public AudioSource audioSource;
        public GameManager.P backupOwner;//ugly as heck TODO proly make this better somehow
        public virtual GameManager.P Owner
        {
            get
            {
                CardSlot slot = GetComponentInParent<CardSlot>();
                return slot == null ? backupOwner : slot.Owner;
            }
        }
        public virtual void StartTurn(bool onTurn) => OnStartTurn?.Invoke(this, new(onTurn));
        public virtual void EndTurn(bool onTurn) => OnEndTurn?.Invoke(this, new(onTurn));

        internal virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }
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
        Image sr;
        Sprite face;
        Sprite cardBack;
        bool hidden;
        public Vector3 standardScale;
        public int[] stats;
        readonly List<Type> scriptTypes = new();
        int SlotIndex { get => GetComponentInParent<CardSlot>().index; }

        [SerializeField] AssetReferenceGameObject MinionAddressable;
        [SerializeField] AssetReferenceGameObject FieldAddressable;
        [SerializeField] AssetReferenceGameObject EffectAddressable;
        GameObject TableActorPrefab;
        GameObject EffectPrefab;//A minion might cause an effect to happen so it is better to have it separate than all in TableActorPrefab

        public AudioClip cardPlaced;
        public bool Targetted { get; private set; } = false;
        public bool Hidden
        {
            get => hidden; set
            {
                hidden = value;
                sr.sprite = (hidden) ? cardBack : face;
            }
        }

        public event EventHandler OnDiscardEvent;
        public event EventHandler<CardPlayedEventArgs> OnSelfPlayed;
        public class CardPlayedEventArgs
        {
            public CardPlayedEventArgs(CardType type, GameActor target) { Target = target; cardType = type; }
            public GameActor Target { get; private set; }
            public CardType cardType { get; private set; }
        }
        /// <summary>
        /// Scripts reasign this to modify what is valid target
        /// </summary>
        public Func<bool> TargetValidator = () => true;//TODO: make this a list of functions

        internal override void Awake()
        {
            base.Awake();
            sr = GetComponent<Image>();
            face = sr.sprite;//placeholder
            standardScale = transform.localScale;
        }
        // Start is called before the first frame update
        private void Start()
        {
            standardScale = transform.localScale;
        }
        private void Update()
        {
            if (GameManager.Instance.cursor != this) return;
            if (!SafeZone.InSafeZone && !Targetted && cardType==CardType.Spell)
            {
                sr.color = new(0.5f, 1, 0.5f, 1);
            }
            else sr.color = Color.white;
        }
        public Card Initialize(CardData.CardData data)
        {
            mana = data.cost;
            cardname = data.name;
            switch (data.type)
            {
                case "Token":
                case "Jednotka":
                    cardType = CardType.Minion;
                    MinionAddressable.LoadAssetAsync().Completed += (handle) => TableActorPrefab = handle.Result;
                    stats = new int[2] { int.Parse(data.attack), int.Parse(data.health) };
                    break;
                case "Spelltoken":
                case "Spell":
                    cardType = CardType.Spell;
                    break;
                case "Pole":
                    cardType = CardType.Field;
                    FieldAddressable.LoadAssetAsync().Completed += (handle) => TableActorPrefab = handle.Result;
                    break;
                default: throw new Exception("Unknown cardtype: " + data.type);
            }
            EffectAddressable.LoadAssetAsync().Completed += (handle) => EffectPrefab = handle.Result;


            expansion = "Tokeny";
            if (CDJsonUtils.expansionMapping.ContainsKey(data.expansion)) expansion = CDJsonUtils.expansionMapping[data.expansion];
            Sprite sprite = Resources.Load<Sprite>("CardData/" + expansion + "/" + cardname);
            if (sprite != null)
            {
                face = sprite;
                sr.sprite = face;
            }
            cardBack = Resources.Load<Sprite>("CardData/card-back");

            if (data.scripts != null)
            {
                Assembly asm = Assembly.Load("CardScripts"); 
                foreach (var script in data.names)
                {
                    Type cardScript = asm.GetType(script);
                    scriptTypes.Add(cardScript);
                    Targetted = cardScript.IsSubclassOf(asm.GetType("TargetableCardScriptBase"));//This could be probably done through the CardScriptBase, but this'll do
                    //Debug.Log("Type: " + cardScript);
                    //if(Targetted) Debug.Log("Targetted");
                    gameObject.AddComponent(cardScript);

                    //((CardScriptBase)cmp).Initialize(script); Figure out how to pass args (Eh. Wont work. CardScriptBase is in a different assembly which we cannot reference since it references us.)
                }
            }
            return this;
        }

        public void OnDiscard() => OnDiscardEvent?.Invoke(this, new());
        private void OnMouseUp()
        {

            if (GameManager.Instance.cursor != this) return;// I am not holding this card.
            sr.color = Color.white;
            if (SafeZone.InSafeZone)
            {
                GameManager.Instance.cursor = null;
                transform.localPosition = Vector3.zero;
                return;
            }
            if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Minion)//We have a targetSlot for placing things
            {

                if (!Targetted) GameManager.Instance.OnUIPlayMinion(SlotIndex, GameManager.Instance.HighlightedSlotIndex);
                else
                {
                    //HEre we need to make the secondary targetting system. (MAMA mia!)
                    int target = 0;
                    GameManager.Instance.OnUIPlayMinion(SlotIndex, GameManager.Instance.HighlightedSlotIndex, target);
                    throw new NotImplementedException("We did not make minions with targetted battlecries yet");//TODO figure out targetted battlecries
                }
            }
            else if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Field) GameManager.Instance.OnUIPlayField(SlotIndex);//We have a slot to place field to
            else if (cardType == CardType.Spell && !Targetted) GameManager.Instance.OnUICastSpell(SlotIndex);//Its not a targetted spell
            else if (cardType == CardType.Spell && GameManager.Instance.highlightedActor != null) GameManager.Instance.OnUICastSpell(SlotIndex, GameManager.Instance.HighlightedActorIndex);//Its targeted and has a target
            else transform.localPosition = Vector3.zero;//Reset
            GameManager.Instance.cursor = null;//Clean cursor
        }

        public bool IsTargetValid(TableActor actor)
        {
            if (actor == null) return false;
            return TargetValidator();
        }

        public void OnMouseDown()
        {
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<HandSlot>() == null) return;//Without visuals of failure
            if (!GameManager.Instance.IsCardPlayable(this)) return;//Possibly with visual indication
            GameManager.Instance.cursor = this;
            GetComponent<AudioSource>().Play();
        }
        public void OnMouseEnter()
        {
            if (Hidden || GameManager.Instance.cursor != null) return;
            transform.localScale = standardScale * 1.2f;
            HighlightCard();
        }
        public void HighlightCard()
        {
            GameManager.Instance.CardHighlighter.texture = face.texture;
            GameManager.Instance.CardHighlighter.gameObject.SetActive(true);
        }
        public void OnMouseExit()
        {
            transform.localScale = standardScale;//we cleanup regardless just in case
            DeHighlightCard();
        }
        public void DeHighlightCard()
        {
            GameManager.Instance.CardHighlighter.gameObject.SetActive(false);
        }

        internal Minion PlayMinion(CardSlot slot, GameActor target)
        {
            OnSelfPlayed?.Invoke(this, new(CardType.Minion, target));
            GameObject g = Instantiate(TableActorPrefab, slot.transform);
            g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(-90, new(0, 0, 1)));
            Minion m = g.GetComponent<Minion>();
            m.Initialize(this);
            m.audioSource.PlayOneShot(cardPlaced);//Cannot do it on card cuz that one gets disabled and cannot do sounds thus
            foreach (Type script in scriptTypes)
            {
                m.gameObject.AddComponent(script);
            }
            m.Summoned(target);
            return m;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">null if no target given</param>
        internal void CastSpell(GameActor target)
        {
            OnSelfPlayed?.Invoke(this, new(CardType.Spell, target));
            audioSource.PlayOneShot(cardPlaced);
        }
        public Effect PlaceEffect(CardSlot slot)
        {
            GameObject g = Instantiate(EffectPrefab, slot.transform);
            g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(0, new(0, 0, 1)));
            Effect e = g.GetComponent<Effect>();
            e.Initialize(this);
            foreach (Type script in scriptTypes)
            {
                e.gameObject.AddComponent(script);
            }
            return e;
        }

        internal Field PlayField()
        {
            OnSelfPlayed?.Invoke(this, new(CardType.Field, null));
            GameObject g = Instantiate(TableActorPrefab, GameManager.Instance.FieldSlot.transform);
            g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(-90, new(0, 0, 1)));
            Field f = g.GetComponent<Field>();
            f.Initialize(this);
            f.audioSource.PlayOneShot(cardPlaced);
            foreach (Type script in scriptTypes)
            {
                f.gameObject.AddComponent(script);
            }
            return f;
        }
    }
}