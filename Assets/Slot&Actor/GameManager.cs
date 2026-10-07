using CardData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CardGame.Card;

namespace CardGame
{

    public static class Extensions
    {
        /// <summary>
        /// Shuffles randomly and returns permutation of elements
        /// </summary>
        /// <typeparam name="T">IList element type</typeparam>
        /// <param name="list">this</param>
        /// <returns></returns>
        public static int[] Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            int[] permutation = new int[n];
            while (n > 1)
            {
                n--;
                int k = permutation[n] = UnityEngine.Random.Range(0, n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
            return permutation;
        }
        /// <summary>
        /// Uses a permutation to shuffle elements
        /// </summary>
        /// <typeparam name="T">IList element type</typeparam>
        /// <param name="list">this</param>
        /// <param name="permutation">Can be aquired from the parameterless Shuffle</param>
        public static void Shuffle<T>(this IList<T> list, int[] permutation)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = permutation[n];
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static GameManager.P Other(this GameManager.P who)
        {
            if (who == GameManager.P.P1) return GameManager.P.P2;
            return GameManager.P.P1;
        }
    }
    public static class MatchResults
    {
        public static string result;
    }
    public struct PlayerData
    {
        public int hp;
        public int maxhp;
        public int mana;
        public List<Card> hand;

    }

    public class GameManager : NetworkBehaviour
    {
        private static WaitForSecondsRealtime _waitForSecondsRealtime0_5 = new WaitForSecondsRealtime(0.5f);

        /// <summary>
        /// A character target encoded from the acting player's point of view.
        /// The value remains an int on the wire, but this type prevents board-global
        /// slot indices from being accidentally passed where a player-relative target is expected.
        /// </summary>
        public readonly struct CharacterTargetIndex
        {
            public const int NoTargetValue = -1;

            public int Value { get; }
            public bool HasTarget => Value != NoTargetValue;

            private CharacterTargetIndex(int value) => Value = value;

            public static CharacterTargetIndex None => new(NoTargetValue);
            public static CharacterTargetIndex FromPlayerPerspective(int value) => new(value);
            public override string ToString() => HasTarget ? Value.ToString() : "None";
        }

        public struct PlayerAction : INetworkSerializable
        {
            public enum ActionType
            {
                Play,
                Attack
            }
            private int c;//Needed for serialization
            public int Source { readonly get => c; private set => c = value; }
            private ActionType actionType;
            public ActionType Actiontype { readonly get => actionType; private set => actionType = value; }
            private int slot;
            public int Slot { readonly get => slot; private set => slot = value; }
            private int target;
            public int Target { readonly get => target; private set => target = value; }//Serialized as an int for multiplayer.
            private int choiceCount;
            private int choice0;
            private int choice1;
            private int choice2;
            public readonly int[] Choices => choiceCount switch
            {
                1 => new[] { choice0 },
                2 => new[] { choice0, choice1 },
                3 => new[] { choice0, choice1, choice2 },
                _ => Array.Empty<int>()
            };
            public readonly CharacterTargetIndex CharacterTarget => CharacterTargetIndex.FromPlayerPerspective(target);
            public static PlayerAction PlayCardAction(int card, int target, int slot = -1, int[] choices = null)
            {
                PlayerAction action = new()
                {
                    Source = card,
                    Actiontype = ActionType.Play,
                    Slot = slot,
                    Target = target,
                    choiceCount = Math.Min(choices?.Length ?? 0, 3)
                };
                if (action.choiceCount > 0) action.choice0 = choices[0];
                if (action.choiceCount > 1) action.choice1 = choices[1];
                if (action.choiceCount > 2) action.choice2 = choices[2];
                return action;
            }
            public static PlayerAction AttackAction(int minion, int target) => new()
            {
                Source = minion,
                Actiontype = ActionType.Attack,
                Target = target
            };

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref c);
                serializer.SerializeValue(ref actionType);
                serializer.SerializeValue(ref slot);
                serializer.SerializeValue(ref target);
                serializer.SerializeValue(ref choiceCount);
                serializer.SerializeValue(ref choice0);
                serializer.SerializeValue(ref choice1);
                serializer.SerializeValue(ref choice2);
            }
        }
        public enum P
        {
            P1,
            P2
        }
        [Serializable]
        class DeckSave
        {
            public int[] data;
            public DeckSave(int[] data) => this.data = data;

            public override string ToString() => new StringBuilder().AppendJoin(" ", data).ToString();
        }
        public static GameManager Instance { get; private set; }
        [SerializeField] Deck OwnDeck;
        [SerializeField] Deck OppDeck;
        [SerializeField] Grave OwnGrave;
        [SerializeField] Grave OppGrave;
        public Dictionary<P, Deck> decks;
        public Dictionary<P, Grave> graves;
        [SerializeField] Transform OwnMin;
        [SerializeField] Transform OppMin;
        Dictionary<P, CardSlot[]> minionSlots;
        [SerializeField] Transform OwnEff;
        [SerializeField] Transform OppEff;
        Dictionary<P, CardSlot[]> EffSlots;
        [SerializeField] Transform OwnHand;
        [SerializeField] Transform OppHand;
        Dictionary<P, CardSlot[]> HandSlots;
        public FieldSlot FieldSlot;
        [SerializeField] HPCounter OwnHPCounter;
        [SerializeField] HPCounter OppHPCounter;
        [SerializeField] Transform OwnCardCounter;
        [SerializeField] Transform OppCardCounter;
        public ManaCounter OwnManaCounter;
        public ManaCounter OppManaCounter;
        public Dictionary<P, ManaCounter> ManaCounters;
        public Dictionary<P, HPCounter> HPCounters;

        #region AI
        public AIPlayerBase AIPlayer;

        public int GetNextFreeMinionSlot(P player)
        {
            for (int i = 0; i < minionSlots[player].Length; i++)
            {
                CardSlot slot = minionSlots[player][i];
                if (!slot.Occupied) return i;
            }
            return -1;
        }
        /// <summary>
        /// Transfers the existing minion and its card without replaying or resummoning it.
        /// Returns false if it is not on the board or the destination is full; a full
        /// destination removes it without death effects. Call during synchronized resolution.
        /// </summary>
        public bool TakeControl(Minion minion, P newController, bool allowImmediateAttack = false)
        {
            if (minion == null || !minion.Alive()) return false;
            CardSlot source = minion.GetComponentInParent<CardSlot>();
            if (source == null || !minionSlots[minion.Owner].Contains(source)) return false;
            if (minion.Owner == newController) return true;

            CardSlot destination = minionSlots[newController].LastOrDefault(slot => !slot.Occupied);
            ClearHighlights();
            if (cursor == minion) cursor = null;
            if (destination == null)
            {
                minion.RemoveWithoutDeath();
                BoardChanged?.Invoke();
                return false;
            }

            minion.TransferControl(destination, allowImmediateAttack);
            BoardChanged?.Invoke();
            return true;
        }

