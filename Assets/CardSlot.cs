using CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public bool Occupied { get => transform.childCount != 0; }
    public Vector3 Position { get => Position; }
    public GameManager.P Owner {  get; set; }
    public int index;
    public void Initialize(GameManager.P owner,int index)
    {
        Owner = owner;
        this.index = index;
    }
    public void PlaceCard(Card c)
    {
        if (Occupied) throw new CardSlotException("Already occupied");
        c.transform.parent = transform;
    }
    public Card GetCard()
    {
        return transform.GetComponentInChildren<Card>();
    }
    public Card PopCard()
    {
        Card c = transform.GetComponentInChildren<Card>();
        c.transform.parent = null;
        return c;
    }

    public class CardSlotException : Exception
    {
        public CardSlotException(string message) : base(message) { }
    }
}
