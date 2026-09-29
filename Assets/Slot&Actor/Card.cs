using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;

namespace CardGame
{
    [RequireComponent(typeof(AudioSource))]
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
        public string cardTag;
        public List<Func<Card, int>> manacostmod = new();
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
[RequireComponent(typeof(AudioSource))]



    [RequireComponent(typeof(Image))]
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
        GameObject manaBadge;
        TextMeshProUGUI manaCostLabel;
        GameObject previewManaBadge;
        TextMeshProUGUI previewManaCostLabel;
        bool previewHighlighted;
        Sprite face;
        public Sprite cardBack {  get; private set; }
        bool hidden;
        public Vector3 standardScale;
        public int[] stats;
        readonly List<Type> scriptTypes = new();
        int SlotIndex { get => GetComponentInParent<CardSlot>().index; }
        int battlecrySlotIndex;
        int battlecryCardIndex;
        public bool IsChoosingBattlecryTarget { get; private set; }

        [SerializeField] AssetReferenceGameObject MinionAddressable;
        [SerializeField] AssetReferenceGameObject FieldAddressable;
        [SerializeField] AssetReferenceGameObject EffectAddressable;
        GameObject TableActorPrefab;
        GameObject EffectPrefab;//A minion might cause an effect to happen so it is better to have it separate than all in TableActorPrefab
        private static AsyncOperationHandle<GameObject> minionPrefabHandle;
        private static AsyncOperationHandle<GameObject> fieldPrefabHandle;
        private static AsyncOperationHandle<GameObject> effectPrefabHandle;

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
        public Func<TableActor,bool> TargetValidator = (_) => true;//TODO: make this a list of functions

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
            UpdateManaBadge();
            if (GameManager.Instance.cursor != this) return;
            if (!SafeZone.InSafeZone && !Targetted && cardType==CardType.Spell)
            {
                sr.color = new(0.5f, 1, 0.5f, 1);
            }
            else sr.color = Color.white;
        }
        private void UpdateManaBadge()
        {
            bool showMana = !Hidden
                && transform.parent != null
                && transform.parent.TryGetComponent(out HandSlot _)
                && GameManager.Instance != null;

            if (!showMana)
            {
                if (manaBadge != null) manaBadge.SetActive(false);
                return;
            }

            EnsureManaBadge();
            if (manaBadge == null) return;

            manaBadge.SetActive(true);
            int currentManaCost = GameManager.Instance.GetManaCost(this);
            UpdateManaCostLabel(manaCostLabel, currentManaCost);
            if (previewHighlighted)
            {
                EnsurePreviewManaBadge();
                if (previewManaBadge != null)
                {
                    previewManaBadge.SetActive(true);
                    UpdateManaCostLabel(previewManaCostLabel, currentManaCost);
                }
            }
        }
        private void EnsureManaBadge()
        {
            if (manaBadge != null) return;

            Sprite manaIcon = Resources.Load<Sprite>("Grafika/Mana-icon");
            if (manaIcon == null)
            {
                Debug.LogError("Could not load Resources/Grafika/Mana-icon.png.");
                return;
            }

            manaBadge = new GameObject("ManaCost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
            {
                layer = gameObject.layer
            };
            manaBadge.transform.SetParent(transform, false);

            RectTransform badgeRect = manaBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.one;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-1.35f, -1.5f);
            badgeRect.sizeDelta = new Vector2(3f, 3f);

            Image badgeImage = manaBadge.GetComponent<Image>();
            badgeImage.sprite = manaIcon;
            badgeImage.preserveAspect = true;
            badgeImage.raycastTarget = false;

            GameObject labelObject = new("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI))
            {
                layer = gameObject.layer
            };
            labelObject.transform.SetParent(manaBadge.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            manaCostLabel = labelObject.GetComponent<TextMeshProUGUI>();
            manaCostLabel.alignment = TextAlignmentOptions.Center;
            manaCostLabel.color = Color.white;
            manaCostLabel.enableAutoSizing = true;
            manaCostLabel.fontSizeMin = 0.75f;
            manaCostLabel.fontSizeMax = 2.2f;
            manaCostLabel.fontStyle = FontStyles.Bold;
            manaCostLabel.raycastTarget = false;
        }
        private void EnsurePreviewManaBadge()
        {
            if (previewManaBadge != null) return;

            Transform existingBadge = GameManager.Instance.CardHighlighter.transform.Find("ManaCost");
            if (existingBadge != null)
            {
                previewManaBadge = existingBadge.gameObject;
                previewManaCostLabel = existingBadge.GetComponentInChildren<TextMeshProUGUI>();
                return;
            }

            Sprite manaIcon = Resources.Load<Sprite>("Grafika/Mana-icon");
            if (manaIcon == null) return;

            previewManaBadge = new GameObject("ManaCost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
            {
                layer = GameManager.Instance.CardHighlighter.gameObject.layer
            };
            previewManaBadge.transform.SetParent(GameManager.Instance.CardHighlighter.transform, false);

            RectTransform badgeRect = previewManaBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.one;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-6.5f, -7.5f);
            badgeRect.sizeDelta = new Vector2(14f, 14f);

            Image badgeImage = previewManaBadge.GetComponent<Image>();
            badgeImage.sprite = manaIcon;
            badgeImage.preserveAspect = true;
            badgeImage.raycastTarget = false;

            GameObject labelObject = new("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI))
            {
                layer = previewManaBadge.layer
            };
            labelObject.transform.SetParent(previewManaBadge.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            previewManaCostLabel = labelObject.GetComponent<TextMeshProUGUI>();
            previewManaCostLabel.alignment = TextAlignmentOptions.Center;
            previewManaCostLabel.enableAutoSizing = true;
            previewManaCostLabel.fontSizeMin = 3f;
            previewManaCostLabel.fontSizeMax = 10f;
            previewManaCostLabel.fontStyle = FontStyles.Bold;
            previewManaCostLabel.raycastTarget = false;
        }
        private void UpdateManaCostLabel(TextMeshProUGUI label, int currentManaCost)
        {
            label.text = currentManaCost.ToString();
            label.color = currentManaCost < mana ? Color.green : currentManaCost > mana ? Color.red : Color.white;
        }
        public Card Initialize(CardData.CardData data)
        {
            mana = data.cost;
            cardname = data.name;
            cardTag = data.tag;
            switch (data.type)
            {
                case "Token":
                case "Jednotka":
                    cardType = CardType.Minion;
                    TableActorPrefab = LoadSharedAsset(ref minionPrefabHandle, MinionAddressable);
                    stats = new int[2] { int.Parse(data.attack), int.Parse(data.health) };
                    break;
                case "Spelltoken":
                case "Spell":
                    cardType = CardType.Spell;
                    break;
                case "Pole":
                    cardType = CardType.Field;
                    TableActorPrefab = LoadSharedAsset(ref fieldPrefabHandle, FieldAddressable);
                    break;
                default: throw new Exception("Unknown cardtype: " + data.type);
            }
            EffectPrefab = LoadSharedAsset(ref effectPrefabHandle, EffectAddressable);


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

        private static GameObject LoadSharedAsset(
            ref AsyncOperationHandle<GameObject> handle,
            AssetReferenceGameObject reference)
        {
            if (!handle.IsValid())
                handle = Addressables.LoadAssetAsync<GameObject>(reference.RuntimeKey);

            return handle.WaitForCompletion();
        }

        public static void ReleaseSharedAssets()
        {
            ReleaseSharedAsset(ref minionPrefabHandle);
            ReleaseSharedAsset(ref fieldPrefabHandle);
            ReleaseSharedAsset(ref effectPrefabHandle);
        }

        private static void ReleaseSharedAsset(ref AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid()) Addressables.Release(handle);
            handle = default;
        }

        public void OnDiscard() => OnDiscardEvent?.Invoke(this, new());

        /// <summary>
        /// Plays this card without paying mana, choosing a random slot and target
        /// where applicable. Discards the card when it cannot be played.
        /// </summary>
        public void PlayRandomly() => GameManager.Instance.PlayRandomly(this);

        private void OnMouseUp()
        {

            if (GameManager.Instance.cursor != this) return;// I am not holding this card.
            if (IsChoosingBattlecryTarget) return;
            sr.color = Color.white;
            if (SafeZone.InSafeZone)
            {
                GameManager.Instance.cursor = null;
                GameManager.Instance.ClearHighlights();
                transform.localPosition = Vector3.zero;
                return;
            }
            if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Minion)//We have a targetSlot for placing things
            {

                if (!Targetted) GameManager.Instance.OnUIPlayMinion(SlotIndex, GameManager.Instance.HighlightedSlotIndex);
                else
                {
                    BeginBattlecryTargeting();
                    return;
                }
            }
            else if (GameManager.Instance.highlightedSlot != null && cardType == CardType.Field) GameManager.Instance.OnUIPlayField(SlotIndex);//We have a slot to place field to
            else if (cardType == CardType.Spell && !Targetted) GameManager.Instance.OnUICastSpell(SlotIndex);//Its not a targetted spell
            else if (cardType == CardType.Spell && GameManager.Instance.highlightedActor != null) GameManager.Instance.OnUICastSpell(SlotIndex, GameManager.Instance.HighlightedActorIndex);//Its targeted and has a target
            else transform.localPosition = Vector3.zero;//Reset
            GameManager.Instance.cursor = null;//Clean cursor
            GameManager.Instance.ClearHighlights();
        }

        public bool IsTargetValid(TableActor actor)
        {
            if (actor == null) return false;
            return TargetValidator(actor);
        }

        public void OnMouseDown()
        {
            if (IsChoosingBattlecryTarget)
            {
                CancelBattlecryTargeting();
                return;
            }
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<HandSlot>() == null) return;//Without visuals of failure
            if (!GameManager.Instance.IsCardPlayable(this,GameManager.P.P1)) return;//Possibly with visual indication
            GameManager.Instance.cursor = this;
            GetComponent<AudioSource>().Play();
        }

        private void BeginBattlecryTargeting()
        {
            battlecryCardIndex = SlotIndex;
            battlecrySlotIndex = GameManager.Instance.HighlightedSlotIndex;
            IsChoosingBattlecryTarget = true;
            GameManager.Instance.highlightedSlot = null;

            Vector3 targetPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.9f, 0.5f));
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }

        public void ChooseBattlecryTarget(TableActor target)
        {
            if (!IsChoosingBattlecryTarget || !IsTargetValid(target)) return;

            GameManager.Instance.highlightedActor = target;
            int targetIndex = GameManager.Instance.HighlightedActorIndex;
            if (targetIndex < 0) return;

            IsChoosingBattlecryTarget = false;
            GameManager.Instance.cursor = null;
            GameManager.Instance.highlightedActor = null;
            GameManager.Instance.OnUIPlayMinion(battlecryCardIndex, battlecrySlotIndex, targetIndex);
        }

        private void CancelBattlecryTargeting()
        {
            IsChoosingBattlecryTarget = false;
            GameManager.Instance.cursor = null;
            GameManager.Instance.ClearHighlights();
            transform.localPosition = Vector3.zero;
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
            previewHighlighted = !Hidden
                && transform.parent != null
                && transform.parent.TryGetComponent(out HandSlot _);
            EnsurePreviewManaBadge();
            if (previewManaBadge != null)
            {
                previewManaBadge.SetActive(previewHighlighted);
                if (previewHighlighted)
                    UpdateManaCostLabel(previewManaCostLabel, GameManager.Instance.GetManaCost(this));
            }
        }
        public void OnMouseExit()
        {
            transform.localScale = standardScale;//we cleanup regardless just in case
            DeHighlightCard();
        }
        public void DeHighlightCard()
        {
            previewHighlighted = false;
            if (previewManaBadge != null) previewManaBadge.SetActive(false);
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
            ;
            m.Summoned(target);//Selfsummon
            GameManager.Instance.InvokeSummoned(m);//TODO: probably do the InvokeSummoned on one place
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
