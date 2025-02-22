using CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public abstract class PlacableSlot : CardSlot
{
    public abstract bool IsCardPlacable(Card c);
    Image HighlightBox;
    Color highlightc = new(0, 1, 0, 0.3f);

    void Start()
    {
        HighlightBox = GetComponent<Image>();
    }
    protected virtual void OnMouseOver()
    {
        Card holding = GameManager.Instance.cursor;
        if (holding != null && IsCardPlacable(holding) && transform.childCount == 0)
        {
            HighlightBox.color = highlightc;
            GameManager.Instance.highlightedSlot = transform;
        }
    }
    private void OnMouseExit()
    {
        if (Owner != GameManager.P.P1) return;
        HighlightBox.color = new(0, 0, 0, 0);
        GameManager.Instance.highlightedSlot = null;
    }
}

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
