using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CardGame
{
    public class Grave : MonoBehaviour, IList<Card>//TODO: might be better to have a list and not ask the tree about it all the time. Sounds effortfull
    {
        List<Card> cards = new();


        public Card this[int index] { get => transform.GetChild(index).GetComponent<Card>(); set => throw new System.NotImplementedException(); }

        public int Count { get => transform.childCount; }

        public bool IsReadOnly => throw new System.NotImplementedException();

        public void Add(Card item)
        {
            item.transform.SetParent(transform);
            item.transform.localPosition = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            item.standardScale = transform.localScale;
            item.Hidden = false;
            cards.Add(item);
        }
        /// <summary>
        /// Destroys all Cards in grave
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(0).gameObject);//Oughta be one cuz y'know
            cards.Clear();
        }

        public bool Contains(Card item) => cards.Contains(item);

        public void CopyTo(Card[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<Card> GetEnumerator() => cards.GetEnumerator();

        public int IndexOf(Card item) => cards.IndexOf(item);

        public void Insert(int index, Card item)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(Card item)
        {
            if (cards.Remove(item))
            {
                for (int i = 0; i < transform.childCount; i++)
                    if (transform.GetChild(i).GetComponent<Card>() == item)
                    {
                        Destroy(transform.GetChild(i).gameObject);
                        return true;
                    }
                throw new System.Exception("Card was removed from list but not found in transform children");
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            cards.RemoveAt(index);
            Destroy(transform.GetChild(index).gameObject);
        }

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
}