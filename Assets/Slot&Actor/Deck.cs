
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CardGame
{
    /// <summary>
    /// Mutable card storage which can be shared by multiple zone wrappers.
    /// This is needed by cards which make both players draw from the same pile.
    /// </summary>
    public sealed class CardPile
    {
        public readonly List<Card> Cards;
        public event Action Changed;

        public CardPile() : this(new List<Card>()) { }
        public CardPile(IEnumerable<Card> cards) => Cards = new List<Card>(cards);
        public void NotifyChanged() => Changed?.Invoke();
    }

    public class Deck : MonoBehaviour, IList<Card>
    {
        private CardPile pile = new();
        public TextMeshProUGUI CardCounter;
        //Its not ideal that we are checking the if we should enable the image in every function but I don't have a better reasonable idea rn.
        public int Count => pile.Cards.Count;

        public bool IsReadOnly => false;

        public Card this[int index]
        {
            get => pile.Cards[index];
            set { pile.Cards[index] = value; pile.NotifyChanged(); }
        }

        public void Add(Card c)
        {
            pile.Cards.Add(c);
            c.transform.SetParent(transform);
            c.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));
            c.Hidden = true;
            RefreshCounter();
            pile.NotifyChanged();
        }
        public Card PopFirst()
        {
            if (pile.Cards.Count == 0) return null;
            Card c = pile.Cards.First();
            pile.Cards.RemoveAt(0);
            c.transform.SetParent(null);
            RefreshCounter();
            pile.NotifyChanged();
            return c;
        }

        public int IndexOf(Card item) => pile.Cards.IndexOf(item);

        /// <summary>
        /// Inserts and adds the card as child
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Card item)
        {
            pile.Cards.Insert(index, item);
            item.transform.SetParent(transform);
            RefreshCounter();
            pile.NotifyChanged();
        }
        /// <summary>
        /// Destroys the card from existence
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            Card card = pile.Cards[index];
            pile.Cards.RemoveAt(index);
            Destroy(card.gameObject);
            RefreshCounter();
            pile.NotifyChanged();
        }
        /// <summary>
        /// Destroys all cards in pile.Cards
        /// </summary>
        public void Clear()
        {
            foreach (var item in pile.Cards)
            {
                Destroy(item.gameObject);
            }
            pile.Cards.Clear();
            RefreshCounter();
            pile.NotifyChanged();
        }

        public bool Contains(Card item) => pile.Cards.Contains(item);
        /// <summary>
        /// I don't know whether this should be used. Just don't delete the cards or something
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(Card[] array, int arrayIndex)
        {
            pile.Cards.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Removes Card from pile.Cards. Does not destroy Card but purges its parent
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Card item)
        {
            if (pile.Cards.Remove(item))
            {
                item.transform.parent = null;
                RefreshCounter();
                pile.NotifyChanged();
                return true;
            }
            return false;
        }

        public IEnumerator<Card> GetEnumerator() => pile.Cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=> pile.Cards.GetEnumerator();
        public void AddRandom(Card spell)
        {
            Insert(UnityEngine.Random.Range(0, pile.Cards.Count+1), spell);//+1 because we want to be able to insert at the end
            //WARNING, TODO: I still did not make a good random. I don't know if it works
        }

        public CardPile Pile => pile;

        public void UsePile(CardPile sharedPile)
        {
            pile.Changed -= RefreshCounter;
            pile = sharedPile ?? throw new ArgumentNullException(nameof(sharedPile));
            pile.Changed += RefreshCounter;
            RefreshCounter();
        }

        public void RefreshCounter()
        {
            if (CardCounter != null) CardCounter.text = pile.Cards.Count.ToString();
        }
    }
}
