using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

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
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    [System.Serializable]
    public class CardSlot
    {
        public Transform pos;
        public bool occupied;
        public Vector3 Position { get => pos.position; }
    }

    public class GameManager : NetworkBehaviour
    {
        public class Deck : IList<Card>
        {
            List<Card> deck = new();
            public TextMeshProUGUI CardCounter;
            public CardSlot cardSlot;//Its not ideal that we are checking the if we should enable the image in every function but I don't have a better reasonable idea rn.
            public int Count => deck.Count;

            public bool IsReadOnly => false;

            public Card this[int index] { get => deck[index]; set => deck[index] = value; }

            public void Add(Card c)
            {
                deck.Add(c);
                c.transform.parent = cardSlot.pos;
                CardCounter.text = deck.Count.ToString();
            }
            public Card PopFirst()
            {
                if (deck.Count == 0) return null;
                Card c = deck.First();
                deck.RemoveAt(0);
                c.transform.parent = null;
                CardCounter.text = deck.Count.ToString();
                return c;
            }

            public int IndexOf(Card item) => deck.IndexOf(item);

            /// <summary>
            /// Inserts and adds the card as child
            /// </summary>
            /// <param name="index"></param>
            /// <param name="item"></param>
            public void Insert(int index, Card item)
            {
                deck.Insert(index, item);
                item.transform.parent = cardSlot.pos;
                CardCounter.text = deck.Count.ToString();
            }
            /// <summary>
            /// Destroys the card from existence
            /// </summary>
            /// <param name="index"></param>
            public void RemoveAt(int index)
            {
                deck.RemoveAt(index);
                Destroy(deck.ElementAt(index).gameObject);
                CardCounter.text = deck.Count.ToString();
            }
            /// <summary>
            /// Destroys all cards in deck
            /// </summary>
            public void Clear()
            {
                foreach (var item in deck)
                {
                    Destroy(item.gameObject);
                }
                deck.Clear();
                CardCounter.text = deck.Count.ToString();
            }

            public bool Contains(Card item) => deck.Contains(item);

            public void CopyTo(Card[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Removes Card from deck. Does not destroy Card but purges its parent
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Remove(Card item)
            {
                if (deck.Remove(item))
                {
                    item.transform.parent = null;
                    CardCounter.text = deck.Count.ToString();
                    return true;
                }
                return false;
            }

            public IEnumerator<Card> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
        public class Grave
        {
            public int count;
        }
        public struct PlayerAction : INetworkSerializable
        {
            public enum ActionType
            {
                Play,
                Attack
            }
            private int c;
            public int Source { readonly get => c; private set => c = value; }
            private ActionType actionType;
            public ActionType Actiontype { readonly get => actionType; private set => actionType = value; }
            private int slot;
            public int Target { readonly get => slot; private set => slot = value; }//Used for targetting various things
            public static PlayerAction PlayCardAction(int card, int slot)
            {
                return new()
                {
                    Source = card,
                    Actiontype = ActionType.Play,
                    Target = slot
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
            }
        }

        public static GameManager Instance { get; private set; }
        Deck deck;
        Deck Oppdeck;
        Grave grave = new();
        Grave Oppgrave = new();
        public Transform OwnMin;
        public Transform OppMin;
        public CardSlot[] minionSlotsOwn;
        public CardSlot[] minionSlotsOpp;
        public CardSlot[] EffSlotsOwn;
        public CardSlot[] EffSlotsOpp;
        public CardSlot[] HandSlotsOwn;
        public CardSlot[] HandSlotsOpp;
        private Dictionary<bool, CardSlot[]> HandSlots;
        public CardSlot Field;
        public CardSlot DeckSlot;
        public CardSlot OppDeckSlot;
        public CardSlot GraveSlot;
        public CardSlot OppGraveSlot;
        public Transform OwnHPLabel;
        public Transform OppHPLabel;
        public Transform OwnCardCounter;
        public Transform OppCardCounter;
        public Dictionary<int, CardData> CardDatabase;
        public GameObject CardPrefab;
        public Button EndTurnBtn;
        private const int maxMinionSlots = 7;

        int health;
        int opphealth;
        public int Health
        {
            get => health; set
            {
                health = value;
                OwnHPLabel.gameObject.GetComponent<TextMeshProUGUI>().text = Health.ToString();
            }
        }
        public int OppHealth
        {
            get => opphealth; set
            {
                opphealth = value;
                OppHPLabel.gameObject.GetComponent<TextMeshProUGUI>().text = OppHealth.ToString();
            }
        }
        public int MaxHealth = 30;
        public int OppMaxHealth = 30;

        public int OwnFatigueVal = 1;
        public int OppFatigueVal = 1;

        public int TurnCount = 0;//This is not Round counter - tzn this is double of RoundCount

        public NetworkVariable<bool> ServerOnTurn = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        internal Card cursor;
        internal Transform highlightedSlot;
        internal int HighlightedSlotIndex
        {
            get
            {
                int slot = -1;
                if (highlightedSlot == null) return slot;
                for (int i = 0; i < minionSlotsOwn.Length; i++)
                {
                    if (minionSlotsOwn[i].pos == highlightedSlot) { slot = i; break; }
                }
                if(slot!=-1)return slot;
                for (int i = 0; i < minionSlotsOwn.Length; i++)
                {
                    if (minionSlotsOpp[i].pos == highlightedSlot) { slot = maxMinionSlots+i; break; }
                }
                return slot;
            }
        }

        public bool OnTurn
        {
            get
            {
                if (IsServer) return ServerOnTurn.Value;
                return !ServerOnTurn.Value;
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
            HandSlots = new()
            {
                {true,HandSlotsOwn },
                {false,HandSlotsOpp }
            };
        }
        private void Update()
        {
            if (cursor != null) {
                Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursor.transform.position = new(mousepos.x,mousepos.y,-1);
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (UnityEngine.Random.Range(0, 2) == 0) ServerOnTurn.Value = false;
                Debug.Log("Host starts?: " + OnTurn);
            }
            EndTurnBtn.onClick.AddListener(() => ToggleTurnServerRpc(new ServerRpcParams()));
            ServerOnTurn.OnValueChanged += (bool prev, bool n) =>
            {
                if (prev == n) return;//Should not happen but to make sure.
                EndTurnEffects();
                StartTurn();

            };
            deck = new() { CardCounter = OwnCardCounter.GetComponent<TextMeshProUGUI>(), cardSlot = DeckSlot };
            Oppdeck = new() { CardCounter = OppCardCounter.GetComponent<TextMeshProUGUI>(), cardSlot = OppDeckSlot };
            //Load cards
            LoadCardDatabase();
            //Load decks
            var OwnDeckList = (List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/MyDeck").text);//TODO import from json in PlayerData
            LoadDeck(deck, OwnDeckList);
            var OppDeckList = (List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/MyDeck").text);//TODO get from Lobby
            LoadDeck(Oppdeck, OppDeckList);
            //StartGame stuff
            Health = MaxHealth;
            OppHealth = OppMaxHealth;

            //Animations (Who against who)


            //
            //Cointoss anim
            for (int i = 0; i < 3; i++)
                DrawCard(OnTurn);
            for (int i = 0; i < 4; i++)
                DrawCard(!OnTurn);
            //Mulligan
            StartTurn();
        }
        private void LoadDeck(Deck d, List<object> DecodedDeckList)
        {
            foreach (long item in DecodedDeckList)
            {
                var c = Instantiate(CardPrefab);
                Card Cardscript = c.GetComponent<Card>();
                Cardscript.initialize(CardDatabase[(int)item]);
                Cardscript.Hidden = true;
                d.Add(Cardscript);
                c.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), -1);
            }
        }
        public void StartTurn()
        {
            if (!OnTurn)
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Opponents Turn";
                EndTurnBtn.interactable = false;
            }
            else
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
                EndTurnBtn.interactable = true;
            }
            TurnCount++;
            DrawCard(OnTurn);
            if (OnTurn)
            {
                //Enable actions etc.
            }
        }
        public void DrawCard(bool who)//We are true opp is false.
        {

            //We draw card
            Card card;
            if (who)
            {
                card = deck.PopFirst();
            }
            else
            {
                card = Oppdeck.PopFirst();
            }
            if (card == null)
            {
                Fatigue(who);
            }
            else
            {
                AddCardToHand(who, card);
            }

        }
        public void AddCardToHand(bool who, Card c)
        {
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                {
                    Discard(c, who);
                    break;
                }
                if (!HandSlots[who][i].occupied)
                {
                    c.Hidden = false;
                    HandSlots[who][i].occupied = true;
                    c.transform.parent = HandSlots[who][i].pos;
                    c.transform.localPosition = new Vector3(0, 0, -1);
                    c.standardScale = c.transform.localScale;
                    break;
                }/*
                else if (!who && !HandSlotsOpp[i].occupied)
                {
                    c.Hidden = true;
                    HandSlotsOpp[i].occupied = true;
                    c.transform.parent = HandSlotsOpp[i].pos;
                    c.transform.localPosition = new Vector3(0, 0, 0);
                    c.standardScale = c.transform.localScale;
                    break;
                }*/

            }
        }
        public void Discard(Card c, bool who)
        {
            c.OnDiscard();
            AddToGrave(c, who);
        }
        public void Fatigue(bool who)
        {
            if (who)
            {
                Health -= OwnFatigueVal;
                OwnFatigueVal++;
            }
            else
            {
                OppHealth -= OppFatigueVal;
                OppFatigueVal++;
            }
        }
        public void ShuffleDeck(bool who)
        {
            if (who) deck.Shuffle();
            else Oppdeck.Shuffle();

        }
        public void EndTurnEffects()
        {
            //Do end turn effects on my cards: (mins,eff,hand). (End of each turn will not be handled. I cannot be bothered)
            //
        }
        void LoadCardDatabase()
        {
            var cardsJSONfile = Resources.Load<TextAsset>("CardData/cards");
            object decodedJSON = MiniJson.JsonDecode(cardsJSONfile.text);
            var loadDict = (Dictionary<string, object>)decodedJSON;

            CardDatabase = new();
            foreach (var pair in loadDict)
            {
                var value = (Dictionary<string, object>)pair.Value;
                if (value["cost"] is String) continue;//We'll deal with you later (These are the ? costs)
                CardData data = new()
                {
                    name = (string)value["name"],
                    rarity = (string)value["rarity"],
                    type = (string)value["type"],
                    cost = (int)((System.Int64)value["cost"]),
                    tag = (string)value["tag"],
                    Class = (string)value["Class"],
                    expansion = (string)value["expansion"],
                    not_for_sale = (bool)value["not_for_sale"]

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
        void AddToGrave(Card c, bool who)
        {
            if (who)
            {
                c.transform.position = GraveSlot.Position;
                grave.count++;
                c.transform.position -= new Vector3(0, 0, ((float)grave.count) / 100);
            }
            else
            {
                c.transform.position = OppGraveSlot.Position;
                Oppgrave.count++;
                c.transform.position -= new Vector3(0, 0, ((float)Oppgrave.count) / 100);
            }

        }
        public void OnUIPlayMinion(Card c)
        {
            int cardIndex = -1;
            for (int i = 0; i < HandSlotsOwn.Length; i++)
            {
                if(c== HandSlotsOwn[i].pos.GetComponentInChildren<Card>())
                {
                    cardIndex= i;
                }
            }
            if (cardIndex < 0) return;

            

            PlayerAction action = PlayerAction.PlayCardAction(cardIndex,HighlightedSlotIndex);
            OnUITakeAction(action);
        }
        public void OnUIAttackMinion()
        {
            throw new NotImplementedException();
            int cardIndex;
            int slot;
            PlayerAction action = PlayerAction.AttackAction(cardIndex,slot);
            OnUITakeAction(action);
        }
        public void OnUICastSpell()
        {
            throw new NotImplementedException();
            int cardIndex;
            int target;
            PlayerAction action = PlayerAction.PlayCardAction(cardIndex, target);
            OnUITakeAction(action);
        }
        public void OnUITakeAction(PlayerAction action)
        {
            if (!ValidateAction(action)) return;
            TakeAction(true, action);//Play it out
            TakeActionServerRpc(action, new());//Send to opponent
        }
        public bool IsCardPlayable(Card card)
        {
            return true;//TODO enough mana to play it. Any eligible targets etc.
            //Mainly for UI purposes
        }
        bool ValidateAction(PlayerAction action)
        {
            //Do we have enough mana
            //Is the slot free
            //etc.
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="who"> True means us. False means opponent</param>
        /// <param name="action"></param>
        void TakeAction(bool who, PlayerAction action)
        {
            if (who)
            {

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
            //DebugInfo
            string message = "Action Recieved on: ";
            if (IsHost) message += "Host";
            else message += "Client";
            message += " with ID: " + crp.Send.TargetClientIds[0];
            Debug.Log(message);


            //TODO simulate opponents action.
            TakeAction(false, action);

        }
        [ServerRpc(RequireOwnership = false)]
        private void TakeActionServerRpc(PlayerAction action, ServerRpcParams srp)
        {
            TakeActionClientRpc(action,
                new()
                {
                    Send = new()
                    {
                        TargetClientIds = new List<ulong> { 1 - srp.Receive.SenderClientId }//We wanna talk to the other un.
                    }
                }
            );
        }
    }
}