using CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Deck : MonoBehaviour, IList<Card>
{
    private readonly List<Card> deck = new();
    public TextMeshProUGUI CardCounter;
    //Its not ideal that we are checking the if we should enable the image in every function but I don't have a better reasonable idea rn.
    public int Count => deck.Count;

    public bool IsReadOnly => false;

    public Card this[int index] { get => deck[index]; set => deck[index] = value; }

    public void Add(Card c)
    {
        deck.Add(c);
        c.transform.parent=transform;
        c.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), -1);
        c.Hidden = true;
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
        item.transform.parent = transform;
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
