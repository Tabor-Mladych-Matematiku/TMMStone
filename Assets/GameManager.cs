using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Purchasing;
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
                CardCounter.text = deck.Count.ToString();
                cardSlot.pos.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            public Card PopFirst()
            {
                if (deck.Count == 0)return null;
                Card c = deck.First();
                deck.RemoveAt(0);
                CardCounter.text = deck.Count.ToString();
                if (deck.Count == 0)cardSlot.pos.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                return c;
            }

            public int IndexOf(Card item)=>deck.IndexOf(item);

            public void Insert(int index, Card item)
            {
                deck.Insert(index, item);
                CardCounter.text = deck.Count.ToString();
                cardSlot.pos.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }

            public void RemoveAt(int index)
            {
                deck.RemoveAt(index);
                CardCounter.text = deck.Count.ToString();
                if (deck.Count == 0)
                {
                    cardSlot.pos.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            public void Clear()
            {
                deck.Clear();
                CardCounter.text = deck.Count.ToString();
            }

            public bool Contains(Card item)=>deck.Contains(item);

            public void CopyTo(Card[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(Card item)
            {
                throw new NotImplementedException();
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
        public bool OnTurn
        {
            get
            {
                if (IsServer) return ServerOnTurn.Value;
                return !ServerOnTurn.Value;
            }
            set {
                if (IsServer) ServerOnTurn.Value = value;
                else ServerOnTurn.Value = !value;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (UnityEngine.Random.Range(0, 2) == 0) ServerOnTurn.Value = false;
                Debug.Log("Server is: " + OnTurn);
            }
            EndTurnBtn.onClick.AddListener (()=>ToggleTurnServerRpc(new ServerRpcParams()));
            ServerOnTurn.OnValueChanged += (bool prev, bool n)=> {
                if (prev == n) return;//Should not happen but to make sure.
                EndTurn();
                if (!OnTurn)
                {
                    EndTurnBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Opponents Turn";
                    EndTurnBtn.interactable = false;
                } 
                else
                {
                    EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
                    EndTurnBtn.interactable = true;
                }
                
                StartTurn();
                
            };
            deck = new() { CardCounter = OwnCardCounter.GetComponent<TextMeshProUGUI>(),cardSlot = DeckSlot };
            Oppdeck = new() { CardCounter = OppCardCounter.GetComponent<TextMeshProUGUI>(),cardSlot = OppDeckSlot };
            //Load cards
            LoadCardDatabase();
            //Load decks
            var OwnDeckList = (List<object>)MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/MyDeck").text);
            foreach (long item in OwnDeckList)
            {
                var c = Instantiate(CardPrefab);
                c.SetActive(false);
                Card Cardscript = c.GetComponent<Card>();
                Cardscript.initialize(CardDatabase[(int)item]);
                deck.Add(Cardscript);
            }
            object OppDeckdecodedJSON = MiniJson.JsonDecode(Resources.Load<TextAsset>("CardData/MyDeck").text);
            var OppDeckList = (List<object>)OppDeckdecodedJSON;
            foreach (long item in OppDeckList)
            {
                var c = Instantiate(CardPrefab);
                c.SetActive(false);
                Card Cardscript = c.GetComponent<Card>();
                Cardscript.initialize(CardDatabase[(int)item]);
                Oppdeck.Add(Cardscript);
            }
            //StartGame stuff
            Health = MaxHealth;
            OppHealth = OppMaxHealth;

            //Animations (Who against who)


            //
            //Cointoss anim
            if (!OnTurn)
            {
                EndTurnBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Opponents Turn";
                EndTurnBtn.interactable = false;
            }
            else
            {
                EndTurnBtn.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
                EndTurnBtn.interactable = true;
            }
            for (int i = 0; i < 3; i++)
                DrawCard(OnTurn);
            for (int i = 0; i < 4; i++)
                DrawCard(!OnTurn);
            //Mulligan
            StartTurn();
        }
        public void StartTurn()
        {
            TurnCount++;
            DrawCard(OnTurn);
            if (OnTurn)
            {
                //Enable actions etc.
                //Enable End Turn
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
            if (card == null)//Proly create a fake fatigue card that gets played on discard essentially and deals damage.
            {
                Fatigue(who);
            }
            else
            {
                card.gameObject.SetActive(true);
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
                if (who && !HandSlotsOwn[i].occupied)
                {
                    HandSlotsOwn[i].occupied = true;
                    c.transform.position = HandSlotsOwn[i].Position;
                    break;
                }
                else if (!who && !HandSlotsOpp[i].occupied)
                {
                    c.Hidden = true;
                    HandSlotsOpp[i].occupied = true;
                    c.transform.position = HandSlotsOpp[i].Position;
                    break;
                }

            }
        }
        public void Discard(Card c, bool who)
        {
            c.OnDiscard();
            AddToGrave(c,who);
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
        public void EndTurn()
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
                c.transform.position -= new Vector3(0,0, ((float)grave.count)/100);
            }
            else {
                c.transform.position = OppGraveSlot.Position;
                Oppgrave.count++;
                c.transform.position -= new Vector3(0, 0, ((float)Oppgrave.count) / 100);
            }

        }
        [ServerRpc(RequireOwnership =false)]
        private void ToggleTurnServerRpc(ServerRpcParams Srpcparams)
        {
            //Debug.Log("SenderID: " + Srpcparams.Receive.SenderClientId);
            if (ServerOnTurn.Value && Srpcparams.Receive.SenderClientId==0)
            {
                ServerOnTurn.Value = false;
            }
            else if(!ServerOnTurn.Value && Srpcparams.Receive.SenderClientId == 1) ServerOnTurn.Value = true;
        }
    }
}