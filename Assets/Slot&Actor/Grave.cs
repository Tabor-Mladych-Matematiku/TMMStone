using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CardGame
{
    public class Grave : MonoBehaviour, IList<Card>//TODO: might be better to have a list and not ask the tree about it all the time. Sounds effortfull
    {
        CardPile pile = new();


        public Card this[int index] { get => pile.Cards[index]; set => throw new System.NotImplementedException(); }

        public int Count => pile.Cards.Count;

        public bool IsReadOnly => false;

        public void Add(Card item)
        {
            if (item == null) throw new System.ArgumentNullException(nameof(item));

            PlaceInGrave(item);
            pile.Cards.Add(item);
            pile.NotifyChanged();
        }

        private void PlaceInGrave(Card item)
        {
            item.transform.SetParent(transform);
            item.transform.localPosition = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            item.standardScale = transform.localScale;
            item.Hidden = false;
        }
        /// <summary>
        /// Destroys all Cards in grave
        /// </summary>
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            pile.Cards.Clear();
            pile.NotifyChanged();
        }

        public bool Contains(Card item) => pile.Cards.Contains(item);

        public void CopyTo(Card[] array, int arrayIndex)
        {
            pile.Cards.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Card> GetEnumerator() => pile.Cards.GetEnumerator();

        public int IndexOf(Card item) => pile.Cards.IndexOf(item);

        public void Insert(int index, Card item)
        {
            if (item == null) throw new System.ArgumentNullException(nameof(item));
            if ((uint)index > (uint)pile.Cards.Count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            PlaceInGrave(item);
            pile.Cards.Insert(index, item);
            pile.NotifyChanged();
        }

        public bool Remove(Card item)
        {
            if (pile.Cards.Remove(item))
            {
                Destroy(item.gameObject);
                pile.NotifyChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a card from the grave without destroying it, so it can be moved
        /// to another zone.
        /// </summary>
        public bool Take(Card item)
        {
            if (!pile.Cards.Remove(item)) return false;

            item.transform.SetParent(null);
            pile.NotifyChanged();
            return true;
        }

        public void RemoveAt(int index)
        {
            Card item = pile.Cards[index];
            pile.Cards.RemoveAt(index);
            Destroy(item.gameObject);
            pile.NotifyChanged();
        }

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public CardPile Pile => pile;

        public void UsePile(CardPile sharedPile)
        {
            pile = sharedPile ?? throw new System.ArgumentNullException(nameof(sharedPile));
        }
    }
}
