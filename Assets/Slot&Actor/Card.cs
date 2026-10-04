using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            public TurnEventArgs(bool onTurn) => OnTurn = onTurn;
            public bool OnTurn { get; private set; }
            public GameManager.P Player => OnTurn ? GameManager.P.P1 : GameManager.P.P2;
        }
        public event EventHandler<TurnEventArgs> OnStartTurn;
        public event EventHandler<TurnEventArgs> OnEndTurn;
        public string expansion;
        public HashSet<CardTag> cardTags = new();
        public bool HasTag(CardTag tag) => cardTags.Contains(tag);
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
        public int ID { get; private set; }
        public string cardname;
        public CardType cardType;
        public bool IsExperiment { get; private set; }
        Image sr;
        Outline playableOutline;
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
        Minion battlecryPreview;
        public bool IsChoosingBattlecryTarget { get; private set; }
        public Vector3 TargetingArrowOrigin => battlecryPreview != null
            ? battlecryPreview.transform.position
            : transform.position;

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
            playableOutline = gameObject.AddComponent<Outline>();
            playableOutline.effectColor = Color.green;
            playableOutline.effectDistance = new Vector2(0.25f, 0.25f);
            playableOutline.useGraphicAlpha = true;
            playableOutline.enabled = false;
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
            UpdatePlayableOutline();
            if (GameManager.Instance.cursor != this) return;
            if (!SafeZone.InSafeZone && !Targetted && cardType==CardType.Spell)
            {
                sr.color = new(0.5f, 1, 0.5f, 1);
            }
            else sr.color = Color.white;
        }
        private void UpdatePlayableOutline()
        {
            if (playableOutline == null || GameManager.Instance == null) return;

            bool inOwnedHand = !Hidden
                && transform.parent != null
                && transform.parent.TryGetComponent(out HandSlot handSlot)
                && handSlot.Owner == GameManager.P.P1;

            playableOutline.enabled = inOwnedHand
                && GameManager.Instance.OnTurn
                && GameManager.Instance.IsCardPlayable(this, GameManager.P.P1);
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
        public Card Initialize(CardData.CardData data, int id = -1)
        {
            ID = id;
            mana = data.cost;
            cardname = data.name;
            cardTags = new HashSet<CardTag>(data.tags);
            switch (data.type)
            {
                case "Token":
                case "Jednotka":
                    cardType = CardType.Minion;
                    TableActorPrefab = GetLoadedSharedAsset(minionPrefabHandle, MinionAddressable);
                    stats = new int[2] { int.Parse(data.attack), int.Parse(data.health) };
                    break;
                case "Spelltoken":
                case "Spell":
                    cardType = CardType.Spell;
                    break;
                case "Experiment":
                    // Experiments use every ordinary spell rule/event, but resolve by
                    // leaving a hidden Effect instead of executing an immediate script.
                    cardType = CardType.Spell;
                    IsExperiment = true;
                    break;
                case "Pole":
                    cardType = CardType.Field;
                    TableActorPrefab = GetLoadedSharedAsset(fieldPrefabHandle, FieldAddressable);
                    break;
                default: throw new Exception("Unknown cardtype: " + data.type);
            }
            EffectPrefab = GetLoadedSharedAsset(effectPrefabHandle, EffectAddressable);


            expansion = "Tokeny";
            if (CDJsonUtils.expansionMapping.ContainsKey(data.expansion)) expansion = CDJsonUtils.expansionMapping[data.expansion];
            CardArt.Load(CardArt.FaceAddress(expansion, cardname), sprite =>
            {
                if (this == null || sprite == null) return;
                face = sprite;
                if (!Hidden) sr.sprite = face;
            });
            CardArt.Load("card-face/card-back", sprite =>
            {
                if (this == null || sprite == null) return;
                cardBack = sprite;
                if (Hidden) sr.sprite = cardBack;
            });

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

        public static IEnumerator PreloadSharedAssets(Card prefab)
        {
            yield return LoadSharedAsset(prefab.MinionAddressable, handle => minionPrefabHandle = handle);
            yield return LoadSharedAsset(prefab.FieldAddressable, handle => fieldPrefabHandle = handle);
            yield return LoadSharedAsset(prefab.EffectAddressable, handle => effectPrefabHandle = handle);
        }

        private static IEnumerator LoadSharedAsset(
            AssetReferenceGameObject reference,
            Action<AsyncOperationHandle<GameObject>> setHandle)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(reference.RuntimeKey);
            setHandle(handle);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"Could not load Addressable prefab: {reference.RuntimeKey}");
        }

        private static GameObject GetLoadedSharedAsset(
            AsyncOperationHandle<GameObject> handle,
            AssetReferenceGameObject reference)
        {
            if (!handle.IsValid() || !handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException($"Addressable prefab was used before it finished loading: {reference.RuntimeKey}");

            return handle.Result;
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
            if (actor == null || actor == battlecryPreview) return false;
            return TargetValidator(actor);
        }

        public void OnMouseDown()
        {
            if (GameManager.Instance.cursor is Card targetingCard && targetingCard.IsChoosingBattlecryTarget) return;
            if (!GameManager.Instance.OnTurn || transform.parent.GetComponent<HandSlot>() == null) return;//Without visuals of failure
            if (!GameManager.Instance.IsCardPlayable(this,GameManager.P.P1)) return;//Possibly with visual indication
            GameManager.Instance.cursor = this;
            GetComponent<AudioSource>().Play();
        }

        private void BeginBattlecryTargeting()
        {
            battlecryCardIndex = SlotIndex;
            battlecrySlotIndex = GameManager.Instance.HighlightedSlotIndex;
            CardSlot targetSlot = GameManager.Instance.highlightedSlot.GetComponent<CardSlot>();
            IsChoosingBattlecryTarget = true;
            GameManager.Instance.SetMinionTargeting(true);
            GameManager.Instance.highlightedSlot = null;
            transform.localPosition = Vector3.zero;
            battlecryPreview = CreateMinionVisual(targetSlot, playSound: false);
        }

        public void ChooseBattlecryTarget(TableActor target)
        {
            if (!IsChoosingBattlecryTarget || !IsTargetValid(target)) return;

            GameManager.Instance.highlightedActor = target;
            int targetIndex = GameManager.Instance.HighlightedActorIndex;
            if (targetIndex < 0) return;

            IsChoosingBattlecryTarget = false;
            GameManager.Instance.SetMinionTargeting(false);
            RemoveBattlecryPreview();
            GameManager.Instance.cursor = null;
            GameManager.Instance.highlightedActor = null;
            GameManager.Instance.OnUIPlayMinion(battlecryCardIndex, battlecrySlotIndex, targetIndex);
        }

        internal void CancelBattlecryTargeting()
        {
            IsChoosingBattlecryTarget = false;
            GameManager.Instance.SetMinionTargeting(false);
            RemoveBattlecryPreview();
            GameManager.Instance.cursor = null;
            GameManager.Instance.ClearHighlights();
            transform.localPosition = Vector3.zero;
        }

        private void RemoveBattlecryPreview()
        {
            if (battlecryPreview == null) return;

            battlecryPreview.transform.SetParent(null);
            Destroy(battlecryPreview.gameObject);
            battlecryPreview = null;
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
            Minion m = CreateMinionVisual(slot, playSound: true);
            foreach (Type script in scriptTypes)
            {
                m.gameObject.AddComponent(script);
            }
            m.Summoned(target);//Selfsummon
            GameManager.Instance.InvokeSummoned(m);//TODO: probably do the InvokeSummoned on one place
            return m;
        }

        internal Minion SummonMinion(CardSlot slot)
        {
            Minion minion = CreateMinionVisual(slot, playSound: true);
            foreach (Type script in scriptTypes)
            {
                minion.gameObject.AddComponent(script);
            }
            minion.Summoned(null);
            GameManager.Instance.InvokeSummoned(minion);
            return minion;
        }

        private Minion CreateMinionVisual(CardSlot slot, bool playSound)
        {
            GameObject g = Instantiate(TableActorPrefab, slot.transform);
            g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(-90, new(0, 0, 1)));
            Minion m = g.GetComponent<Minion>();
            m.Initialize(this);
            if (playSound) m.audioSource.PlayOneShot(cardPlaced);//Cannot do it on card cuz that one gets disabled and cannot do sounds thus
            return m;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">null if no target given</param>
        internal void CastSpell(GameActor target)
        {
            if (IsExperiment)
            {
                CardSlot slot = GameManager.Instance.GetFreeEffectSlot(Owner);
                if (slot == null) throw new InvalidOperationException("An Experiment was cast without a free Effect slot.");
                Effect effect = PlaceEffect(slot);
                effect.isExperiment = true;
            }
            else OnSelfPlayed?.Invoke(this, new(CardType.Spell, target));
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
            GameManager.Instance.RefreshHandVisibility();
            return f;
        }
    }
}