        public int GetRandomFreeMinionSlot(P player)
        {
            List<int> freeSlots = new();
            for (int i = 0; i < minionSlots[player].Length; i++)
            {
                CardSlot slot = minionSlots[player][i];
                if (!slot.Occupied) freeSlots.Add(i);
            }
            return (freeSlots.Count > 0) ? freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)] : -1;
        }


        public PlayerData GetPlayerData(P player) => new()
        {
            hp = HPCounters[player].Health,
            maxhp = HPCounters[player].maxHP,
            mana = ManaCounters[player].Mana,
            hand = HandSlots[player].Select(s => s.GetCard()).ToList()
        };

        public void OnAIPlayMinion(int cardindex, int minionSlotIndex, CharacterTargetIndex target) => OnAITakeAction(PlayerAction.PlayCardAction(cardindex, target.Value, minionSlotIndex));
        public void OnAIMinionAttack(int minionSlotIndex, int actorSlotTarget) => OnAITakeAction(PlayerAction.AttackAction(minionSlotIndex, actorSlotTarget));
        public void OnAICastSpell(int cardindex, int target = -1) => OnAITakeAction(PlayerAction.PlayCardAction(cardindex, target));
        public void OnAIPlayField(int cardindex) => OnAITakeAction(PlayerAction.PlayCardAction(cardindex, -1));
        public void OnAITakeAction(PlayerAction action)
        {
            TakeAction(P.P2, action);//Play it out
        }
        #endregion AI

        IEnumerable<CardSlot> AllSlots { get => new CardSlot[] { FieldSlot }.Concat(minionSlots[P.P1]).Concat(minionSlots[P.P2]).Concat(EffSlots[P.P1]).Concat(EffSlots[P.P2]).Concat(HandSlots[P.P1]).Concat(HandSlots[P.P2]); }
        IEnumerable<CardSlot> AllTableSlots { get => new CardSlot[] { FieldSlot }.Concat(minionSlots[P.P1]).Concat(minionSlots[P.P2]).Concat(EffSlots[P.P1]).Concat(EffSlots[P.P2]); }
        IEnumerable<GameActor> AllActors { get => from CardSlot s in AllSlots let c = s.GetComponentInChildren<GameActor>() where c != null select c; }//Will fail on null exception if you forget to assign Field slot.
        IEnumerable<TableActor> AllTableActors { get => from CardSlot s in AllTableSlots let c = s.GetComponentInChildren<TableActor>() where c != null select c; }
        public IEnumerable<CardSlot> AllCharacterSlots { get => from CardSlot s in minionSlots[P.P1].Concat(minionSlots[P.P2]).Concat(new CardSlot[] { OwnHPCounter, OppHPCounter }) select s; }
        public IEnumerable<DamageableActor> AllCharacters { get => from CardSlot s in AllCharacterSlots let d = s.GetComponentInChildren<DamageableActor>() where d != null select d; }
        public IEnumerable<Minion> AllMinions { get => from CardSlot s in minionSlots[P.P1].Concat(minionSlots[P.P2]) where s.Occupied let m = s.GetMinion() where m != null select m; }
        public IEnumerable<Effect> AllEffects { get => from CardSlot s in EffSlots[P.P1].Concat(EffSlots[P.P2]) where s.Occupied let e = s.GetComponentInChildren<Effect>() where e!=null select e; }
        public IEnumerable<Card> AllCards {get=> from CardSlot s in HandSlots[P.P1].Concat(HandSlots[P.P2]) where s.Occupied let c = s.GetCard() where c!=null select c; }
        public IEnumerable<Minion> GetAllMinionsOwnedBy(P owner) => from CardSlot s in minionSlots[owner] where s.Occupied let m = s.GetMinion() where m != null select m;
        public IEnumerable<Effect> GetAllEffectsOwnedBy(P owner) => from CardSlot s in EffSlots[owner] where s.Occupied let e = s.GetComponentInChildren<Effect>() where e != null select e;
        public IEnumerable<DamageableActor> GetAllCharactersOwnedBy(P owner) => from CardSlot s in minionSlots[owner].Concat(new[] { HPCounters[owner] }) where s.Occupied let m = s.GetComponentInChildren<DamageableActor>() where m != null select m;
        public IEnumerable<Card> GetCardsInHandOwnedBy(P owner) => from CardSlot s in HandSlots[owner] where s.Occupied let c = s.GetCard() where c != null select c;
        public Dictionary<int, CardData.CardData> CardDatabase;
        public AssetReferenceGameObject CardAddressable;
        GameObject CardPrefab;
        private AsyncOperationHandle<GameObject> cardPrefabHandle;
        private bool prefabsReady;
        public Button EndTurnBtn;
        private GameObject targetingArrow;
        private LineRenderer targetingArrowShaft;
        private LineRenderer targetingArrowHeadLeft;
        private LineRenderer targetingArrowHeadRight;
        private Material targetingArrowMaterial;
        private bool minionTargeting;
        public const int maxMinionSlots = 7;
        public const int maxEffSlots = 6;
        public const int maxHandSlots = 10;
        public const bool DEBUG = true;
        public bool online;
        private bool gameEnding;

        public int seed { get; private set; }

        public event EventHandler<CardPlayedEventArgs> OnPlayed;//TODO: OnPlayed should trigger first. Now, at least with minions, it happens last after all the OnSummon effects
        public event EventHandler OnSummoned;//Whenever minion is summoned
        public event EventHandler<CancelableCardEventArgs> BeforeSpellPlayed;
        public event EventHandler<Minion.TargetedEventEventArgs> BeforeAttackDeclared;
        public event EventHandler<Minion.TargetedEventEventArgs> AfterAttackResolved;
        public event EventHandler MinionDied;
        public event EventHandler MinionPlayedForReactions;
        public event Action BoardChanged;
        public GameStats Stats { get; private set; } = new();

        public sealed class CancelableCardEventArgs : EventArgs
        {
            public CancelableCardEventArgs(Card card) => Card = card;
            public Card Card { get; }
            public bool Cancel { get; set; }
        }

        public IEnumerable<Minion> GetAdjacentMinions(Minion minion)
        {
            var slots = minionSlots[minion.Owner];
            int index = Array.IndexOf(slots, minion.GetComponentInParent<CardSlot>());
            if (index < 0) yield break;
            if (index > 0 && slots[index - 1].GetMinion() is Minion left) yield return left;
            if (index + 1 < slots.Length && slots[index + 1].GetMinion() is Minion right) yield return right;
        }

        public void InvokeSummoned(Minion minion)
        {
            // Self-summon effects see previous summons; global reactions include this one.
            Stats.RecordSummon(minion.Owner, minion.CardID);
            OnSummoned?.Invoke(minion, new());
            BoardChanged?.Invoke();
        }

        public Dictionary<P, int> MaxHealths = new() {
            {P.P1,30 },
            {P.P2,30},
        };

        public Dictionary<P, int> FatigueVals = new() {
            {P.P1,1 },
            {P.P2,1},
        };

        public int TurnCount = 0;//This is not Round counter - tzn this is double of RoundCount

        public NetworkVariable<bool> ServerOnTurn = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private bool? resolvingLocalTurn;
        internal GameActor cursor;
        internal Transform highlightedSlot;
        internal GameActor highlightedActor;

        /// <summary>
        /// Discard cards subscribe to this
        /// </summary>
        public event EventHandler<CardActionEventArgs> CardDiscarded;
        public class CardActionEventArgs
        {
            public CardActionEventArgs(Card c) => card = c;
            public readonly Card card;
        }

        internal int HighlightedSlotIndex
        {
            get
            {
                int slot = -1;
                if (highlightedSlot == null) return slot;
                for (int i = 0; i < maxMinionSlots; i++)
                {
                    if (minionSlots[P.P1][i].transform == highlightedSlot) { slot = i; break; }
                }
                if (slot != -1) return slot;
                for (int i = 0; i < maxMinionSlots; i++)
                {
                    if (minionSlots[P.P2][i].transform == highlightedSlot) { slot = maxMinionSlots + i; break; }
                }
                return slot;
            }
        }
        internal int HighlightedActorIndex
        {
            get
            {
                int slot = -1;
                if (highlightedActor == null) return slot;
                if (highlightedActor is Minion m)
                {
                    for (int i = 0; i < maxMinionSlots; i++)
                    {
                        if (((MinionSlot)minionSlots[P.P1][i]).GetMinion() == m) { slot = i; break; }
                    }
                    if (slot != -1) return slot;
                    for (int i = 0; i < maxMinionSlots; i++)
                    {
                        if (minionSlots[P.P2][i].GetMinion() == m) { slot = maxMinionSlots + i; break; }
                    }
                }
                else if (highlightedActor is Face f)
                {
                    if (f.Owner == P.P1) return 2 * maxMinionSlots;
                    else return 2 * maxMinionSlots + 1;
                }
                return slot;
            }
        }

        public bool OnTurn
        {
            get => resolvingLocalTurn ?? IsServer == ServerOnTurn.Value;
            set
            {
                if (IsServer) ServerOnTurn.Value = value;
                else ServerOnTurn.Value = !value;
            }
        }
        public P PlayerOnTurn { get => OnTurn ? P.P1 : P.P2; }
        public GameObject LoadingScreen;
        public RawImage CardHighlighter;

        private void Awake()
        {
            Instance = this;
            EffSlots = new()
            {
                {P.P1,new EffectSlot[maxEffSlots] },
                {P.P2,new CardSlot[maxEffSlots] }
            };
            minionSlots = new()
            {
                {P.P1,new MinionSlot[maxMinionSlots] },
                {P.P2,new CardSlot[maxMinionSlots] }
            };
            HandSlots = new()
            {
                {P.P1,new HandSlot[maxHandSlots] },
                {P.P2,new CardSlot[maxHandSlots] }
            };

        }
        private IEnumerator Start()
        {
            RefreshDropdown();
            for (int i = 0; i < maxMinionSlots; i++)
            {
                minionSlots[P.P1][i] = OwnMin.GetChild(i).GetComponent<MinionSlot>();
                minionSlots[P.P2][i] = OppMin.GetChild(i).GetComponent<CardSlot>();
                minionSlots[P.P1][i].Initialize(P.P1, i);
                minionSlots[P.P2][i].Initialize(P.P2, maxMinionSlots + i);
            }
            for (int i = 0; i < maxEffSlots; i++)
            {
                EffSlots[P.P1][i] = OwnEff.GetChild(i).GetComponent<EffectSlot>();
                EffSlots[P.P2][i] = OppEff.GetChild(i).GetComponent<CardSlot>();
                EffSlots[P.P1][i].Initialize(P.P1, i);
                EffSlots[P.P2][i].Initialize(P.P2, maxEffSlots + i);
            }
            for (int i = 0; i < maxHandSlots; i++)
            {
                HandSlots[P.P1][i] = OwnHand.GetChild(i).GetComponent<HandSlot>();
                HandSlots[P.P2][i] = OppHand.GetChild(i).GetComponent<CardSlot>();
                HandSlots[P.P1][i].Initialize(P.P1, i);
                HandSlots[P.P2][i].Initialize(P.P2, maxHandSlots + i);
            }
            decks = new()
            {
                {P.P1,OwnDeck },
                {P.P2,OppDeck}
            };
            graves = new()
            {
                {P.P1,OwnGrave },
                {P.P2,OppGrave}
            };
            ManaCounters = new() {
                {P.P1 , OwnManaCounter },
                {P.P2 , OppManaCounter }
            };
            HPCounters = new(){
                {P.P1,OwnHPCounter},
                {P.P2,OppHPCounter}
            };
            HPCounters[P.P1].Initialize(P.P1, 2 * maxMinionSlots);
            HPCounters[P.P2].Initialize(P.P2, 2 * maxMinionSlots + 1);
            HPCounters[P.P1].Death += (_, _) => GameManager_PlayerDeath(P.P1);
            HPCounters[P.P2].Death += (_, _) => GameManager_PlayerDeath(P.P2);
            //Load cards
            cardPrefabHandle = Addressables.LoadAssetAsync<GameObject>(CardAddressable.RuntimeKey);
            CardDatabase = CDJsonUtils.LoadCardDatabase();

            yield return cardPrefabHandle;
            if (cardPrefabHandle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"Could not load the Addressable card prefab: {CardAddressable.RuntimeKey}");

            CardPrefab = cardPrefabHandle.Result;
            yield return Card.PreloadSharedAssets(CardPrefab.GetComponent<Card>());
            yield return CardArt.Preload(GetCardArtAddresses());
            prefabsReady = true;

            //Debug.Log(CardDatabase);
        }

        private IEnumerable<string> GetCardArtAddresses()
        {
            yield return "card-face/card-back";

            foreach (CardData.CardData card in CardDatabase.Values)
            {
                string expansion = CDJsonUtils.expansionMapping.TryGetValue(card.expansion, out string mappedExpansion)
                    ? mappedExpansion
                    : "Tokeny";

                yield return CardArt.FaceAddress(expansion, card.name);
                if (card.type is "Token" or "Jednotka" or "Pole")
                    yield return CardArt.PlainAddress(expansion, card.name);
            }
        }

        private void GameManager_PlayerDeath(P who)
        {
            EndGame(who == P.P1 ? "You lose!" : "You win!");
        }

        private void EndGame(string result)
        {
            if (gameEnding) return;
            gameEnding = true;
            MatchResults.result = result;
            EndTurnBtn.interactable = false;
            ClearHighlights();
            StartCoroutine(FinishGame());
        }

        private IEnumerator FinishGame()
        {
            // Let the action or turn change that caused lethal damage reach the
            // other peer before shutting down the transport.
            if (online) yield return _waitForSecondsRealtime0_5;

            if (NetworkManager != null)
            {
                NetworkManager.OnClientDisconnectCallback -= ClientDisconnected;
                NetworkManager.Shutdown();
            }

            Addressables.LoadSceneAsync("Assets/Scenes/ResultsScene.unity", LoadSceneMode.Single);
        }

        public void ClearHighlights()
        {
            highlightedSlot = null;
            highlightedActor = null;
        }

        private void Update()
        {
            if (cursor is Card targetingCard
                && targetingCard.IsChoosingPlayTarget
                && Input.GetMouseButtonDown(1))
            {
                targetingCard.CancelBeforePlayed();
                HideTargetingArrow();
                return;
            }

            if (cursor == null)
            {
                HideTargetingArrow();
                return;
            }

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (cursor is Minion minion)
            {
                ShowTargetingArrow(minion.transform.position, mousePosition);
                return;
            }

            if (cursor is Card card && card.IsChoosingPlayTarget)
            {
                ShowTargetingArrow(card.TargetingArrowOrigin, mousePosition);
                return;
            }

            HideTargetingArrow();
            cursor.transform.position = new(mousePosition.x, mousePosition.y);
        }

        internal void SetMinionTargeting(bool active)
        {
            minionTargeting = active;
            EndTurnBtn.interactable = !active && OnTurn && !gameEnding;
        }

        private void ShowTargetingArrow(Vector3 start, Vector3 mousePosition)
        {
            EnsureTargetingArrow();

            Vector3 end = new(mousePosition.x, mousePosition.y, start.z);
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                targetingArrow.SetActive(false);
                return;
            }

            targetingArrow.SetActive(true);
            direction /= distance;

            float headLength = Mathf.Clamp(distance * 0.12f, 0.8f, 2.5f);
            Vector3 headLeft = end - Quaternion.Euler(0, 0, 30) * direction * headLength;
            Vector3 headRight = end - Quaternion.Euler(0, 0, -30) * direction * headLength;

            SetArrowLine(targetingArrowShaft, start, end);
            SetArrowLine(targetingArrowHeadLeft, end, headLeft);
            SetArrowLine(targetingArrowHeadRight, end, headRight);
        }

        private void EnsureTargetingArrow()
        {
            if (targetingArrow != null) return;

            targetingArrow = new GameObject("TargetingArrow");
            targetingArrowMaterial = new Material(Shader.Find("Sprites/Default"));
            targetingArrowShaft = CreateArrowLine("Shaft");
            targetingArrowHeadLeft = CreateArrowLine("HeadLeft");
            targetingArrowHeadRight = CreateArrowLine("HeadRight");
            targetingArrow.SetActive(false);
        }

        private LineRenderer CreateArrowLine(string lineName)
        {
            GameObject lineObject = new(lineName);
            lineObject.transform.SetParent(targetingArrow.transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.sharedMaterial = targetingArrowMaterial;
            line.startColor = Color.yellow;
            line.endColor = Color.yellow;
            line.startWidth = 0.35f;
            line.endWidth = 0.35f;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.numCapVertices = 4;
            line.sortingOrder = 1000;
            return line;
        }

        private static void SetArrowLine(LineRenderer line, Vector3 start, Vector3 end)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        private void HideTargetingArrow()
        {
            if (targetingArrow != null) targetingArrow.SetActive(false);
        }
        public string OwnDeckData;
        public string AIDeckData;
        public void OnOfflineStart()
        {
            if (!prefabsReady)
            {
                StartCoroutine(StartOfflineWhenReady());
                return;
            }

            StartOfflineGame();
        }

        private IEnumerator StartOfflineWhenReady()
        {
            yield return new WaitUntil(() => prefabsReady);
            StartOfflineGame();
        }

        private void StartOfflineGame()
        {
            Stats = new();
            online = false;
            seed = (int)DateTime.Now.Ticks;
            UnityEngine.Random.InitState(seed);
            bool start = UnityEngine.Random.Range(0, 2) == 0;
            if (DEBUG) start = false;
            ServerOnTurn.Value = start;//UnityEngine.Random.Range(0, 2) == 0;

            int[] OwnDeckList;
            int[] OppDeckList;
            LoadSelectedDecksData();
            if (OwnDeckData == null || AIDeckData == null)//This uses decks from Assets and is Debug only
            {//TODO: probably remove this and use the json from PlayerData
                OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/MyDeck").text));
                OppDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/OppDeck").text));
            }
            else
            {
                OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(OwnDeckData));
                OppDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(AIDeckData));
            }
            LoadDeck(decks[P.P1], OwnDeckList);
            LoadDeck(decks[P.P2], OppDeckList);
            HPCounters[P.P1].maxHP = MaxHealths[P.P1];
            HPCounters[P.P2].maxHP = MaxHealths[P.P2];
            HPCounters[P.P1].Health = MaxHealths[P.P1];
            HPCounters[P.P2].Health = MaxHealths[P.P2];
            LoadingScreen.SetActive(false);
            for (int i = 0; i < 3; i++) DrawCard(P.P1);
            for (int i = 0; i < 4; i++) DrawCard(P.P2);
            const int bodID = 1;
            AddCardToHandByID(P.P2, bodID);//Bod

            EndTurnBtn.onClick.AddListener(ToggleTurnOffline);
            StartTurn();
        }



        public override void OnNetworkSpawn()//TODO: this is probably what we can replace with the offline play.
        {
            StartCoroutine(StartNetworkGameWhenReady());
        }

        private IEnumerator StartNetworkGameWhenReady()
        {
            yield return new WaitUntil(() => prefabsReady);
            StartNetworkGame();
        }

        private void StartNetworkGame()
        {
            Stats = new();
            online = true;
            Console.WriteLine("NetworkSpawn");
            if (IsServer)
            {
                ServerOnTurn.Value = UnityEngine.Random.Range(0, 2) == 0;//TODO: this uses unities random. which is not synced. but it should be fine for now.
                Debug.Log("Host starts?: " + OnTurn);
                seed = (int)DateTime.Now.Ticks;
            }
            else Debug.Log("Client starts?: " + OnTurn);
            EndTurnBtn.onClick.AddListener(() => ToggleTurnServerRpc(new()));
            NetworkManager.OnClientDisconnectCallback += ClientDisconnected;
            //Load decks
            LoadSelectedDecksData();
            int[] OwnDeckList = null;
            if (OwnDeckData == null)//This uses decks from Assets and is Debug only
            {
                string debugstringchoice = "MyDeck";
                if (IsServer) debugstringchoice = "OppDeck";
                OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/" + debugstringchoice).text));//TODO import from json in PlayerData
            }
            else//This should be set from LobbyUI
            {
                OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(OwnDeckData));
            }
            if (!DEBUG) OwnDeckList.Shuffle();
            LoadDeck(decks[P.P1], OwnDeckList);


            HPCounters[P.P1].maxHP = MaxHealths[P.P1];
            HPCounters[P.P2].maxHP = MaxHealths[P.P2];
            HPCounters[P.P1].Health = MaxHealths[P.P1];
            HPCounters[P.P2].Health = MaxHealths[P.P2];
            if (IsServer)
                NetworkManager.OnClientConnectedCallback += (clientId) => { if (clientId != NetworkManager.LocalClientId) OnOpponentConnected(OwnDeckList, seed); };
            else
            {
                OnOpponentConnected(OwnDeckList);
                //System.IO.File.WriteAllText("CurrentDeck.json", JsonUtility.ToJson(new DeckSave(OwnDeckList)));
                //Debug.Log(JsonUtility.FromJson<DeckSave>(System.IO.File.ReadAllText("CurrentDeck.json")).ToString());
            }
        }

        private void ClientDisconnected(ulong id)
        {
            Console.WriteLine("ClientDisconnected: " + id);
            EndGame("Opponent disconnected.");
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null)
                NetworkManager.OnClientDisconnectCallback -= ClientDisconnected;

            base.OnNetworkDespawn();
        }

        private void OnOpponentConnected(int[] OwnDeckList, int seed = 0) => AnnounceDeckServerRpc(OwnDeckList, seed, new());//loads own deck and sends it to the other.
        private int[] GetCardIDs(List<object> DecodedDeckList)
        {
            int[] ids = new int[DecodedDeckList.Count];

            for (int i = 0; i < DecodedDeckList.Count; i++)
            {
                long item = (long)DecodedDeckList[i];
                ids[i] = (int)item;
            }
            return ids;
        }
        private void LoadDeck(Deck d, int[] ids)
        {
            foreach (int id in ids)
            {
                d.Add(Instantiate(CardPrefab).GetComponent<Card>().Initialize(CardDatabase[id], id));
            }

        }
        public void StartTurn() => StartTurn(OnTurn);
        private void StartTurn(bool localOnTurn)
        {
            foreach (TableActor actor in AllTableActors.ToArray())//Snapshot because effects can move actors between slots while the turn starts.
            {
                // An earlier StartTurn callback can destroy another actor from this snapshot.
                // Unity keeps the managed reference until the end of the frame, but it compares
                // equal to null and must not be dereferenced. Non-effect actors can also become
                // detached from their card before their turn in the snapshot is reached.
                if (actor == null) continue;
                if (actor is not Effect &&
                    (actor.transform.parent == null || actor.transform.parent.GetComponentInChildren<Card>(true) == null)) continue;
                actor.StartTurn(localOnTurn);//TODO: destruction of things should be proably queued and done after the effect has ended
                if (gameEnding) return;
            }

            TurnCount++;
            if (!localOnTurn)
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Opponents Turn";
                EndTurnBtn.interactable = false;
                if (OppManaCounter.MaxMana < 10) OppManaCounter.MaxMana++;
                OppManaCounter.Mana = OppManaCounter.MaxMana;
                DrawCard(P.P2);
            }
            else
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
                EndTurnBtn.interactable = !minionTargeting;
                if (OwnManaCounter.MaxMana < 10) OwnManaCounter.MaxMana++;
                OwnManaCounter.Mana = OwnManaCounter.MaxMana;
                DrawCard(P.P1);
            }
        }
        public void DrawCard(P who)//We are true opp is false.
        {
            Card card = decks[who].PopFirst();

            if (card == null)
            {
                Fatigue(who);
            }
            else
            {
                AddCardToHand(who, card, true);
            }

        }
        public bool HandsRevealed
        {
            get
            {
                Field field = FieldSlot.GetField();
                return field != null && field.GetComponents<IHandRevealProvider>().Any(provider => provider.RevealsHands);
            }
        }

        public void RefreshHandVisibility()
        {
            bool revealed = HandsRevealed;
            foreach (Card card in AllCards)
                card.Hidden = card.Owner != P.P1 && !revealed;
        }

        public void AddCardToHand(P who, Card c, bool discardExcesive = false)
        {
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                {
                    if (discardExcesive) Discard(c, who);
                    else Destroy(c.gameObject);
                    break;
                }
                if (!HandSlots[who][i].Occupied)
                {
                    HandSlots[who][i].PlaceCard(c);
                    c.Hidden = who != P.P1 && !HandsRevealed;
                    c.transform.localPosition = Vector3.zero;
                    c.standardScale = c.transform.localScale;
                    break;
                }
            }
        }
        public void AddCardToHandByID(P who, int ID, bool discardExcesive = false)
        {
            AddCardToHand(who, Instantiate(CardPrefab).GetComponent<Card>().Initialize(CardDatabase[ID], ID), discardExcesive);
        }
        public void AddCardToDeckByID(P who, int ID)
        {
            decks[who].Add(Instantiate(CardPrefab).GetComponent<Card>().Initialize(CardDatabase[ID], ID));
        }
        public void AddCardToDeckAtRandomByID(P who, int ID)
        {
            Card card = Instantiate(CardPrefab).GetComponent<Card>().Initialize(CardDatabase[ID], ID);
            decks[who].AddRandom(card);
        }

        public Minion SummonMinion(P who, int id)
        {
            int slotIndex = GetNextFreeMinionSlot(who);
            if (slotIndex == -1) return null;

            Card card = Instantiate(CardPrefab).GetComponent<Card>().Initialize(CardDatabase[id], id);
            CardSlot slot = minionSlots[who][slotIndex];
            slot.PlaceCard(card);
            card.gameObject.SetActive(false);
            return card.SummonMinion(slot);
        }
        internal CardSlot GetMinionSlot(P who, int index) => minionSlots[who][index];
        public void Discard(Card c, P who)
        {
            c.OnDiscard();
            CardDiscarded?.Invoke(this, new(c));
            DetachCard(c, who);
            AddToGrave(c, who);
        }
        public void Fatigue(P who)
        {
            HPCounters[who].Health -= FatigueVals[who];
            FatigueVals[who]++;
            //Debug.Log(FatigueVals[who]);
        }
        public void EndTurn() => EndTurn(OnTurn);
        private void EndTurn(bool localOnTurn)
        {
            P endingPlayer = localOnTurn ? P.P1 : P.P2;
            foreach (GameActor actor in AllActors.ToArray())
            {
                actor.EndTurn(localOnTurn);
                if (gameEnding) return;
            }
            Stats.ResetCardsPlayedThisTurn(endingPlayer);
        }
        public void AddToGrave(Card c, P who)
        {
            graves[who].Add(c);
            //c.transform.localPosition -= new Vector3(0, 0, ((float)graves[who].Count) / 100);
        }
        public void OnUIPlayMinion(int cardindex, int minionSlotIndex, int target = -1, int[] choices = null) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, target, minionSlotIndex, choices));
        public void OnUIMinionAttack(int minionSlotIndex, int actorSlotTarget) => OnUITakeAction(PlayerAction.AttackAction(minionSlotIndex, actorSlotTarget));
        public void OnUICastSpell(int cardindex, int target = -1, int[] choices = null) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, target, -1, choices));
        public void OnUIPlayField(int cardindex, int[] choices = null) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, -1, -1, choices));
        public void OnUITakeAction(PlayerAction action)
        {
            TakeAction(P.P1, action);//Play it out
            if (online) TakeActionServerRpc(action, new());//Send to opponent
            ClearHighlights();
        }

        /// <summary>
        /// Check whether card can be played. If the player playing it has enough mana and whether there is at least one target if targetable
        /// </summary>
        /// <param name="card">Which card</param>
        /// <param name="player">By whom</param>
        /// <returns></returns>

        public bool IsCardPlayable(Card card, P player)
            =>GetManaCost(card) <= ManaCounters[player].Mana
               && (!card.Targetted || ValidTargetExists(card))
               && (!card.IsExperiment || CanStartExperiment(card, player));

        private bool CanStartExperiment(Card card, P player)
            => GetFreeEffectSlot(player) != null
               && !GetAllEffectsOwnedBy(player).Any(effect =>
                   effect.isExperiment && effect.CardID == card.ID);

        public bool ValidTargetExists(Card card)
            => AllCharacters.Any(character => character != null && card.IsTargetValid(character));

        public void PlayRandomly(Card card)
        {
            P owner = card.Owner;
            GameActor target = null;
            int[] choices = card.GetRandomChoices();

            if (!IsCardPlayable(card, owner))
            {
                Discard(card, owner);
                return;
            }

            if (card.Targetted && card.cardType != Card.CardType.Field)
            {
                List<DamageableActor> validTargets = AllCharacters
                    .Where(character => character != null && card.IsTargetValid(character))
                    .ToList();

                if (validTargets.Count == 0)
                {
                    if (card.cardType == Card.CardType.Spell)
                    {
                        Discard(card, owner);
                        return;
                    }
                }
                else
                {
                    target = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
                }
            }

            switch (card.cardType)
            {
                case Card.CardType.Minion:
                    int slotIndex = GetRandomFreeMinionSlot(owner);
                    if (slotIndex == -1)
                    {
                        Discard(card, owner);
                        return;
                    }

                    DetachCard(card, owner);
                    CardSlot minionSlot = minionSlots[owner][slotIndex];
                    minionSlot.PlaceCard(card);
                    card.gameObject.SetActive(false);
                    Minion randomlyPlayedMinion = card.PlayMinion(minionSlot, target, choices);
                    InvokeMinionPlayed(randomlyPlayedMinion);
                    OnPlayed?.Invoke(randomlyPlayedMinion, new(Card.CardType.Minion, target, choices));
                    break;

                case Card.CardType.Spell:
                    DetachCard(card, owner);
                    if (TryCounterSpell(card, owner))
                    {
                        AddToGrave(card, owner);
                        break;
                    }
                    card.CastSpell(target, choices);
                    AddToGrave(card, owner);
                    card.standardScale = Vector3.one;
                    OnPlayed?.Invoke(card, new(Card.CardType.Spell, target, choices));
                    break;

                case Card.CardType.Field:
                    DetachCard(card, owner);
                    FieldSlot.ClearField();
                    FieldSlot.Initialize(owner, FieldSlot.index);
                    FieldSlot.PlaceCard(card);
                    card.gameObject.SetActive(false);
                    OnPlayed?.Invoke(card.PlayField(choices), new(Card.CardType.Field, null, choices));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(card.cardType), card.cardType, "Unknown card type.");
            }
        }

        private static void DetachCard(Card card, P owner)
        {
            card.backupOwner = owner;

            if (card.transform.parent != null && card.transform.parent.TryGetComponent(out CardSlot slot))
            {
                slot.PopCard();
                return;
            }

            if (card.transform.parent != null && card.transform.parent.TryGetComponent(out Deck deck))
            {
                deck.Remove(card);
                return;
            }

            if (card.transform.parent != null && card.transform.parent.TryGetComponent(out Grave grave))
            {
                grave.Take(card);
                return;
            }

            card.transform.SetParent(null);
        }

        


        /// <summary>
        /// Simulate move (good for network, could be used for AI etc.)
        /// </summary>
        /// <param name="who"></param>
        /// <param name="action"></param>
        void TakeAction(P who, PlayerAction action)
        {
            switch (action.Actiontype)
            {
                case PlayerAction.ActionType.Play:
                    CardSlot handSlot = HandSlots[who][action.Source];
                    Card card = handSlot.GetCard();
                    ManaCounters[who].Mana -= GetManaCost(card);
                    handSlot.PopCard();
                    Stats.RecordCardPlayed(who);
                    CardSlot targetSlot = null;
                    GameActor target = null;
                    if (action.CharacterTarget.HasTarget)
                    {
                        targetSlot = ResolveCharacterTarget(who, action.CharacterTarget);
                        target = targetSlot.GetComponentInChildren<DamageableActor>();
                    }
                    if (card.cardType == CardType.Minion)
                    {
                        CardSlot slot = minionSlots[who][action.Slot];
                        slot.PlaceCard(card);
                        card.gameObject.SetActive(false);
                        Minion playedMinion = card.PlayMinion(slot, target, action.Choices);
                        InvokeMinionPlayed(playedMinion);
                        OnPlayed?.Invoke(playedMinion, new(CardType.Minion, target, action.Choices));
                    }
                    else if (card.cardType == CardType.Field)
                    {
                        FieldSlot.ClearField();
                        FieldSlot.Initialize(who, FieldSlot.index);
                        FieldSlot.PlaceCard(card);
                        card.gameObject.SetActive(false);
                        OnPlayed?.Invoke(card.PlayField(action.Choices), new(CardType.Field, target, action.Choices));
                    }
                    else if (card.cardType == CardType.Spell)
                    {
                        if (TryCounterSpell(card, who))
                        {
                            AddToGrave(card, who);
                            card.standardScale = Vector3.one;
                            break;
                        }
                        card.CastSpell(target, action.Choices);
                        AddToGrave(card, who);
                        card.standardScale = Vector3.one;//Maybe?
                        OnPlayed?.Invoke(card, new(CardType.Spell, target, action.Choices));
                    }
                    else throw new Exception("Unknown cardType");
                    break;
                case PlayerAction.ActionType.Attack:
                    Minion Ownminion = minionSlots[who][action.Source].GetMinion();//Opponents dont have minion slots so we cannot cast and do GetMinion
                    if (action.Target < 2 * maxMinionSlots)//action.Target can remain as is cuz you cannot attack your own stuff
                    {
                        Minion Oppminion = minionSlots[who.Other()][action.Target - maxMinionSlots].GetMinion();
                        Ownminion.AttackAction(Oppminion);
                    }
                    else
                    {
                        Ownminion.AttackAction(HPCounters[who.Other()].Face);
                    }
                    break;
                default:
                    throw new NotImplementedException("TakeActionCase not implemented");
            }
        }

        public int GetManaCost(Card card)
        {
            int cost = card.mana;
            foreach (var item in AllActors)
            {
                foreach (var manamod in item.manacostmod)
                {
                    cost += manamod(card);
                }
            }
            return math.max(cost, 0);
        }

        private bool TryCounterSpell(Card spell, P caster)
        {
            CancelableCardEventArgs args = new(spell);
            BeforeSpellPlayed?.Invoke(spell, args);
            return args.Cancel;
        }

        public void InvokeBeforeAttack(Minion attacker, Minion.TargetedEventEventArgs args)
            => BeforeAttackDeclared?.Invoke(attacker, args);

        public void InvokeAfterAttack(Minion attacker, DamageableActor target)
            => AfterAttackResolved?.Invoke(attacker, new Minion.TargetedEventEventArgs { target = target });

        public void InvokeMinionDied(Minion minion)
        {
            MinionDied?.Invoke(minion, EventArgs.Empty);
            BoardChanged?.Invoke();
        }

        public void InvokeMinionPlayed(Minion minion) => MinionPlayedForReactions?.Invoke(minion, EventArgs.Empty);

        public bool IsOpponentTurn(P effectOwner) => PlayerOnTurn == effectOwner.Other();

        public int InvertIndex(int index)
        {

            if (index < maxMinionSlots) index += maxMinionSlots;
            else if (index == 2 * maxMinionSlots) index++;
            else if (index == 2 * maxMinionSlots + 1) index--;
            else index -= maxMinionSlots;

            return index;
        }

        /// <summary>Converts a board-global character slot to the acting player's wire representation.</summary>
        private CharacterTargetIndex EncodeCharacterTarget(P player, CardSlot slot)
        {
            int playerRelativeIndex = player == P.P1 ? slot.index : InvertIndex(slot.index);
            return CharacterTargetIndex.FromPlayerPerspective(playerRelativeIndex);
        }

        /// <summary>Resolves a player-relative wire target back to the local board slot.</summary>
        private CardSlot ResolveCharacterTarget(P player, CharacterTargetIndex target)
        {
            int boardIndex = player == P.P1 ? target.Value : InvertIndex(target.Value);
            if (boardIndex < 0 || boardIndex >= 2 * maxMinionSlots + 2)
                throw new ArgumentOutOfRangeException(nameof(target), target.Value, "Invalid character target index.");

            return AllCharacterSlots.ElementAt(boardIndex);
        }

        public void ToggleTurnOffline()
        {
            bool previousLocalTurn = OnTurn;
            ServerOnTurn.Value = !ServerOnTurn.Value;
            ResolveTurnTransition(previousLocalTurn, OnTurn);
            if (!gameEnding && !OnTurn) AIPlayer.OnTurnStart();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ToggleTurnServerRpc(ServerRpcParams Srpcparams)
        {
            ulong expectedPlayer = ServerOnTurn.Value ? NetworkManager.ServerClientId : NetworkManager.ConnectedClientsIds.First(id => id != NetworkManager.ServerClientId);
            if (Srpcparams.Receive.SenderClientId != expectedPlayer) return;

            bool previousServerTurn = ServerOnTurn.Value;
            ServerOnTurn.Value = !previousServerTurn;
            ResolveTurnTransitionClientRpc(previousServerTurn, ServerOnTurn.Value);
        }
        [ClientRpc]
        private void ResolveTurnTransitionClientRpc(bool previousServerTurn, bool newServerTurn)
        {
            ResolveTurnTransition(IsServer == previousServerTurn, IsServer == newServerTurn);
        }
        private void ResolveTurnTransition(bool previousLocalTurn, bool newLocalTurn)
        {
            resolvingLocalTurn = newLocalTurn;
            EndTurn(previousLocalTurn);
            if (!gameEnding) StartTurn(newLocalTurn);
            resolvingLocalTurn = null;
        }
        [ClientRpc(RequireOwnership = false)]
        private void TakeActionClientRpc(PlayerAction action, ClientRpcParams _) => TakeAction(P.P2, action);//This runs only on opponent.
        [ServerRpc(RequireOwnership = false)]
        private void TakeActionServerRpc(PlayerAction action, ServerRpcParams srp) => TakeActionClientRpc(action, new()
        {
            Send = new()
            {
                // TODO: Resolve the opponent from ConnectedClientsIds if reconnecting is supported.
                TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
            }
        }
        );
        [ServerRpc(RequireOwnership = false)]
        private void AnnounceDeckServerRpc(int[] deck, int seed, ServerRpcParams srp) => AnnounceDeckClientRpc(deck, seed, new()
        {
            Send = new()
            {
                // TODO: Resolve the opponent from ConnectedClientsIds if reconnecting is supported.
                TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
            }
        });
        [ClientRpc(RequireOwnership = false)]
        private void AnnounceDeckClientRpc(int[] deck, int seed, ClientRpcParams _)//This runs only on opponent.
        {
            LoadDeck(decks[P.P2], deck);
            //StartGame stuff
            if (!IsServer) UnityEngine.Random.InitState(seed);
            else UnityEngine.Random.InitState(this.seed);
            LoadingScreen.SetActive(false);
            //TODO: Animations (Who against who)


            //TODO: Cointoss anim
            for (int i = 0; i < 3; i++) DrawCard(PlayerOnTurn);
            for (int i = 0; i < 4; i++) DrawCard(PlayerOnTurn.Other());
            const int bodID = 1;
            AddCardToHandByID(PlayerOnTurn.Other(), bodID);//Bod
            //TODO: Mulligan
            StartTurn();
        }
        public void ShuffleDeckRequest(P who)
        {
            int[] permutation = decks[who].Shuffle();//I shuffle mine and tell the other guy how I shuffled it.
            ShuffleDeckServerRpc(who.Other(), permutation, new());
        }
        [ServerRpc(RequireOwnership = false)]
        private void ShuffleDeckServerRpc(P who, int[] permutation, ServerRpcParams srp) => ShuffleDeckClientRpc(who, permutation, new()
        {
            Send = new()
            {
                // TODO: Resolve the opponent from ConnectedClientsIds if reconnecting is supported.
                TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
            }
        });

        [ClientRpc(RequireOwnership = false)]
        private void ShuffleDeckClientRpc(P who, int[] permutation, ClientRpcParams crp)
        {
            decks[who].Shuffle(permutation);
        }

        public CardSlot GetFreeEffectSlot(P who)
        {
            CardSlot[] slots = EffSlots[who];
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].Occupied)
                {
                    return slots[i];
                }
            }
            return null;
        }
        [SerializeField] private TMP_Dropdown OwnDeckDropdown;
        [SerializeField] private TMP_Dropdown OppDeckDropdown;

        private readonly List<string> savePaths = new();
        public void RefreshDropdown()
        {
            OwnDeckDropdown.ClearOptions();
            OppDeckDropdown.ClearOptions();
            savePaths.Clear();

            List<string> optionNames = new();

            foreach (string deckName in DeckStorage.GetDeckNames())
            {
                savePaths.Add(deckName);
                optionNames.Add(deckName);
            }

            if (optionNames.Count == 0)
            {
                optionNames.Add("<No Saves>");
            }

            OwnDeckDropdown.AddOptions(optionNames);
            OppDeckDropdown.AddOptions(optionNames);
        }
        public void LoadSelectedDecksData()
        {
            if (savePaths.Count == 0)
            {
                Debug.Log("No save selected.");
                return;
            }

            OwnDeckData = DeckStorage.Read(savePaths[OwnDeckDropdown.value]);
            AIDeckData = DeckStorage.Read(savePaths[OppDeckDropdown.value]);
        }

        public CharacterTargetIndex GetRandomTargetForCard(Card card, P player)
        {
            List<CardSlot> validTargets = (from DamageableActor c in AllCharacters
                                           where c != null && card.IsTargetValid(c)
                                           select c.GetComponentInParent<CardSlot>()).ToList();
            if (validTargets.Count == 0) return CharacterTargetIndex.None;

            CardSlot target = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
            return EncodeCharacterTarget(player, target);
        }

        public CharacterTargetIndex GetRandomTargetForMinion(Minion minion, P player)
        {
            List<CardSlot> validTargets = (from DamageableActor character in AllCharacters
                                           where character != null && minion.IsTargetValid(character)
                                           select character.GetComponentInParent<CardSlot>()).ToList();
            if (validTargets.Count == 0) return CharacterTargetIndex.None;

            CardSlot target = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
            return EncodeCharacterTarget(player, target);
        }

        public override void OnDestroy()
        {
            if (targetingArrow != null) Destroy(targetingArrow);
            if (targetingArrowMaterial != null) Destroy(targetingArrowMaterial);
            if (cardPrefabHandle.IsValid()) Addressables.Release(cardPrefabHandle);
            ReleaseSharedAssets();

            if (Instance == this)
            {
                CardArt.ReleaseAll();
                Instance = null;
            }
            base.OnDestroy();
        }
    }
}
