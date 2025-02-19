using CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grave : MonoBehaviour,IList<Card>//TODO: might be better to have a list and not ask the tree about it all the time. Sounds effortfull
{
    public Card this[int index] { get => transform.GetComponentInChildren<Card>(); set => throw new System.NotImplementedException(); }

    public int Count { get => transform.childCount; }

    public bool IsReadOnly => throw new System.NotImplementedException();

    public void Add(Card item)
    {
        item.transform.parent = transform;
        item.transform.localPosition = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), item.transform.localPosition.z);
        item.Hidden = false;
    }
    /// <summary>
    /// Destroys all Cards in grave
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(0).gameObject);//Oughta be one cuz y'know
    }

    public bool Contains(Card item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(Card[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator<Card> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    public int IndexOf(Card item)
    {
        throw new System.NotImplementedException();
    }

    public void Insert(int index, Card item)
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(Card item)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}
