using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardData;

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
                int k = permutation[n] =  UnityEngine.Random.Range(0, n + 1);
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

    public class GameManager : NetworkBehaviour
    {
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
            public int Target { readonly get => target; private set => target = value; }//Used for targetting various things
            public static PlayerAction PlayCardAction(int card, int target, int slot = -1) => new()
            {
                Source = card,
                Actiontype = ActionType.Play,
                Slot = slot,
                Target = target
            };
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
        private Dictionary<P, Deck> decks;
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
        [SerializeField] public FieldSlot FieldSlot;
        [SerializeField] HPCounter OwnHPCounter;
        [SerializeField] HPCounter OppHPCounter;
        [SerializeField] Transform OwnCardCounter;
        [SerializeField] Transform OppCardCounter;
        public ManaCounter OwnManaCounter;
        public ManaCounter OppManaCounter;
        Dictionary<P, ManaCounter> ManaCounters;
        Dictionary<P, HPCounter> HPCounters;
        IEnumerable<CardSlot> AllSlots { get => new CardSlot[] { FieldSlot }.Concat(minionSlots[P.P1]).Concat(minionSlots[P.P2]).Concat(EffSlots[P.P1]).Concat(EffSlots[P.P2]).Concat(HandSlots[P.P1]).Concat(HandSlots[P.P2]); }
        IEnumerable<GameActor> AllActors { get => from CardSlot s in AllSlots let c = s.GetComponentInChildren<GameActor>() where c != null select c; }//Will fail on null exception if you forget to assign Field slot.
        public Dictionary<int, CardData.CardData> CardDatabase;
        public GameObject CardPrefab;
        public Button EndTurnBtn;
        public const int maxMinionSlots = 7;
        public const int maxEffSlots = 6;
        public const int maxHandSlots = 10;
        public const bool DEBUG = false;

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
            readonly Card card;
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
                        if (minionSlots[P.P2][i].GetComponentInChildren<Minion>() == m) { slot = maxMinionSlots + i; break; }
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
            get => IsServer == ServerOnTurn.Value;
            set
            {
                if (IsServer) ServerOnTurn.Value = value;
                else ServerOnTurn.Value = !value;
            }
        }
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
        private void Start()
        {
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
            HPCounters[P.P1].GetComponent<Face>().o = P.P1;
            HPCounters[P.P2].GetComponent<Face>().o = P.P2;
            HPCounters[P.P1].Death += (_,_)=> GameManager_PlayerDeath(P.P1);
            HPCounters[P.P2].Death += (_, _) => GameManager_PlayerDeath(P.P2);
            //Load cards
            CardDatabase = CDJsonUtils.LoadCardDatabase();
            Debug.Log(CardDatabase);
        }

        private void GameManager_PlayerDeath(P who)
        {
            //TODO: You win screen
            MatchResults.result = who == P.P1 ? "You lose!" : "You win!";
            SceneManager.LoadScene("ResultsScene");
            SceneManager.UnloadSceneAsync(1);//To reset it maybe?
        }

        private void Update()
        {
            if (cursor != null)
            {
                Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursor.transform.position = new(mousepos.x, mousepos.y, -1);
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (UnityEngine.Random.Range(0, 2) == 0) ServerOnTurn.Value = false;
                Debug.Log("Host starts?: " + OnTurn);
            }
            else Debug.Log("Client starts?: " + OnTurn);
            EndTurnBtn.onClick.AddListener(() => ToggleTurnServerRpc(new ServerRpcParams()));
            ServerOnTurn.OnValueChanged += (bool prev, bool n) =>
            {
                if (prev == n) return;//Should not happen but to make sure.
                EndTurn();
                StartTurn();
            };
            NetworkManager.OnClientDisconnectCallback += (id) => ClientDisconnected(id);
            //Load decks
            string debugstringchoice = "MyDeck";
            if (IsServer) debugstringchoice = "OppDeck";
            int[] OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/" + debugstringchoice).text));//TODO import from json in PlayerData

            if (!DEBUG) OwnDeckList.Shuffle();
            LoadDeck(decks[P.P1], OwnDeckList);
               

            
            HPCounters[P.P1].Health = MaxHealths[P.P1];
            HPCounters[P.P2].Health = MaxHealths[P.P2];
            if (IsServer)
                NetworkManager.OnClientConnectedCallback += (clientId) => { if (clientId != NetworkManager.LocalClientId) OnOpponentConnected(OwnDeckList); };
            else
            {
                OnOpponentConnected(OwnDeckList);
                //System.IO.File.WriteAllText("CurrentDeck.json", JsonUtility.ToJson(new DeckSave(OwnDeckList)));
                //Debug.Log(JsonUtility.FromJson<DeckSave>(System.IO.File.ReadAllText("CurrentDeck.json")).ToString());
            }
        }

        private void ClientDisconnected(ulong id)
        {
            if (DEBUG) Application.Quit();
            //TODO: "Yer opponent left!"
            GameManager_PlayerDeath((P)id);//This might work
        }

        private void OnOpponentConnected(int[] OwnDeckList) => AnnounceDeckServerRpc(OwnDeckList, new());//loads own deck and sends it to the other.
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
                Card Cardscript = Instantiate(CardPrefab).GetComponent<Card>();
                Cardscript.Initialize(CardDatabase[id]);
                d.Add(Cardscript);
            }

        }
        public void StartTurn()
        {
            foreach (GameActor actor in AllActors)//Might be reasonable to do it through events. but this should ensure consistency with the rules as in which order things happen
            {
                actor.StartTurn(OnTurn);
            }

            TurnCount++;
            if (!OnTurn)
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
                EndTurnBtn.interactable = true;
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
                AddCardToHand(who, card);
            }

        }
        public void AddCardToHand(P who, Card c)
        {
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                {
                    Discard(c, who);
                    break;
                }
                if (!HandSlots[who][i].Occupied)
                {
                    HandSlots[who][i].PlaceCard(c);
                    c.Hidden = who != P.P1;
                    c.transform.localPosition = new Vector3(0, 0, -1);
                    c.standardScale = c.transform.localScale;
                    break;
                }
            }
        }
        public void Discard(Card c, P who)
        {
            c.OnDiscard();
            CardDiscarded?.Invoke(this, new(c));
            AddToGrave(c, who);
        }
        public void Fatigue(P who)
        {
            HPCounters[who].Health -= FatigueVals[who];
            FatigueVals[who]++;
            //Debug.Log(FatigueVals[who]);
        }
        public void EndTurn()
        {
            foreach (GameActor actor in AllActors)
            {
                actor.EndTurn(OnTurn);
            }
        }
        public void AddToGrave(Card c, P who)
        {
            graves[who].Add(c);
            c.transform.localPosition -= new Vector3(0, 0, ((float)graves[who].Count) / 100);
        }
        public void OnUIPlayMinion(int cardindex, int minionSlotIndex, int target = -1) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, target, minionSlotIndex));
        public void OnUIMinionAttack(int minionSlotIndex, int actorSlotTarget) => OnUITakeAction(PlayerAction.AttackAction(minionSlotIndex, actorSlotTarget));
        public void OnUICastSpell(int cardindex, int target = -1) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, target));
        public void OnUIPlayField(int cardindex) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, -1));
        public void OnUITakeAction(PlayerAction action)
        {
            TakeAction(P.P1, action);//Play it out
            TakeActionServerRpc(action, new());//Send to opponent
        }

        /// <summary>
        /// Mainly for UI purposes
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool IsCardPlayable(Card card)
        {
            return card.mana <= ManaCounters[P.P1].Mana;
            //TODO Any eligible targets etc.
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
                    Card card = HandSlots[who][action.Source].PopCard();
                    ManaCounters[who].Mana -= card.mana;//TODO: mana modifiers. probably do it on card.mana
                    if (card.cardType == Card.CardType.Minion)
                    {
                        CardSlot slot = minionSlots[who][action.Slot];
                        slot.PlaceCard(card);
                        card.PlayMinion(slot, action.Target);
                        card.gameObject.SetActive(false);

                    }
                    else if (card.cardType == Card.CardType.Field)
                    {
                        FieldSlot.ClearField();
                        FieldSlot.PlaceCard(card);
                        card.PlayField();
                        card.gameObject.SetActive(false);
                    }
                    else if (card.cardType == Card.CardType.Spell)
                    {
                        card.CastSpell(action.Target);
                        AddToGrave(card, who);
                        card.standardScale = Vector3.one;//Maybe?
                    }
                    else throw new Exception("Unknown cardType");
                    break;
                case PlayerAction.ActionType.Attack:
                    Minion Ownminion = minionSlots[who][action.Source].GetComponentInChildren<Minion>();//Opponents dont have minion slots so we cannot cast and do GetMinion
                    if (action.Target < 2 * maxMinionSlots)
                    {
                        Minion Oppminion = minionSlots[who.Other()][action.Target - maxMinionSlots].GetComponentInChildren<Minion>();
                        Ownminion.AttackAction(Oppminion);
                    }
                    else
                    {
                        Ownminion.AttackAction(HPCounters[who.Other()]);
                    }
                    break;
                default:
                    throw new NotImplementedException("TakeActionCase not implemented");
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void ToggleTurnServerRpc(ServerRpcParams Srpcparams)
        {
            //Debug.Log("SenderID: " + Srpcparams.Receive.SenderClientId);
            if (ServerOnTurn.Value && Srpcparams.Receive.SenderClientId == 0) ServerOnTurn.Value = false;
            else if (!ServerOnTurn.Value && Srpcparams.Receive.SenderClientId == 1) ServerOnTurn.Value = true;
        }
        [ClientRpc(RequireOwnership = false)]
        private void TakeActionClientRpc(PlayerAction action, ClientRpcParams crp) => TakeAction(P.P2, action);//This runs only on opponent.
        [ServerRpc(RequireOwnership = false)]
        private void TakeActionServerRpc(PlayerAction action, ServerRpcParams srp) => TakeActionClientRpc(action, new()
        {
            Send = new()
            {
                TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
            }
        }
        );
        [ServerRpc(RequireOwnership = false)]
        private void AnnounceDeckServerRpc(int[] deck, ServerRpcParams srp) => AnnounceDeckClientRpc(deck, new()
        {
            Send = new()
            {
                TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
            }
        });
        [ClientRpc(RequireOwnership = false)]
        private void AnnounceDeckClientRpc(int[] deck, ClientRpcParams crp)//This runs only on opponent.
        {
            LoadDeck(decks[P.P2], deck);
            //StartGame stuff
            //Animations (Who against who)


            //Cointoss anim
            for (int i = 0; i < 3; i++)
                if (OnTurn) DrawCard(P.P1);
                else DrawCard(P.P2);
            for (int i = 0; i < 4; i++)
                if (!OnTurn) DrawCard(P.P1);
                else DrawCard(P.P2);
            //Mulligan
            StartTurn();
        }
        private void ShuffleDeckRequest(P who)
        {
            int[] permutation = decks[who].Shuffle();//I shuffle mine and tell the other guy how I shuffled it.
            ShuffleDeckServerRpc(who.Other(),permutation, new());
        }
        [ServerRpc(RequireOwnership = false)]
        private void ShuffleDeckServerRpc(P who, int[] permutation, ServerRpcParams srp) => ShuffleDeckClientRpc(who, permutation, new()
            {
                Send = new()
                {
                    TargetClientIds = new List<ulong>() { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
                }
            });

        [ClientRpc(RequireOwnership = false)]
        private void ShuffleDeckClientRpc(P who, int[] permutation, ClientRpcParams crp)
        {
            decks[who].Shuffle(permutation);
        }
    }
}