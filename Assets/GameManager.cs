using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.UI;

namespace CardGame
{

    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

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
            public static PlayerAction PlayCardAction(int card, int target, int slot=-1)
            {
                return new()
                {
                    Source = card,
                    Actiontype = ActionType.Play,
                    Slot = slot,
                    Target = target
                };

            }
            public static PlayerAction AttackAction(int card, int slot)
            {
                return new()
                {
                    Source = card,
                    Actiontype = ActionType.Attack,
                    Target = slot
                };

            }

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
            public DeckSave(int[] data)
            {
                this.data = data;
            }
            public override string ToString()
            {
                return new StringBuilder().AppendJoin(" ", data).ToString();
            }
        }
        public class HealthDict{
            IDictionary<P,int> dict;
            private readonly Transform OwnHPLabel;
            private readonly Transform OppHPLabel;

            public HealthDict(Dictionary<P, int> dict, Transform ownHPLabel, Transform oppHPLabel)
            {
                this.dict = dict;
                OwnHPLabel = ownHPLabel;
                OppHPLabel = oppHPLabel;
            }

            public int this[P key]
            {
                get { return dict[key]; }
                set {
                    dict[key] = value;
                    if(key ==P.P1) OwnHPLabel.gameObject.GetComponent<TextMeshProUGUI>().text = value.ToString();
                    else OppHPLabel.gameObject.GetComponent<TextMeshProUGUI>().text = value.ToString();
                }
            }
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
        [SerializeField] Transform OwnHPLabel;
        [SerializeField] Transform OppHPLabel;
        [SerializeField] Transform OwnCardCounter;
        [SerializeField] Transform OppCardCounter;
        IEnumerable<CardSlot> AllSlots { get => new CardSlot[] { FieldSlot }.Concat(minionSlots[P.P1]).Concat(minionSlots[P.P2]).Concat(EffSlots[P.P1]).Concat(EffSlots[P.P2]).Concat(HandSlots[P.P1]).Concat(HandSlots[P.P2]); }
        IEnumerable<GameActor> AllActors { get => from CardSlot s in AllSlots let c = s.GetComponentInChildren<GameActor>() where c!=null select c; }//Will fail on null exception if you forget to assign Field slot.
        public Dictionary<int, CardData> CardDatabase;
        public GameObject CardPrefab;
        public Button EndTurnBtn;
        private const int maxMinionSlots = 7;
        private const int maxEffSlots = 6;
        private const int maxHandSlots = 10;

        public HealthDict Healths;
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
        internal Card cursor;
        internal Transform highlightedSlot;

        /// <summary>
        /// Discard cards subscribe to this
        /// </summary>
        public event EventHandler<CardActionEventArgs> CardDiscarded;
        public class CardActionEventArgs
        {
            public CardActionEventArgs(Card c)
            {
                card = c;
            }
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

        public bool OnTurn
        {
            get
            {
                return IsServer == ServerOnTurn.Value;
            }
            set
            {
                if (IsServer) ServerOnTurn.Value = value;
                else ServerOnTurn.Value = !value;
            }
        }
        private void Awake()
        {
            Instance = this;
            Healths = new( new(){
                {P.P1,MaxHealths[P.P1] },
                {P.P2,MaxHealths[P.P2]}
            },OwnHPLabel,OppHPLabel);
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
                minionSlots[P.P2][i].Initialize(P.P2,maxMinionSlots +  i);
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
            //Load cards
            LoadCardDatabase();
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
                EndTurnEffects();
                StartTurn();

            };
            NetworkManager.OnClientDisconnectCallback +=(_)=>Application.Quit();
            //Load decks
            string debugstringchoice = "MyDeck";
            if (IsServer) debugstringchoice = "OppDeck";
            int[] OwnDeckList = GetCardIDs((List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/"+ debugstringchoice).text));//TODO import from json in PlayerData
            
            LoadDeck(decks[P.P1], OwnDeckList);
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += (clientId) => { if (clientId != NetworkManager.LocalClientId) onOpponentConnected(clientId, OwnDeckList); };
            }
            else { onOpponentConnected(NetworkManager.LocalClientId, OwnDeckList);
                
                //System.IO.File.WriteAllText("CurrentDeck.json", JsonUtility.ToJson(new DeckSave(OwnDeckList)));
                //Debug.Log(JsonUtility.FromJson<DeckSave>(System.IO.File.ReadAllText("CurrentDeck.json")).ToString());
            }

        }
        private void onOpponentConnected(ulong clientId, int[] OwnDeckList)
        {
            //Debug.Log("calling srpc");
            AnnounceDeckServerRpc(OwnDeckList, new());//loads own deck and sends it to the other.
            //var OppDeckList = (List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/OppDeck").text);//TODO get from Lobby
            //LoadDeck(decks[P.P2], OppDeckList);
        }


        private int[] GetCardIDs(List<object> DecodedDeckList)
        {
            int[] ids = new int[DecodedDeckList.Count];

            for (int i = 0; i < DecodedDeckList.Count; i++)
            {
                long item = (long)DecodedDeckList[i];
                ids[i]=(int)item;
            }
                return ids;
        }
        private void LoadDeck(Deck d, int[] ids)
        {
            foreach (int id in ids)
            {
                var c = Instantiate(CardPrefab);
                Card Cardscript = c.GetComponent<Card>();
                Cardscript.Initialize(CardDatabase[id]);
                d.Add(Cardscript);
            }
            
        }
        public void StartTurn()
        {
            foreach (GameActor actor in AllActors )
            {
                actor.StartTurn(OnTurn);
            }

            TurnCount++;
            if (!OnTurn)
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Opponents Turn";
                EndTurnBtn.interactable = false;
                DrawCard(P.P2);
            }
            else
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
                EndTurnBtn.interactable = true;
                DrawCard(P.P1);
            }


            
        }
        public void DrawCard(P who)//We are true opp is false.
        {

            //We draw card
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
            Healths[who] -= FatigueVals[who];
            FatigueVals[who]++;
            Debug.Log(FatigueVals[who]);
        }
        public void ShuffleDeck(P who) => decks[who].Shuffle();
        public void EndTurnEffects()
        {
            foreach (GameActor actor in AllActors)
            {
                //if (actor != null)
                actor.EndTurn(OnTurn);
            }
        }
        Dictionary<string, Dictionary<string, string>> ParseExtraJSON()
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
        void LoadCardDatabase()
        {
            var cardsJSONfile = Resources.Load<TextAsset>("CardData/cards");
            object decodedJSON = MiniJson.JsonDecode(cardsJSONfile.text);
            var loadDict = (Dictionary<string, object>)decodedJSON;

            Dictionary<string, Dictionary<string, string>> extraData = ParseExtraJSON();

            CardDatabase = new();
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
        }
        public void AddToGrave(Card c, P who)
        {
            graves[who].Add(c);
            c.transform.localPosition -= new Vector3(0, 0, ((float)graves[who].Count) / 100);


        }
        public void OnUIPlayMinion(int cardindex, int minionSlotIndex,int target=-1)=>OnUITakeAction(PlayerAction.PlayCardAction(cardindex,target, minionSlotIndex));
        public void OnUIAttackMinion(int minionSlotIndex, int minionSlotTarget)=>OnUITakeAction(PlayerAction.AttackAction(minionSlotIndex, minionSlotTarget));
        public void OnUICastSpell(int cardindex, int target = -1)=>OnUITakeAction(PlayerAction.PlayCardAction(cardindex, target));
        public void OnUIPlayField(int cardindex) => OnUITakeAction(PlayerAction.PlayCardAction(cardindex, -1));
        public void OnUITakeAction(PlayerAction action)
        {
            if (!ValidateAction(action)) return;
            TakeAction(P.P1, action);//Play it out
            //Debug.Log("About to call ServerRPC!");
            TakeActionServerRpc(action, new());//Send to opponent
        }
        
        /// <summary>
        /// Mainly for UI purposes
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool IsCardPlayable(Card card) => true;
        /*{
            //TODO enough mana to play it. Any eligible targets etc.
        }*/
        bool ValidateAction(PlayerAction action)
        {
            //TODO: Do we have enough mana
            //Is the slot free
            //etc.
            return true;
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
                    if (card.cardType == Card.CardType.Minion)
                    {
                        CardSlot slot = minionSlots[who][action.Slot];
                        /*CardSlot target = null;
                        if (action.Target != -1)
                        {
                            target = minionSlots[who][action.Target];
                        }*/
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
                        AddToGrave(card,who);
                        card.standardScale = Vector3.one;//Maybe?
                    }
                    else throw new Exception("Unknown cardType");
                    break;
                case PlayerAction.ActionType.Attack:
                    throw new NotImplementedException("TakeAction Attack case not implemented");
                    break;
                default:
                    throw new NotImplementedException("TakeActionCase not implemented");
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void ToggleTurnServerRpc(ServerRpcParams Srpcparams)
        {
            //Debug.Log("SenderID: " + Srpcparams.Receive.SenderClientId);
            if (ServerOnTurn.Value && Srpcparams.Receive.SenderClientId == 0)
            {
                ServerOnTurn.Value = false;
            }
            else if (!ServerOnTurn.Value && Srpcparams.Receive.SenderClientId == 1) ServerOnTurn.Value = true;
        }
        [ClientRpc(RequireOwnership = false)]
        private void TakeActionClientRpc(PlayerAction action, ClientRpcParams crp)//This runs only on opponent.
        {
            /*
            //DebugInfo
            string message = "Action Recieved on: ";
            if (IsHost) message += "Host";
            else message += "Client";
            //message += " with ID: " + crp.Send.TargetClientIds[0];
            Debug.Log(message);
            */
            TakeAction(P.P2, action);

        }
        [ServerRpc(RequireOwnership = false)]
        private void TakeActionServerRpc(PlayerAction action, ServerRpcParams srp)
        {
            ulong target = 1 - srp.Receive.SenderClientId;
            //Debug.Log("We are in the Server RPC! This guy called: " + srp.Receive.SenderClientId + " Calling to: " + target);
            TakeActionClientRpc(action,
                new()
                {
                    Send = new()
                    {
                        TargetClientIds = new List<ulong>() { target }//We wanna talk to the other un.
                    }
                }
            );
        }
        [ServerRpc(RequireOwnership = false)]
        private void AnnounceDeckServerRpc(int[] deck, ServerRpcParams srp)
        {
            ulong target = 1 - srp.Receive.SenderClientId;
            Debug.Log("We are in the Server RPC! This guy called: " + srp.Receive.SenderClientId + " Calling to: " + target);
            AnnounceDeckClientRpc(deck,
                new()
                {
                    Send = new()
                    {
                        TargetClientIds = new List<ulong>() { target }//We wanna talk to the other un.
                    }
                }
            );
        }
        [ClientRpc(RequireOwnership = false)]
        private void AnnounceDeckClientRpc(int[] deck, ClientRpcParams crp)//This runs only on opponent.
        {
            LoadDeck(decks[P.P2], deck);
            //StartGame stuff


            //Animations (Who against who)


            //
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
    }
}